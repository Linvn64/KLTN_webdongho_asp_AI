function initWarehousePage(yeuCauXuatHoaDon, hangDaChon, activeTab) {
    document.addEventListener("DOMContentLoaded", function () {

        // 1. Ghi nhớ Tab khi nhấn thủ công
        document.querySelectorAll('a[data-bs-toggle="tab"]').forEach(tabLink => {
            tabLink.addEventListener('shown.bs.tab', function (e) {
                const id = e.target.getAttribute('href').replace('#', '');
                sessionStorage.setItem('warehouseActiveTab', id);
            });
        });

        // 2. Tải Excel hóa đơn nếu vừa lưu thành công
        if (yeuCauXuatHoaDon === "true") {
            xuatHoaDonExcel(hangDaChon);
        }

        // 3. Tự động ẩn Alert sau 3s
        const alertBox = document.getElementById('autoCloseAlert');
        if (alertBox) {
            setTimeout(() => {
                alertBox.style.transition = "opacity 0.5s ease";
                alertBox.style.opacity = "0";
                setTimeout(() => alertBox.remove(), 500);
            }, 3000);
        }
    });
}

function xuatHoaDonExcel(hang) {
    let data = [["HÓA ĐƠN NHẬP KHO"], ["Ngày:", new Date().toLocaleString()], ["Hãng:", hang], [], ["ID", "TÊN SP", "GIÁ", "SL", "THÀNH TIỀN"]];
    document.querySelectorAll("#bangNhapKho tbody tr").forEach(row => {
        const sl = parseInt(row.querySelector(".o-nhap").value) || 0;
        if (sl > 0) {
            const ma = row.querySelector(".sp-ma").innerText;
            const ten = row.querySelector(".sp-ten").innerText;
            const gia = parseInt(row.querySelector(".sp-gia").innerText.replace(/\D/g, ''));
            data.push([ma, ten, gia, sl, gia * sl]);
        }
    });
    const ws = XLSX.utils.aoa_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "HoaDon");
    XLSX.writeFile(wb, "PhieuNhap_" + Date.now() + ".xlsx");
}

function xuatExcelLichSu(tongTien) {
    let data = [["BÁO CÁO NHẬP KHO"], ["Thời gian:", new Date().toLocaleString()], ["Tổng vốn:", tongTien + "đ"], [], ["THỜI GIAN", "SẢN PHẨM", "HÃNG", "SL", "GIÁ", "THÀNH TIỀN", "NGƯỜI NHẬP"]];
    document.querySelectorAll("#bangLichSu tbody tr").forEach(row => {
        const cols = row.querySelectorAll("td");
        if (cols.length > 0) {
            data.push([cols[0].innerText.replace(/\n/g, ' '), cols[1].innerText.trim(), cols[2].innerText.trim(), cols[3].innerText.replace('+', '').trim(), cols[4].innerText.trim(), cols[5].innerText.trim()]);
        }
    });
    const ws = XLSX.utils.aoa_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "BaoCao");
    XLSX.writeFile(wb, "BaoCao_Kho_" + Date.now() + ".xlsx");
}

