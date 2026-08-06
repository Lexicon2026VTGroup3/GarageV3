using System.ComponentModel.DataAnnotations;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;

        var attribute = enumValue.GetType()
            .GetField(enumValue.ToString())
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return attribute?.Name ?? enumValue.ToString();
    }

    public static string GetDisplayIcon(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;

        var attribute = enumValue.GetType()
            .GetField(enumValue.ToString())
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return attribute?.ShortName ?? enumValue.ToString();
    }
}
