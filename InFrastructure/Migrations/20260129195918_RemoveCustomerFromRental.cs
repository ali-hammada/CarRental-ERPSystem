using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InFrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerFromRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalContracts_Customers_CustomerId",
                table: "RentalContracts");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "RentalContracts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "RentalContracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RentalContracts_EmployeeId",
                table: "RentalContracts",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalContracts_Customers_CustomerId",
                table: "RentalContracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalContracts_Employee_EmployeeId",
                table: "RentalContracts",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalContracts_Customers_CustomerId",
                table: "RentalContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalContracts_Employee_EmployeeId",
                table: "RentalContracts");

            migrationBuilder.DropIndex(
                name: "IX_RentalContracts_EmployeeId",
                table: "RentalContracts");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "RentalContracts");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "RentalContracts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalContracts_Customers_CustomerId",
                table: "RentalContracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
