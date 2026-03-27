let globalActiveIdx = 0;
let currentThumbScroll = 0;
const THUMB_STEP = 80; 
const VISIBLE_COUNT = 5;

// --- 1. HÀM CỐT LÕI: CẬP NHẬT GIAO DIỆN THEO INDEX ẢNH ---
function activeImageByIndex(index) {
    const items = document.querySelectorAll(".thumb-item");
    if (index < 0 || index >= items.length) return;

    globalActiveIdx = index;
    const target = items[index];
    const src = target.getAttribute("data-src");
    const maBT = parseInt(target.getAttribute("data-mabienthe"));

    const imgChinh = document.getElementById("imgChinh");
    imgChinh.style.opacity = "0.5";
    setTimeout(() => {
        imgChinh.src = src;
        imgChinh.style.opacity = "1";
    }, 100);

    const imgModal = document.getElementById("imgPhongTo");
    if (imgModal) imgModal.src = src;

    document.querySelectorAll(".thumb-item").forEach(item => item.classList.remove("active"));
    target.classList.add("active");

    syncScroll(index);

    const bt = bienThes.find(b => b.MaBienThe === maBT);
    if (bt) {
        capNhatThongTin(bt);
        dongBoButtons(bt.MauSac, bt.DuongKinhMat, bt.ChatLieuDay);
    }
}

// --- 2. LOGIC ĐIỀU HƯỚNG ---
function scrollThumbs(dir) {
    const items = document.querySelectorAll(".thumb-item");
    const maxScroll = items.length - VISIBLE_COUNT;
    currentThumbScroll = Math.max(0, Math.min(currentThumbScroll + dir, maxScroll));
    document.getElementById("thumbInner").style.transform = `translateX(-${currentThumbScroll * THUMB_STEP}px)`;
}

function syncScroll(index) {
    if (index < currentThumbScroll) currentThumbScroll = index;
    else if (index >= currentThumbScroll + VISIBLE_COUNT) currentThumbScroll = index - VISIBLE_COUNT + 1;
    document.getElementById("thumbInner").style.transform = `translateX(-${currentThumbScroll * THUMB_STEP}px)`;
}

function chonThumb(index) { activeImageByIndex(index); }

function chuyenAnhMuiTen(dir) {
    const total = document.querySelectorAll(".thumb-item").length;
    let next = globalActiveIdx + dir;
    if (next < 0) next = total - 1;
    if (next >= total) next = 0;
    activeImageByIndex(next);
}

function phongToAnh() {
    const activeThumb = document.querySelector(".thumb-item.active img");
    const src = activeThumb ? activeThumb.src : document.getElementById("imgChinh").src;

    document.getElementById("imgPhongTo").src = src;

    new bootstrap.Modal(document.getElementById('modalPhongTo')).show();
}

document.addEventListener("keydown", (e) => {
    if (e.key === "ArrowLeft") chuyenAnhMuiTen(-1);
    else if (e.key === "ArrowRight") chuyenAnhMuiTen(1);
    else if (e.key === "Escape") {
        const modal = bootstrap.Modal.getInstance(document.getElementById('modalPhongTo'));
        if (modal) modal.hide();
    }
});

