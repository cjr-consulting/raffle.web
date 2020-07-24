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
        public static void AddRaffleItem(this IServiceCollection services, EmailAddress managerEmail)
        {
            services.AddScoped<ICommandHandler<AddRaffleItemCommand>, AddRaffleItemCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateRaffleItemCommand>, UpdateRaffleItemCommandHandler>();
            services.AddScoped<IQueryHandler<GetRaffleOrderQuery, RaffleOrder>, GetRaffleOrderQueryHandler>();

            services.AddScoped<ICommandHandler<UpdateOrderCommand>, UpdateOrderCommandHandler>();
            services.AddScoped<IQueryHandler<StartRaffleOrderQuery, int>, StartRaffleOrderQueryHandler>();
            services.AddScoped<ICommandHandler<CompleteRaffleOrderCommand>>(services =>
                {
                    return new CompleteRaffleOrderCommandHandler(
                                services.GetService<RaffleDbConfiguration>(),
                                services.GetService<IRaffleEmailSender>(),
                                services.GetService<EmbeddedResourceReader>(),
                                managerEmail);
                });

            services.AddScoped<IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult>, GetRaffleOrdersQueryHandler>();

            services.AddScoped<IRaffleItemRepository, RaffleItemRepository>();
            services.AddScoped<IRaffleEventRepository, RaffleEventRepository>();
        }
    }
}
