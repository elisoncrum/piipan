using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Shared.Helpers;
using TimeZoneConverter;

namespace Piipan.Dashboard.Extensions
{
    public static class ParticipantUploadExtensions
    {
        public static string RelativeUploadedAt(this Piipan.Metrics.Api.ParticipantUpload participantUpload)
        {
            return DateFormatters.RelativeTime(DateTime.UtcNow, participantUpload.UploadedAt);
        }

        public static string FormattedUploadedAt(this Piipan.Metrics.Api.ParticipantUpload participantUpload)
        {
            var tz = TZConvert.GetTimeZoneInfo("America/New_York");
            string time = TimeZoneInfo.ConvertTimeFromUtc(participantUpload.UploadedAt, tz).ToString("MM/dd/yyyy h:mmtt");
            return time + " EST";
        }
    }
}