using System;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Piipan.Metrics.Core.DataAccess;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Func.Builders
{
    public class MetaBuilder : IMetaBuilder
    {
        private Meta _meta = new Meta();
        private string? _state;
        private readonly IParticipantUploadDao _participantUploadDao;

        public MetaBuilder(IParticipantUploadDao participantUploadDao)
        {
            _participantUploadDao = participantUploadDao;
        }

        public Meta Build()
        {
            _meta.total = _participantUploadDao.GetParticipantUploadCount(_state);
            SetPrevPage(_state);
            SetNextPage(_state);
            return _meta;
        }

        public IMetaBuilder SetPage(int page)
        {
            _meta.page = page;
            return this;
        }

        public IMetaBuilder SetPerPage(int perPage)
        {
            _meta.perPage = perPage;
            return this;
        }

        public IMetaBuilder SetState(string? state)
        {
            _state = state;
            return this;
        }

        private void SetPrevPage(string? state)
        {
            var newPage = _meta.page - 1;
            if (newPage <= 0) return;

            string result = "";
            if (!String.IsNullOrEmpty(state))
                result = QueryHelpers.AddQueryString(result, "state", state);
            result = QueryHelpers.AddQueryString(result, "page", newPage.ToString());
            result = QueryHelpers.AddQueryString(result, "perPage", _meta.perPage.ToString());
            _meta.prevPage = result;
        }

        private void SetNextPage(string? state)
        {
            string result = "";
            int nextPage = _meta.page + 1;
            // if there are next pages to be had
            if (_meta.total >= (_meta.page * _meta.perPage))
            {
                if (!String.IsNullOrEmpty(state))
                    result = QueryHelpers.AddQueryString(result, "state", state);
                result = QueryHelpers.AddQueryString(result, "page", nextPage.ToString());
                result = QueryHelpers.AddQueryString(result, "perPage", _meta.perPage.ToString());
            }
            _meta.nextPage = result;
        }
    }
}