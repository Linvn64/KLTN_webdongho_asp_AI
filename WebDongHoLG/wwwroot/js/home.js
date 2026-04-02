document.addEventListener("DOMContentLoaded", function () {
    new Swiper(".pri-slider", {
        loop: true,
        effect: "fade", 
        speed: 1500,    
        autoplay: {
            delay: 4000,
            disableOnInteraction: false,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
    });

    new Swiper(".brand-logo-slider", {
        loop: true,
        speed: 6000, 
        allowTouchMove: false,
        autoplay: {
            delay: 0, 
            disableOnInteraction: false,
        },
        slidesPerView: 2,
        spaceBetween: 50,
        freeMode: true,
        breakpoints: {
            768: { slidesPerView: 4 },
            1024: { slidesPerView: 6 }
        }
    });



    document.body.addEventListener("click", function (e) {
        if (e.target.id === "modalBtnPlus" || e.target.closest("#modalBtnPlus")) {
            const input = document.getElementById("modalSoLuong");
            let val = parseInt(input.value) || 1;
            if (val >= _tonKhoChon) {
                alert(`Chỉ còn ${_tonKhoChon} sản phẩm!`);
                return;
            }
            input.value = val + 1;
        }

        if (e.target.id === "modalBtnMinus" || e.target.closest("#modalBtnMinus")) {
            const input = document.getElementById("modalSoLuong");
            let val = parseInt(input.value) || 1;
            if (val <= 1) return;
            input.value = val - 1;
        }
    });
});


let _dsBienThe = [];
let _maBienTheChon = null;
let _tonKhoChon = 0;
window.isBuyNowFlow = false
// --- PHẦN HÀM XỬ LÝ MUA NGAY ---
function moModalMuaNgay(maSp) {
    _maBienTheGioHangCu = null; // Nếu chưa có biến này thì khai báo thêm let _maBienTheGioHangCu = null; ở đầu
    window.isBuyNowFlow = true;
    _moModal(maSp);
}

// --- HÀM RUỘT XỬ LÝ MODAL (PHẢI CÓ CÁI NÀY MỚI CHẠY ĐƯỢC) ---
function _moModal(maSp) {
    _dsBienThe = [];
    _maBienTheChon = null;
    _tonKhoChon = 0;

    // Reset giao diện Modal về trạng thái chờ
    document.getElementById("modalBienTheList").innerHTML = '<div class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary"></div></div>';
    document.getElementById("modalTenSp").innerText = "Đang tải...";
    document.getElementById("modalGia").innerText = "--";
    document.getElementById("modalSoLuong").value = "1";

    // Bật Modal lên
    new bootstrap.Modal(document.getElementById("modalChonBienThe")).show();

    // Lấy dữ liệu biến thể từ Server
    fetch(`/SanPhams/GetBienTheChonNhanh?maSp=${maSp}`)
        .then(r => r.json())
        .then(data => {
            _dsBienThe = data.bienThes;
            document.getElementById("modalTenSp").innerText = data.tenSp;
            document.getElementById("modalThuongHieu").innerText = data.tenThuongHieu || "";
            _renderBienTheButtons();

            // Tự động chọn biến thể đầu tiên còn hàng
            const first = _dsBienThe.find(bt => bt.soLuongTon > 0);
            if (first) _chonBienThe(first.maBienThe);
        })
        .catch(() => {
            document.getElementById("modalBienTheList").innerHTML = '<p class="text-danger">Không tải được dữ liệu!</p>';
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
    document.getElementById("modalGia").innerText = bt.giaBan ? parseInt(bt.giaBan).toLocaleString("vi-VN") + " ₫" : "Liên hệ";
    document.getElementById("modalTonKho").innerText = `Còn ${bt.soLuongTon} sản phẩm`;
    document.getElementById("modalBtnThem").disabled = bt.soLuongTon <= 0;
}

function xacNhanThemGio() {
    if (!_maBienTheChon) {
        alert("Vui lòng chọn biến thể!");
        return;
    }

    const qty = parseInt(document.getElementById("modalSoLuong").value) || 1;

    if (qty > _tonKhoChon) {
        alert(`Chỉ còn ${_tonKhoChon} sản phẩm!`);
        return;
    }

    if (window.isBuyNowFlow) {
       
        window.location.href = `/Checkout/Index?selectedIds=${_maBienTheChon}&qty=${qty}&isBuyNow=true`;
    }
    else {
        window.location.href = `/GioHangs/AddToCart?id=${_maBienTheChon}&quantity=${qty}`;
    }
}