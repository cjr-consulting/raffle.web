using Microsoft.Extensions.Caching.Memory;

using Raffle.Core.Models.App;
using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Cache
{
    public class CacheRaffleEventRepository : IRaffleEventRepository
    {
        readonly IMemoryCache cache;
        readonly IRaffleEventRepository repository;

        public CacheRaffleEventRepository(IRaffleEventRepository repository, IMemoryCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        public RaffleEvent GetById(int id)
        {
            if (!cache.TryGetValue($"{CacheKeys.RaffleEventById}{id}", out RaffleEvent cacheEntry))
            {
                cacheEntry = repository.GetById(id);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                cache.Set($"{CacheKeys.RaffleEventById}{id}", cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }
    }
}
