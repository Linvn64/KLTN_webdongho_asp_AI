document.addEventListener("DOMContentLoaded", function () {
    const formatVN = (val) => new Intl.NumberFormat('vi-VN').format(val);
    const cleanNum = (str) => String(str).replace(/\D/g, '');

    const el = {
        giaNhapDisp: document.getElementById("giaNhapDisplay"),
        giaNhapRaw: document.getElementById("giaNhapRaw"),
        giaBanDisp: document.getElementById("giaBanDisplay"),
        giaBanRaw: document.getElementById("giaBanRaw"),
        product: document.getElementById("inputProduct"),
        color: document.getElementById("inputColor"),
        strap: document.getElementById("inputStrap"),
        size: document.getElementById("inputSize"),
        sku: document.getElementById("inputSku"),
        folder: document.getElementById("inputFolder"),
        previewImages: document.getElementById("previewImages"),
        mainImageInput: document.getElementById("mainImageInput"),
        imageOrdersInput: document.getElementById("imageOrdersInput"),
        fileUpload: document.getElementById("fileUpload"),
        previewMainText: document.getElementById("previewMainImageText"),
        hiddenImageUrl: document.getElementById("hiddenImageUrl"),
        productInfoBadge: document.getElementById("productInfoBadge")
    };

    const toSlug = (str) => {
        if (!str) return "";
        return str.toString().toLowerCase()
            .replace(/đ/g, 'd')
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/[^\w\s-]/g, "")
            .replace(/[\s_-]+/g, "-")
            .replace(/^-+|-+$/g, "");
    };

    let cachedProductInfo = {};

    const updateAll = () => {
        let productId = el.product.value;
        if (!productId) return;

        let colorVal = el.color.value.trim();
        let strapVal = el.strap.value.trim();
        let sizeVal = el.size.value;

        if (cachedProductInfo[productId]) {
            buildFolderAndSku(cachedProductInfo[productId], colorVal, strapVal, sizeVal, productId);
        } else {
            fetch(`/Admin/BienTheSanPhams/GetProductInfo/${productId}`)
                .then(res => res.json())
                .then(data => {
                    cachedProductInfo[productId] = data;
                    buildFolderAndSku(data, colorVal, strapVal, sizeVal, productId);
                });
        }
    };

    function buildFolderAndSku(data, colorVal, strapVal, sizeVal, productId) {
        let colorShort = toSlug(colorVal).toUpperCase() || "MAU";
        el.sku.value = `${productId}-${colorShort}-${sizeVal || "SIZE"}`;

        let spSlug = toSlug(data.tenSanPham + (data.doiTuong ? " " + data.doiTuong : ""));
        let variantParts = [toSlug(colorVal), toSlug(strapVal)].filter(x => x !== "");
        let variantSlug = variantParts.length > 0 ? `${spSlug}-${variantParts.join("-")}` : spSlug;
        let suggestedPath = `${spSlug}/${variantSlug}`;

        el.folder.value = suggestedPath;
        // Gọi hàm load ảnh cũ ngay khi gõ thông số
        loadExistingImages(suggestedPath);
    }

    function loadExistingImages(folderName) {
        // LUÔN LUÔN load nếu có folder, không quan tâm fileUpload có hay không
        if (!folderName) return;
        fetch(`/Admin/BienTheSanPhams/GetImagesInFolder?folderName=${encodeURIComponent(folderName)}`)
            .then(res => res.json())
            .then(files => {
                if (files && files.length > 0) {
                    renderGallery(files, `/hinhanhbienthe/${folderName}/`);
                } else {
                    el.previewImages.innerHTML = '<div class="col-12 text-center py-3 text-muted small">Folder trống hoặc chưa tạo.</div>';
                }
            });
    }

    function renderGallery(fileNames, basePath, blobUrls = null) {
        el.previewImages.innerHTML = "";
        let currentMain = el.mainImageInput.value;

        fileNames.forEach((file, index) => {
            let src = (blobUrls && blobUrls[index]) ? blobUrls[index] : (basePath + file);
            let isMain = (file === currentMain);

            const div = document.createElement("div");
            div.className = "col-4 mb-2 text-center";
            div.innerHTML = `
                <div class="image-box border shadow-sm ${isMain ? 'border-danger border-3' : ''}" 
                     onclick="setMainImage('${file}')"
                     style="background-image: url('${src}'); width: 100%; height: 80px; background-size: cover; cursor: pointer; border-radius: 6px;">
                </div>
                <small class="text-truncate d-block mt-1 ${isMain ? 'text-danger fw-bold' : ''}">${file}</small>
            `;
            el.previewImages.appendChild(div);
        });
    }

    // Lắng nghe thay đổi
    ["inputProduct", "inputColor", "inputSize", "inputStrap"].forEach(id => {
        document.getElementById(id)?.addEventListener("change", updateAll);
    });

    // Khởi tạo nếu là trang Edit
    if (el.product.value) updateAll();
}); // CHỈ CÓ 1 DẤU ĐÓNG NÀY THÔI GIANG NHÉ

// Hàm chọn ảnh chính (Phải để ngoài DOMContentLoaded để div onclick gọi được)
function setMainImage(fileName) {
    document.getElementById("mainImageInput").value = fileName;
    document.getElementById("previewMainImageText").innerText = fileName;
    // Kích hoạt lại update để vẽ lại viền đỏ
    let folder = document.getElementById("inputFolder").value;
    if (folder) loadExistingImages(folder);
}