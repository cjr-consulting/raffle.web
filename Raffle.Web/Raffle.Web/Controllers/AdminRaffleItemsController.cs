using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Raffle.Web.Controllers
{
    public class AdminRaffleItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
