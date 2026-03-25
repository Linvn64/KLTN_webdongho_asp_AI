function showAddForm() {
    $('#addressSubmitForm')[0].reset();
    $('#editMaDiaChi').val(0);
    $('#btnSubmitAddr').text('Xác nhận thêm mới').removeClass('btn-primary').addClass('btn-danger');
    $('#formAddress').toggle();
}

$(document).on('submit', '#addressSubmitForm', function (e) {
    e.preventDefault();
    let sdt = $('#inputSdt').val();
    if (sdt.length !== 10) {
        Swal.fire('Lỗi', 'Số điện thoại phải có đúng 10 chữ số!', 'error');
        return;
    }

    $.ajax({
        url: '/Checkout/UpdateAddress',
        type: 'POST',
        data: $(this).serialize(),
        success: function (res) {
            if (res.success) {
                renderAddressList(res.data);
                $('#formAddress').hide();
                Swal.fire('Thành công', 'Cập nhật địa chỉ thành công!', 'success');
            } else {
                Swal.fire('Thất bại', res.message, 'error');
            }
        }
    });
});

function renderAddressList(list) {
    let html = '<h6 class="fw-bold border-bottom pb-2">Chọn địa chỉ nhận hàng:</h6>';
    let hoTenCheck = $('#inputHoTen').val() || "Khách hàng";

    list.forEach(dc => {
        html += `
        <div class="form-check d-flex align-items-center border-bottom py-3 custom-radio-address">
            <input class="form-check-input me-3" type="radio" name="rdAddress" id="addr-${dc.maDiaChi}" value="${dc.maDiaChi}" onchange="changeDefaultAddress(${dc.maDiaChi})">
            <label class="form-check-label w-100" for="addr-${dc.maDiaChi}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${hoTenCheck} (${dc.sdtNhanHang})</strong>
                        <br /><span class="text-muted small">${dc.diaChi}</span>
                    </div>
                    <div class="action-icons">
                        <a href="javascript:void(0)" onclick="fillEditForm('${dc.maDiaChi}', '${dc.sdtNhanHang}', '${dc.diaChi}', '${dc.ghiChu}')" class="text-primary me-2"><i class="fas fa-edit"></i></a>
                        <a href="javascript:void(0)" onclick="deleteAddressAjax(${dc.maDiaChi}, this)" class="text-danger"><i class="fas fa-trash"></i></a>
                    </div>
                </div>
            </label>
        </div>`;
    });
    $('#addressListContainer').html(html);
}

function deleteAddressAjax(maDiaChi, element) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        text: "Bạn không thể hoàn tác hành động này!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ee4d2d',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Đồng ý xóa'
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Checkout/DeleteAddress', { maDc: maDiaChi }, function (res) {
                if (res.success) {
                    $(element).closest('.form-check').fadeOut(300);
                }
            });
        }
    });
}

function changeDefaultAddress(maDiaChi) {
    window.location.href = '/Checkout/SelectAddress?maDc=' + maDiaChi;
}

function fillEditForm(id, sdt, diachi, tinh) {
    $('#formAddress').show();
    $('#editMaDiaChi').val(id);
    $('#inputSdt').val(sdt);
    $('#selectTinh').val(tinh);
    let parts = diachi.split(',');
    $('#inputChiTiet').val(parts[0].trim());
    $('#btnSubmitAddr').text('Cập nhật địa chỉ này').removeClass('btn-danger').addClass('btn-primary');
    document.getElementById('formAddress').scrollIntoView({ behavior: 'smooth' });
}

function updatePriceWithVoucher(phanTram) {
    let tongTienHang = parseFloat($('#valTongTienHang').val());
    let phiShip = parseFloat($('#valPhiShip').val());

    let tienGiam = tongTienHang * phanTram;
    let thanhToan = tongTienHang - tienGiam + phiShip;

    $('#displayGiamGia').text("-" + tienGiam.toLocaleString('vi-VN') + " đ");
    $('#displayTongThanhToan').text(thanhToan.toLocaleString('vi-VN') + " đ");
    $('#displayTongThanhToan').fadeOut(100).fadeIn(100);
}

$(document).ready(function () {
    function calculateTotal() {
        let total = 0;
        let count = 0;
        $('.item-check:checked').each(function () {
            total += parseFloat($(this).data('price'));
            count++;
        });

        $('#totalSelectedPrice').text(total.toLocaleString('vi-VN'));
        $('#selectedCount').text(count);

        if (count > 0) {
            $('#btnProceed').removeClass('opacity-50');
        } else {
            $('#btnProceed').addClass('opacity-50');
        }
    }

    $('#checkAll').on('change', function () {
        $('.item-check').prop('checked', $(this).prop('checked'));
        calculateTotal();
    });

    $(document).on('change', '.item-check', function () {
        calculateTotal();
        if ($('.item-check:checked').length === $('.item-check').length) {
            $('#checkAll').prop('checked', true);
        } else {
            $('#checkAll').prop('checked', false);
        }
    });

    $('.qty-input').on('change', function () {
        var newQty = $(this).val();
        var productId = $(this).data('id');
        if (newQty < 1) newQty = 1;
        window.location.href = "/GioHangs/UpdateQuantity?id=" + productId + "&quantity=" + newQty;
    });
});

function proceedToCheckout() {
    let selectedIds = [];

    $('.item-check:checked').each(function () {
        selectedIds.push($(this).val());
    });

    if (selectedIds.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Thông báo',
            text: 'Bạn chưa chọn sản phẩm nào để thanh toán cả.',
            confirmButtonColor: '#ee4d2d',
            confirmButtonText: 'Để mình chọn đã'
        });
        return; 
    }

    window.location.href = "/Checkout/Index?selectedIds=" + selectedIds.join(',');
}