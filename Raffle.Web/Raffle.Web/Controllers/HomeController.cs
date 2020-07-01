using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;

using System.Collections.Generic;
using System.Diagnostics;

namespace Raffle.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
