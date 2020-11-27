using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Raffle.Core;
using Raffle.Core.Cache;
using Raffle.Core.Data;
using Raffle.Core.Repositories;
using Raffle.Web.Services;

namespace Raffle.Web.Config
{
    public static class RaffleItemsExtensions
    {
        public static void AddRaffleItem(this IServiceCollection services)
        {
            services.AddScoped<IRaffleItemRepository, RaffleItemRepository>();
            services.AddScoped<IRaffleEventRepository, RaffleEventRepository>();
        }

        public static void AddRaffleItemCache(this IServiceCollection services)
        {
            services.Decorate<IRaffleItemRepository, CacheRaffleItemRepository>();
            services.Decorate<IRaffleEventRepository, CacheRaffleEventRepository>();
        }
    }

    public static class StorageServiceExtensions
    {
        public static void AddAzureBlobStorageService(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.AddTransient<IStorageService>(x => new AzureBlobStorageService(
                configurationSection.GetValue<string>("AzureBlobStorage:ConnectionString"),
                configurationSection.GetValue<string>("AzureBlobStorage:ContainerName"))
            );
        }
    }
}
