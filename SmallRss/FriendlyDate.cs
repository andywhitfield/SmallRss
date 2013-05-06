using System;

namespace SmallRss
{
    public static class FriendlyDate
    {
        public static string ToString(DateTime? articleDate, int? utcOffset)
        {
            var format = "dd-MMM-yyyy HH:mm";

            if (!articleDate.HasValue)
                articleDate = DateTime.UtcNow;

            var age = DateTime.UtcNow - articleDate;

            if (age < TimeSpan.FromDays(1) && DateTime.UtcNow.Day == articleDate.Value.Day)
                format = "HH:mm";
            else if (age < TimeSpan.FromDays(5))
                format = "ddd HH:mm";
            else if (age < TimeSpan.FromDays(100))
                format = "dd-MMM HH:mm";

            if (utcOffset.HasValue)
                articleDate = articleDate.Value.AddMinutes(-utcOffset.Value);

            return articleDate.Value.ToString(format);
        }
    }
}
