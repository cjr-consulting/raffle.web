
using MediatR;

using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Queries;
using Raffle.Core.Repositories;

using Raffle.Web.Models.Admin.RaffleOrder;

using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly IMediator mediator;
        readonly IRaffleItemRepository raffleItemRepository;

        public OrdersController(IMediator mediator, IRaffleItemRepository raffleItemRepository)
        {
            this.raffleItemRepository = raffleItemRepository;
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<RaffleOrderRowModel>>> Get()
        {
            var ordersResult = await mediator.Send(new GetRaffleOrdersQuery());

            return ordersResult.Orders.Select(x => new RaffleOrderRowModel
            {
                RaffleOrderId = x.Id,
                TicketNumber = x.TicketNumber,
                DonationDate = x.DonationDate,
                Email = x.Customer.Email,
                Name = $"{x.Customer.FirstName} {x.Customer.LastName}",
                TotalPoints = x.TotalPoints,
                TotalTickets = x.TotalTickets,
                StartDate = x.StartDate,
                CompletedDate = x.CompletedDate.Value.ToUniversalTime(),
                HowDidYouHear = x.HowDidYouHear
            }).ToList();
        }

        [HttpGet("raffleitems")]
        public async Task<ActionResult<List<AdminListRaffleItem>>> GetRaffleItems()
        {
            var adminRaffleItems = await mediator.Send(new GetAdminRaffleItemsQuery());
            return adminRaffleItems.RaffleItems.OrderByDescending(x => x.TotalTicketsEntered).ToList();
        }
    }

    public class RaffleItemStatsModel
    {
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public List<RaffleItemTicketModel> Tickets { get; set; }
    }

    public class RaffleItemTicketModel
    {
        public DateTime CompletedDate { get; set; }
        public int Count { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
