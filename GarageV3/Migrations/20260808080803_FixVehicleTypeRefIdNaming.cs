using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class FixVehicleTypeRefIdNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "ParkedVehicle");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleTypeRefId",
                table: "ParkedVehicle",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle",
                column: "VehicleTypeRefId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleTypeRefId",
                table: "ParkedVehicle",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeId",
                table: "ParkedVehicle",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle",
                column: "VehicleTypeRefId",
                principalTable: "VehicleTypes",
                principalColumn: "Id");
        }
    }
}
