using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "NguoiDung",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_IdentityUserId",
                table: "NguoiDung",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_UserId",
                table: "NguoiDung",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_AspNetUsers_IdentityUserId",
                table: "NguoiDung",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_AspNetUsers_UserId",
                table: "NguoiDung",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_AspNetUsers_IdentityUserId",
                table: "NguoiDung");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_AspNetUsers_UserId",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_NguoiDung_IdentityUserId",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_NguoiDung_UserId",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "NguoiDung");
        }
    }
}
