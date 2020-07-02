using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Models
{
    public class RaffleOrder
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public Customer Customer { get; set; }
        public IReadOnlyList<RaffleOrderLine> MyProperty { get; set; }
    }

    public class Customer
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class RaffleOrderLine
    {
        public RaffleItem Item { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Amount { get; set; }
    }
}
