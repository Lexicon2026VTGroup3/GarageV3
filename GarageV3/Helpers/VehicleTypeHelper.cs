using GarageV3.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

public static class VehicleTypeHelper
{
    public static string GetBadgeColor(string typeName) =>
        typeName switch
        {
            "Car" => "#006AA7",
            "Motorcycle" => "#FECC02",
            "Truck" => "#2c3e50",
            "Bus" => "#1a7a4c",
            "Boat" => "#0891b2",
            "Airplane" => "#6b7280",
            _ => "#6c757d"
        };

    public static string GetBadgeTextColor(string typeName) =>
        typeName == "Motorcycle" ? "#1a1a1a" : "#ffffff";

    public static IEnumerable<SelectListItem> GetValidVehicleTypes(string currentTypeName, IEnumerable<string> allTypeNames)
    {
        var allItems = allTypeNames.Select(name => new SelectListItem
        {
            Text = name,
            Value = name
        });

        IEnumerable<SelectListItem> filtered = currentTypeName switch
        {
            "Airplane" or "Boat" =>
                allItems,

            "Bus" or "Truck" =>
                allItems.Where(v => v.Value != "Airplane" && v.Value != "Boat"),

            "Car" =>
                allItems.Where(v => v.Value != "Airplane" && v.Value != "Boat" && v.Value != "Bus" && v.Value != "Truck"),

            "Motorcycle" =>
                allItems.Where(v => v.Value == "Motorcycle"),

            "Bicycle" =>
                allItems.Where(v => v.Value == "Bicycle"),

            _ => allItems
        };

        if (filtered.Count() == allItems.Count())
            return filtered;

        var hintOption = new SelectListItem
        {
            Text = "Check out and check in again for other types",
            Value = "",
            Disabled = true
        };

        return filtered.Append(hintOption);
    }
}