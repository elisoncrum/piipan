using System;
using System.Collections.Generic;
using Piipan.Metrics.Models;

namespace Piipan.Metrics.Api
{
    namespace Serializers
    {
        public class ParticipantUploadsResponse : Response
        {
            public IEnumerable<ParticipantUpload> data;
            public ParticipantUploadsResponse(
                IEnumerable<ParticipantUpload> responseData,
                Meta _meta)
            {
                data = responseData;
                meta = _meta;
            }
        }
    }
}
