using System;

namespace Raffle.Core.Models.App
{
    public class RaffleEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime VisibleDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CloseDate { get; set; }

    }
}
