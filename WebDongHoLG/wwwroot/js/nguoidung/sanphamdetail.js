/* =======================================================
   LG WATCH - FILE: sanphamdetail.js
   ======================================================= */

let globalActiveIdx = 0;
let currentThumbScroll = 0;
const THUMB_STEP = 80;
const VISIBLE_COUNT = 5;

let luachon_mau = '';
let luachon_size = '';
let luachon_day = '';


// --- 1. CẬP NHẬT GIAO DIỆN THEO INDEX ẢNH (ảnh nhỏ + ảnh chính) ---
function activeImageByIndex(index) {
    const items = document.querySelectorAll(".thumb-item");
    if (index < 0 || index >= items.length) return;

    globalActiveIdx = index;
    const target = items[index];
    const src = target.getAttribute("data-src");
    const maBT = parseInt(target.getAttribute("data-mabienthe"));

    const imgChinh = document.getElementById("imgChinh");
    if (imgChinh) {
        imgChinh.style.opacity = "0.5";
        setTimeout(() => { imgChinh.src = src; imgChinh.style.opacity = "1"; }, 100);
    }

    items.forEach(item => item.classList.remove("active"));
    target.classList.add("active");
    syncScroll(index);

    if (typeof bienThes !== 'undefined') {
        const bt = bienThes.find(b => b.MaBienThe === maBT);
        if (bt) {
            capNhatThongTin(bt);
            luachon_mau = bt.MauSac || '';
            luachon_size = bt.DuongKinhMat?.toString() || '';
            luachon_day = bt.ChatLieuDay || '';
            capNhatTrangThaiNut();
        }
    }
}

// --- 2. ĐIỀU HƯỚNG ẢNH NHỎ ---
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


// --- 3. PHÓNG TO ẢNH ---
function phongToAnh() {
    const src = document.getElementById("imgChinh")?.src;
    const imgModal = document.getElementById("imgPhongTo");
    if (!src || !imgModal) return;

    imgModal.src = src;
    const modalEl = document.getElementById('modalPhongTo');
    if (modalEl) {
        const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        modal.show();
    }
}

// Chuyển ảnh TRONG MODAL (không động vào ảnh nhỏ bên ngoài)
function chuyenAnhModal(dir) {
    const items = document.querySelectorAll(".thumb-item");
    const total = items.length;
    let next = globalActiveIdx + dir;
    if (next < 0) next = total - 1;
    if (next >= total) next = 0;

    globalActiveIdx = next;
    const imgModal = document.getElementById("imgPhongTo");
    if (imgModal) imgModal.src = items[next].getAttribute("data-src");
}

// Khi đóng modal: đồng bộ lại ảnh nhỏ theo ảnh đang xem
document.addEventListener("DOMContentLoaded", () => {
    document.getElementById('modalPhongTo')?.addEventListener('hidden.bs.modal', () => {
        activeImageByIndex(globalActiveIdx);
    });
});

// Phím mũi tên & ESC trong modal
document.addEventListener("keydown", (e) => {
    const modalEl = document.getElementById('modalPhongTo');
    if (!modalEl?.classList.contains('show')) return;
    if (e.key === "ArrowLeft") chuyenAnhModal(-1);
    else if (e.key === "ArrowRight") chuyenAnhModal(1);
    else if (e.key === "Escape") bootstrap.Modal.getInstance(modalEl)?.hide();
});


// --- 4. KIỂM TRA ĐỦ ĐIỀU KIỆN MUA ---
function kiemTraDuDieuKienMua() {
    const coMau = document.querySelectorAll('.chon-mau').length > 0;
    const coSize = document.querySelectorAll('.chon-size').length > 0;
    const coDay = document.querySelectorAll('.chon-day').length > 0;

    const duDieuKien = !(coMau && !luachon_mau) && !(coSize && !luachon_size) && !(coDay && !luachon_day);

    [
        document.querySelector('button[onclick*="themVaoGio"]'),
        document.querySelector('button[onclick*="muaNgay"]')
    ].forEach(btn => {
        if (!btn) return;
        btn.disabled = !duDieuKien;
        btn.style.opacity = duDieuKien ? '1' : '0.4';
    });
}


