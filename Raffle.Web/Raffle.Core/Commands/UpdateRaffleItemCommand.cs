using Dapper;

using MediatR;

using Raffle.Core.Events;
using Raffle.Core.Models;
using Raffle.Core.Repositories;

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class UpdateRaffleItemCommand : INotification
    {
        public int Id { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; } = string.Empty;
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool ForOver21 { get; set; } = true;
        public bool LocalPickupOnly { get; set; } = true;
        public int NumberOfDraws { get; set; } = 1;
        public string WinningTickets { get; set; } = string.Empty;
        public StorageFile ImageFile { get; set; }
    }

    public class UpdateRaffleItemCommandHandler : INotificationHandler<UpdateRaffleItemCommand>
    {
        readonly string connectionString;
        readonly IMediator mediator;
        readonly IRaffleItemRepository repository;
        readonly IStorageService storageService;

        public UpdateRaffleItemCommandHandler(
            RaffleDbConfiguration config,
            IRaffleItemRepository repository,
            IStorageService storageService,
            IMediator mediator)
        {
            this.storageService = storageService;
            this.repository = repository;
            this.mediator = mediator;
            connectionString = config.ConnectionString;
        }

        public async Task Handle(UpdateRaffleItemCommand notification, CancellationToken cancellationToken)
        {
            var existingRafflItem = repository.GetById(notification.Id);
            if(NoChange(existingRafflItem, notification))
            {
                return;
            }

            const string query = "UPDATE [RaffleItems] SET" +
                " ItemNumber = @ItemNumber, " +
                " Title = @Title," +
                " Description = @Description," +
                " ImageUrl = @ImageUrl," +
                " Category = @Category," +
                " Sponsor = @Sponsor," +
                " ItemValue = @ItemValue," +
                " Cost = @Cost," +
                " IsAvailable = @IsAvailable, " +
                " ForOver21 = @ForOver21, " +
                " LocalPickupOnly = @LocalPickupOnly," +
                " NumberOfDraws = @NumberOfDraws," +
                " WinningTickets = @WinningTickets, " +
                " UpdatedDate = GETUTCDATE() " +
                "WHERE Id = @Id";

            const string replaceImage = "DELETE FROM RaffleItemImages WHERE RaffleItemId = @Id; " +
                "INSERT INTO RaffleItemImages(RaffleItemId, ImageRoute, Title) VALUES " +
                "(@Id, @ImageRoute, @Title);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(query, notification);

                if(notification.ImageFile != null)
                {
                    var path = await storageService.SaveFile(notification.ImageFile);
                    await conn.ExecuteAsync(replaceImage, new { notification.Id, ImageRoute = path, Title = notification.Title });
                }

                var raffleItem = repository.GetById(notification.Id);
                await mediator.Publish(new RaffleItemUpdated { RaffleItem = raffleItem }, cancellationToken);
            }
        }

        bool NoChange(RaffleItem raffleItem, UpdateRaffleItemCommand notification)
        {
            return raffleItem.ItemNumber == notification.ItemNumber
                && raffleItem.Title == notification.Title
                && raffleItem.Description == notification.Description
                && raffleItem.Category == notification.Category
                && raffleItem.Order == notification.Order
                && raffleItem.ItemValue == notification.ItemValue
                && raffleItem.Sponsor == notification.Sponsor
                && raffleItem.Cost == notification.Cost
                && raffleItem.IsAvailable == notification.IsAvailable
                && raffleItem.ForOver21 == notification.ForOver21
                && raffleItem.LocalPickupOnly == notification.LocalPickupOnly
                && raffleItem.NumberOfDraws == notification.NumberOfDraws
                && raffleItem.WinningTickets == notification.WinningTickets
                && notification.ImageFile == null;
        }

    }
}
