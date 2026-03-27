using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class updatedanhgia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonHang_MaDonHangNavigationMaDonHang",
                table: "DanhGia");

            migrationBuilder.DropIndex(
                name: "IX_DanhGia_MaDonHangNavigationMaDonHang",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "MaDonHangNavigationMaDonHang",
                table: "DanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaDonHang",
                table: "DanhGia",
                column: "MaDonHang");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonHang",
                table: "DanhGia",
                column: "MaDonHang",
                principalTable: "DonHang",
                principalColumn: "maDonHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonHang",
                table: "DanhGia");

            migrationBuilder.DropIndex(
                name: "IX_DanhGia_MaDonHang",
                table: "DanhGia");

            migrationBuilder.AddColumn<int>(
                name: "MaDonHangNavigationMaDonHang",
                table: "DanhGia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaDonHangNavigationMaDonHang",
                table: "DanhGia",
                column: "MaDonHangNavigationMaDonHang");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonHang_MaDonHangNavigationMaDonHang",
                table: "DanhGia",
                column: "MaDonHangNavigationMaDonHang",
                principalTable: "DonHang",
                principalColumn: "maDonHang");
        }
    }
}
