using Raffle.Core.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raffle.Core.Repositories
{
    public interface IRaffleItemRepository
    {
        IReadOnlyList<RaffleItem> GetAll();
    }
}
