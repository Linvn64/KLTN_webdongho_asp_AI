using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class ShopDongHoDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ShopDongHoDbContext()
    {
    }

    public ShopDongHoDbContext(DbContextOptions<ShopDongHoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BienTheSanPham> BienTheSanPhams { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }

    public virtual DbSet<DiaChiGiaoHang> DiaChiGiaoHangs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<HinhAnhBienThe> HinhAnhBienThes { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }

    public virtual DbSet<LichSuDonHang> LichSuDonHangs { get; set; }

    public virtual DbSet<LichSuTuVanAi> LichSuTuVanAis { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhapKho> NhapKhos { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<ThongSoSanPham> ThongSoSanPhams { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }
    public DbSet<ThuongHieu> ThuongHieus { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); 


        modelBuilder.Entity<BienTheSanPham>(entity =>
        {
            entity.HasKey(e => e.MaBienThe).HasName("PK__BienTheS__CFD6B0B9B4CD9442");

            entity.ToTable("BienTheSanPham");

            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.ChatLieuDay)
                .HasMaxLength(100)
                .HasColumnName("chatLieuDay");
            entity.Property(e => e.DuongKinhMat).HasColumnName("duongKinhMat");
            entity.Property(e => e.GiaBan)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("giaBan");
            entity.Property(e => e.GiaNhap)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("giaNhap");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("imageUrl");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.MaSku)
                .HasMaxLength(100)
                .HasColumnName("maSKU");
            entity.Property(e => e.MaSp).HasColumnName("maSp");
            entity.Property(e => e.MauSac)
                .HasMaxLength(50)
                .HasColumnName("mauSac");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.BienTheSanPhams)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BienTheSan__maSp__4BAC3F29");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.MaDonHang, e.MaBienThe });

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaDonHang).HasColumnName("maDonHang");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.DonGiaTaiThoiDiem)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("donGiaTaiThoiDiem");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaBienThe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__maBie__5EBF139D");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__maDon__5DCAEF64");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => new { e.MaGioHang, e.MaBienThe });

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.MaGioHang).HasColumnName("maGioHang");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaBienThe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGi__maBie__628FA481");

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGi__maGio__619B8048");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__6B15DD9A3DF1A8C8");

            entity.Property(e => e.MaDanhGia).HasColumnName("maDanhGia");
            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.MaSp).HasColumnName("maSp");
            entity.Property(e => e.NgayDanhGia)
                .HasColumnType("datetime")
                .HasColumnName("ngayDanhGia");
            entity.Property(e => e.NoiDung)
                .HasMaxLength(500)
                .HasColumnName("noiDung");
            entity.Property(e => e.SoSao).HasColumnName("soSao");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DanhGia__maNguoi__6EF57B66");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__DanhGia__maSp__6FE99F9F");
            entity.HasOne(d => d.MaBienTheNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.MaBienThe)
                  .HasConstraintName("FK_DanhGia_BienThe");
            entity.HasOne(d => d.MaDonHangNavigation)
                 .WithMany()
                 .HasForeignKey(d => d.MaDonHang)
                 .HasConstraintName("FK_DanhGia_DonHang");
        });

        modelBuilder.Entity<DanhMucSanPham>(entity =>
        {
            entity.HasKey(e => e.IdDanhMuc).HasName("PK__DanhMucS__927F1878D3D46117");

            entity.ToTable("DanhMucSanPham");

            entity.Property(e => e.IdDanhMuc).HasColumnName("idDanhMuc");
            entity.Property(e => e.MoTa)
                .HasMaxLength(255)
                .HasColumnName("moTa");
            entity.Property(e => e.TenDanhMuc)
                .HasMaxLength(100)
                .HasColumnName("tenDanhMuc");
        });

        modelBuilder.Entity<DiaChiGiaoHang>(entity =>
        {
            entity.HasKey(e => e.MaDiaChi).HasName("PK__DiaChiGi__0A7DF6203061843A");

            entity.ToTable("DiaChiGiaoHang");

            entity.Property(e => e.MaDiaChi).HasColumnName("maDiaChi");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("diaChi");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.SdtNhanHang)
                .HasMaxLength(15)
                .HasColumnName("sdtNhanHang");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DiaChiGiaoHangs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DiaChiGia__maNgu__5165187F");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__871D3819019D948B");

            entity.ToTable("DonHang");

            entity.Property(e => e.MaDonHang).HasColumnName("maDonHang");
            entity.Property(e => e.MaDiaChi).HasColumnName("maDiaChi");
            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngayDat");
            entity.Property(e => e.TongTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tongTien");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasColumnName("trangThai");

            entity.Property(e => e.PhiVanChuyen)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("phiVanChuyen")
                .HasDefaultValue(0); // Mặc định là 0 nếu freeship

            entity.Property(e => e.TienGiamGia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tienGiamGia")
                .HasDefaultValue(0);

            entity.HasOne(d => d.MaDiaChiNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaDiaChi)
                .HasConstraintName("FK__DonHang__maDiaCh__59FA5E80");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonHang__maNguoi__59063A47");

            entity.HasOne(d => d.MaVoucherNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaVoucher)
                .HasConstraintName("FK__DonHang__MaVouch__5AEE82B9");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.IdFaq).HasName("PK__FAQ__39CD51D041A94D26");

            entity.ToTable("FAQ");

            entity.Property(e => e.IdFaq).HasColumnName("idFAQ");
            entity.Property(e => e.CauHoi).HasColumnName("cauHoi");
            entity.Property(e => e.CauTraLoi).HasColumnName("cauTraLoi");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasColumnName("trangThai");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGioHang).HasName("PK__GioHang__2C76D2032AD946F4");

            entity.ToTable("GioHang");

            entity.Property(e => e.MaGioHang).HasColumnName("maGioHang");
            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngayTao");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__GioHang__maNguoi__5535A963");
        });

        modelBuilder.Entity<HinhAnhBienThe>(entity =>
        {
            entity.HasKey(e => e.IdHinhAnh).HasName("PK__HinhAnhB__4187C930E7E48F85");

            entity.ToTable("HinhAnhBienThe");

            entity.Property(e => e.IdHinhAnh).HasColumnName("idHinhAnh");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("imageUrl");
            entity.Property(e => e.LaAnhChinh).HasColumnName("laAnhChinh");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thuTuHienThi");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.HinhAnhBienThes)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK__HinhAnhBi__maBie__4E88ABD4");
        });

        modelBuilder.Entity<Kho>(entity =>
        {
            entity.HasKey(e => e.IdKho).HasName("PK__Kho__3FBE9E30C4176819");

            entity.ToTable("Kho");

            entity.Property(e => e.IdKho).HasColumnName("idKho");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.SoLuongTon).HasColumnName("soLuongTon");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.Khos)
                .HasForeignKey(d => d.MaBienThe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Kho__maBienThe__656C112C");
        });

        modelBuilder.Entity<LichSuDonHang>(entity =>
        {
            entity.HasKey(e => e.IdLichSu).HasName("PK__LichSuDo__C6FF3816F59E239E");

            entity.ToTable("LichSuDonHang");

            entity.Property(e => e.IdLichSu).HasColumnName("idLichSu");
            entity.Property(e => e.MaDonHang).HasColumnName("maDonHang");
            entity.Property(e => e.NguoiCapNhat)
                .HasMaxLength(100)
                .HasColumnName("nguoiCapNhat");
            entity.Property(e => e.ThoiGianCapNhat)
                .HasColumnType("datetime")
                .HasColumnName("thoiGianCapNhat");
            entity.Property(e => e.TrangThaiCu)
                .HasMaxLength(50)
                .HasColumnName("trangThaiCu");
            entity.Property(e => e.TrangThaiMoi)
                .HasMaxLength(50)
                .HasColumnName("trangThaiMoi");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.LichSuDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__LichSuDon__maDon__72C60C4A");
        });

        modelBuilder.Entity<LichSuTuVanAi>(entity =>
        {
            entity.HasKey(e => e.MaTuVan).HasName("PK__LichSuTu__E93A68B59F52ECC2");

            entity.ToTable("LichSuTuVanAI");

            entity.Property(e => e.MaTuVan).HasColumnName("maTuVan");
            entity.Property(e => e.CauHoi).HasColumnName("cauHoi");
            entity.Property(e => e.CauTraLoi).HasColumnName("cauTraLoi");
            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.ThoiGian)
                .HasColumnType("datetime")
                .HasColumnName("thoiGian");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuTuVanAis)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__LichSuTuV__maNgu__75A278F5");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__446439EA8A038624");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.MaNguoiDung).HasColumnName("maNguoiDung");
            entity.Property(e => e.AnhDaiDien).HasColumnName("anhDaiDien");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(10)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("hoTen");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngayTao");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("sdt");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne<IdentityUser>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .HasPrincipalKey(u => u.Id)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NhapKho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhapKho__3214EC0713937234");

            entity.ToTable("NhapKho");

            entity.Property(e => e.GiaNhapLuuTru).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.NhapKhos)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK__NhapKho__MaBienT__68487DD7");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__7A227A5A7BDC6FB9");

            entity.ToTable("SanPham");

            entity.Property(e => e.MaSp).HasColumnName("maSp");
            entity.Property(e => e.DoiTuong)
                .HasMaxLength(50)
                .HasColumnName("doiTuong");
            entity.Property(e => e.IdDanhMuc).HasColumnName("idDanhMuc");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TenSanPham)
                .HasMaxLength(255)
                .HasColumnName("tenSanPham");
            entity.HasOne(d => d.ThuongHieuNavigation)
                .WithMany(t => t.SanPhams)
                .HasForeignKey(d => d.ThuongHieuId)
    .HasConstraintName("FK_SanPham_ThuongHieu");

            entity.HasOne(d => d.IdDanhMucNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.IdDanhMuc)
                .HasConstraintName("FK__SanPham__idDanhM__44FF419A");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.IdThanhToan).HasName("PK__ThanhToa__2DE12A667F86F32A");

            entity.ToTable("ThanhToan");

            entity.HasIndex(e => e.MaDonHang, "UQ__ThanhToa__871D38184511D759").IsUnique();

            entity.Property(e => e.IdThanhToan).HasColumnName("idThanhToan");
            entity.Property(e => e.MaDonHang).HasColumnName("maDonHang");
            entity.Property(e => e.PhuongThuc)
                .HasMaxLength(50)
                .HasColumnName("phuongThuc");
            entity.Property(e => e.ThoiGianThanhToan)
                .HasColumnType("datetime")
                .HasColumnName("thoiGianThanhToan");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaDonHangNavigation).WithOne(p => p.ThanhToan)
                .HasForeignKey<ThanhToan>(d => d.MaDonHang)
                .HasConstraintName("FK__ThanhToan__maDon__6C190EBB");
        });

        modelBuilder.Entity<ThongSoSanPham>(entity =>
        {
            entity.HasKey(e => e.IdThongSo).HasName("PK__ThongSoS__0816FFC2CFE26183");

            entity.ToTable("ThongSoSanPham");

            entity.Property(e => e.IdThongSo).HasColumnName("idThongSo");
            entity.Property(e => e.GiaTri)
                .HasMaxLength(200)
                .HasColumnName("giaTri");
            entity.Property(e => e.MaSp).HasColumnName("maSp");
            entity.Property(e => e.TenThongSo)
                .HasMaxLength(100)
                .HasColumnName("tenThongSo");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ThongSoSanPhams)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__ThongSoSan__maSp__47DBAE45");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.MaVoucher).HasName("PK__Voucher__0AAC5B11BDA1118B");

            entity.ToTable("Voucher");

            entity.Property(e => e.DaDung).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.SoLuong).HasDefaultValue(0);
            entity.Property(e => e.TenVoucher).HasMaxLength(100);
            entity.Property(e => e.GiaTriToiThieu)
                                  .HasColumnType("decimal(18, 2)") 
                                  .HasDefaultValue(0);
        });

        modelBuilder.Entity<ThuongHieu>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("ThuongHieu");

            entity.Property(e => e.TenThuongHieu)
                .HasMaxLength(100)
                .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
