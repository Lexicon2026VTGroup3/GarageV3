using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingSpotNumber = table.Column<int>(type: "int", nullable: false),
                    ParkedVehicleId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ArriveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_ParkedVehicle_ParkedVehicleId",
                        column: x => x.ParkedVehicleId,
                        principalTable: "ParkedVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkedVehicleId",
                table: "ParkingSessions",
                column: "ParkedVehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSessions");
        }
    }
}
