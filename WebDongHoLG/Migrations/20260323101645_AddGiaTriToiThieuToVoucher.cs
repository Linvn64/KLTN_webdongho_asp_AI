using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDongHoLG.Migrations
{
    /// <inheritdoc />
    public partial class AddGiaTriToiThieuToVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GiaTriToiThieu",
                table: "Voucher",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "isActive",
                table: "BienTheSanPham",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaTriToiThieu",
                table: "Voucher");

            migrationBuilder.AlterColumn<bool>(
                name: "isActive",
                table: "BienTheSanPham",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
