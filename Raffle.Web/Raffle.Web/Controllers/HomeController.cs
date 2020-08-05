using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;
using Raffle.Web.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        readonly IRaffleEmailSender emailSender;
        readonly IRaffleItemRepository raffleItemRepository;
        readonly IRaffleEventRepository raffleEventRepository;
        readonly IMediator mediator;

        public HomeController(
            ILogger<HomeController> logger,
            IRaffleEmailSender emailSender,
            IRaffleItemRepository raffleItemRepository,
            IRaffleEventRepository raffleEventRepository,
            IMediator mediator)
        {
            this.mediator = mediator;
            this.raffleEventRepository = raffleEventRepository;
            this.raffleItemRepository = raffleItemRepository;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [HttpPost("/updateorder/updateitem/{raffleItemId}")]
        public async Task<IActionResult> UpdateTicketCount(int raffleItemId, [FromBody] UpdateOrderItemModel model)
        {
            var raffleItems = raffleItemRepository.GetAll().ToList();

            if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies[CookieKeys.RaffleOrderId]);
                var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId }));

                var raffleItem = raffleItems.FirstOrDefault(x => x.Id == raffleItemId);
                if (raffleItem == null)
                    return Json(new { Success = false });

                await mediator.Publish(new UpdateOrderCommand
                {
                    OrderId = orderId,
                    RaffleOrderItems = new List<UpdateOrderCommand.RaffleOrderItem> {
                        new UpdateOrderCommand.RaffleOrderItem
                        {
                            RaffleItemId = raffleItemId,
                            Name = $"#{raffleItem.ItemNumber} {raffleItem.Title}",
                            Price = raffleItem.Cost,
                            Count = model.Amount
                        }
                    }
                });

                return Json(new { Success = true });
            }

            return Json(new { Success = false });
        }

        public async Task<IActionResult> Index(string sortBy, string searchFilter, string categoryFilter)
        {
            int? orderId = null;

            if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
            {
                orderId = int.Parse(HttpContext.Request.Cookies[CookieKeys.RaffleOrderId]);
                if (!await mediator.Send(new RaffleOrderExistsQuery { OrderId = orderId.Value }))
                {
                    orderId = null;
                }
            }

            if(HttpContext.Request.Query["emoji"] == "on")
            {
                ViewData["emoji"] = "on";
            }
            if (HttpContext.Request.Query["menu"] == "on")
            {
                ViewData["graph"] = "on";
            }

            ViewData["itemNumberSortParam"] = sortBy == "itemNumber" ? "itemNumber_desc" : "itemNumber";
            ViewData["titleSortParam"] = sortBy == "title" ? "title_desc" : "title";
            ViewData["categorySortParam"] = sortBy == "category" ? "category_desc" : "category";
            ViewData["pointsSortParam"] = sortBy == "points" ? "points_desc" : "points";
            ViewData["currentFilter"] = searchFilter;
            ViewData["categoryFilter"] = categoryFilter;

            var raffleItems = raffleItemRepository.GetAll()
                .Select(x => new RaffleItemModel
                {
                    Id = x.Id,
                    ItemNumber = x.ItemNumber,
                    Title = x.Title,
                    Description = x.Description,
                    Category = x.Category,
                    Sponsor = x.Sponsor,
                    Cost = x.Cost,
                    Value = x.ItemValue,
                    ForOver21 = x.ForOver21,
                    LocalPickupOnly = x.LocalPickupOnly,
                    NumberOfDraws = x.NumberOfDraws,
                    TotalTicketsEntered = x.TotalTicketsEntered,
                    Pictures = x.ImageUrls
                }).ToList();

            if(orderId.HasValue)
            { 
                var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId.Value }));
                foreach(var line in order.Lines)
                {
                    raffleItems.First(x => x.Id == line.RaffleItemId).Amount = line.Count;
                }
            } 
            else
            {
                orderId = (await mediator.Send(new StartRaffleOrderQuery()));
                HttpContext.Response.Cookies.Append(
                    CookieKeys.RaffleOrderId,
                    orderId.ToString(),
                    new CookieOptions
                    {
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = new DateTimeOffset(DateTime.Now.AddDays(7))
                    });
            }

            if(!string.IsNullOrEmpty(searchFilter))
            {
                raffleItems = raffleItems.Where(x => x.Title.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                    || x.Description.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                    || x.Category.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if(!string.IsNullOrEmpty(categoryFilter))
            {
                raffleItems = raffleItems.Where(x => x.Category.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            switch(sortBy)
            {
                case "title_desc":
                    raffleItems = raffleItems.OrderByDescending(x => x.Title).ToList();
                    break;
                case "title":
                    raffleItems = raffleItems.OrderBy(x => x.Title).ToList();
                    break;
                case "category":
                    raffleItems = raffleItems.OrderBy(x => x.Category).ToList();
                    break;
                case "category_desc":
                    raffleItems = raffleItems.OrderByDescending(x => x.Category).ToList();
                    break;
                case "itemNumber":
                    raffleItems = raffleItems.OrderBy(x => x.ItemNumber).ToList();
                    break;
                case "itemNumber_desc":
                    raffleItems = raffleItems.OrderByDescending(x => x.ItemNumber).ToList();
                    break;
                case "points":
                    raffleItems = raffleItems.OrderBy(x => x.Cost).ToList();
                    break;
                case "points_desc":
                    raffleItems = raffleItems.OrderByDescending(x => x.Cost).ToList();
                    break;
                default:
                    raffleItems = raffleItems.OrderBy(x => x.ItemNumber).ToList();
                    break;
            }

            var raffleEvent = raffleEventRepository.GetById(1);
            var model = new RaffleOrderViewModel
            {
                StartDate = raffleEvent.StartDate,
                CloseDate = raffleEvent.CloseDate.Value.ToUniversalTime(),
                Categories = raffleItemRepository.GetUsedCategories().OrderBy(x => x).ToList(),
                RaffleItems = raffleItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RaffleOrderViewModel model)
        {
            RaffleOrder order = null;
            var raffleEvent = raffleEventRepository.GetById(1);
            if (DateTime.UtcNow >= raffleEvent.CloseDate)
            {
                return RedirectToAction("Index");
            }

            if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies[CookieKeys.RaffleOrderId]);
                order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId }));
            }

            if ((order != null && !order.Lines.Any(x=>x.Count > 0))
                && !model.RaffleItems.Any(x => x.Amount > 0))
            {
                var raffleItems = raffleItemRepository.GetAll()
                .Select(x => new RaffleItemModel
                {
                    Id = x.Id,
                    ItemNumber = x.ItemNumber,
                    Title = x.Title,
                    Description = x.Description,
                    Category = x.Category,
                    Sponsor = x.Sponsor,
                    Cost = x.Cost,
                    Value = x.ItemValue,
                    ForOver21 = x.ForOver21,
                    LocalPickupOnly = x.LocalPickupOnly,
                    NumberOfDraws = x.NumberOfDraws,
                    TotalTicketsEntered = x.TotalTicketsEntered,
                    Pictures = x.ImageUrls
                }).ToList();
                model = new RaffleOrderViewModel
                {
                    StartDate = raffleEvent.StartDate,
                    CloseDate = raffleEvent.CloseDate.Value.ToUniversalTime(),
                    Categories = raffleItemRepository.GetUsedCategories().OrderBy(x => x).ToList(),
                    RaffleItems = raffleItems,
                    ErrorMessage = "A raffle needs to be selected."
                };

                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (order != null)
                {
                    var lineItems = order.Lines.Select(x => new UpdateOrderCommand.RaffleOrderItem
                    {
                        RaffleItemId = x.RaffleItemId,
                        Name = x.Name,
                        Price = x.Price,
                        Count = x.Count
                    }).ToList();
                    var newLineItems = model.RaffleItems
                            .Where(x => x.Amount > 0)
                            .Select(x => new UpdateOrderCommand.RaffleOrderItem
                            {
                                RaffleItemId = x.Id,
                                Name = $"#{x.ItemNumber} {x.Title}",
                                Price = x.Cost,
                                Count = x.Amount
                            }).ToList();

                    lineItems.AddRange(newLineItems.Where(x => !lineItems.Any(y => y.RaffleItemId == x.RaffleItemId)));
                    foreach (var item in lineItems)
                    {
                        var newItem = newLineItems.FirstOrDefault(x => x.RaffleItemId == item.RaffleItemId);
                        if (newItem == null)
                            continue;

                        item.Price = newItem.Price;
                        item.Count = newItem.Count;
                    }

                    var updateOrder = new UpdateOrderCommand
                    {
                        OrderId = order.Id,
                        ReplaceAll = true,
                        RaffleOrderItems = lineItems
                    };
                    await mediator.Publish(updateOrder);
                }
                else
                {
                    var startRaffleOrder = new StartRaffleOrderQuery
                    {
                        RaffleOrderItems = model.RaffleItems
                            .Where(x => x.Amount > 0)
                            .Select(x => new StartRaffleOrderQuery.RaffleOrderItem
                            {
                                RaffleItemId = x.Id,
                                Name = $"#{x.ItemNumber} {x.Title}",
                                Price = x.Cost,
                                Count = x.Amount
                            }).ToList()
                    };

                    var orderId = (await mediator.Send(startRaffleOrder));
                    order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId }));
                }

                HttpContext.Response.Cookies.Append(
                        CookieKeys.RaffleOrderId,
                        order.Id.ToString(),
                        new CookieOptions
                        {
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = new DateTimeOffset(DateTime.Now.AddDays(7))
                        });

                return RedirectToAction("CompleteRaffle", new { orderId = order.Id });
            }

            return View(model);
        }

        [HttpGet("ClearDonation")]
        public async Task<IActionResult> ClearDonation()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies[CookieKeys.RaffleOrderId]);
                var updateOrder = new UpdateOrderCommand
                {
                    OrderId = orderId,
                    ReplaceAll = true,
                };
                await mediator.Publish(updateOrder);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Complete/{orderId}")]
        public async Task<IActionResult> CompleteRaffle(int orderId)
        {
            var raffleEvent = raffleEventRepository.GetById(1);
            if (DateTime.UtcNow >= raffleEvent.CloseDate)
            {
                return RedirectToAction("Index");
            }

            var order = await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId });

            if(order.CompletedDate.HasValue)
            {
                if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
                {
                    HttpContext.Response.Cookies.Delete(CookieKeys.RaffleOrderId);
                }

                return RedirectToAction("DonationSuccessful", new { orderId });
            }

            var raffleItems = raffleItemRepository.GetAll();
            var model = new CompleteRaffleModel
            {
                TotalPrice = order.TotalPrice,
                TotalTickets = order.TotalTickets,
                Items = order.Lines.Select(x => new CompleteRaffleLineItemModel
                {
                    Name = x.Name,
                    Price = x.Price,
                    Amount = x.Count,
                    ImageUrls = raffleItems.First(ri => ri.Id == x.RaffleItemId).ImageUrls
                }).ToList()
            };

            return base.View(model);
        }

        [HttpPost("Complete/{orderId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRaffle(int orderId, CompleteRaffleModel model)
        {
            var raffleEvent = raffleEventRepository.GetById(1);
            if (DateTime.UtcNow >= raffleEvent.CloseDate)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var command = new CompleteRaffleOrderCommand
                {
                    OrderId = orderId,
                    Confirmed21 = model.Confirmed21,
                    FirstName = model.CustomerFirstName.Trim(),
                    LastName = model.CustomerLastName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Email = model.CustomerEmail.Trim(),
                    AddressLine1 = model.AddressLine1.Trim(),
                    AddressLine2 = model.AddressLine2?.Trim(),
                    City = model.City?.Trim(),
                    State = model.State?.Trim(),
                    Zip = model.Zip?.Trim(),
                    IsInternational = model.IsInternational,
                    InternationalAddress = model.InternationalAddress?.Trim() ?? string.Empty,
                    HowDidYouHear = model.HowDidYouHear?.Trim() ?? string.Empty,
                    HowDidYouHearOther = model.HowDidYouHearOther?.Trim() ?? string.Empty
                };

                await mediator.Publish(command);
                HttpContext.Response.Cookies.Delete(CookieKeys.RaffleOrderId);
                return RedirectToAction("DonationSuccessful", new { orderId });
            }

            var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId }));
            var raffleItems = raffleItemRepository.GetAll();
            model.Items = order.Lines.Select(x => new CompleteRaffleLineItemModel
            {
                Name = x.Name,
                Price = x.Price,
                Amount = x.Count,
                ImageUrls = raffleItems.First(ri => ri.Id == x.RaffleItemId).ImageUrls
            }).ToList();
            model.TotalPrice = order.TotalPrice;
            model.TotalTickets = order.TotalTickets;

            return View(model);
        }

        [HttpGet("DonationSuccess/{orderId}")]
        public async Task<IActionResult> DonationSuccessful(int orderId)
        {
            var order = (await mediator.Send(new GetRaffleOrderQuery { OrderId = orderId }));
            var raffleItems = raffleItemRepository.GetAll();

            var model = new SuccessDonationViewModel
            {
                CustomerEmail = order.Customer.Email,
                Confirmed21 = order.Confirmed21,
                CustomerFirstName = order.Customer.FirstName,
                CustomerLastName = order.Customer.LastName,
                PhoneNumber = order.Customer.PhoneNumber,
                AddressLine1 = order.Customer.AddressLine1,
                AddressLine2 = order.Customer.AddressLine2,
                City = order.Customer.City,
                State = order.Customer.State,
                Zip = order.Customer.Zip,
                IsInternational = order.Customer.IsInternational,
                InternationalAddress = order.Customer.InternationalAddress,
                TicketNumber = order.TicketNumber,
                TotalPrice = order.TotalPrice,
                TotalTickets = order.TotalTickets,
                Items = order.Lines.Select(x=>
                    new SuccessDonationLineItemModel
                    {
                        Name = x.Name,
                        Amount = x.Count,
                        Price= x.Price,
                        ImageUrls = raffleItems.First(ri => ri.Id == x.RaffleItemId).ImageUrls
                    }).ToList()
            };

            if (HttpContext.Request.Cookies.ContainsKey(CookieKeys.RaffleOrderId))
            {
                HttpContext.Response.Cookies.Delete(CookieKeys.RaffleOrderId);
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

        [HttpGet("HowManyTickets")]
        public IActionResult HowManyTickets()
        {
            return View();
        }
    }
}
