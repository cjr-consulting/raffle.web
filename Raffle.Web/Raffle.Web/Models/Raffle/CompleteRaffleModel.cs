using System;
using System.Collections.Generic;
using System.Linq;

namespace Raffle.Web.Models.Raffle
{
    public class CompleteRaffleModel
    {
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerEmailRepeat { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }

        public List<CompleteRaffleItemModel> Items { get; set; } = new List<CompleteRaffleItemModel>();
    }

    public class CompleteRaffleItemModel
    {
        public int RaffleItemId { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
    }
}
