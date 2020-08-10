using System;
using System.Collections.Generic;
using System.Linq;

namespace Raffle.Web.Models.Admin.RaffleOrder
{
    public class RaffleOrderListViewModel
    {
        public List<RaffleOrderRowModel> RaffleOrders { get; set; } = new List<RaffleOrderRowModel>();
    }

    public class RaffleOrderRowModel
    {
        public int RaffleOrderId { get; set; }
        public string TicketNumber { get; set; }
        public DateTime? DonationDate { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int TotalTickets { get; set; }
        public int TotalPoints { get; set; }
        public string HowDidYouHear { get; set; }
    }
}
