using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;
using Raffle.Web.Models.Admin.RaffleItem;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Controllers
{
    [Authorize(Policy = "Administration")]
    [Route("/admin/raffleitem")]
    public class AdminRaffleItemsController : Controller
    {
        readonly IRaffleItemRepository raffleItemRepository;
        readonly IMediator mediator;

        public AdminRaffleItemsController(
            IRaffleItemRepository raffleItemRepository,
            IMediator mediator)
        {
            this.mediator = mediator;
            this.raffleItemRepository = raffleItemRepository;
        }

        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var model = (await mediator.Send(new GetAdminRaffleItemsQuery()))
                .RaffleItems
                .Select(x => new RaffleItemModel
                {
                    Id = x.Id,
                    ItemNumber = x.ItemNumber,
                    TotalTicketsEntered = x.TotalTicketsEntered,
                    Title = x.Title,
                    Description = x.Description,
                    Category = x.Category,
                    Sponsor = x.Sponsor,
                    Cost = x.Cost,
                    Value = x.ItemValue,
                    IsAvailable = x.IsAvailable,
                    ForOver21 = x.ForOver21,
                    LocalPickupOnly = x.LocalPickupOnly,
                    NumberOfDraws = x.NumberOfDraws,
                    WinningTickets = x.WinningTickets
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
        public async Task<IActionResult> Add(RaffleItemAddModel model)
        {
            if (ModelState.IsValid)
            {
                StorageFile file = null;
                if (model.ItemImage != null)
                {
                    var ms = new MemoryStream();
                    model.ItemImage.OpenReadStream().CopyTo(ms);
                    file = new StorageFile(
                        model.ItemImage.FileName,
                        ms.ToArray(),
                        "tftraffle");
                }

                await mediator.Publish(new AddRaffleItemCommand
                {
                    ItemNumber = model.ItemNumber,
                    Title = model.Title,
                    Description = model.Description ?? string.Empty,
                    Category = model.Category ?? string.Empty,
                    Sponsor = model.Sponsor ?? string.Empty,
                    Cost = model.Cost,
                    ItemValue = model.ItemValue ?? string.Empty,
                    IsAvailable = model.IsAvailable,
                    ForOver21 = model.ForOver21,
                    LocalPickupOnly = model.LocalPickupOnly,
                    NumberOfDraws = model.NumberOfDraws,
                    Order = model.Order,
                    ImageFile = file
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
                ItemNumber = raffleItem.ItemNumber,
                Title = raffleItem.Title,
                Description = raffleItem.Description,
                Category = raffleItem.Category,
                Sponsor = raffleItem.Sponsor,
                ItemValue = raffleItem.ItemValue,
                Cost = raffleItem.Cost,
                IsAvailable = raffleItem.IsAvailable,
                ForOver21 = raffleItem.ForOver21,
                LocalPickupOnly = raffleItem.LocalPickupOnly,
                NumberOfDraws = raffleItem.NumberOfDraws,
                Order = raffleItem.Order,
                WinningTickets = raffleItem.WinningTickets,
                ImageUrl = raffleItem.ImageUrls.Any() ? raffleItem.ImageUrls.First() : string.Empty
            };
            return View("RaffleItemUpdate", model);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, RaffleItemUpdateModel model)
        {
            var raffleItem = raffleItemRepository.GetById(id);
            if (!ModelState.IsValid)
            {
                model.ImageUrl = raffleItem.ImageUrls.Any() ? raffleItem.ImageUrls.First() : string.Empty;
                return View("RaffleItemUpdate", model);
            }

            StorageFile file = null;
            if (model.ItemImage != null)
            {
                var ms = new MemoryStream();
                model.ItemImage.OpenReadStream().CopyTo(ms);
                file = new StorageFile(
                    model.ItemImage.FileName,
                    ms.ToArray(),
                    "tftraffle");
            }

            var command = new UpdateRaffleItemCommand
            {
                Id = id,
                ItemNumber = model.ItemNumber,
                Title = model.Title,
                Description = model.Description ?? string.Empty,
                Category = model.Category ?? string.Empty,
                Sponsor = model.Sponsor ?? string.Empty,
                Cost = model.Cost,
                ItemValue = model.ItemValue ?? string.Empty,
                IsAvailable = model.IsAvailable,
                ForOver21 = model.ForOver21,
                LocalPickupOnly = model.LocalPickupOnly,
                NumberOfDraws = model.NumberOfDraws,
                Order = model.Order,
                WinningTickets = model.WinningTickets ?? string.Empty,
                ImageFile = file
            };

            var result = await mediator.Send(command);
            if (result.Failed)
            {
                model.ImageUrl = raffleItem.ImageUrls.Any() ? raffleItem.ImageUrls.First() : string.Empty;
                foreach (var errorMessage in result.ErrorMessages)
                {
                    ModelState.AddModelError("WinningTickets", errorMessage);
                }

                return View("RaffleItemUpdate", model);
            }

            return RedirectToAction("Index");
        }
    }
}
