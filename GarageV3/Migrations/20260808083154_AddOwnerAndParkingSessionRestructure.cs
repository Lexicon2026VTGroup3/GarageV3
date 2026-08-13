using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerAndParkingSessionRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_ParkedVehicle_ParkedVehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_ParkedVehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "ParkedVehicleId",
                table: "ParkingSessions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ParkingSessions",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "ParkingSpotNumber",
                table: "ParkingSessions",
                newName: "ParkingSpotId");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRateAtCheckIn",
                table: "ParkingSessions",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "ParkingSessions",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ParkedVehicle",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkingSpotId",
                table: "ParkingSessions",
                column: "ParkingSpotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkedVehicle_OwnerId",
                table: "ParkedVehicle",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkedVehicle_AspNetUsers_OwnerId",
                table: "ParkedVehicle",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_ParkedVehicle_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                principalTable: "ParkedVehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_ParkingSpots_ParkingSpotId",
                table: "ParkingSessions",
                column: "ParkingSpotId",
                principalTable: "ParkingSpots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkedVehicle_AspNetUsers_OwnerId",
                table: "ParkedVehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_ParkedVehicle_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_ParkingSpots_ParkingSpotId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_ParkingSpotId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkedVehicle_OwnerId",
                table: "ParkedVehicle");

            migrationBuilder.DropColumn(
                name: "HourlyRateAtCheckIn",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ParkedVehicle");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "ParkingSessions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ParkingSpotId",
                table: "ParkingSessions",
                newName: "ParkingSpotNumber");

            migrationBuilder.AddColumn<int>(
                name: "ParkedVehicleId",
                table: "ParkingSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkedVehicleId",
                table: "ParkingSessions",
                column: "ParkedVehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_ParkedVehicle_ParkedVehicleId",
                table: "ParkingSessions",
                column: "ParkedVehicleId",
                principalTable: "ParkedVehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
