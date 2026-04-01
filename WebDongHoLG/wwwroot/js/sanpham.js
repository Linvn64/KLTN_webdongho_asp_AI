
        const loadedMap = { };

        function toggleBienThe(maSp, row) {
        const bienTheRow = document.getElementById(`bienthe-row-${maSp}`);
        const icon = row.querySelector('.toggle-icon');
        const isHidden = bienTheRow.classList.contains('d-none');

        bienTheRow.classList.toggle('d-none', !isHidden);
        icon.style.transform = isHidden ? 'rotate(90deg)' : 'rotate(0deg)';

        if (isHidden && !loadedMap[maSp]) {
            loadedMap[maSp] = true;
        loadBienThe(maSp);
            }
        }

        function loadBienThe(maSp) {
            const container = document.getElementById(`bienthe-content-${maSp}`);

        fetch(`/Admin/BienTheSanPhams/GetBienTheBySpId?maSp=${maSp}&active=true`)
                .then(res => res.json())
            .then(data => {
                    const activeData = data.filter(x => x.isActive === true);
                if (!activeData || activeData.length === 0) {
            container.innerHTML = `
                            <div class="d-flex align-items-center justify-content-between">
                                <span class="text-muted small italic">Chưa có biến thể nào.</span>
                                <a href="/Admin/BienTheSanPhams/Create?maSp=${maSp}" class="btn btn-sm btn-primary">
                                    <i class="ti ti-plus me-1"></i>Thêm biến thể
                                </a>
                            </div>`;
        return;
                    }

        let html = `
        <div class="d-flex justify-content-between align-items-center mb-2">
            <small class="text-muted fw-bold text-uppercase">
                ${data.length} biến thể
            </small>
            <a href="/Admin/BienTheSanPhams/Create?maSp=${maSp}" class="btn btn-sm btn-primary">
                <i class="ti ti-plus me-1"></i>Thêm biến thể
            </a>
        </div>
        <div class="table-responsive">
            <table class="table table-sm table-vcenter mb-0">
                <thead class="table-light">
                    <tr>
                        <th>Ảnh</th>
                        <th>SKU</th>
                        <th>Màu sắc</th>
                        <th>Size</th>
                        <th>Dây</th>
                        <th>Giá bán</th>
                        <th>Trạng thái</th>
                        <th class="text-end">Thao tác</th>
                    </tr>
                </thead>
                <tbody>`;

                activeData.forEach(bt => {
                        html += `
                            <tr>
                                <td>
                                    ${bt.imageUrl
                            ? `<span class="avatar avatar-sm border shadow-sm"
                                                style="background-image:url('${bt.imageUrl}')"></span>`
                            : `<span class="avatar avatar-sm border"><i class="ti ti-photo-off"></i></span>`}
                                </td>
                                <td class="font-monospace small">${bt.maSku ?? '-'}</td>
                                <td><span class="badge bg-blue-lt">${bt.mauSac ?? '-'}</span></td>
                                <td>${bt.duongKinhMat ? bt.duongKinhMat + ' mm' : '-'}</td>
                                <td class="small text-muted">${bt.chatLieuDay ?? '-'}</td>
                                <td class="fw-bold text-danger">${bt.giaBan ? bt.giaBan.toLocaleString('vi-VN') + ' ₫' : '-'}</td>
                                <td>
                                    ${bt.isActive
                            ? `<span class="status status-green">
                                               <span class="status-dot status-dot-animated"></span> Hoạt động
                                           </span>`
                            : `<span class="status status-red">
                                               <span class="status-dot"></span> Ngừng bán
                                           </span>`}
                                </td>
                                <td class="text-end">
                                    <div class="d-flex justify-content-end gap-1">
                                        <a href="/Admin/BienTheSanPhams/Edit/${bt.maBienThe}"
                                           class="btn btn-icon btn-sm btn-outline-warning" title="Sửa">
                                            <i class="ti ti-edit"></i>
                                        </a>
                                        <a href="/Admin/BienTheSanPhams/Details/${bt.maBienThe}"
                                           class="btn btn-icon btn-sm btn-outline-info" title="Xem">
                                            <i class="ti ti-eye"></i>
                                        </a>
                                        <a href="/Admin/BienTheSanPhams/Delete/${bt.maBienThe}"
                                           class="btn btn-icon btn-sm btn-outline-danger" title="Xóa">
                                            <i class="ti ti-trash"></i>
                                        </a>
                                    </div>
                                </td>
                            </tr>`;
                    });

                    html += `</tbody></table></div>`;
        container.innerHTML = html;
                })
                .catch(() => {
            container.innerHTML = `<div class="text-danger small p-2">
                        <i class="ti ti-wifi-off me-1"></i>Không tải được biến thể.
                    </div>`;
                });
        }
