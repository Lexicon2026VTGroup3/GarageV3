using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageV3.Migrations
{
    /// <inheritdoc />
    public partial class FixVehicleTypeSpaceUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrects RequiredSpaceUnits for large vehicle types, which
            // were stuck at the default value (3) on any database that had
            // already seeded VehicleTypes before this value was introduced
            // (US12). Runs for everyone via Update-Database, regardless of
            // their local database's existing seed state.
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 1 WHERE Name = 'Motorcycle';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 3 WHERE Name = 'Car';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 6 WHERE Name = 'Bus';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 6 WHERE Name = 'Truck';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 6 WHERE Name = 'Boat';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 9 WHERE Name = 'Airplane';");
            migrationBuilder.Sql("UPDATE VehicleTypes SET RequiredSpaceUnits = 1 WHERE Name = 'Bicycle';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible in a meaningful way — the "down" state (all 3)
            // was itself the bug, so there's nothing useful to revert to.
        }
    }
}