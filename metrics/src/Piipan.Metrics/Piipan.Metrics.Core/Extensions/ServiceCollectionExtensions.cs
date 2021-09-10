using Microsoft.Extensions.DependencyInjection;
using Piipan.Metrics.Core.DataAccess;

namespace Piipan.Metrics.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterCoreServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IParticipantUploadDao, ParticipantUploadDao>();
        }
    }
}