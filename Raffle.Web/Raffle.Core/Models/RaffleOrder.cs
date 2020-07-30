
using System;
using System.Collections.Generic;

namespace Raffle.Core.Models
{
    public class RaffleOrder
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public Customer Customer { get; set; }
        public bool IsOrderConfirmed { get; set; }
        public bool Confirmed21 { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? UpdatedDate{ get; set; }
        public IReadOnlyList<RaffleOrderLine> Lines { get; set; }
        public int TotalPrice
        {
            get
            {
                var price = 0;
                foreach (var line in Lines)
                {
                    price += line.Price * line.Count;
                }

                return price;
            }
        }
        public int TotalTickets
        {
            get
            {
                var tickets = 0;
                foreach(var line in Lines)
                {
                    tickets += line.Count; 
                }

                return tickets;
            }
        }
    }

    public class Customer
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public bool IsInternational { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string InternationalAddress { get; set; }
    }

    public class RaffleOrderLine
    {
        public int RaffleItemId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }
}
