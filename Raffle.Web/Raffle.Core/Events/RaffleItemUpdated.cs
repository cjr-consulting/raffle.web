using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Events
{
    public class RaffleItemUpdated : BaseDomainEvent
    {
        public RaffleItem RaffleItem { get; set; }
    }
}