function capNhatThongTin(bt) {
    document.getElementById("giaHienTai").innerText = bt.GiaBan ? parseInt(bt.GiaBan).toLocaleString("vi-VN") + " ₫" : "Liên hệ";
    document.getElementById("skuHienTai").innerText = bt.MaSku;
    document.getElementById("maBienTheChon").value = bt.MaBienThe;
    document.getElementById("mauDangChon").innerText = bt.MauSac;
    document.getElementById("sizeDangChon").innerText = bt.DuongKinhMat + " mm";
    document.getElementById("dayDangChon").innerText = bt.ChatLieuDay;

    // Cập nhật Tab thông số
    document.getElementById("tab-mau").innerText = bt.MauSac;
    document.getElementById("tab-day").innerText = bt.ChatLieuDay;
    document.getElementById("tab-size").innerText = bt.DuongKinhMat + " mm";

    // Lấy các DOM elements của nút bấm
    const btnThemVaoGio = document.querySelector('button[onclick="themVaoGio()"]');
    const btnMuaNgay = document.querySelector('button[onclick="muaNgay()"]');
    const txtSoLuong = document.getElementById("txtSoLuong");
    const btnMinus = document.querySelector(".btn-minus");
    const btnPlus = document.querySelector(".btn-plus");

    // Xử lý bật/tắt nút dựa vào tồn kho
    if (bt.SoLuongTon > 0) {
        document.getElementById("tonKhoHienTai").innerHTML = `<span class="text-success"><i class="fa fa-check-circle me-1"></i>Còn <strong>${bt.SoLuongTon}</strong> sản phẩm</span>`;

        // Mở khóa các nút
        if (btnThemVaoGio) btnThemVaoGio.disabled = false;
        if (btnMuaNgay) btnMuaNgay.disabled = false;
        if (txtSoLuong) txtSoLuong.disabled = false;
        if (btnMinus) btnMinus.disabled = false;
        if (btnPlus) btnPlus.disabled = false;

        // Nếu số lượng nhập đang lớn hơn tồn kho thì đưa về max tồn kho
        if (parseInt(txtSoLuong.value) > bt.SoLuongTon) {
            txtSoLuong.value = bt.SoLuongTon;
        }
    } else {
        document.getElementById("tonKhoHienTai").innerHTML = `<span class="text-danger"><i class="fa fa-times-circle me-1"></i>Hết hàng</span>`;

        // Khóa các nút
        if (btnThemVaoGio) btnThemVaoGio.disabled = true;
        if (btnMuaNgay) btnMuaNgay.disabled = true;
        if (txtSoLuong) {
            txtSoLuong.disabled = true;
            txtSoLuong.value = 1; // Reset số lượng hiển thị về 1
        }
        if (btnMinus) btnMinus.disabled = true;
        if (btnPlus) btnPlus.disabled = true;
    }
}
function chonMau(mau, btn) {
    const bt = bienThes.find(b => b.MauSac === mau);
    if (bt) {
        const thumbIdx = Array.from(document.querySelectorAll(".thumb-item")).findIndex(t => t.getAttribute("data-mabienthe") == bt.MaBienThe);
        if (thumbIdx !== -1) activeImageByIndex(thumbIdx);
    }
}

function chonSize(size, btn) {
    const bt = bienThes.find(b => b.MauSac === document.getElementById("mauDangChon").innerText && b.DuongKinhMat == size);
    if (bt) {
        const thumbIdx = Array.from(document.querySelectorAll(".thumb-item")).findIndex(t => t.getAttribute("data-mabienthe") == bt.MaBienThe);
        if (thumbIdx !== -1) activeImageByIndex(thumbIdx);
    }
}

function chonDay(day, btn) {
    const bt = bienThes.find(b => b.MauSac === document.getElementById("mauDangChon").innerText && b.ChatLieuDay == day);
    if (bt) {
        const thumbIdx = Array.from(document.querySelectorAll(".thumb-item")).findIndex(t => t.getAttribute("data-mabienthe") == bt.MaBienThe);
        if (thumbIdx !== -1) activeImageByIndex(thumbIdx);
    }
}

function dongBoButtons(mau, size, day) {
    document.querySelectorAll(".chon-mau").forEach(b => b.classList.toggle("btn-primary", b.getAttribute("data-mau") === mau));
    document.querySelectorAll(".chon-size").forEach(b => b.classList.toggle("btn-primary", b.getAttribute("data-size") == size));
    document.querySelectorAll(".chon-day").forEach(b => b.classList.toggle("btn-primary", b.getAttribute("data-day") === day));
}

