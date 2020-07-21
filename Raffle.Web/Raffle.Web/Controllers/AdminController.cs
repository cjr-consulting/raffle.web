
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using System;

using System.Linq;

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
