using System.Threading.Tasks;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.Builders
{
    public interface IMetaBuilder
    {
        Task<Meta> Build();
        IMetaBuilder SetPage(int page);
        IMetaBuilder SetPerPage(int perPage);
        IMetaBuilder SetState(string state);
    }
}