using System;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Api;
using System.Threading.Tasks;
using Piipan.Metrics.Core.Builders;

#nullable enable

namespace Piipan.Metrics.Core.Services
{
    public class ParticipantUploadService : IParticipantUploadReaderApi, IParticipantUploadWriterApi
    {
        private readonly IParticipantUploadDao _participantUploadDao;
        private readonly IMetaBuilder _metaBuilder;

        public ParticipantUploadService(IParticipantUploadDao participantUploadDao, IMetaBuilder metaBuilder)
        {
            _participantUploadDao = participantUploadDao;
            _metaBuilder = metaBuilder;
        }

        public async Task<Int64> GetUploadCount(string? state)
        {
            return _participantUploadDao.GetUploadCount(state);
        }

        public async Task<GetParticipantUploadsResponse> GetLatestUploadsByState()
        {
            var uploads = _participantUploadDao.GetLatestUploadsByState();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = await _metaBuilder.Build()
            };
        }

        public async Task<int> AddUpload(string state, DateTime uploadedAt)
        {
            return _participantUploadDao.AddUpload(state, uploadedAt);
        }

        public async Task<GetParticipantUploadsResponse> GetUploads(string? state, int perPage, int page = 0)
        {
            var limit = perPage;
            var offset = perPage * (page - 1);
            var uploads = _participantUploadDao.GetUploads(state, limit, offset);

            var meta = await _metaBuilder
                .SetPage(page)
                .SetPerPage(perPage)
                .SetState(state)
                .Build();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = meta
            };
        }
    }
}