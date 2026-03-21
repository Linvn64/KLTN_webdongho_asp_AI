document.addEventListener("DOMContentLoaded", function () {
    const formatVN = (val) => new Intl.NumberFormat('vi-VN').format(val);
    const cleanNum = (str) => str.replace(/\D/g, '');

    const el = {
        giaNhapDisp: document.getElementById("giaNhapDisplay"),
        giaNhapRaw: document.getElementById("giaNhapRaw"),
        giaBanDisp: document.getElementById("giaBanDisplay"),
        giaBanRaw: document.getElementById("giaBanRaw"),
        color: document.getElementById("inputColor"),
        size: document.getElementById("inputSize"),
        sku: document.getElementById("inputSku"),
        folder: document.getElementById("inputFolder"),
        previewPrice: document.getElementById("previewPrice"),
        previewSku: document.getElementById("previewSku"),
        previewImages: document.getElementById("previewImages")
    };

    // Tiền tệ
    const handleMoney = (disp, raw, isNhap) => {
        disp.addEventListener("input", function () {
            let val = cleanNum(this.value);
            this.value = val ? formatVN(val) : "";
            raw.value = val;
            if (isNhap && val && window.location.href.toLowerCase().includes("create")) {
                let auto = Math.round(val * 1.3);
                el.giaBanRaw.value = auto;
                el.giaBanDisp.value = formatVN(auto);
                el.previewPrice.innerText = formatVN(auto) + " ₫";
            } else if (!isNhap) {
                el.previewPrice.innerText = (this.value || "0") + " ₫";
            }
        });
    };
    if (el.giaNhapDisp) handleMoney(el.giaNhapDisp, el.giaNhapRaw, true);
    if (el.giaBanDisp) handleMoney(el.giaBanDisp, el.giaBanRaw, false);

    // Folder & Images
    el.folder?.addEventListener("change", function () {
        const folderName = this.value;
        if (!folderName) { el.previewImages.innerHTML = ""; return; }
        fetch(`/Admin/BienTheSanPhams/GetImagesInFolder?folderName=${folderName}`)
            .then(res => res.json())
            .then(files => {
                el.previewImages.innerHTML = files.map(f =>
                    `<img src="/hinhanhbienthe/${folderName}/${f}" class="border rounded" style="width:60px;height:60px;object-fit:cover;">`
                ).join('');
            });
    });
});