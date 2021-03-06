using Microsoft.Extensions.DependencyInjection;

using Raffle.Core.Cache;
using Raffle.Core.Data;
using Raffle.Core.Repositories;

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
}
