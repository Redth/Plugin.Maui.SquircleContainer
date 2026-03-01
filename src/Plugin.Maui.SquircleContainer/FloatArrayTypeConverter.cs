using System;
using System.ComponentModel;
using System.Globalization;

namespace Plugin.Maui.SquircleContainer;

/// <summary>
/// Converts a comma-separated string like "8,4" into a float[] for StrokeDashPattern.
/// </summary>
public class FloatArrayTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            var parts = str.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                result[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);
            return result;
        }
        return base.ConvertFrom(context, culture, value);
    }
}
