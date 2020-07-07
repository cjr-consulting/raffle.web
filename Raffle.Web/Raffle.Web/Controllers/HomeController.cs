using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Repositories;
using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        readonly IEmailSender emailSender;
        readonly IRaffleItemRepository raffleItemRepository;
        readonly StartRaffleOrderQueryHandler startOrderCommandHandler;
        readonly GetRaffleOrderQueryHandler raffleOrderQueryHandler;
        readonly CompleteRaffleOrderCommandHandler completeRaffleOrderCommandHandler;

        public HomeController(
            ILogger<HomeController> logger,
            IEmailSender emailSender,
            IRaffleItemRepository raffleItemRepository,
            StartRaffleOrderQueryHandler startOrderCommandHandler,
            GetRaffleOrderQueryHandler getRaffleOrderQueryHandler,
            CompleteRaffleOrderCommandHandler completeRaffleOrderCommandHandler)
        {
            this.completeRaffleOrderCommandHandler = completeRaffleOrderCommandHandler;
            raffleOrderQueryHandler = getRaffleOrderQueryHandler;
            this.startOrderCommandHandler = startOrderCommandHandler;
            this.raffleItemRepository = raffleItemRepository;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var raffleItems = raffleItemRepository.GetAll()
                .Select(x => new RaffleItemModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Category = x.Category,
                    Sponsor = x.Sponsor,
                    Cost = x.Cost,
                    Value = x.ItemValue
                }).ToList();
            var model = new RaffleOrderViewModel
            {
                RaffleItems = raffleItems
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RaffleOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new StartRaffleOrderQuery
                {
                    RaffleOrderItems = model.RaffleItems
                        .Where(x => x.Amount > 0)
                        .Select(x => new StartRaffleOrderQuery.RaffleOrderItem
                        {
                            RaffleItemId = x.Id,
                            Name = x.Title,
                            Price = x.Cost,
                            Count = x.Amount
                        }).ToList()
                };
                var orderId = startOrderCommandHandler.Handle(command);
                return RedirectToAction("CompleteRaffle", new { orderId });
            }

            return View(model);
        }

        [HttpGet("Complete/{orderId}")]
        public IActionResult CompleteRaffle(int orderId)
        {
            var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });
            var model = new CompleteRaffleModel
            {
                TotalPrice = order.TotalPrice,
                TotalTickets = order.TotalTickets,
                Items = order.Lines.Select(x => new CompleteRaffleItemModel
                {
                    Name = x.Name,
                    Cost = x.Price,
                    Amount = x.Count
                }).ToList()
            };

            return base.View(model);
        }

        [HttpPost("Complete/{orderId}")]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteRaffle(int orderId, CompleteRaffleModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new CompleteRaffleOrderCommand
                {
                    OrderId = orderId,
                    FirstName = model.CustomerFirstName,
                    LastName = model.CustomerLastName,
                    Email = model.CustomerEmail,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip
                };

                completeRaffleOrderCommandHandler.Handle(command);
                return RedirectToAction("OrderSuccessful", new { orderId });
            }

            return View(model);
        }

        [HttpGet("OrderSucces/{orderId}")]
        public IActionResult OrderSuccessful(int orderId)
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
