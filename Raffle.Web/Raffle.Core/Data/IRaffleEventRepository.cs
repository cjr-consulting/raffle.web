using Raffle.Core.Models.App;

namespace Raffle.Core.Data
{
    public interface IRaffleEventRepository
    {
        RaffleEvent GetById(int id);
    }
}
