using System;
using Piipan.Shared.Helpers;
using TimeZoneConverter;

namespace Piipan.Dashboard
{
    public class ParticipantUpload
    {
        public string State { get; set; }
        public DateTime UploadedAt { get; set; }
        public TimeZoneInfo TZ = TZConvert.GetTimeZoneInfo("America/New_York");

        public ParticipantUpload(string state, DateTime uploaded_at)
        {
            State = state;
            UploadedAt = uploaded_at;
        }

        public string RelativeUploadedAt()
        {
            return DateFormatters.RelativeTime(DateTime.UtcNow, UploadedAt);
        }

        public string FormattedUploadedAt()
        {
            string time = TimeZoneInfo.ConvertTimeFromUtc(UploadedAt, TZ).ToString("MM/dd/yyyy h:mmtt");
            return time + " EST";
        }
    }
}
