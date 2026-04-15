using System.Globalization;

namespace Service.Helpers
{
    public static class HelperClass
    {
        public static string? FormatArabicTimeInterval(TimeOnly? from, TimeOnly? to)
        {
            if (from == null || to == null) return null;

            // Intercept 23:59 and treat it as 12:00 AM
            TimeOnly formattedTo = to.Value;
            if (formattedTo.Hour == 23 && formattedTo.Minute == 59)
            {
                formattedTo = new TimeOnly(0, 0);
            }

            // 1. Get the time with AM/PM translated to ص/م
            string timeString = $"{from.Value.ToString("hh:mm tt", new CultureInfo("ar-EG"))} - {formattedTo.ToString("hh:mm tt", new CultureInfo("ar-EG"))}";

            // 2. Brute-force the digits to Arabic-Indic
            // (This is actually the safest cross-platform way to guarantee Eastern Arabic numerals!)
            return timeString
                .Replace("0", "٠").Replace("1", "١").Replace("2", "٢")
                .Replace("3", "٣").Replace("4", "٤").Replace("5", "٥")
                .Replace("6", "٦").Replace("7", "٧").Replace("8", "٨")
                .Replace("9", "٩");
        }
        public static DateTime GetEgyptNow()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "Egypt Standard Time"
            );
        }

        public static DateTime ConvertUtcToEgyptTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcTime, "Egypt Standard Time");
        }
    }
}