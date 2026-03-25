using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Services.Momo;
using WebDongHoLG.Services.Vnpay;
using WebDongHoLG.ViewModels;
using WebDongHoLG.ViewModels.vnpay;

namespace WebDongHoLG.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly ShopDongHoDbContext _context;

        public ThanhToanController(IVnPayService vnPayService, ShopDongHoDbContext context, IMomoService momoService)
        {
            _momoService = momoService;
            _context = context;
            _vnPayService = vnPayService;
        }

        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentMomo(model);
            return Redirect(response.PayUrl);
        }

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

       





        [HttpGet]
        public async Task<IActionResult> PaymentCallBackMomo()
        {
            // 1. Tóm gọn cả errorCode (V1) và resultCode (V2)
            var resultCode = HttpContext.Request.Query["resultCode"].ToString();
            var errorCode = HttpContext.Request.Query["errorCode"].ToString();

            // Thằng nào có dữ liệu thì lấy thằng đó
            var finalCode = (!string.IsNullOrEmpty(resultCode) ? resultCode : errorCode).Trim();

            // 2. Lấy OrderId và cắt bỏ cái đuôi thời gian để lấy mã gốc (Ví dụ: "28_6391007..." -> lấy số 28)
            var orderIdString = HttpContext.Request.Query["orderId"].ToString().Trim();
            int maDonHang = 0;

            if (!string.IsNullOrEmpty(orderIdString))
            {
                var mangChuoi = orderIdString.Split('_');
                int.TryParse(mangChuoi[0], out maDonHang);
            }

            // Fallback phòng hờ trường hợp rớt mạng
            if (maDonHang == 0) maDonHang = HttpContext.Session.GetInt32("PendingOrderId") ?? 0;

            // 3. Xử lý trạng thái đơn hàng
            if (maDonHang > 0)
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang);
                if (donHang != null)
                {
                    // MÃ 0 LÀ THÀNH CÔNG RỰC RỠ
                    if (finalCode == "0")
                    {
                        donHang.TrangThai = "Đã thanh toán";
                        await _context.SaveChangesAsync();

                        HttpContext.Session.Remove("PendingOrderId");
                        TempData["ToastSuccess"] = "Thanh toán Momo thành công!";

                        // Đá thẳng về trang OrderSuccess
                        return RedirectToAction("OrderSuccess", "Checkout", new { maDonHang = maDonHang });
                    }
                    else
                    {
                        // 1. Cập nhật trạng thái trong DB: Nếu là lỗi hủy (1006) thì để "Chờ thanh toán", lỗi khác thì ghi "Thanh toán thất bại"
                        donHang.TrangThai = (finalCode == "1006") ? "Chờ thanh toán" : "Thanh toán thất bại";
                        await _context.SaveChangesAsync();

                        // 2. Tạo câu thông báo lỗi cho thân thiện
                        string msg = finalCode switch
                        {
                            "1006" => "Bạn đã hủy giao dịch MoMo.",
                            "1005" => "Giao dịch MoMo đã hết hạn.",
                            "49" => "Người dùng từ chối xác nhận thanh toán.",
                            _ => $"Thanh toán thất bại! (Mã lỗi: {finalCode})"
                        };
                        TempData["ToastError"] = msg;

                        // 3. QUYẾT ĐỊNH: Luôn quay về trang PaymentPending thay vì Giỏ hàng
                        // Điều này giúp khách xem lại đơn hàng vừa đặt và có thể ấn "Thanh toán lại" ngay
                        return RedirectToAction("PaymentPending", "Checkout", new { maDonHang = maDonHang });
                    }
                }
            }
            else
            {
                TempData["ToastError"] = "Lỗi: Không tìm thấy Mã đơn hàng từ MoMo trả về!";
            }

            return RedirectToAction("Index", "GioHangs");
        }


        [HttpPost]
        public IActionResult MomoNotify()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var transactionStatus = Request.Query["vnp_TransactionStatus"].ToString();
            var maDonHang = HttpContext.Session.GetInt32("PendingOrderId");

            bool thanhCong = response.Success
                            && responseCode == "00"
                            && transactionStatus == "00";

            if (thanhCong && maDonHang.HasValue)
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang.Value);
                if (donHang != null)
                {
                    donHang.TrangThai = "Đã thanh toán";
                    await _context.SaveChangesAsync();
                }
                HttpContext.Session.Remove("PendingOrderId");
                TempData["ToastSuccess"] = "Thanh toán VNPay thành công!";
                return RedirectToAction("OrderSuccess", "Checkout",
                    new { maDonHang = maDonHang.Value });
            }

            if (maDonHang.HasValue)
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang.Value);
                if (donHang != null)
                {
                   
                    donHang.TrangThai = (responseCode == "24" || responseCode == "11")
                        ? "Chờ thanh toán"
                        : "Thanh toán thất bại";
                    await _context.SaveChangesAsync();
                }

                string msg = responseCode switch
                {
                    "24" => "Bạn đã hủy thanh toán. Vui lòng chọn phương thức khác.",
                    "11" => "Giao dịch hết hạn! Vui lòng thử lại.",
                    "09" => "Thẻ/tài khoản chưa đăng ký dịch vụ.",
                    _ => $"Thanh toán thất bại! Mã lỗi: {responseCode}"
                };
                TempData["ToastError"] = msg;

                if (responseCode == "24" || responseCode == "11")
                {
                    return RedirectToAction("PaymentPending", "Checkout",
                        new { maDonHang = maDonHang.Value });
                }
            }

            HttpContext.Session.Remove("PendingOrderId");
            return RedirectToAction("Index", "GioHangs");
        }


    }
}
