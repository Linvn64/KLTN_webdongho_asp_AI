    const bienThes = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.BienThes));

        document.querySelectorAll(".chon-bien-the").forEach(btn => {
        btn.addEventListener("click", function () {
            document.querySelectorAll(".chon-bien-the").forEach(b => {
                b.classList.remove("btn-primary");
                b.classList.add("btn-outline-secondary");
            });
            this.classList.remove("btn-outline-secondary");
            this.classList.add("btn-primary");

            const gia = this.getAttribute("data-gia");
            document.getElementById("giaHienTai").innerText =
                gia ? parseInt(gia).toLocaleString("vi-VN") + " ₫" : "Liên hệ";

            const ton = parseInt(this.getAttribute("data-ton"));
            const tonEl = document.getElementById("tonKhoHienTai");
            tonEl.innerHTML = ton > 0
                ? `<span class="badge bg-success">Còn ${ton} sản phẩm</span>`
                : `<span class="badge bg-danger">Hết hàng</span>`;

            const img = this.getAttribute("data-img");
            if (img) document.getElementById("imgChinh").src = img;

            const hinhs = JSON.parse(this.getAttribute("data-hinhs") || "[]");
            const gallery = document.getElementById("galleryAnhPhu");
            gallery.innerHTML = "";
            hinhs.forEach(h => {
                const el = document.createElement("img");
                el.src = h;
                el.className = "rounded border";
                el.style = "width:70px; height:70px; object-fit:cover; cursor:pointer;";
                el.onclick = () => document.getElementById("imgChinh").src = h;
                gallery.appendChild(el);
            });

            document.getElementById("maBienTheChon").value = this.getAttribute("data-mabienthe");
        });
        });

    document.querySelector(".btn-plus").addEventListener("click", function () {
        let val = parseInt(document.getElementById("txtSoLuong").value) || 1;
    document.getElementById("txtSoLuong").value = val + 1;
        });
    document.querySelector(".btn-minus").addEventListener("click", function () {
        let val = parseInt(document.getElementById("txtSoLuong").value) || 1;
            if (val > 1) document.getElementById("txtSoLuong").value = val - 1;
        });

 