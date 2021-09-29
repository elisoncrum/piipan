using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Piipan.Match.Func.Api.Parsers
{
    public class OrchMatchRequestParser : IStreamParser<OrchMatchRequest>
    {
        private readonly IValidator<OrchMatchRequest> _validator;
        private readonly ILogger<OrchMatchRequestParser> _logger;
        
        public OrchMatchRequestParser(
            IValidator<OrchMatchRequest> validator,
            ILogger<OrchMatchRequestParser> logger)
        {
            _validator = validator;
            _logger = logger;
        }
        
        public async Task<OrchMatchRequest> Parse(Stream stream)
        {
            try
            {
                OrchMatchRequest request = null;

                var reader = new StreamReader(stream);
                var serialized = await reader.ReadToEndAsync();

                request = JsonConvert.DeserializeObject<OrchMatchRequest>(serialized);

                if (request is null)
                {
                    throw new JsonSerializationException("stream must not be empty.");
                }
                
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException("request validation failed", validationResult.Errors);
                }
                
                return request;
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Found validation errors while parsing: {ex.Errors.ToString()}");
                throw ex;
            }
            catch (Exception ex)
            {
                throw new StreamParserException(ex.Message, ex);
            }
        }
    }
}