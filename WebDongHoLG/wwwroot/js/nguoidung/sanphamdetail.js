// Biến trạng thái toàn cục
let globalActiveIdx = 0;
let currentThumbScroll = 0;
const THUMB_STEP = 80; // 72px width + 8px gap
const VISIBLE_COUNT = 5;

// --- 1. HÀM CỐT LÕI: CẬP NHẬT GIAO DIỆN THEO INDEX ẢNH ---
function activeImageByIndex(index) {
    const items = document.querySelectorAll(".thumb-item");
    if (index < 0 || index >= items.length) return;

    globalActiveIdx = index;
    const target = items[index];
    const src = target.getAttribute("data-src");
    const maBT = parseInt(target.getAttribute("data-mabienthe"));

    // Đổi ảnh chính
    const imgChinh = document.getElementById("imgChinh");
    imgChinh.style.opacity = "0.5";
    setTimeout(() => {
        imgChinh.src = src;
        imgChinh.style.opacity = "1";
    }, 100);

    // Đổi ảnh trong Modal (nếu đang mở)
    const imgModal = document.getElementById("imgPhongTo");
    if (imgModal) imgModal.src = src;

    // Highlight thumbnail
    document.querySelectorAll(".thumb-item").forEach(item => item.classList.remove("active"));
    target.classList.add("active");

    // Cuộn dải thumbnail nếu bị khuất
    syncScroll(index);

    // Đồng bộ thuộc tính và thông tin biến thể
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

// --- 3. BÀN PHÍM & MODAL ---
function phongToAnh() {
    // Lấy ảnh của thumbnail đang có class 'active'
    const activeThumb = document.querySelector(".thumb-item.active img");
    const src = activeThumb ? activeThumb.src : document.getElementById("imgChinh").src;

    // Nạp vào ảnh trong Modal
    document.getElementById("imgPhongTo").src = src;

    // Hiển thị Modal
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

// --- 4. NGHIỆP VỤ BIẾN THỂ ---
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

    document.getElementById("tonKhoHienTai").innerHTML = bt.SoLuongTon > 0
        ? `<span class="text-success"><i class="fa fa-check-circle me-1"></i>Còn <strong>${bt.SoLuongTon}</strong> sản phẩm</span>`
        : `<span class="text-danger"><i class="fa fa-times-circle me-1"></i>Hết hàng</span>`;
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

// Khởi tạo tăng giảm số lượng
document.addEventListener("DOMContentLoaded", () => {
    document.querySelector(".btn-plus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        input.value = parseInt(input.value) + 1;
    });
    document.querySelector(".btn-minus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        if (parseInt(input.value) > 1) input.value = parseInt(input.value) - 1;
    });
});

function themVaoGio() {
    const id = document.getElementById("maBienTheChon").value;
    const qty = document.getElementById("txtSoLuong").value;
    location.href = `/GioHangs/AddToCart?id=${id}&quantity=${qty}`;
}

function muaNgay() {
    const id = document.getElementById("maBienTheChon").value;
    const qty = document.getElementById("txtSoLuong").value;
    location.href = `/GioHangs/Checkout?selectedIds=${id}&qty=${qty}&isBuyNow=true`;
}