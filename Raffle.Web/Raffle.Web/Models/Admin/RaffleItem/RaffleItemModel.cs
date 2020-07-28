using System;
using System.Linq;

namespace Raffle.Web.Models.Admin.RaffleItem
{
    public class RaffleItemModel
    {
        public int Id { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string Value { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public int Amount { get; set; }
        public bool IsAvailable { get; set; }
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
        public int NumberOfDraws { get; set; }
        public string WinningTickets { get; set; }
    }
}
