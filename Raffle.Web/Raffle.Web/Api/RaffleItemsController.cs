using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using EllipticCurve;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;

using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Operators;

using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;

namespace Raffle.Web.Api
{
    [Route("api/[controller]")] 
    [ApiController]
    public class RaffleItemsController : ControllerBase
    {
        readonly IRaffleItemRepository repository;
        readonly IMediator mediator;

        public RaffleItemsController(IMediator mediator, IRaffleItemRepository repository)
        {
            this.mediator = mediator;
            this.repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<RaffleItemDuringRunModel>>> Get([FromQuery] RaffleItemRequest request)
        {
            var raffleItems = repository.GetAll();

            var items = raffleItems
                .Where(x => !request.HasTicketDrawn || (request.HasTicketDrawn && x.HasBeenDrawn))
                .Select(RaffleItemDuringRunMapper.Map)
                .ToList();


            var ordersResult = await mediator.Send(new GetRaffleOrdersQuery());
            var possibleWinners = ordersResult.Orders.Select(x =>
                new
                {
                    Name = $"{x.Customer.FirstName} {x.Customer.LastName}",
                    Tickets = GetOrderTickets(x.TicketNumber)
                });

            foreach (var item in items)
            {
                foreach (var winningTicket in item.WinningTickets)
                {
                    var winner = possibleWinners.Where(x => x.Tickets.Contains(winningTicket.Number)).FirstOrDefault();
                    item.Winners.Add(new Winner { Name = winner.Name, For = winningTicket.For, TicketNumber = winningTicket.Number });
                }
            }

            return items;
        }
        private List<string> GetOrderTickets(string orderTickets)
        {
            if (orderTickets == null || string.IsNullOrEmpty(orderTickets))
                return new List<string>();

            var phase1 = orderTickets.Split('(')[0].Trim();
            var tickets = phase1.Split(",", StringSplitOptions.RemoveEmptyEntries);

            return tickets.Select(x => x.Trim()).ToList();
        }
    }

    public class RaffleItemRequest
    {
        public bool HasTicketDrawn { get; set; } = true;
    }

    public class RaffleItemDuringRunModel
    {
        public int Id { get; set; }
        public string Image { get;set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Sponsor { get; set; }
        public string ItemValue { get; set; }
        public List<Ticket> WinningTickets { get; set; } = new List<Ticket>();
        public List<Winner> Winners { get; set; } = new List<Winner>();
        public DateTime UpdatedDate { get; set; }
    }

    public class Ticket
    {
        public string Type { get; set; }
        public string Number { get; set; }
        public string For { get; set; }
    }

    public class Winner
    {
        public string TicketNumber { get; set; }
        public string Name { get; set; }
        public string For { get; set; }
    }
}
