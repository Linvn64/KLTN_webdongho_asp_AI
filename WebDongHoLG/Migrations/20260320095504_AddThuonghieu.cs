using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class AddThuonghieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thuongHieu",
                table: "SanPham");

            migrationBuilder.AddColumn<int>(
                name: "ThuongHieuId",
                table: "SanPham",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThuongHieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuongHieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieu", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_ThuongHieuId",
                table: "SanPham",
                column: "ThuongHieuId");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPham_ThuongHieu",
                table: "SanPham",
                column: "ThuongHieuId",
                principalTable: "ThuongHieu",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPham_ThuongHieu",
                table: "SanPham");

            migrationBuilder.DropTable(
                name: "ThuongHieu");

            migrationBuilder.DropIndex(
                name: "IX_SanPham_ThuongHieuId",
                table: "SanPham");

            migrationBuilder.DropColumn(
                name: "ThuongHieuId",
                table: "SanPham");

            migrationBuilder.AddColumn<string>(
                name: "thuongHieu",
                table: "SanPham",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
