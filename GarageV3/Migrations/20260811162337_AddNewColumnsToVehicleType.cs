using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnsToVehicleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnumValue",
                table: "VehicleTypes",
                newName: "RequiredSpots");

            migrationBuilder.AddColumn<string>(
                name: "BadgeColor",
                table: "VehicleTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BadgeTextColor",
                table: "VehicleTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "VehicleTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxVehiclesPerSpot",
                table: "VehicleTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "VehicleTypes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadgeColor",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "BadgeTextColor",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "MaxVehiclesPerSpot",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "VehicleTypes");

            migrationBuilder.RenameColumn(
                name: "RequiredSpots",
                table: "VehicleTypes",
                newName: "EnumValue");
        }
    }
}
