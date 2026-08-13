using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountToParkingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AppliedDiscountPercentage",
                table: "ParkingSessions",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedDiscountPercentage",
                table: "ParkingSessions");
        }
    }
}
