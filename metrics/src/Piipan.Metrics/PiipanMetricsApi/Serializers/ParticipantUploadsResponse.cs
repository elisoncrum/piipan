using System;
using System.Collections.Generic;
using Piipan.Metrics.Models;

namespace Piipan.Metrics.Api
{
    namespace Serializers
    {
        public class ParticipantUploadsResponse : Response
        {
            public Meta meta;
            public List<ParticipantUpload> data;
            public ParticipantUploadsResponse(
                List<ParticipantUpload> responseData,
                Meta _meta)
            {
                data = responseData;
                meta = _meta;
            }
        }
    }
}
