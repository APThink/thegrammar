using System.ComponentModel;
using System.Reflection;

namespace TheGrammar.Common;

public static class EnumExtensions
{
    public static string GetDescription<TEnum>(this TEnum? enumValue) where TEnum : struct, Enum
    {
        var name = enumValue.ToString();

        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        return GetDescription<TEnum>(name);
    }


    public static string GetDescription<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        var name = enumValue.ToString();

        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        return GetDescription<TEnum>(name);
    }

    private static string GetDescription<TEnum>(string? name) where TEnum : struct, Enum
    {
        var field = typeof(TEnum).GetField(name);

        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }
}