using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raffle.Web.Models.Admin.RaffleOrder
{
    public class RaffleOrderUpdateViewModel
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public bool Confirmed21 { get; set; }
        public RaffleOrderUpdateCustomer Customer { get; set; }
        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }
        public DateTime? CompleteDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DonationDate { get; set; }
        public string DonationNote { get; set; }
        public List<RaffleOrderUpdateLineModel> Lines { get; set; } = new List<RaffleOrderUpdateLineModel>();
        public int TotalTicketsFromSheet { get; set; }
        public int TotalTicketsOne { get; set; }
        public int TotalTicketsTwo { get; set; }
    }

    public class RaffleOrderUpdateLineModel
    {
        public int RaffleItemId { get; set; }
        public string Name { get; set; }
        public int Cost { get { return Amount * Price; } }
        public int Amount { get; set; }
        public int Price { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    public class RaffleOrderUpdateCustomer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool IsInternational { get; set; }
        public string InternationalAddress { get; set; }
    }
}
