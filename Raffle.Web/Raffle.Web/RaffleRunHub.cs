using MediatR;

using Microsoft.AspNetCore.SignalR;
using Raffle.Core.Events;
using Raffle.Core.Models;
using Raffle.Core.Queries;
using Raffle.Web.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Web
{
    public class RaffleRunHub : Hub<IRaffleRunHub>
    {
        public Task RaffleItemUpdated(RaffleItem raffleItem)
        {
            return Clients.All.RaffleItemUpdated(RaffleItemDuringRunMapper.Map(raffleItem));
        }
    }

    public interface IRaffleRunHub
    {
        Task RaffleItemUpdated(RaffleItemDuringRunModel raffleItem);
    }

    public class NotifyRaffleRunRaffleItemUpdate : INotificationHandler<RaffleItemUpdated>
    {
        readonly IHubContext<RaffleRunHub, IRaffleRunHub> hub;
        readonly IMediator mediator;

        public NotifyRaffleRunRaffleItemUpdate(IHubContext<RaffleRunHub, IRaffleRunHub> hub, IMediator mediator)
        {
            this.mediator = mediator;
            this.hub = hub;
        }

        public async Task Handle(RaffleItemUpdated notification, CancellationToken cancellationToken)
        {
            var raffleItem = RaffleItemDuringRunMapper.Map(notification.RaffleItem);
            var ordersResult = await mediator.Send(new GetRaffleOrdersQuery());
            var possibleWinners = ordersResult.Orders.Select(x =>
                new
                {
                    Name = $"{x.Customer.FirstName} {x.Customer.LastName}",
                    Tickets = GetOrderTickets(x.TicketNumber)
                });

            foreach (var winningTicket in raffleItem.WinningTickets)
            {
                var winner = possibleWinners.Where(x => x.Tickets.Contains(winningTicket.Number)).FirstOrDefault();
                raffleItem.Winners.Add(new Winner { Name = winner.Name, For = winningTicket.For, TicketNumber = winningTicket.Number });
            }

            await hub.Clients.All.RaffleItemUpdated(raffleItem);
        }

        private List<string> GetOrderTickets(string orderTickets)
        {
            if (orderTickets == null || string.IsNullOrEmpty(orderTickets))
                return new List<string>();

            var phase1 = orderTickets.Split('(')[0].Trim();
            var tickets = phase1.Split(",", StringSplitOptions.RemoveEmptyEntries);

            return tickets.Select(x => x.Trim()).ToList();
        }
    }

    public static class RaffleItemDuringRunMapper
    {
        public static RaffleItemDuringRunModel Map(RaffleItem item)
        {
            return new RaffleItemDuringRunModel
            {
                Id = item.Id,
                ItemNumber = item.ItemNumber,
                Image = item.ImageUrls.Any() ? item.ImageUrls.First() : string.Empty,
                Images = item.ImageUrls.Skip(1).ToList(),
                Title = item.Title,
                Description = item.Description,
                Sponsor = item.Sponsor,
                ItemValue = item.ItemValue,
                WinningTickets = GetListOfTickets(item.WinningTickets),
                UpdatedDate = item.UpdatedDate.ToUniversalTime()
            };
        }

        private static List<Ticket> GetListOfTickets(string winningTickets)
        {
            if (winningTickets == null || string.IsNullOrEmpty(winningTickets))
                return new List<Ticket>();

            var phase1 = winningTickets.Split('(')[0].Trim();
            var tickets = phase1.Split(",", StringSplitOptions.RemoveEmptyEntries);

            var result = new List<Ticket>();
            foreach (var ticketNumber in tickets)
            {
                var ticket = new Ticket { Type = "Normal" };
                var number = ticketNumber.Trim().ToLower();
                ticket.Number = number;
                if(!int.TryParse(number, out var ignore))
                {
                    if (number.Contains("t"))
                    {
                        ticket.For = "TV";
                        ticket.Number = number.Replace("t", "");
                    }

                    if (number.Contains("i")) {
                        ticket.For = "iPad";
                        ticket.Number = number.Replace("i", "");
                    }

                    if (number.Contains("x"))
                    {
                        ticket.For = "XBOX";
                        ticket.Number = number.Replace("x", "");
                    }
                }

                result.Add(ticket);
            }

            return result;
        }
    }
}
