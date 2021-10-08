using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Authentication
{
    public class AzureTokenProvider<T> : ITokenProvider<T>
    {
        private readonly TokenCredential _tokenCredential;
        private readonly AzureTokenProviderOptions<T> _options;
        private readonly ILogger<AzureTokenProvider<T>> _logger;

        public AzureTokenProvider(TokenCredential tokenCredential,
            IOptions<AzureTokenProviderOptions<T>> options,
            ILogger<AzureTokenProvider<T>> logger)
        {
            _tokenCredential = tokenCredential;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> RetrieveAsync()
        {
            _logger.LogInformation(_options.ResourceUri);
            var context = new TokenRequestContext(new[] { _options.ResourceUri });
            var token = await _tokenCredential.GetTokenAsync(context, default(CancellationToken));

            return token.Token;
        }
    }
}