function showToast(message, type) {
    const colors = {
        success: { bg: '#2ecc71', icon: 'fa-check-circle' },
        error: { bg: '#e74c3c', icon: 'fa-times-circle' },
        warning: { bg: '#f39c12', icon: 'fa-exclamation-triangle' }
    };
    const c = colors[type] || colors.success;
    const toast = document.createElement('div');
    toast.style.cssText = `
        background:${c.bg}; color:white; padding:14px 18px;
        border-radius:10px; margin-bottom:10px;
        display:flex; align-items:flex-start; gap:12px;
        box-shadow:0 4px 15px rgba(0,0,0,0.15);
        animation:slideIn 0.3s ease; font-size:14px; line-height:1.5; z-index:9999;
    `;
    toast.innerHTML = `
        <i class="fa ${c.icon}" style="font-size:18px; margin-top:2px; flex-shrink:0;"></i>
        <div style="flex:1;">${message}</div>
        <button onclick="this.parentElement.remove()"
                style="background:none;border:none;color:white;font-size:18px;
                       cursor:pointer;padding:0;line-height:1;flex-shrink:0;opacity:0.8;">
            &times;
        </button>
    `;

    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.style.cssText = 'position:fixed; top:20px; right:20px; z-index:9999;';
        document.body.appendChild(container);
    }
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease forwards';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function formatMoney(n) {
    return Math.round(n).toLocaleString("vi-VN");
}

function updateSummary() {
    let count = 0, total = 0;

    // Chỉ tính toán cho những checkbox ĐƯỢC TÍCH và KHÔNG BỊ KHÓA (chưa hết hàng)
    document.querySelectorAll(".item-check:checked:not([disabled])").forEach(cb => {
        const id = cb.value;
        const input = document.querySelector(`.qty-input[data-id="${id}"]`);
        const gia = parseFloat(cb.getAttribute("data-gia")) || 0;
        const sl = parseInt(input?.value) || 1;
        count++;
        total += gia * sl;
    });

    document.getElementById("selectedCount").innerText = count;
    document.getElementById("btnCount").innerText = count;
    document.getElementById("totalPrice").innerText = formatMoney(total);

    // XỬ LÝ KHÓA/MỞ NÚT MUA HÀNG
    const btnMuaHang = document.getElementById("btnMuaHang");
    if (btnMuaHang) {
        if (count > 0) {
            btnMuaHang.disabled = false;
            btnMuaHang.style.opacity = "1";
            btnMuaHang.style.cursor = "pointer";
        } else {
            btnMuaHang.disabled = true;
            btnMuaHang.style.opacity = "0.5";
            btnMuaHang.style.cursor = "not-allowed";
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {
    // Đề phòng trình duyệt lưu cache giữ lại dấu tích ở sản phẩm đã hết hàng
    document.querySelectorAll(".item-check[disabled]").forEach(cb => {
        cb.checked = false;
        // Vô hiệu hóa luôn nút + - của sp hết hàng ngoài UI
        const id = cb.value;
        document.querySelector(`.btn-plus[data-id="${id}"]`)?.setAttribute('disabled', 'true');
        document.querySelector(`.btn-minus[data-id="${id}"]`)?.setAttribute('disabled', 'true');
        document.querySelector(`.qty-input[data-id="${id}"]`)?.setAttribute('disabled', 'true');
    });

    // Cập nhật lại UI lúc mới load
    updateSummary();

    // Checkbox All
    document.getElementById("checkAll")?.addEventListener("change", function () {
        document.querySelectorAll(".item-check:not([disabled])").forEach(cb => cb.checked = this.checked);
        updateSummary();
    });

    // Checkbox Từng sản phẩm
    document.querySelectorAll(".item-check").forEach(cb => {
        cb.addEventListener("change", function () {
            const all = document.querySelectorAll(".item-check:not([disabled])").length;
            const checked = document.querySelectorAll(".item-check:checked").length;
            const checkAll = document.getElementById("checkAll");
            if (checkAll) checkAll.checked = (all > 0 && all === checked);
            updateSummary();
        });
    });

    // Nút tăng số lượng
    document.querySelectorAll(".btn-plus").forEach(btn => {
        btn.addEventListener("click", function () {
            if (this.hasAttribute('disabled')) return;
            const id = this.getAttribute("data-id");
            const max = parseInt(this.getAttribute("data-max")) || 99;
            const input = document.querySelector(`.qty-input[data-id="${id}"]`);
            let val = parseInt(input.value) || 1;
            if (val >= max) {
                showToast(`Chỉ còn <strong>${max}</strong> sản phẩm trong kho!`, 'warning');
                return;
            }
            input.value = val + 1;
            saveQuantity(id, val + 1, parseFloat(input.getAttribute("data-gia")));
        });
    });

    // Nút giảm số lượng
    document.querySelectorAll(".btn-minus").forEach(btn => {
        btn.addEventListener("click", function () {
            if (this.hasAttribute('disabled')) return;
            const id = this.getAttribute("data-id");
            const input = document.querySelector(`.qty-input[data-id="${id}"]`);
            let val = parseInt(input.value) || 1;
            if (val <= 1) return;
            input.value = val - 1;
            saveQuantity(id, val - 1, parseFloat(input.getAttribute("data-gia")));
        });
    });

    // Nhập số lượng trực tiếp
    document.querySelectorAll(".qty-input").forEach(input => {
        input.addEventListener("change", function () {
            if (this.hasAttribute('disabled')) return;
            const id = this.getAttribute("data-id");
            const max = parseInt(this.getAttribute("data-max")) || 99;
            let val = parseInt(this.value) || 1;
            if (val < 1) val = 1;
            if (val > max) val = max;
            this.value = val;
            saveQuantity(id, val, parseFloat(this.getAttribute("data-gia")));
        });
    });
});

function saveQuantity(id, quantity, gia) {
    fetch(`/GioHangs/UpdateQuantity`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
        },
        body: JSON.stringify({ id: parseInt(id), quantity: quantity })
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                const el = document.getElementById(`thanhTien-${id}`);
                if (el) el.innerText = data.thanhTien + " ₫";
                const cb = document.querySelector(`.item-check[value="${id}"]`);
                if (cb) cb.setAttribute("data-soluong", quantity);
            } else {
                showToast(data.message || 'Lỗi cập nhật số lượng!', 'error');
            }
            updateSummary();
        })
        .catch(() => showToast('Có lỗi xảy ra, vui lòng thử lại!', 'error'));
}

