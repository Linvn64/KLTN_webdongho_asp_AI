using WebDongHoLG.ViewModels;
using WebDongHoLG.ViewModels.momo;

namespace WebDongHoLG.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);

        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
