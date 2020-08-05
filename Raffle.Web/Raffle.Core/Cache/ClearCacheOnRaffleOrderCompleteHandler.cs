using MediatR;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Raffle.Core.Events;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Cache
{
    public class ClearCacheOnRaffleOrderCompleteHandler : INotificationHandler<RaffleOrderCompleteEvent>
    {
        readonly IMemoryCache cache;
        readonly ILogger<ClearCacheOnRaffleOrderCompleteHandler> logger;

        public ClearCacheOnRaffleOrderCompleteHandler(IMemoryCache cache, ILogger<ClearCacheOnRaffleOrderCompleteHandler> logger)
        {
            this.logger = logger;
            this.cache = cache;
        }

        public Task Handle(RaffleOrderCompleteEvent notification, CancellationToken cancellationToken)
        {
            cache.Remove(CacheKeys.RaffleItemsAll);
            logger.LogDebug("RaffleOrderComplete: Clear RaffleItemsAll Cache");
            return Task.CompletedTask;
        }
    }
}
