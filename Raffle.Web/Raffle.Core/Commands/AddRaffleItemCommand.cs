using Dapper;

using MediatR;

using Raffle.Core.Events;
using Raffle.Core.Repositories;

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
        public StorageFile ImageFile { get; set; }
    }

    public class AddRaffleItemCommandHandler : INotificationHandler<AddRaffleItemCommand>
    {
        readonly string connectionString;
        readonly IStorageService storageService;
        readonly IRaffleItemRepository repository;
        readonly IMediator mediator;

        public AddRaffleItemCommandHandler(
            RaffleDbConfiguration config,
            IRaffleItemRepository repository,
            IStorageService storageService,
            IMediator mediator)
        {
            this.mediator = mediator;
            this.repository = repository;
            this.storageService = storageService;
            connectionString = config.ConnectionString;
        }

        public async Task Handle(AddRaffleItemCommand notification, CancellationToken cancellationToken)
        {
            const string query = "INSERT INTO [RaffleItems] " +
                "(ItemNumber, Title, Description, ImageUrl, Category, Sponsor, ItemValue, Cost, IsAvailable, ForOver21, LocalPickupOnly, NumberOfDraws) " +
                "VALUES " +
                "(@ItemNumber, @Title, @Description, @ImageUrl, @Category, @Sponsor, @ItemValue, @Cost, @IsAvailable, @ForOver21, @LocalPickupOnly, @NumberOfDraws);" +
                "SELECT SCOPE_IDENTITY();";

            const string insertImage = "INSERT INTO RaffleItemImages(RaffleItemId, ImageRoute, Title) VALUES " +
                "(@RaffleItemId, @ImageRoute, @Title);";

            using (var conn = new SqlConnection(connectionString))
            {
                var raffleItemId = await conn.ExecuteScalarAsync<int>(query, notification);

                var raffleItem = repository.GetById(raffleItemId);
                if (notification.ImageFile != null)
                {
                    var path = await storageService.SaveFile(notification.ImageFile);
                    await conn.ExecuteAsync(insertImage, new { RaffleItemId = raffleItemId, ImageRoute = path, Title = notification.Title });
                }

                await mediator.Publish(new RaffleItemUpdated { RaffleItem = raffleItem }, cancellationToken);
            }
        }
    }
}
