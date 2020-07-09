using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Repositories;
using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;
using System;
using System.Diagnostics;
using System.Linq;

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

            if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies["dfdoid"]);
                var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });
                foreach(var line in order.Lines)
                {
                    raffleItems.First(x => x.Id == line.RaffleItemId).Amount = line.Count;
                }
            }

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
            if (!model.RaffleItems.Where(x => x.Amount > 0).Any())
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
                model = new RaffleOrderViewModel
                {
                    RaffleItems = raffleItems,
                    ErrorMessage = "A raffle needs to be selected."
                };

                return View(model);
            }

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
                HttpContext.Response.Cookies.Append("dfdoid", orderId.ToString(), new CookieOptions
                    {
                        Secure = true,
                        Expires = new DateTimeOffset(DateTime.Now.AddDays(7))
                    });
                return RedirectToAction("CompleteRaffle", new { orderId });
            }

            return View(model);
        }

        [HttpGet("ClearDonation")]
        public IActionResult ClearDonation()
        {
            if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
            {
                HttpContext.Response.Cookies.Delete("dfdoid");
            }

            return RedirectToAction("Index");
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
                    Zip = model.Zip,

                };

                completeRaffleOrderCommandHandler.Handle(command);
                return RedirectToAction("DonationSuccessful", new { orderId });
            }

            var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });
            model.Items = order.Lines.Select(x => new CompleteRaffleItemModel
            {
                Name = x.Name,
                Cost = x.Price,
                Amount = x.Count
            }).ToList();
            model.TotalPrice = order.TotalPrice;
            model.TotalTickets = order.TotalTickets;

            return View(model);
        }

        [HttpGet("DonationSuccess/{orderId}")]
        public IActionResult DonationSuccessful(int orderId)
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
