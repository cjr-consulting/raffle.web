
using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Commands;

using Raffle.Core.Repositories;

using Raffle.Web.Models.Raffle;

using System;
using System.Linq;

namespace Raffle.Web.Controllers
{
    [Route("/admin/raffleitem")]
    public class AdminRaffleItemsController : Controller
    {
        readonly UpdateRaffleItemCommandHandler updateHandler;
        readonly AddRaffleItemCommandHandler addHandler;
        readonly IRaffleItemRepository raffleItemRepository;

        public AdminRaffleItemsController(
            IRaffleItemRepository raffleItemRepository,
            AddRaffleItemCommandHandler addHandler,
            UpdateRaffleItemCommandHandler updateHandler)
        {
            this.raffleItemRepository = raffleItemRepository;
            this.addHandler = addHandler;
            this.updateHandler = updateHandler;
        }

        [HttpGet()]
        public IActionResult Index()
        {
            var model = raffleItemRepository.GetAll()
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
            return View("RaffleItemList", model);
        }

        [HttpGet("add")]
        public IActionResult Add()
        {
            return View("RaffleItemAdd", new RaffleItemAddModel());
        }

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public IActionResult Add(RaffleItemAddModel model)
        {
            if (ModelState.IsValid)
            {
                addHandler.Handle(new AddRaffleItemCommand
                {
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    Sponsor = model.Sponsor,
                    Cost = model.Cost,
                    ItemValue = model.ItemValue,
                    IsAvailable = model.IsAvailable,
                    Order = model.Order
                });
                return RedirectToAction("Index");
            }

            return View("RaffleItemAdd");
        }

        [HttpGet("{id}")]
        public IActionResult Update(int id)
        {
            var raffleItem = raffleItemRepository.GetById(id);
            var model = new RaffleItemUpdateModel
            {
                Id = id,
                Title = raffleItem.Title,
                Description = raffleItem.Description,
                Category = raffleItem.Category,
                Sponsor = raffleItem.Sponsor,
                ItemValue = raffleItem.ItemValue,
                Cost = raffleItem.Cost,
                IsAvailable = raffleItem.IsAvailable,
                Order = raffleItem.Order
            };
            return View("RaffleItemUpdate", model);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, RaffleItemUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new UpdateRaffleItemCommand
                {
                    Id = id,
                    Title = model.Title,
                    Description = model.Description,
                    ItemValue = model.ItemValue,
                    Category = model.Category,
                    Sponsor = model.Sponsor,
                    Cost = model.Cost,
                    IsAvailable = model.IsAvailable,
                    Order = model.Order
                };
                updateHandler.Handle(command);
                return RedirectToAction("Index");
            }

            return View("RaffleItemUpdate");
        }
    }
}
