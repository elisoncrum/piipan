using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Newtonsoft.Json;

namespace Piipan.Match.Func.Api.Parsers
{
    public class OrchMatchRequestParser : IStreamParser<OrchMatchRequest>
    {
        private readonly IValidator<OrchMatchRequest> _validator;
        
        public OrchMatchRequestParser(IValidator<OrchMatchRequest> validator)
        {
            _validator = validator;
        }
        
        public async Task<OrchMatchRequest> Parse(Stream stream)
        {
            var reader = new StreamReader(stream);
            var serialized = await reader.ReadToEndAsync();

            var request = JsonConvert.DeserializeObject<OrchMatchRequest>(serialized);

            if (request is null)
            {
                throw new JsonSerializationException("stream must not be empty.");
            }

            await _validator.ValidateAndThrowAsync(request);

            return request;
        }
    }
}