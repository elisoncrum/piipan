using System;

namespace Piipan.Dashboard
{
    public class ParticipantUpload
    {
        public string State { get; set; }
        public DateTime UploadedAt { get; set; }

        public ParticipantUpload(string state, DateTime uploaded_at)
        {
            State = state;
            UploadedAt = uploaded_at;
        }
    }
}
