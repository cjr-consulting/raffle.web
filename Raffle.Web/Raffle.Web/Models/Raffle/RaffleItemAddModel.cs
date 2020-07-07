using System;
using System.Linq;

namespace Raffle.Web.Models.Raffle
{
    public class RaffleItemAddModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
