using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raffle.Web.Models.Admin.RaffleItem
{
    public class RaffleItemUpdateModel
    {
        public int Id { get; set; }
        [Required]
        public int ItemNumber { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        [Required]
        public int Cost { get; set; }
        public int NumberOfDraws { get; set; } = 1;
        public bool IsAvailable { get; set; }
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
        public string WinningTickets { get; set; } = string.Empty;
    }
}
