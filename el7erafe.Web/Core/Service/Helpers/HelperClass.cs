using System.Globalization;

namespace Service.Helpers
{
    public static class HelperClass
    {
        public static string FormatArabicTimeInterval(TimeOnly? from, TimeOnly? to)
        {
            if (from == null || to == null) return null;

            // 1. Get the time with AM/PM translated to ص/م
            string timeString = $"{from.Value.ToString("hh:mm tt", new CultureInfo("ar-EG"))} - {to.Value.ToString("hh:mm tt", new CultureInfo("ar-EG"))}";

            // 2. Brute-force the digits to Arabic-Indic
            return timeString
                .Replace("0", "٠").Replace("1", "١").Replace("2", "٢")
                .Replace("3", "٣").Replace("4", "٤").Replace("5", "٥")
                .Replace("6", "٦").Replace("7", "٧").Replace("8", "٨")
                .Replace("9", "٩");
        }
    }
}