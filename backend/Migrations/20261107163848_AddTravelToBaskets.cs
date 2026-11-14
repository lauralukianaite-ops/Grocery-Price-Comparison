using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelToBaskets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Total",
                table: "Baskets",
                newName: "GroceriesTotal");

            migrationBuilder.AddColumn<double>(
                name: "DistanceKm",
                table: "Baskets",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TravelCost",
                table: "Baskets",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LocationAddress",
                table: "Baskets",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "Baskets");

            migrationBuilder.DropColumn(
                name: "TravelCost",
                table: "Baskets");

            migrationBuilder.DropColumn(
                name: "LocationAddress",
                table: "Baskets");

            migrationBuilder.RenameColumn(
                name: "GroceriesTotal",
                table: "Baskets",
                newName: "Total");
        }
    }
}
