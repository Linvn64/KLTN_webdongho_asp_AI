function updatePriceWithVoucher(phanTramGiam) {
    const tongTienHang = parseFloat(document.getElementById("valTongTienHang").value) || 0;

    const phiShip = (tongTienHang >= 5000000) ? 0 : 30000;

    const giamGia = tongTienHang * phanTramGiam;
    const tongCuoi = tongTienHang - giamGia + phiShip;

    document.getElementById("displayGiamGia").innerText =
        "- " + Math.round(giamGia).toLocaleString("vi-VN") + " ₫";

    const displayPhiShip = document.getElementById("displayPhiShip");
    if (phiShip === 0) {
        displayPhiShip.innerHTML = '<span class="text-success fw-bold">Miễn phí</span>';
    } else {
        displayPhiShip.innerText = phiShip.toLocaleString("vi-VN") + " ₫";
    }

    document.getElementById("displayTongThanhToan").innerText =
        Math.round(tongCuoi).toLocaleString("vi-VN") + " ₫";
}
function changeDefaultAddress(maDiaChi) {
    fetch(`/Checkout/SelectAddress?maDc=${maDiaChi}`)
        .then(() => location.reload());
}

function showAddForm() {
    const form = document.getElementById("formAddress");
    form.style.display = form.style.display === "none" ? "block" : "none";
    document.getElementById("editMaDiaChi").value = "0";
    document.getElementById("inputSdt").value = "";
    document.getElementById("inputQuan").value = "";
    document.getElementById("inputXa").value = "";
    document.getElementById("inputChiTiet").value = "";
}

function fillEditForm(maDiaChi, sdt, diaChi, tinhThanh) {
    document.getElementById("formAddress").style.display = "block";
    document.getElementById("editMaDiaChi").value = maDiaChi;
    document.getElementById("inputSdt").value = sdt;
    document.getElementById("inputChiTiet").value = diaChi;
    const sel = document.getElementById("selectTinh");
    for (let i = 0; i < sel.options.length; i++) {
        if (sel.options[i].value === tinhThanh) { sel.selectedIndex = i; break; }
    }
}

function deleteAddressAjax(maDiaChi, el) {
    if (!confirm("Xóa địa chỉ này?")) return;

    const params = new URLSearchParams();
    params.append('maDc', maDiaChi);

    fetch("/Checkout/DeleteAddress", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: params.toString()
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                el.closest(".custom-radio-address").remove(); 
                showToast("Đã xóa địa chỉ!", "success");
            } else {
                showToast("Không thể xóa!", "error");
            }
        });
}

document.getElementById("addressSubmitForm")?.addEventListener("submit", function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());

    fetch("/Checkout/UpdateAddress", {
        method: "POST",
        headers: { "Content-Type": "application/json" }, 
        body: JSON.stringify(data) 
    })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                showToast("Thao tác thành công!", "success");
                setTimeout(() => location.reload(), 800);
            } else {
                showToast(res.message, "error");
            }
        });
});