let _dsBienThe = [];
let _maBienTheChon = null;
let _tonKhoChon = 0;
let _maBienTheGioHangCu = null;

function moModalChonBienThe(maSp) {
    _maBienTheGioHangCu = null;
    _moModal(maSp);
}

function moModalDoiBienThe(maBienTheHienTai, maSp) {
    _maBienTheGioHangCu = maBienTheHienTai;
    _moModal(maSp);
}

function _moModal(maSp) {
    _dsBienThe = [];
    _maBienTheChon = null;
    _tonKhoChon = 0;

    const maBienTheHienTai = _maBienTheGioHangCu;

    document.getElementById("modalBienTheList").innerHTML =
        '<div class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary"></div></div>';
    document.getElementById("modalTenSp").innerText = "Đang tải...";
    document.getElementById("modalThuongHieu").innerText = "";
    document.getElementById("modalGia").innerText = "--";
    document.getElementById("modalSoLuong").value = "1";
    document.getElementById("modalTonKho").innerText = "";
    document.getElementById("modalHetHang").classList.add("d-none");
    document.getElementById("modalAnhBienThe").src = "/home/img/no-image.jpg";
    document.getElementById("modalBtnThem").disabled = true;

    new bootstrap.Modal(document.getElementById("modalChonBienThe")).show();

    fetch(`/SanPhams/GetBienTheChonNhanh?maSp=${maSp}`)
        .then(r => r.json())
        .then(data => {
            _dsBienThe = data.bienThes;
            document.getElementById("modalTenSp").innerText = data.tenSp;
            document.getElementById("modalThuongHieu").innerText = data.tenThuongHieu || "";
            _renderBienTheButtons();

        
            if (maBienTheHienTai !== null) {
                const btHienTai = _dsBienThe.find(bt => bt.maBienThe === maBienTheHienTai);
                if (btHienTai) {
                    _chonBienThe(maBienTheHienTai);
                } else {
                    const first = _dsBienThe.find(bt => bt.soLuongTon > 0);
                    if (first) _chonBienThe(first.maBienThe);
                }
            } else {
                const first = _dsBienThe.find(bt => bt.soLuongTon > 0);
                if (first) _chonBienThe(first.maBienThe);
            }
        })
        .catch(() => {
            document.getElementById("modalBienTheList").innerHTML =
                '<p class="text-danger">Không tải được dữ liệu!</p>';
        });
}
function _renderBienTheButtons() {
    const container = document.getElementById("modalBienTheList");
    container.innerHTML = "";
    _dsBienThe.forEach(bt => {
        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = `bien-the-btn ${bt.soLuongTon <= 0 ? "het-hang" : ""}`;
        btn.setAttribute("data-mabienthe", bt.maBienThe);
        let label = bt.mauSac || "";
        if (bt.duongKinhMat) label += ` ${bt.duongKinhMat}mm`;
        if (bt.chatLieuDay) label += ` | ${bt.chatLieuDay}`;
        btn.innerText = label;
        if (bt.soLuongTon > 0) btn.onclick = () => _chonBienThe(bt.maBienThe);
        container.appendChild(btn);
    });
}

function _chonBienThe(maBienThe) {
    _maBienTheChon = maBienThe;
    const bt = _dsBienThe.find(b => b.maBienThe === maBienThe);
    if (!bt) return;
    _tonKhoChon = bt.soLuongTon;
    document.querySelectorAll(".bien-the-btn").forEach(b => {
        b.classList.toggle("active", parseInt(b.getAttribute("data-mabienthe")) === maBienThe);
    });
    if (bt.imageUrl) document.getElementById("modalAnhBienThe").src = bt.imageUrl;
    document.getElementById("modalGia").innerText = bt.giaBan
        ? parseInt(bt.giaBan).toLocaleString("vi-VN") + " ₫" : "Liên hệ";
    document.getElementById("modalTonKho").innerText = `Còn ${bt.soLuongTon} sản phẩm`;
    document.getElementById("modalHetHang").classList.toggle("d-none", bt.soLuongTon > 0);
    document.getElementById("modalBtnThem").disabled = bt.soLuongTon <= 0;
    document.getElementById("modalSoLuong").value = "1";
}

function xacNhanThemGio() {
    if (!_maBienTheChon) { showToast("Vui lòng chọn biến thể!", "warning"); return; }
    const qty = parseInt(document.getElementById("modalSoLuong").value) || 1;
    if (qty > _tonKhoChon) {
        showToast(`Chỉ còn <strong>${_tonKhoChon}</strong> sản phẩm!`, "warning");
        return;
    }

    if (_maBienTheGioHangCu !== null) {
        if (_maBienTheChon === _maBienTheGioHangCu) {
            bootstrap.Modal.getInstance(document.getElementById("modalChonBienThe")).hide();
            return;
        }
        _doiBienTheGioHang(_maBienTheGioHangCu, _maBienTheChon, qty);
        return;
    }

    window.location.href = `/GioHangs/AddToCart?id=${_maBienTheChon}&quantity=${qty}`;
}
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("modalBtnPlus")?.addEventListener("click", () => {
        const input = document.getElementById("modalSoLuong");
        let val = parseInt(input.value) || 1;
        if (val >= _tonKhoChon) {
            showToast(`Chỉ còn <strong>${_tonKhoChon}</strong> sản phẩm!`, "warning");
            return;
        }
        input.value = val + 1;
    });

    document.getElementById("modalBtnMinus")?.addEventListener("click", () => {
        const input = document.getElementById("modalSoLuong");
        let val = parseInt(input.value) || 1;
        if (val <= 1) return;
        input.value = val - 1;
    });
});

function _doiBienTheGioHang(maBienTheCu, maBienTheMoi, soLuong) {
    fetch("/GioHangs/DoiBienThe", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ maBienTheCu, maBienTheMoi, soLuong })
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                bootstrap.Modal.getInstance(document.getElementById("modalChonBienThe")).hide();
                showToast("Đã cập nhật biến thể!", "success");
                setTimeout(() => location.reload(), 1000);
            } else {
                showToast(data.message || "Không thể đổi biến thể!", "error");
            }
        })
        .catch(() => showToast("Có lỗi xảy ra!", "error"));
}