using Piipan.Metrics.Api;

namespace Piipan.Metrics.Func.Builders
{
    public interface IMetaBuilder
    {
        Meta Build();
        IMetaBuilder SetPage(int page);
        IMetaBuilder SetPerPage(int perPage);
        IMetaBuilder SetState(string state);
    }
}