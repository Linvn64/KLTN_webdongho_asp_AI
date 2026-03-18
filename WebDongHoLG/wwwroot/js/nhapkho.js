function initWarehousePage(yeuCauXuatHoaDon, hangDaChon, activeTab) {
    document.addEventListener("DOMContentLoaded", function () {

        // 1. Ghi nhớ Tab khi nhấn thủ công
        document.querySelectorAll('a[data-bs-toggle="tab"]').forEach(tabLink => {
            tabLink.addEventListener('shown.bs.tab', function (e) {
                const id = e.target.getAttribute('href').replace('#', '');
                sessionStorage.setItem('warehouseActiveTab', id);
            });
        });

        // 2. Tải Excel hóa đơn nếu vừa lưu thành công
        if (yeuCauXuatHoaDon === "true") {
            xuatHoaDonExcel(hangDaChon);
        }

        // 3. Tự động ẩn Alert sau 3s
        const alertBox = document.getElementById('autoCloseAlert');
        if (alertBox) {
            setTimeout(() => {
                alertBox.style.transition = "opacity 0.5s ease";
                alertBox.style.opacity = "0";
                setTimeout(() => alertBox.remove(), 500);
            }, 3000);
        }
    });
}

function xuatHoaDonExcel(hang) {
    let data = [["HÓA ĐƠN NHẬP KHO"], ["Ngày:", new Date().toLocaleString()], ["Hãng:", hang], [], ["ID", "TÊN SP", "GIÁ", "SL", "THÀNH TIỀN"]];
    document.querySelectorAll("#bangNhapKho tbody tr").forEach(row => {
        const sl = parseInt(row.querySelector(".o-nhap").value) || 0;
        if (sl > 0) {
            const ma = row.querySelector(".sp-ma").innerText;
            const ten = row.querySelector(".sp-ten").innerText;
            const gia = parseInt(row.querySelector(".sp-gia").innerText.replace(/\D/g, ''));
            data.push([ma, ten, gia, sl, gia * sl]);
        }
    });
    const ws = XLSX.utils.aoa_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "HoaDon");
    XLSX.writeFile(wb, "PhieuNhap_" + Date.now() + ".xlsx");
}

function xuatExcelLichSu(tongTien) {
    let data = [["BÁO CÁO NHẬP KHO"], ["Thời gian:", new Date().toLocaleString()], ["Tổng vốn:", tongTien + "đ"], [], ["THỜI GIAN", "SẢN PHẨM", "HÃNG", "SL", "GIÁ", "THÀNH TIỀN", "NGƯỜI NHẬP"]];
    document.querySelectorAll("#bangLichSu tbody tr").forEach(row => {
        const cols = row.querySelectorAll("td");
        if (cols.length > 0) {
            data.push([cols[0].innerText.replace(/\n/g, ' '), cols[1].innerText.trim(), cols[2].innerText.trim(), cols[3].innerText.replace('+', '').trim(), cols[4].innerText.trim(), cols[5].innerText.trim()]);
        }
    });
    const ws = XLSX.utils.aoa_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "BaoCao");
    XLSX.writeFile(wb, "BaoCao_Kho_" + Date.now() + ".xlsx");
}