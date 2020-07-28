using Microsoft.Extensions.DependencyInjection;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Data;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;

namespace Raffle.Web.Config
{
    public static class RaffleItemsExtensions
    {
        public static void AddRaffleItem(this IServiceCollection services)
        {
            services.AddScoped<IRaffleItemRepository, RaffleItemRepository>();
            services.AddScoped<IRaffleEventRepository, RaffleEventRepository>();
        }
    }
}
