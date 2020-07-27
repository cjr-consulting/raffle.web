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

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    [Route("/admin/raffleorder")]
    public class AdminRaffleOrdersController : Controller
    {
        readonly IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult> getRaffleOrdersQueryHandler;
        readonly IQueryHandler<GetRaffleOrderQuery, RaffleOrder> getRaffleOrderQueryHandler;
        readonly IRaffleItemRepository raffleItemRepository;
        readonly ICommandHandler<UpdateOrderTicketCommand> updateOrderTicketCommandHandler;

        public AdminRaffleOrdersController(
            IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult> getRaffleOrdersQueryHandler,
            IQueryHandler<GetRaffleOrderQuery, RaffleOrder> getRaffleOrderQueryHandler,
            ICommandHandler<UpdateOrderTicketCommand> updateOrderTicketCommandHandler,
            IRaffleItemRepository raffleItemRepository
            )
        {
            this.updateOrderTicketCommandHandler = updateOrderTicketCommandHandler;
            this.raffleItemRepository = raffleItemRepository;
            this.getRaffleOrderQueryHandler = getRaffleOrderQueryHandler;
            this.getRaffleOrdersQueryHandler = getRaffleOrdersQueryHandler;
        }

        public IActionResult Index(string sortBy)
        {
            if(string.IsNullOrEmpty(sortBy))
            {
                sortBy = "completed";
            }

            ViewData["nameSortParam"] = sortBy == "name" ? "name_desc" : "name";
            ViewData["emailSortParam"] = sortBy == "email" ? "email_desc" : "email";
            ViewData["completedSortParam"] = sortBy == "completed" ? "completed_desc" : "completed";

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
                })
                .ToList()
            };

            switch (sortBy)
            {
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
        public IActionResult Update(int id)
        {
            var items = raffleItemRepository.GetAll();
            var order = getRaffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = id });
            var model = MapOrderModel(id, items, order);
            return View("RaffleOrderUpdate", model);
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
                CompleteDate = order.CompletedDate.Value,
                UpdateDate = order.UpdatedDate,
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
                Zip = order.Customer.Zip
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

        [HttpPost("update/{id}")]
        public IActionResult Update(int id, RaffleOrderUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new UpdateOrderTicketCommand
                {
                    OrderId = id,
                    TicketNumber = model.TicketNumber
                };
                updateOrderTicketCommandHandler.Handle(command);

                return RedirectToAction("Index", "AdminRaffleOrders");
            }

            var items = raffleItemRepository.GetAll();
            var order = getRaffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = id });
            var newModel = MapOrderModel(id, items, order);
            newModel.TicketNumber = model.TicketNumber;
            return View("RaffleOrderUpdate", newModel);
        }
    }
}
