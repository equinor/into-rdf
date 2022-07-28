using Common.FusekiModels;
using System.Globalization;

namespace Services.FusekiServices;

public static class FusekiExtensions
{
    /// <summary>
    /// Returns the value for the given name, otherwise returns an empty string
    /// </summary>
    public static string GetFusekiString(this Dictionary<string, FusekiTriplet> dictionary, string name)
        => dictionary.TryGetValue(name, out var value) ? value.Value ?? "" : "";

    public static decimal GetFusekiDecimal(this Dictionary<string, FusekiTriplet> dictionary, string name)
    {
        var value = GetFusekiString(dictionary, name);
        return ParseToDecimal(value);
    }
    
    private static decimal ParseToDecimal(this string value)
    {
        return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
            out var decimalValue)
            ? decimalValue
            : 0;
    }

    public static int GetFusekiInteger(this Dictionary<string, FusekiTriplet> dictionary, string name)
    {
        var value = GetFusekiString(dictionary, name);
        return int.TryParse(value, out var intValue) ? intValue : 0;
    }

    public static Uri? GetFusekiUri(this Dictionary<string, FusekiTriplet> dictionary, string name)
    {
        var value = GetFusekiString(dictionary, name);
        return Uri.TryCreate(value, UriKind.Absolute, out var result) ? result : null;
    }

    public static DateTime GetFusekiDateTime(this Dictionary<string, FusekiTriplet> dictionary, string name)
    {
        var value = GetFusekiString(dictionary, name);
        return DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : DateTime.MinValue;
    }
}