using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class again : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaDonHang",
                table: "DanhGia",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaDonHang",
                table: "DanhGia");
        }
    }
}
