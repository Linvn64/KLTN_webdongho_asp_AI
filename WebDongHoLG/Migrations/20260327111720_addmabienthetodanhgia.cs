using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class addmabienthetodanhgia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaBienThe",
                table: "DanhGia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaBienThe",
                table: "DanhGia",
                column: "MaBienThe");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_BienThe",
                table: "DanhGia",
                column: "MaBienThe",
                principalTable: "BienTheSanPham",
                principalColumn: "maBienThe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_BienThe",
                table: "DanhGia");

            migrationBuilder.DropIndex(
                name: "IX_DanhGia_MaBienThe",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "MaBienThe",
                table: "DanhGia");
        }
    }
}
