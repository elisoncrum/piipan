using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Func.Api.Models;
using Newtonsoft.Json;
using Xunit;

namespace Piipan.Match.Func.Api.Tests.Models
{
    public class ParticipantTests
    {
        [Fact]
        public void ParticipantRecordJson()
        {
            // Arrange
            var json = @"{participant_id: 'baz', case_id: 'foo', benefits_end_month: '2020-01', recent_benefit_months: ['2019-12', '2019-11', '2019-10'], protect_location: true}";
            var record = JsonConvert.DeserializeObject<Participant>(json);

            string jsonRecord = record.ToJson();

            Assert.Contains("\"state\": null", jsonRecord);
            Assert.Contains("\"participant_id\": \"baz\"", jsonRecord);
            Assert.Contains("\"case_id\": \"foo\"", jsonRecord);
            Assert.Contains("\"benefits_end_month\": \"2020-01\"", jsonRecord);
            Assert.Contains("\"recent_benefit_months\": [", jsonRecord);
            Assert.Contains("\"2019-12\",", jsonRecord);
            Assert.Contains("\"2019-11\",", jsonRecord);
            Assert.Contains("\"2019-10\"", jsonRecord);
            Assert.Contains("\"protect_location\": true", jsonRecord);
        }
    }
}