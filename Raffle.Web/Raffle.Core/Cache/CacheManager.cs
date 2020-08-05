using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Cache
{
    public class CacheManager : ICacheManager
    {
        readonly ILogger<CacheManager> logger;
        readonly IMemoryCache cache;

        public CacheManager(IMemoryCache cache, ILogger<CacheManager> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public void ResetAllCache()
        {
            logger.LogInformation("The cache has been reset");
            cache.Remove(CacheKeys.RaffleItemsAll);
            cache.Remove(CacheKeys.RaffleItemCategories);
        }
    }
}
