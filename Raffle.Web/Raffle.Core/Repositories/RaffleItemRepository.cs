using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Repositories
{
    public class RaffleItemRepository
    {
        public IReadOnlyList<RaffleItem> GetAll()
        {
            return new List<RaffleItem>();
        }
    }

    public class RaffleItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string Value { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; }
    }
}
