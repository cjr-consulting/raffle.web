using Raffle.Core.Models.App;

namespace Raffle.Core.Repositories
{
    public interface IRaffleEventRepository
    {
        RaffleEvent GetById(int id);
    }
}
