using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Queries;
using Raffle.Web.Models.Admin.RaffleOrder;

namespace Raffle.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly IMediator mediator;

        public OrdersController(IMediator mediator)
        {
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
                CompletedDate = x.CompletedDate.Value.ToUniversalTime()
            }).ToList();
        }
    } 
}