// --- 5. LÀM MỜ BIẾN THỂ & CẬP NHẬT TRẠNG THÁI NÚT ---
function capNhatTrangThaiNut() {
    if (typeof bienThes === 'undefined') return;

    // Làm mờ size không hợp lệ theo màu đang chọn
    if (luachon_mau) {
        const sizeHopLe = [...new Set(bienThes.filter(bt => bt.MauSac === luachon_mau).map(bt => bt.DuongKinhMat?.toString()))];
        document.querySelectorAll('.chon-size').forEach(btn =>
            btn.classList.toggle('btn-mmo', !sizeHopLe.includes(btn.getAttribute('data-size')))
        );
    }

    // Làm mờ dây không hợp lệ theo màu + size đang chọn
    if (luachon_mau && luachon_size) {
        const dayHopLe = [...new Set(
            bienThes.filter(bt => bt.MauSac === luachon_mau && bt.DuongKinhMat?.toString() === luachon_size).map(bt => bt.ChatLieuDay)
        )];
        document.querySelectorAll('.chon-day').forEach(btn =>
            btn.classList.toggle('btn-mmo', !dayHopLe.includes(btn.getAttribute('data-day')))
        );
    }

    // Đổi màu nút đang chọn
    const highlight = (selector, attr, value) => {
        document.querySelectorAll(selector).forEach(b => {
            b.classList.remove('btn-primary');
            b.classList.add('btn-outline-secondary');
            if (b.getAttribute(attr) === value) b.classList.replace('btn-outline-secondary', 'btn-primary');
        });
    };
    highlight('.chon-mau', 'data-mau', luachon_mau);
    highlight('.chon-size', 'data-size', luachon_size);
    highlight('.chon-day', 'data-day', luachon_day);

    // Cập nhật text hiển thị
    const el = id => document.getElementById(id);
    if (el("mauDangChon")) el("mauDangChon").innerText = luachon_mau || '...';
    if (el("sizeDangChon")) el("sizeDangChon").innerText = luachon_size ? luachon_size + ' mm' : '...';
    if (el("dayDangChon")) el("dayDangChon").innerText = luachon_day || '...';

    kiemTraDuDieuKienMua();
}


// --- 6. CẬP NHẬT THÔNG TIN BIẾN THỂ TRÊN GIAO DIỆN ---
function capNhatThongTin(bt) {
    const el = id => document.getElementById(id);

    el("giaHienTai").innerText = bt.GiaBan ? parseInt(bt.GiaBan).toLocaleString("vi-VN") + " ₫" : "Liên hệ";
    el("skuHienTai").innerText = bt.MaSku || "--";
    el("maBienTheChon").value = bt.MaBienThe;

    if (el("tab-mau")) el("tab-mau").innerText = bt.MauSac;
    if (el("tab-day")) el("tab-day").innerText = bt.ChatLieuDay;
    if (el("tab-size")) el("tab-size").innerText = bt.DuongKinhMat + " mm";

    const txtSoLuong = el("txtSoLuong");
    const btnMinus = document.querySelector(".btn-minus");
    const btnPlus = document.querySelector(".btn-plus");
    const conHang = bt.SoLuongTon > 0;

    el("tonKhoHienTai").innerHTML = conHang
        ? `<span class="text-success"><i class="fa fa-check-circle me-1"></i>Còn <strong>${bt.SoLuongTon}</strong> sản phẩm</span>`
        : `<span class="text-danger"><i class="fa fa-times-circle me-1"></i>Hết hàng</span>`;

    [txtSoLuong, btnMinus, btnPlus].forEach(el => { if (el) el.disabled = !conHang; });

    if (txtSoLuong) {
        if (!conHang) txtSoLuong.value = 1;
        else if (parseInt(txtSoLuong.value) > bt.SoLuongTon) txtSoLuong.value = bt.SoLuongTon;
    }
}


// --- 7. XỬ LÝ CHỌN MÀU / SIZE / DÂY ---
function xuLyChonBienThe(loai, giaTri, btn) {
    if (loai === 'mau') {
        if (luachon_mau === giaTri) return;
        luachon_mau = giaTri;
        luachon_size = '';
        luachon_day = '';
        const bt = bienThes.find(b => b.MauSac === giaTri);
        if (bt) jumpToThumb(bt.MaBienThe);
    } else if (loai === 'size') {
        const sizeStr = giaTri.toString();
        if (luachon_size === sizeStr) return;
        luachon_size = sizeStr;
        luachon_day = '';
        const bt = bienThes.find(b => b.MauSac === luachon_mau && b.DuongKinhMat?.toString() === sizeStr);
        if (bt) jumpToThumb(bt.MaBienThe);
    } else if (loai === 'day') {
        if (luachon_day === giaTri) return;
        luachon_day = giaTri;
        const bt = bienThes.find(b => b.MauSac === luachon_mau && b.DuongKinhMat?.toString() === luachon_size && b.ChatLieuDay === giaTri);
        if (bt) jumpToThumb(bt.MaBienThe);
    }
    capNhatTrangThaiNut();
}

function jumpToThumb(maBienThe) {
    const idx = Array.from(document.querySelectorAll(".thumb-item"))
        .findIndex(t => t.getAttribute("data-mabienthe") == maBienThe);
    if (idx !== -1) activeImageByIndex(idx);
}


