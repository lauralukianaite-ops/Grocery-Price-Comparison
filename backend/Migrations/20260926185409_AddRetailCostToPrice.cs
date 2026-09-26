using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRetailCostToPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Prices",
                newName: "Cost");

            migrationBuilder.AddColumn<decimal>(
                name: "RetailCost",
                table: "Prices",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetailCost",
                table: "Prices");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "Prices",
                newName: "Amount");
        }
    }
}
