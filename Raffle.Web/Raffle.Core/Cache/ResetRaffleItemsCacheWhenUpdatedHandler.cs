using MediatR;

using Microsoft.Extensions.Caching.Memory;

using Raffle.Core.Events;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Cache
{
    public class ResetRaffleItemsCacheWhenUpdatedHandler : INotificationHandler<RaffleItemUpdated>
    {
        readonly IMemoryCache cache;

        public ResetRaffleItemsCacheWhenUpdatedHandler(IMemoryCache cache)
        {
            this.cache = cache;
        }
        public Task Handle(RaffleItemUpdated notification, CancellationToken cancellationToken)
        {
            cache.Remove(CacheKeys.RaffleItemsAll);
            cache.Remove(CacheKeys.RaffleItemCategories);
            return Task.CompletedTask;
        }
    }
}
