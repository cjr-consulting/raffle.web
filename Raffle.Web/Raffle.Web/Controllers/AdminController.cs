
using Microsoft.AspNetCore.Mvc;


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
    }
}
