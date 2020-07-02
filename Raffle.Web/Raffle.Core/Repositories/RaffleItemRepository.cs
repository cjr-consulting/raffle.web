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
}