document.addEventListener("DOMContentLoaded", () => {
    // Xử lý nút Cộng
    document.querySelector(".btn-plus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        const maBT = document.getElementById("maBienTheChon").value;
        const bt = bienThes.find(b => b.MaBienThe == maBT); // Tìm biến thể hiện tại

        let currentVal = parseInt(input.value);
        if (bt && currentVal < bt.SoLuongTon) {
            input.value = currentVal + 1;
        } else if (bt && currentVal >= bt.SoLuongTon) {
            Swal.fire("Thông báo", "Số lượng sản phẩm trong kho chỉ còn " + bt.SoLuongTon, "info");
        }
    });

    // Xử lý nút Trừ
    document.querySelector(".btn-minus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        if (parseInt(input.value) > 1) input.value = parseInt(input.value) - 1;
    });

    // Xử lý khi người dùng tự gõ số vào ô input (chặn nhập tào lao)
    document.getElementById("txtSoLuong")?.addEventListener("change", function () {
        const maBT = document.getElementById("maBienTheChon").value;
        const bt = bienThes.find(b => b.MaBienThe == maBT);

        let val = parseInt(this.value);
        if (isNaN(val) || val < 1) val = 1; // Nếu nhập chữ hoặc < 1 thì về 1

        if (bt && val > bt.SoLuongTon) {
            val = bt.SoLuongTon; // Nếu gõ số lớn hơn tồn kho thì ép về số tồn kho lớn nhất
            Swal.fire("Thông báo", "Số lượng sản phẩm trong kho chỉ còn " + bt.SoLuongTon, "info");
        }
        this.value = val;
    });
});

function themVaoGio() {
    if (!isLoggedIn) {
        Swal.fire({
            title: 'Bạn chưa đăng nhập',
            text: "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!",
            icon: 'info',
            showCancelButton: true,
            confirmButtonColor: '#ee4d2d',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Đăng nhập ngay',
            cancelButtonText: 'Để sau'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = `/Account/Login?ReturnUrl=${window.location.pathname}`;
            }
        });
        return;
    }

    const maBT = document.getElementById("maBienTheChon").value;
    const qty = document.getElementById("txtSoLuong").value;

    if (!maBT || maBT === "0") {
        Swal.fire("Thông báo", "Vui lòng chọn Màu sắc / Kích thước!", "warning");
        return;
    }

    window.location.href = `/GioHangs/AddToCart?id=${maBT}&quantity=${qty}`;
}

function muaNgay() {
    if (!isLoggedIn) {
        window.location.href = `/Account/Login?ReturnUrl=${window.location.pathname}`;
        return;
    }

    const maBT = document.getElementById("maBienTheChon").value;
    const qty = document.getElementById("txtSoLuong").value;

    if (!maBT || maBT === "0") {
        Swal.fire("Thông báo", "Vui lòng chọn Màu sắc / Kích thước!", "warning");
        return;
    }

    window.location.href = `/Checkout/Index?selectedIds=${maBT}&qty=${qty}&isBuyNow=true`;
}


function locSao(sao) {
    document.querySelectorAll('[id^="btn-sao-"]').forEach(btn => {
        btn.classList.remove('active', 'btn-warning');
        btn.classList.add('btn-outline-warning');
    });

    const btnActive = document.getElementById('btn-sao-' + sao);
    btnActive.classList.add('active');
    if (sao !== 0) btnActive.classList.remove('btn-outline-warning'), btnActive.classList.add('btn-warning');

    const items = document.querySelectorAll('.dg-item'); 
    let count = 0;

    items.forEach(item => {
        const itemSao = parseInt(item.getAttribute('data-sao'));
        if (sao === 0 || itemSao === sao) {
            item.style.setProperty('display', 'block', 'important');
            count++;
        } else {
            item.style.setProperty('display', 'none', 'important');
        }
    });

    const noReviewMsg = document.getElementById('khong-co-dg');
    if (count === 0) {
        noReviewMsg.style.display = 'block';
    } else {
        noReviewMsg.style.display = 'none';
    }
}