function proceedToCheckout() {
    const selectedElements = [...document.querySelectorAll(".item-check:checked")];
    if (!selectedElements.length) {
        showToast('Vui lòng chọn ít nhất 1 sản phẩm để mua!', 'warning');
        return;
    }

    const hasOutOfStock = selectedElements.some(cb => parseInt(cb.getAttribute("data-tonkho")) <= 0);
    if (hasOutOfStock) {
        showToast('Có sản phẩm đã hết hàng. Vui lòng bỏ chọn!', 'error');
        return;
    }

    const selectedIds = selectedElements.map(cb => cb.value);
    window.location.href = `/Checkout/Index?selectedIds=${selectedIds.join(",")}&isBuyNow=false`;
}

// ================= PHẦN 2: POPUP ĐỔI BIẾN THỂ TRONG GIỎ HÀNG =================
let _dsBienTheGH = [];
let _maBienTheChonGH = null;
let _tonKhoChonGH = 0;
let _maBienTheGioHangCuGH = null;

// Phải đưa ra window để thẻ html gọi được
window.moModalDoiBienTheGH = function (maBienTheHienTai, maSp, currentQty) {
    _maBienTheGioHangCuGH = maBienTheHienTai;
    _moModalGH(maSp, currentQty);
};

function _moModalGH(maSp, currentQty) {
    _dsBienTheGH = [];
    _maBienTheChonGH = null;
    _tonKhoChonGH = 0;

    document.getElementById("modalBienTheListGH").innerHTML =
        '<div class="text-center py-3 w-100"><div class="spinner-border spinner-border-sm text-primary"></div></div>';
    document.getElementById("modalTenSpGH").innerText = "Đang tải...";
    document.getElementById("modalThuongHieuGH").innerText = "";
    document.getElementById("modalGiaGH").innerText = "--";
    document.getElementById("modalSoLuongGH").value = currentQty || 1;
    document.getElementById("modalTonKhoGH").innerText = "";
    document.getElementById("modalHetHangGH").classList.add("d-none");
    document.getElementById("modalAnhBienTheGH").src = "/home/img/no-image.jpg";
    document.getElementById("modalBtnXacNhanGH").disabled = true;

    const modalElement = document.getElementById("modalChonBienTheGH");
    modalElement.removeAttribute("aria-hidden");

    // Fix Bootstrap 5.0.0 Modal Issue
    let modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (!modalInstance) {
        modalInstance = new bootstrap.Modal(modalElement);
    }
    modalInstance.show();

    fetch(`/SanPhams/GetBienTheChonNhanh?maSp=${maSp}`)
        .then(r => r.json())
        .then(data => {
            _dsBienTheGH = data.bienThes;
            document.getElementById("modalTenSpGH").innerText = data.tenSp;
            document.getElementById("modalThuongHieuGH").innerText = data.tenThuongHieu || "";
            _renderBienTheButtonsGH();

            if (_maBienTheGioHangCuGH !== null) {
                const btHienTai = _dsBienTheGH.find(bt => bt.maBienThe === _maBienTheGioHangCuGH);
                _chonBienTheGH(btHienTai ? _maBienTheGioHangCuGH : (_dsBienTheGH.find(bt => bt.soLuongTon > 0)?.maBienThe), currentQty);
            } else {
                const first = _dsBienTheGH.find(bt => bt.soLuongTon > 0);
                if (first) _chonBienTheGH(first.maBienThe, 1);
            }
        })
        .catch(() => {
            document.getElementById("modalBienTheListGH").innerHTML = '<p class="text-danger">Không tải được dữ liệu!</p>';
        });
}

