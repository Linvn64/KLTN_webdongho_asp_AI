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
        animation:slideIn 0.3s ease; font-size:14px; line-height:1.5;
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
    document.getElementById('toastContainer').appendChild(toast);
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
    document.querySelectorAll(".item-check:checked").forEach(cb => {
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
}

document.getElementById("checkAll")?.addEventListener("change", function () {
    document.querySelectorAll(".item-check").forEach(cb => cb.checked = this.checked);
    updateSummary();
});

document.querySelectorAll(".item-check").forEach(cb => {
    cb.addEventListener("change", function () {
        const all = document.querySelectorAll(".item-check").length;
        const checked = document.querySelectorAll(".item-check:checked").length;
        const checkAll = document.getElementById("checkAll");
        if (checkAll) checkAll.checked = all === checked;
        updateSummary();
    });
});

document.querySelectorAll(".btn-plus").forEach(btn => {
    btn.addEventListener("click", function () {
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

document.querySelectorAll(".btn-minus").forEach(btn => {
    btn.addEventListener("click", function () {
        const id = this.getAttribute("data-id");
        const input = document.querySelector(`.qty-input[data-id="${id}"]`);
        let val = parseInt(input.value) || 1;
        if (val <= 1) {
            showToast('Số lượng tối thiểu là 1!', 'warning');
            return;
        }
        input.value = val - 1;
        saveQuantity(id, val - 1, parseFloat(input.getAttribute("data-gia")));
    });
});

document.querySelectorAll(".qty-input").forEach(input => {
    input.addEventListener("change", function () {
        const id = this.getAttribute("data-id");
        const max = parseInt(this.getAttribute("data-max")) || 99;
        let val = parseInt(this.value) || 1;
        if (val < 1) {
            val = 1;
            showToast('Số lượng tối thiểu là 1!', 'warning');
        }
        if (val > max) {
            val = max;
            showToast(`Chỉ còn <strong>${max}</strong> sản phẩm trong kho!`, 'warning');
        }
        this.value = val;
        saveQuantity(id, val, parseFloat(this.getAttribute("data-gia")));
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
                showToast('Đã cập nhật số lượng!', 'success');
            } else {
                showToast(data.message || 'Không thể cập nhật số lượng!', 'error');
            }
            updateSummary();
        })
        .catch(() => showToast('Có lỗi xảy ra, vui lòng thử lại!', 'error'));
}

function proceedToCheckout() {
    const selected = [...document.querySelectorAll(".item-check:checked")].map(cb => cb.value);
    if (!selected.length) {
        showToast('Vui lòng chọn ít nhất 1 sản phẩm để mua!', 'warning');
        return;
    }
    window.location.href = `/Checkout/Index?selectedIds=${selected.join(",")}&isBuyNow=false`; }