// --- 8. KHỞI TẠO ---
document.addEventListener("DOMContentLoaded", () => {
    // Gán giá trị mặc định từ biến thể đầu tiên
    const maBTMd = document.getElementById("maBienTheChon")?.value;
    if (maBTMd && typeof bienThes !== 'undefined') {
        const bt = bienThes.find(b => b.MaBienThe == maBTMd);
        if (bt) {
            luachon_mau = bt.MauSac || '';
            luachon_size = bt.DuongKinhMat?.toString() || '';
            luachon_day = bt.ChatLieuDay || '';
        }
    }
    capNhatTrangThaiNut();

    // Nút +/-
    document.querySelector(".btn-plus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        const bt = bienThes.find(b => b.MaBienThe == document.getElementById("maBienTheChon").value);
        const val = parseInt(input.value);
        if (bt && val < bt.SoLuongTon) input.value = val + 1;
        else if (bt) Swal.fire("Thông báo", "Kho chỉ còn " + bt.SoLuongTon + " sản phẩm", "info");
    });

    document.querySelector(".btn-minus")?.addEventListener("click", () => {
        const input = document.getElementById("txtSoLuong");
        if (parseInt(input.value) > 1) input.value = parseInt(input.value) - 1;
    });

    document.getElementById("txtSoLuong")?.addEventListener("change", function () {
        const bt = bienThes.find(b => b.MaBienThe == document.getElementById("maBienTheChon").value);
        let val = parseInt(this.value);
        if (isNaN(val) || val < 1) val = 1;
        if (bt && val > bt.SoLuongTon) {
            val = bt.SoLuongTon;
            Swal.fire("Thông báo", "Kho chỉ còn " + bt.SoLuongTon + " sản phẩm", "info");
        }
        this.value = val;
    });

    // Đồng bộ ảnh nhỏ khi đóng modal
    document.getElementById('modalPhongTo')?.addEventListener('hidden.bs.modal', () => {
        activeImageByIndex(globalActiveIdx);
    });
});


// --- 9. MUA HÀNG ---
function themVaoGio() {
    if (typeof isLoggedIn !== 'undefined' && !isLoggedIn) {
        Swal.fire({
            title: 'Bạn chưa đăng nhập',
            text: "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!",
            icon: 'info',
            showCancelButton: true,
            confirmButtonColor: '#6a4010',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Đăng nhập ngay',
            cancelButtonText: 'Để sau'
        }).then(r => { if (r.isConfirmed) window.location.href = `/Identity/Account/Login?ReturnUrl=${window.location.pathname}`; });
        return;
    }
    const maBT = document.getElementById("maBienTheChon").value;
    if (!maBT || maBT === "0") { Swal.fire("Thông báo", "Vui lòng chọn đầy đủ Màu sắc / Kích thước!", "warning"); return; }
    window.location.href = `/GioHangs/AddToCart?id=${maBT}&quantity=${document.getElementById("txtSoLuong").value}`;
}

function muaNgay() {
    if (typeof isLoggedIn !== 'undefined' && !isLoggedIn) {
        window.location.href = `/Identity/Account/Login?ReturnUrl=${window.location.pathname}`; return;
    }
    const maBT = document.getElementById("maBienTheChon").value;
    if (!maBT || maBT === "0") {
        Swal.fire({ title: "Thông báo", text: "Vui lòng chọn đầy đủ Màu sắc / Kích thước trước khi mua!", icon: "warning", confirmButtonColor: "#6a4010" });
        return;
    }
    window.location.href = `/Checkout/Index?selectedIds=${maBT}&qty=${document.getElementById("txtSoLuong").value}&isBuyNow=true`;
}


// --- 10. LỌC ĐÁNH GIÁ SAO ---
function locSao(sao) {
    document.querySelectorAll('[id^="btn-sao-"]').forEach(btn => {
        btn.classList.remove('active', 'btn-warning');
        btn.classList.add('btn-outline-warning');
    });
    const btnActive = document.getElementById('btn-sao-' + sao);
    if (btnActive) {
        btnActive.classList.add('active');
        if (sao !== 0) { btnActive.classList.remove('btn-outline-warning'); btnActive.classList.add('btn-warning'); }
    }

    let count = 0;
    document.querySelectorAll('.dg-item').forEach(item => {
        const hien = sao === 0 || parseInt(item.getAttribute('data-sao')) === sao;
        item.style.setProperty('display', hien ? 'block' : 'none', 'important');
        if (hien) count++;
    });

    const noReviewMsg = document.getElementById('khong-co-dg');
    if (noReviewMsg) noReviewMsg.style.display = count === 0 ? 'block' : 'none';
}