function _renderBienTheButtonsGH() {
    const container = document.getElementById("modalBienTheListGH");
    container.innerHTML = "";
    _dsBienTheGH.forEach(bt => {
        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = `bien-the-btn ${bt.soLuongTon <= 0 ? "het-hang" : ""}`;
        btn.setAttribute("data-mabienthe", bt.maBienThe);

        let label = bt.mauSac || "";
        if (bt.duongKinhMat) label += ` ${bt.duongKinhMat}mm`;
        if (bt.chatLieuDay) label += ` | ${bt.chatLieuDay}`;

        btn.innerText = label || "Option";

        // Vẫn cho bấm để khách xem thông tin kể cả khi hết hàng, nhưng sẽ khóa số lượng & nút Xác nhận
        btn.onclick = () => _chonBienTheGH(bt.maBienThe, parseInt(document.getElementById("modalSoLuongGH").value));
        container.appendChild(btn);
    });
}

function _chonBienTheGH(maBienThe, qty = 1) {
    if (!maBienThe) return;
    _maBienTheChonGH = maBienThe;
    const bt = _dsBienTheGH.find(b => b.maBienThe === maBienThe);
    if (!bt) return;

    _tonKhoChonGH = bt.soLuongTon;

    document.querySelectorAll("#modalBienTheListGH .bien-the-btn").forEach(b => {
        b.classList.toggle("active", parseInt(b.getAttribute("data-mabienthe")) === maBienThe);
    });

    if (bt.imageUrl) document.getElementById("modalAnhBienTheGH").src = bt.imageUrl;
    document.getElementById("modalGiaGH").innerText = bt.giaBan ? parseInt(bt.giaBan).toLocaleString("vi-VN") + " ₫" : "Liên hệ";
    document.getElementById("modalTonKhoGH").innerText = `Còn ${bt.soLuongTon} sản phẩm`;

    // Logic khóa UI nếu biến thể đó hết hàng
    const isOutOfStock = bt.soLuongTon <= 0;
    document.getElementById("modalHetHangGH").classList.toggle("d-none", !isOutOfStock);
    document.getElementById("modalBtnXacNhanGH").disabled = isOutOfStock;
    document.getElementById("modalBtnMinusGH").disabled = isOutOfStock;
    document.getElementById("modalBtnPlusGH").disabled = isOutOfStock;
    document.getElementById("modalSoLuongGH").disabled = isOutOfStock;

    document.getElementById("modalSoLuongGH").value = Math.min(qty || 1, _tonKhoChonGH > 0 ? _tonKhoChonGH : 1);
}

// Nút cộng trừ trong Modal
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("modalBtnPlusGH")?.addEventListener("click", function () {
        if (this.hasAttribute('disabled')) return;
        const input = document.getElementById("modalSoLuongGH");
        let val = parseInt(input.value) || 1;
        if (val >= _tonKhoChonGH) {
            showToast(`Chỉ còn ${_tonKhoChonGH} sản phẩm trong kho!`, "warning");
            return;
        }
        input.value = val + 1;
    });

    document.getElementById("modalBtnMinusGH")?.addEventListener("click", function () {
        if (this.hasAttribute('disabled')) return;
        const input = document.getElementById("modalSoLuongGH");
        let val = parseInt(input.value) || 1;
        if (val <= 1) return;
        input.value = val - 1;
    });
});

function xacNhanDoiBienTheGH() {
    if (!_maBienTheChonGH) {
        showToast("Vui lòng chọn phân loại hàng!", "warning");
        return;
    }
    const qty = parseInt(document.getElementById("modalSoLuongGH").value) || 1;

    if (_maBienTheChonGH === _maBienTheGioHangCuGH) {
        let modalInstance = bootstrap.Modal.getInstance(document.getElementById("modalChonBienTheGH"));
        if (modalInstance) modalInstance.hide();

        const inputHienTai = document.querySelector(`.qty-input[data-id="${_maBienTheGioHangCuGH}"]`);
        if (inputHienTai && parseInt(inputHienTai.value) !== qty) {
            saveQuantity(_maBienTheGioHangCuGH, qty, parseFloat(inputHienTai.getAttribute("data-gia")));
            inputHienTai.value = qty;
        }
        return;
    }

    fetch("/GioHangs/DoiBienThe", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ maBienTheCu: _maBienTheGioHangCuGH, maBienTheMoi: _maBienTheChonGH, soLuong: qty })
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                let modalInstance = bootstrap.Modal.getInstance(document.getElementById("modalChonBienTheGH"));
                if (modalInstance) modalInstance.hide();
                showToast("Đã cập nhật phân loại!", "success");
                setTimeout(() => location.reload(), 800);
            } else {
                showToast(data.message || "Lỗi cập nhật phân loại!", "error");
            }
        })
        .catch(() => showToast("Lỗi hệ thống!", "error"));
}