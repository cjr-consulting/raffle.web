using Raffle.Core.Models;

using System.Collections.Generic;

namespace Raffle.Core.Repositories
{
    public interface IRaffleItemRepository
    {
        IReadOnlyList<RaffleItem> GetAll();

        RaffleItem GetById(int id);
        IReadOnlyList<string> GetUsedCategories();
    }
}
