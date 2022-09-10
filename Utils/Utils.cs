using System;

namespace HK_Rando_4_Log_Display.Utils
{
    public class Utils
    {
        public static string GetAgeInMinutes(DateTime referenceTime, DateTime time)
        {
            if ((int)(referenceTime - time).TotalDays > 10)
                return $"A long time ago";

            var totalMinutes = (int)(referenceTime - time).TotalMinutes;

            if (totalMinutes <= 0)
                return "*new*";

            if (totalMinutes == 1)
                return "1 min ago";

            if (totalMinutes < 60)
                return $"{totalMinutes} mins ago";

            var totalHours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            var minutesString = minutes == 1 ? "1 min" : $"{minutes} mins";

            if (totalHours == 1)
                return $"1 hr ${minutesString} ago";

            if (totalHours < 24)
                return $"{totalHours} hrs ${minutesString} ago";

            var totalDays = totalHours / 24;
            var hours = totalHours % 24;

            var hourMinutesString = $"{hours}:{minutes:00} hrs";

            return totalDays == 1
                ? $"1 day ${hourMinutesString} ago"
                : $"{totalDays} days {hourMinutesString} ago";
        }
    }
}
