using Dapper;

using MediatR;

using Raffle.Core.Events;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class UpdateRaffleItemCommand : IRequest<Result>
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

    public class UpdateRaffleItemCommandHandler : IRequestHandler<UpdateRaffleItemCommand, Result>
    {
        readonly string connectionString;
        readonly IMediator mediator;
        readonly IRaffleItemRepository repository;
        readonly IStorageService storageService;

        List<string> errorMessages = new List<string>();

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

        public async Task<Result> Handle(UpdateRaffleItemCommand request, CancellationToken cancellationToken)
        {
            var existingRafflItem = repository.GetById(request.Id);
            if (NoChange(existingRafflItem, request))
            {
                return Result.Valid();
            }

            var ordersResult = await mediator.Send(new GetRaffleOrdersQuery());
            var raffleTickets = ordersResult
                .Orders
                .SelectMany(x => SplitTicketString(x.TicketNumber))
                .Where(x => x != null);

            if (InvalidTicketNumbers(request.WinningTickets, raffleTickets.ToList()))
            {
                return Result.Fail(errorMessages);
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
                await conn.ExecuteAsync(query, request);

                if (request.ImageFile != null)
                {
                    var path = await storageService.SaveFile(request.ImageFile);
                    await conn.ExecuteAsync(replaceImage, new { request.Id, ImageRoute = path, Title = request.Title });
                }

                var raffleItem = repository.GetById(request.Id);
                await mediator.Publish(new RaffleItemUpdated { RaffleItem = raffleItem }, cancellationToken);
            }

            return Result.Valid();
        }

        private bool NoChange(RaffleItem raffleItem, UpdateRaffleItemCommand notification)
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

        bool InvalidTicketNumbers(string winningTickets, List<string> orderTickets)
        {
            bool hasErrors = false;
            var tickets = SplitTicketString(winningTickets);

            foreach(var ticket in tickets)
            {
                if(!int.TryParse(ticket, out var ticketNum))
                {
                    errorMessages.Add($"Invalid number in ticket [{ticket}]");
                    hasErrors = true;
                    continue;
                }

                if(!orderTickets.Any(x=> x == ticket))
                {
                    errorMessages.Add($"Ticket number [{ticket}] wasn't used on an order");
                    hasErrors = true;
                    continue;
                }
            }

            return hasErrors;
        }

        IReadOnlyList<string> SplitTicketString(string ticketString)
        {
            if(ticketString == null)
            {
                return new List<string>();
            }

            return ticketString
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
        }
    }
}
