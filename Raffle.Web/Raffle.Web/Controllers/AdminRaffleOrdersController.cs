using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Queries;
using Raffle.Core.Shared;
using Raffle.Web.Models.Admin.RaffleOrder;

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    [Route("/admin/raffleorder")]
    public class AdminRaffleOrdersController : Controller
    {
        readonly IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult> getRaffleOrdersQueryHandler;
        public AdminRaffleOrdersController(
            IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult> getRaffleOrdersQueryHandler
            )
        {
            this.getRaffleOrdersQueryHandler = getRaffleOrdersQueryHandler;
        }

        public IActionResult Index()
        {
            var orders = getRaffleOrdersQueryHandler.Handle(new GetRaffleOrdersQuery());
            var model = new RaffleOrderListViewModel
            {
                RaffleOrders = orders.Orders.Select(x => new RaffleOrderRowModel
                {
                    RaffleOrderId = x.Id,
                    TicketNumber = x.TicketNumber,
                    Email = x.Customer.Email,
                    Name = $"{x.Customer.FirstName} {x.Customer.LastName}",
                    TotalPoints = x.TotalPoints,
                    TotalTickets = x.TotalTickets,
                    StartDate = x.StartDate,
                    CompletedDate = x.CompletedDate
                }).ToList()
            };

            return View("RaffleOrderList", model);
        }

        [HttpGet("add")]
        public IActionResult Add()
        {
            return View("RaffleOrderAdd");
        }

        [HttpGet("update")]
        public IActionResult Update()
        {
            return View("RaffleOrderUpdate");
        }
    }
}