// file create nhapkho
        let phieu = [];
        let bienTheCache = { };

        const formatVN = (val) => new Intl.NumberFormat('vi-VN').format(val || 0);

        // 1. Chọn sản phẩm → load biến thể
        document.getElementById("selectSanPham").addEventListener("change", function () {
            const maSp = this.value;
        const container = document.getElementById("bienTheContainer");
        const loading = document.getElementById("loadingBienThe");

        if (!maSp) {
            container.classList.add("d-none");
        return;
            }

        container.classList.add("d-none");
        loading.classList.remove("d-none");

        fetch(`/Admin/NhapKho/GetBienThe?maSp=${maSp}`)
                .then(res => res.json())
                .then(data => {
            loading.classList.add("d-none");
        bienTheCache = { };
                    data.forEach(bt => bienTheCache[bt.maBienThe] = bt);

        const list = document.getElementById("bienTheList");
        list.innerHTML = "";

        if (data.length === 0) {
            list.innerHTML = '<div class="list-group-item text-muted small py-3 text-center">Không có biến thể nào.</div>';
                    } else {
            data.forEach(bt => {
                const sudaCo = phieu.find(p => p.maBienThe === bt.maBienThe);
                const item = document.createElement("div");
                item.className = `list-group-item ${sudaCo ? 'list-group-item-success' : ''}`;
                item.id = `btItem-${bt.maBienThe}`;
                item.innerHTML = `
                                <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                        <div class="fw-bold small font-monospace">${bt.maSku}</div>
                                        <div class="text-muted" style="font-size:11px;">
                                            ${bt.mauSac ?? ''}
                                            ${bt.duongKinhMat ? ' | ' + bt.duongKinhMat + 'mm' : ''}
                                            ${bt.chatLieuDay ? ' | ' + bt.chatLieuDay : ''}
                                        </div>
                                    </div>
                                    <div class="text-end d-flex align-items-center gap-2">
                                        <div>
                                            <div class="small text-muted">Tồn</div>
                                            <div class="fw-bold ${(bt.tonHienTai ?? 0) <= 0 ? 'text-danger' : (bt.tonHienTai ?? 0) <= 5 ? 'text-warning' : 'text-success'}">
                                                ${bt.tonHienTai ?? 0}
                                            </div>
                                        </div>
                                        <button type="button"
                                                class="btn btn-sm ${sudaCo ? 'btn-success' : 'btn-primary'}"
                                                onclick="themVaoPhieu(${bt.maBienThe})"
                                                id="btnThem-${bt.maBienThe}">
                                            <i class="ti ti-${sudaCo ? 'check' : 'plus'}"></i>
                                        </button>
                                    </div>
                                </div>`;
                list.appendChild(item);
            });
                    }

        container.classList.remove("d-none");
                })
                .catch(() => {
            loading.classList.add("d-none");
        container.classList.remove("d-none");
                });
        });

        // 2. Thêm vào phiếu
        function themVaoPhieu(maBienThe) {
            if (phieu.find(p => p.maBienThe === maBienThe)) return;

        const bt = bienTheCache[maBienThe];
        if (!bt) return;

        phieu.push({
            maBienThe: bt.maBienThe,
        maSku: bt.maSku,
        mauSac: bt.mauSac ?? '',
        duongKinhMat: bt.duongKinhMat,
        chatLieuDay: bt.chatLieuDay ?? '',
        tonHienTai: bt.tonHienTai ?? 0,
        giaNhap: bt.giaNhap ?? 0,
        giaNhapGanNhat: bt.giaNhapGanNhat ?? 0,
        soLuongNhap: 10
        });

        // Cập nhật nút trong danh sách
        const btn = document.getElementById(`btnThem-${maBienThe}`);
        const item = document.getElementById(`btItem-${maBienThe}`);
        if (btn) {
            btn.className = "btn btn-sm btn-success";
        btn.innerHTML = '<i class="ti ti-check"></i>';
            }
        if (item) item.classList.add("list-group-item-success");

        renderPhieu();
        }

        // 3. Thêm tất cả
        document.getElementById("btnThemTatCa").addEventListener("click", function () {
            Object.values(bienTheCache).forEach(bt => {
                if (!phieu.find(p => p.maBienThe === bt.maBienThe)) {
                    phieu.push({
                        maBienThe: bt.maBienThe,
                        maSku: bt.maSku,
                        mauSac: bt.mauSac ?? '',
                        duongKinhMat: bt.duongKinhMat,
                        chatLieuDay: bt.chatLieuDay ?? '',
                        tonHienTai: bt.tonHienTai ?? 0,
                        giaNhap: bt.giaNhap ?? 0,
                        soLuongNhap: 10
                    });
                }
            });

            // Cập nhật tất cả nút
            Object.keys(bienTheCache).forEach(id => {
                const btn = document.getElementById(`btnThem-${id}`);
        const item = document.getElementById(`btItem-${id}`);
        if (btn) {btn.className = "btn btn-sm btn-success"; btn.innerHTML = '<i class="ti ti-check"></i>'; }
        if (item) item.classList.add("list-group-item-success");
            });

        renderPhieu();
        });

        // 4. Render phiếu
        function renderPhieu() {
            const body = document.getElementById("phieuBody");
        const phieuTrong = document.getElementById("phieuTrong");
        const phieuTable = document.getElementById("phieuTable");
        const btnLuu = document.getElementById("btnLuu");

        if (phieu.length === 0) {
            phieuTrong.classList.remove("d-none");
        phieuTable.classList.add("d-none");
        btnLuu.disabled = true;
        document.getElementById("soLuongPhieu").innerText = "0 biến thể";
        document.getElementById("tongSoLuong").innerText = "0";
        document.getElementById("tongGiaTri").innerText = "0 ₫";
        return;
            }

        phieuTrong.classList.add("d-none");
        phieuTable.classList.remove("d-none");
        btnLuu.disabled = false;

        body.innerHTML = "";
        let tongSoLuong = 0;
        let tongGiaTri = 0;

            phieu.forEach((item, index) => {
            tongSoLuong += item.soLuongNhap;
        tongGiaTri += item.soLuongNhap * item.giaNhap;

        const tr = document.createElement("tr");
        tr.innerHTML = `
        <td>
            <div class="fw-bold small font-monospace">${item.maSku}</div>
            <div class="text-muted" style="font-size:11px;">
                ${item.mauSac}
                ${item.duongKinhMat ? ' | ' + item.duongKinhMat + 'mm' : ''}
                ${item.chatLieuDay ? ' | ' + item.chatLieuDay : ''}
            </div>
            <input type="hidden" name="MaBienThes" value="${item.maBienThe}" />
        </td>
        <td class="text-center">
            <span class="badge fw-bold ${item.tonHienTai <= 0 ? 'bg-red' : item.tonHienTai <= 5 ? 'bg-warning' : 'bg-green-lt'}">
                ${item.tonHienTai}
            </span>
        </td>
        <td>
            <input type="number" name="SoLuongNhaps"
                value="${item.soLuongNhap}" min="1"
                class="form-control form-control-sm text-center"
                onchange="capNhatSoLuong(${index}, this.value)" />
        </td>
        <td>
            <div class="small text-muted">
                Dự kiến: ${formatVN(item.giaNhap)} ₫
                ${item.giaNhapGanNhat ? `<br>Gần nhất: ${formatVN(item.giaNhapGanNhat)} ₫` : ''}
            </div>

            <div class="input-group input-group-sm mt-1">
                <input type="number" name="GiaNhaps"
                    value="${item.giaNhap}" min="0" step="1000"
                    class="form-control text-end"
                    onchange="capNhatGia(${index}, this.value)" />
                <span class="input-group-text">₫</span>
            </div>

            <div class="small mt-1">
                ${renderSoSanhGia(item)}
            </div>
        </td>
        <td>
            <button type="button" class="btn btn-icon btn-sm btn-outline-danger"
                onclick="xoaKhoiPhieu(${index})">
                <i class="ti ti-x"></i>
            </button>
        </td>`;
        body.appendChild(tr);
            });

        document.getElementById("soLuongPhieu").innerText = `${phieu.length} biến thể`;
        document.getElementById("tongSoLuong").innerText = tongSoLuong;
        document.getElementById("tongGiaTri").innerText = formatVN(tongGiaTri) + " ₫";
        }

        function capNhatSoLuong(index, value) {
            phieu[index].soLuongNhap = Math.max(1, parseInt(value) || 1);
        updateTotals();
        }

        function capNhatGia(index, value) {
            phieu[index].giaNhap = parseFloat(value) || 0;
        updateTotals();
        renderPhieu(); // Cần để cập nhật lại phần so sánh giá
        }

        function updateTotals() {
            const tongSoLuong = phieu.reduce((sum, p) => sum + p.soLuongNhap, 0);
            const tongGiaTri = phieu.reduce((sum, p) => sum + p.soLuongNhap * p.giaNhap, 0);
        document.getElementById("tongSoLuong").innerText = tongSoLuong;
        document.getElementById("tongGiaTri").innerText = formatVN(tongGiaTri) + " ₫";
        }

        function xoaKhoiPhieu(index) {
            const maBienThe = phieu[index].maBienThe;
        phieu.splice(index, 1);

        // Reset nút trong danh sách
        const btn = document.getElementById(`btnThem-${maBienThe}`);
        const item = document.getElementById(`btItem-${maBienThe}`);
        if (btn) {btn.className = "btn btn-sm btn-primary"; btn.innerHTML = '<i class="ti ti-plus"></i>'; }
        if (item) item.classList.remove("list-group-item-success");

        renderPhieu();
        }
    
        function xoaTatCa() {
            if (phieu.length === 0) return;
        if (!confirm("Xóa tất cả biến thể trong phiếu?")) return;

            phieu.forEach(p => {
                const btn = document.getElementById(`btnThem-${p.maBienThe}`);
        const item = document.getElementById(`btItem-${p.maBienThe}`);
        if (btn) {btn.className = "btn btn-sm btn-primary"; btn.innerHTML = '<i class="ti ti-plus"></i>'; }
        if (item) item.classList.remove("list-group-item-success");
            });

        phieu = [];
        renderPhieu();
        }
        function renderSoSanhGia(item) {
            const giaMoi = item.giaNhap;
            const giaCu = item.giaNhapGanNhat;

            if (!giaCu) return '<span class="text-muted">Chưa có dữ liệu</span>';

            const diff = giaMoi - giaCu;
            const percent = ((diff / giaCu) * 100).toFixed(1);

            if (diff > 0) {
                return `
                    <span class="badge bg-red-lt text-red fw-bold">
                        ⬆ +${percent}%
                    </span>
                `;
            }
            else if (diff < 0) {
                return `
                    <span class="badge bg-green-lt text-green fw-bold">
                        ⬇ ${percent}%
                    </span>
                `;
            }
            else {
                return `<span class="badge bg-secondary-lt">= 0%</span>`;
            }
        }


        document.addEventListener("DOMContentLoaded", function () {
            const select = document.getElementById("selectSanPham");
        if (select.value) {
            select.dispatchEvent(new Event("change"));
            }
        });
