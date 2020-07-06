using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Raffle.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        readonly IEmailSender emailSender;
        readonly AddRaffleItemCommandHandler addHandler;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, AddRaffleItemCommandHandler addHandler)
        {
            this.addHandler = addHandler;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var model = new RaffleOrderViewModel
            {
                RaffleItems = new List<RaffleItemModel>
                {
                    new RaffleItemModel
                    {
                        Id = 1,
                        Title = "Title",
                        Description = "Description",
                        Category = "Category",
                        Value = "Value",
                        Cost = 5,
                        IsAvailable = true,
                        Order = 0,
                        Sponsor = "Sponsor"
                    },
                    new RaffleItemModel
                    {
                        Id = 2,
                        Title = "Title2",
                        Description = "Description2",
                        Category = "Category2",
                        Value = "Value2",
                        Cost = 5,
                        IsAvailable = true,
                        Order = 0,
                        Sponsor = "Sponsor"
                    }
                }
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RaffleOrderViewModel model)
        {
            if (ModelState.IsValid)
            {

            }

            return View(model);
        }

        public IActionResult CompleteRaffle()
        {
            return View(new CompleteRaffleView
            {
                TotalPrice = 150,
                TotalTickets = 45,
                Items = new List<CompleteRaffleItem>
                {
                    new CompleteRaffleItem
                    {
                        Name = "First Item",
                        Cost = 5,
                        Amount = 15
                    },
                    new CompleteRaffleItem
                    {
                        Name = "Second Item",
                        Cost = 5,
                        Amount = 15
                    },
                    new CompleteRaffleItem
                    {
                        Name = "Third Item",
                        Cost = 2,
                        Amount = 15
                    }
                }
            });
        }


        public async Task<IActionResult> Privacy()
        {
            addHandler.Handle(new AddRaffleItemCommand
            {
                Title = "Title2",
                Description = "Description2",
                Category = "Category2",
                ItemValue = "Value2",
                ImageUrl = "url",
                Cost = 5,
                IsAvailable = true,
                Order = 0,
                Sponsor = "Sponsor"
            });
            // await emailSender.SendEmailAsync("johnnynibbles@gmail.com", "John Meade", "Test Email", "Test Plain Text", "<strong>Html Text</strong>");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
