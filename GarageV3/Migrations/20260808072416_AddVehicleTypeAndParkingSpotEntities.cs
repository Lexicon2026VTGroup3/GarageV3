using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeAndParkingSpotEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeId",
                table: "ParkedVehicle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeRefId",
                table: "ParkedVehicle",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsOutOfService = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkedVehicle_VehicleTypeRefId",
                table: "ParkedVehicle",
                column: "VehicleTypeRefId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_Number",
                table: "ParkingSpots",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Name",
                table: "VehicleTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle",
                column: "VehicleTypeRefId",
                principalTable: "VehicleTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkedVehicle_VehicleTypes_VehicleTypeRefId",
                table: "ParkedVehicle");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropIndex(
                name: "IX_ParkedVehicle_VehicleTypeRefId",
                table: "ParkedVehicle");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "ParkedVehicle");

            migrationBuilder.DropColumn(
                name: "VehicleTypeRefId",
                table: "ParkedVehicle");
        }
    }
}
