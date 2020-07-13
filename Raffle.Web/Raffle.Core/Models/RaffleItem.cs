using System;

namespace Raffle.Core.Models
{
    public class RaffleItem
    {
        public int Id { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; }
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
    }
}
