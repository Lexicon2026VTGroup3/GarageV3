using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;

        var enumString = enumValue.ToString();
        var field = enumValue.GetType().GetField(enumString);

        var attribute = field?
            .GetCustomAttribute<DisplayAttribute>();

        return attribute?.GetName() ?? attribute?.Name ?? enumString;
    }

    public static string GetDisplayIcon(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;

        var enumString = enumValue.ToString();
        var field = enumValue.GetType().GetField(enumString);

        var attribute = field?
            .GetCustomAttribute<DisplayAttribute>();

        return attribute?.GetShortName() ?? attribute?.ShortName ?? enumString;
    }
}
