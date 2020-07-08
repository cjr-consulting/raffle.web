using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raffle.Web.Models.Raffle
{
    public class RaffleItemAddModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Order { get; set; }
        public string ItemValue { get; set; } = string.Empty;
        public string Sponsor { get; set; } = string.Empty;
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
