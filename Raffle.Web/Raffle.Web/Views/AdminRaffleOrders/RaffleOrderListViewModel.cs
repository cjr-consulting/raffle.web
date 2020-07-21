using System;
using System.Linq;

namespace Raffle.Web.Views.AdminRaffleOrders
{
    public class RaffleOrderListViewModel
    {
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int NumberOfTickets { get; set; }
    }
}
