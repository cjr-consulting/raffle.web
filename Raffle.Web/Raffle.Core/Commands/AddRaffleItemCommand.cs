using Dapper;
using MediatR;
using Raffle.Core.Events;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class AddRaffleItemCommand : INotification
    {
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
        public int NumberOfDraws { get; set; } = 1;
    }

    public class AddRaffleItemCommandHandler : INotificationHandler<AddRaffleItemCommand>
    {
        readonly string connectionString;
        readonly IMediator mediator;

        public AddRaffleItemCommandHandler(RaffleDbConfiguration config, IMediator mediator)
        {
            this.mediator = mediator;
            connectionString = config.ConnectionString;
        }

        public async Task Handle(AddRaffleItemCommand notification, CancellationToken cancellationToken)
        {
            const string query = "INSERT INTO [RaffleItems] " +
                "(ItemNumber, Title, Description, ImageUrl, Category, Sponsor, ItemValue, Cost, IsAvailable, ForOver21, LocalPickupOnly, NumberOfDraws) " +
                "VALUES " +
                "(@ItemNumber, @Title, @Description, @ImageUrl, @Category, @Sponsor, @ItemValue, @Cost, @IsAvailable, @ForOver21, @LocalPickupOnly, @NumberOfDraws)";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(query, notification);
            }

            await mediator.Publish(new RaffleItemUpdated(), cancellationToken);
        }
    }
}
