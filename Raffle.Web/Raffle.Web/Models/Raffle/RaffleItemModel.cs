using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Models.Raffle
{
    public class RaffleItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string Value { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public int Amount { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class RaffleOrderViewModel
    {
        public List<RaffleItemModel> RaffleItems { get; set; } = new List<RaffleItemModel>();
    }
}
