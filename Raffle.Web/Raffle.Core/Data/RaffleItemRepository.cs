using Raffle.Core.Models;
using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raffle.Core.Data
{
    public class RaffleItemRepository : IRaffleItemRepository
    {
        public Task AddAsync(RaffleItemAdd raffleItem)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<RaffleItem> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
