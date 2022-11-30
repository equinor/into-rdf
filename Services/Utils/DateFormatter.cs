using System.Globalization;

namespace Services.Utils;

public static class DateFormatter
{
    public static DateTime FormateToDate(string date)
    {
       return DateTime.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public static string FormateToString(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}