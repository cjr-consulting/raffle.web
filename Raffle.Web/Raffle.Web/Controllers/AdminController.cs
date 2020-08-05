
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Cache;

using System;

using System.Linq;

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    public class AdminController : Controller
    {
        readonly ICacheManager cacheManager;

        public AdminController(ICacheManager cacheManager)
        {
            this.cacheManager = cacheManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Performance()
        {
            return View();
        }

        public IActionResult ClearCache()
        {
            cacheManager.ResetAllCache();
            return View();
        }
    }
}
