using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Core.Builders
{
    public class MetaBuilder : IMetaBuilder
    {
        private Meta _meta = new Meta();
        private string? _state;
        private readonly IParticipantUploadReaderApi _participantUploadReaderApi;

        public MetaBuilder(IParticipantUploadReaderApi participantUploadReaderApi)
        {
            _participantUploadReaderApi = participantUploadReaderApi;
        }

        public async Task<Meta> Build()
        {
            _meta.Total = await _participantUploadReaderApi.GetUploadCount(_state);
            SetPrevPage(_state);
            SetNextPage(_state);
            return _meta;
        }

        public IMetaBuilder SetPage(int page)
        {
            _meta.Page = page;
            return this;
        }

        public IMetaBuilder SetPerPage(int perPage)
        {
            _meta.PerPage = perPage;
            return this;
        }

        public IMetaBuilder SetState(string? state)
        {
            _state = state;
            return this;
        }

        private void SetPrevPage(string? state)
        {
            var newPage = _meta.Page - 1;
            if (newPage <= 0) return;

            string result = "";
            if (!String.IsNullOrEmpty(state))
                result = QueryHelpers.AddQueryString(result, "state", state);
            result = QueryHelpers.AddQueryString(result, "page", newPage.ToString());
            result = QueryHelpers.AddQueryString(result, "perPage", _meta.PerPage.ToString());
            _meta.PrevPage = result;
        }

        private void SetNextPage(string? state)
        {
            string result = "";
            int nextPage = _meta.Page + 1;
            // if there are next pages to be had
            if (_meta.Total > (_meta.Page * _meta.PerPage))
            {
                if (!String.IsNullOrEmpty(state))
                    result = QueryHelpers.AddQueryString(result, "state", state);
                result = QueryHelpers.AddQueryString(result, "page", nextPage.ToString());
                result = QueryHelpers.AddQueryString(result, "perPage", _meta.PerPage.ToString());
            }
            _meta.NextPage = result;
        }
    }
}