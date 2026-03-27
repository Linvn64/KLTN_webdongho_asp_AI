using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class danhgiamadonhang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
