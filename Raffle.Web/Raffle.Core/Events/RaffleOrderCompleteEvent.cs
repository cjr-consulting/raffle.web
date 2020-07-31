using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;

namespace Raffle.Core.Events
{
    public class RaffleOrderCompleteEvent : BaseDomainEvent
    {
        public RaffleOrder Order { get; set; }
    }
}