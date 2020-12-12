using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;

namespace Raffle.Core.Events
{
    public class RaffleItemUpdated : BaseDomainEvent
    {
        public RaffleItem RaffleItem { get; set; }
    }
}
