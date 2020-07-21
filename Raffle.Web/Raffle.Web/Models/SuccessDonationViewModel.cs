using System;
using System.Collections.Generic;
using System.Linq;

namespace Raffle.Web.Models
{
    public class SuccessDonationViewModel
    {
        public string TicketNumber { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }

        public List<SuccessDonationLineItemModel> Items { get; set; } = new List<SuccessDonationLineItemModel>();
    }

    public class SuccessDonationLineItemModel
    {
        public string Name { get; set; }
        public int Cost { get { return Amount * Price; } }
        public int Amount { get; set; }
        public int Price { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
