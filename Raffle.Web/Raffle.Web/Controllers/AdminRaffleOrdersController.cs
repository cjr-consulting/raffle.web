using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using Raffle.Core.Commands;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;

using Raffle.Web.Models.Admin.RaffleOrder;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    [Route("/admin/raffleorder")]
    public class AdminRaffleOrdersController : Controller
    {
        readonly IRaffleItemRepository raffleItemRepository;
        readonly IMediator mediator;

        public AdminRaffleOrdersController(
            IMediator mediator,
            IRaffleItemRepository raffleItemRepository
            )
        {
            this.mediator = mediator;
            this.raffleItemRepository = raffleItemRepository;
        }

        public async Task<IActionResult> Index(string sortBy)
        {
            if(string.IsNullOrEmpty(sortBy))
            {
                sortBy = "completed_desc";
            }

            ViewData["orderIdSortParam"] = sortBy == "orderId" ? "orderId_desc" : "orderId";
            ViewData["nameSortParam"] = sortBy == "name" ? "name_desc" : "name";
            ViewData["emailSortParam"] = sortBy == "email" ? "email_desc" : "email";
            ViewData["completedSortParam"] = sortBy == "completed" ? "completed_desc" : "completed";

            var orders = (await mediator.Send(new GetRaffleOrdersQuery()));

            var model = new RaffleOrderListViewModel
            {
                RaffleOrders = orders.Orders.Select(x => new RaffleOrderRowModel
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
                })
                .ToList()
            };

            switch (sortBy)
            {
                case "orderId_desc":
                    model.RaffleOrders = model.RaffleOrders.OrderByDescending(x => x.RaffleOrderId).ToList();
                    break;
                case "orderId":
                    model.RaffleOrders = model.RaffleOrders.OrderBy(x => x.RaffleOrderId).ToList();
                    break;
                case "name_desc":
                    model.RaffleOrders = model.RaffleOrders.OrderByDescending(x => x.Name).ToList();
                    break;
                case "name":
                    model.RaffleOrders = model.RaffleOrders.OrderBy(x => x.Name).ToList();
                    break;
                case "email_desc":
                    model.RaffleOrders = model.RaffleOrders.OrderByDescending(x => x.Email).ToList();
                    break;
                case "email":
                    model.RaffleOrders = model.RaffleOrders.OrderBy(x => x.Email).ToList();
                    break;
                case "completed_desc":
                    model.RaffleOrders = model.RaffleOrders.OrderByDescending(x => x.CompletedDate).ToList();
                    break;
                case "completed":
                    model.RaffleOrders = model.RaffleOrders.OrderBy(x => x.CompletedDate).ToList();
                    break;
            }


            return View("RaffleOrderList", model);
        }

        [HttpGet("add")]
        public IActionResult Add()
        {
            return View("RaffleOrderAdd");
        }

        [HttpGet("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var items = raffleItemRepository.GetAll();
            var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = id }));
            var model = MapOrderModel(id, items, order);
            return View("RaffleOrderUpdate", model);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, RaffleOrderUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new UpdateOrderTicketCommand
                {
                    OrderId = id,
                    TicketNumber = model.TicketNumber,
                    DonationDate = model.DonationDate,
                    DonationNote = model.DonationNote ?? string.Empty
                };
                await mediator.Publish(command);

                return RedirectToAction("Index", "AdminRaffleOrders");
            }

            var items = raffleItemRepository.GetAll();
            var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = id }));
            var newModel = MapOrderModel(id, items, order);
            newModel.TicketNumber = model.TicketNumber;
            return View("RaffleOrderUpdate", newModel);
        }

        private static RaffleOrderUpdateViewModel MapOrderModel(
            int id,
            IReadOnlyList<RaffleItem> items,
            RaffleOrder order)
        {
            var model = new RaffleOrderUpdateViewModel
            {
                Id = id,
                TicketNumber = order.TicketNumber,
                CompleteDate = order.CompletedDate.Value.ToUniversalTime(),
                Confirmed21 = order.Confirmed21,
                UpdateDate = order.UpdatedDate,
                DonationDate = order.DonationDate,
                DonationNote = order.DonationNote,
                TotalPrice = order.TotalPrice,
                TotalTickets = order.TotalTickets
            };
            model.Customer = new RaffleOrderUpdateCustomer
            {
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Email = order.Customer.Email,
                PhoneNumber = order.Customer.PhoneNumber,
                AddressLine1 = order.Customer.AddressLine1,
                AddressLine2 = order.Customer.AddressLine2,
                City = order.Customer.City,
                State = order.Customer.State,
                Zip = order.Customer.Zip,
                IsInternational = order.Customer.IsInternational,
                InternationalAddress = order.Customer.InternationalAddress
            };
            model.Lines = order.Lines.Select(x =>
            new RaffleOrderUpdateLineModel
            {
                RaffleItemId = x.RaffleItemId,
                Amount = x.Count,
                Price = x.Price,
                Name = x.Name,
                ImageUrls = items.First(i => i.Id == x.RaffleItemId).ImageUrls
            }).ToList();
            return model;
        }

    }
}
