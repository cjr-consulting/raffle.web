
using Microsoft.AspNetCore.Mvc;

using Raffle.Web.Models.Raffle;

using System;

using System.Linq;

namespace Raffle.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddRaffleItem()
        {
            return View(new RaffleItemAddView());
        }
    }
}
