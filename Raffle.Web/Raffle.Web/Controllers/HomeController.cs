using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raffle.Core;
using Raffle.Core.Commands;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Core.Repositories;
using Raffle.Core.Shared;
using Raffle.Web.Models;
using Raffle.Web.Models.Raffle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Raffle.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        readonly IRaffleEmailSender emailSender;
        readonly IRaffleItemRepository raffleItemRepository;
        readonly IQueryHandler<StartRaffleOrderQuery, int> startOrderCommandHandler;
        readonly IQueryHandler<GetRaffleOrderQuery, RaffleOrder> raffleOrderQueryHandler;
        readonly ICommandHandler<CompleteRaffleOrderCommand> completeRaffleOrderCommandHandler;
        readonly ICommandHandler<UpdateOrderCommand> updateOrderCommandHandler;

        public HomeController(
            ILogger<HomeController> logger,
            IRaffleEmailSender emailSender,
            IRaffleItemRepository raffleItemRepository,
            IQueryHandler<StartRaffleOrderQuery, int> startOrderCommandHandler,
            IQueryHandler<GetRaffleOrderQuery, RaffleOrder> getRaffleOrderQueryHandler,
            ICommandHandler<CompleteRaffleOrderCommand> completeRaffleOrderCommandHandler,
            ICommandHandler<UpdateOrderCommand> updateOrderCommandHandler)
        {
            this.updateOrderCommandHandler = updateOrderCommandHandler;
            this.completeRaffleOrderCommandHandler = completeRaffleOrderCommandHandler;
            raffleOrderQueryHandler = getRaffleOrderQueryHandler;
            this.startOrderCommandHandler = startOrderCommandHandler;
            this.raffleItemRepository = raffleItemRepository;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [HttpPost("/updateorder/updateitem/{raffleItemId}")]
        public IActionResult UpdateTicketCount(int raffleItemId, [FromForm] UpdateOrderItemModel model)
        {
            var raffleItems = raffleItemRepository.GetAll().ToList();

            if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies["dfdoid"]);
                var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });

                var raffleItem = raffleItems.FirstOrDefault(x => x.Id == raffleItemId);
                if (raffleItem == null)
                    return Json(new { Success = false });

                updateOrderCommandHandler.Handle(new UpdateOrderCommand
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

        public IActionResult Index(string sortBy, string searchFilter, string categoryFilter)
        {
            int? orderId = null;

            if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
            {
                orderId = int.Parse(HttpContext.Request.Cookies["dfdoid"]);
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
                    Pictures = x.ImageUrls
                }).ToList();

            if(orderId.HasValue)
            { 
                var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId.Value });
                foreach(var line in order.Lines)
                {
                    raffleItems.First(x => x.Id == line.RaffleItemId).Amount = line.Count;
                }
            } 
            else
            {
                orderId = startOrderCommandHandler.Handle(new StartRaffleOrderQuery());
                HttpContext.Response.Cookies.Append(
                    "dfdoid",
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
            }

            var model = new RaffleOrderViewModel
            {
                Categories = raffleItemRepository.GetUsedCategories().OrderBy(x => x).ToList(),
                RaffleItems = raffleItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RaffleOrderViewModel model)
        {
            if (!model.RaffleItems.Where(x => x.Amount > 0).Any())
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
                    Pictures = x.ImageUrls
                }).ToList();
                model = new RaffleOrderViewModel
                {
                    RaffleItems = raffleItems,
                    ErrorMessage = "A raffle needs to be selected."
                };

                return View(model);
            }

            if (ModelState.IsValid)
            {
                int orderId = 0;
                if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
                {
                    orderId = int.Parse(HttpContext.Request.Cookies["dfdoid"]);
                    var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });

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
                    foreach(var item in lineItems)
                    {
                        var newItem = newLineItems.FirstOrDefault(x => x.RaffleItemId == item.RaffleItemId);
                        if (newItem == null)
                            continue;

                        item.Price = newItem.Price;
                        item.Count = newItem.Count;
                    }

                    var updateOrder = new UpdateOrderCommand
                    {
                        OrderId = orderId,
                        ReplaceAll = true,
                        RaffleOrderItems = lineItems
                    };
                    updateOrderCommandHandler.Handle(updateOrder);
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

                    orderId = startOrderCommandHandler.Handle(startRaffleOrder);
                }

                HttpContext.Response.Cookies.Append(
                        "dfdoid",
                        orderId.ToString(),
                        new CookieOptions
                        {
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = new DateTimeOffset(DateTime.Now.AddDays(7))
                        });

                return RedirectToAction("CompleteRaffle", new { orderId });
            }

            return View(model);
        }

        [HttpGet("ClearDonation")]
        public IActionResult ClearDonation()
        {
            if (HttpContext.Request.Cookies.ContainsKey("dfdoid"))
            {
                var orderId = int.Parse(HttpContext.Request.Cookies["dfdoid"]);
                var updateOrder = new UpdateOrderCommand
                {
                    OrderId = orderId,
                    ReplaceAll = true,
                };
                updateOrderCommandHandler.Handle(updateOrder);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Complete/{orderId}")]
        public IActionResult CompleteRaffle(int orderId)
        {
            var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });

            if(order.CompletedDate.HasValue)
            {
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
        public IActionResult CompleteRaffle(int orderId, CompleteRaffleModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new CompleteRaffleOrderCommand
                {
                    OrderId = orderId,
                    FirstName = model.CustomerFirstName,
                    LastName = model.CustomerLastName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.CustomerEmail,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,

                };

                completeRaffleOrderCommandHandler.Handle(command);
                HttpContext.Response.Cookies.Delete("dfdoid");
                return RedirectToAction("DonationSuccessful", new { orderId });
            }

            var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });
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
        public IActionResult DonationSuccessful(int orderId)
        {
            var order = raffleOrderQueryHandler.Handle(new GetRaffleOrderQuery { OrderId = orderId });
            var raffleItems = raffleItemRepository.GetAll();

            var model = new SuccessDonationViewModel
            {
                CustomerEmail = order.Customer.Email,
                CustomerFirstName = order.Customer.FirstName,
                CustomerLastName = order.Customer.LastName,
                PhoneNumber = order.Customer.PhoneNumber,
                AddressLine1 = order.Customer.AddressLine1,
                AddressLine2 = order.Customer.AddressLine2,
                City = order.Customer.City,
                State = order.Customer.State,
                Zip = order.Customer.Zip,
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
