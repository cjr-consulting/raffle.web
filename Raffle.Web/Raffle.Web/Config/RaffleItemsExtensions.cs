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
        public static void AddAzureBlobStorageService(this IServiceCollection services)
        {
            services.AddTransient<IStorageService>(x => new AzureBlobStorageService(
                "DefaultEndpointsProtocol=https;AccountName=dfdrafflestorage;AccountKey=OYWYIzGtd34iOybRDSj0YTjz0E2KYRhzefSZ4OFEAvP0PwFhn2ob/tEQQVruJktdTHrawGTjyarrKYlW3rh2OQ==;EndpointSuffix=core.windows.net",
                "testcontiner"));
        }
    }
}
