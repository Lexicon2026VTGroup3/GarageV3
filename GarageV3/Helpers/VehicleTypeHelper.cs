using GarageV3.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

public static class VehicleTypeHelper
{
    public static string GetBadgeColor(VehicleType type) =>
        type switch
        {
            VehicleType.Car        => "#006AA7",
            VehicleType.Motorcycle => "#FECC02",
            VehicleType.Truck      => "#2c3e50",
            VehicleType.Bus        => "#1a7a4c",
            VehicleType.Boat       => "#0891b2",
            VehicleType.Airplane   => "#6b7280",
            _                      => "#6c757d"
        };

    public static string GetBadgeTextColor(VehicleType type) =>
        type == VehicleType.Motorcycle ? "#1a1a1a" : "#ffffff";

    public static IEnumerable<SelectListItem> GetValidVehicleTypes(VehicleType vehicleType)
    {
        var allTypes = Enum.GetValues(typeof(VehicleType))
                .Cast<VehicleType>()
                .Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(),
                    Value = v.ToString()
                });

        IEnumerable<SelectListItem> filtered = vehicleType switch
        {
            VehicleType.Airplane or VehicleType.Boat =>
                    allTypes,

            VehicleType.Bus or VehicleType.Truck =>
                    allTypes.Where(v =>
                        v.Value != nameof(VehicleType.Airplane) &&
                        v.Value != nameof(VehicleType.Boat)
                    ),

            VehicleType.Car =>
                    allTypes.Where(v =>
                        v.Value != nameof(VehicleType.Airplane) &&
                        v.Value != nameof(VehicleType.Boat) &&
                        v.Value != nameof(VehicleType.Bus) &&
                        v.Value != nameof(VehicleType.Truck)
                    ),

            VehicleType.Motorcycle =>
                    allTypes.Where(v =>
                        v.Value == nameof(VehicleType.Motorcycle)
                    ),

            VehicleType.Bicycle =>
                    allTypes.Where(v =>
                        v.Value == nameof(VehicleType.Bicycle)
                    ),

            _ => allTypes
        };

        if (filtered.Count() == allTypes.Count())
        return filtered;

        var hintOption = new SelectListItem
        {
            Text = "Check out and check in again for other types",
            Value = "",
            Disabled = true
        };

        return filtered.Append(hintOption);
    }

    public static VehicleType ToEnum(string vehicleTypeName)
    {
        return vehicleTypeName switch
        {
            "Car" => VehicleType.Car,
            "Motorcycle" => VehicleType.Motorcycle,
            "Bus" => VehicleType.Bus,
            "Truck" => VehicleType.Truck,
            "Bicycle" => VehicleType.Bicycle,
            "Airplane" => VehicleType.Airplane,
            "Boat" => VehicleType.Boat,
            _ => VehicleType.Car
        };
    }

}
