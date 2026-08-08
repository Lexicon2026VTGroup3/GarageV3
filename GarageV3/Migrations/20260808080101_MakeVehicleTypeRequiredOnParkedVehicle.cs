using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class MakeVehicleTypeRequiredOnParkedVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "ParkedVehicle");

            migrationBuilder.AddColumn<int>(
                name: "EnumValue",
                table: "VehicleTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleTypeId",
                table: "ParkedVehicle",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnumValue",
                table: "VehicleTypes");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleTypeId",
                table: "ParkedVehicle",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "ParkedVehicle",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
