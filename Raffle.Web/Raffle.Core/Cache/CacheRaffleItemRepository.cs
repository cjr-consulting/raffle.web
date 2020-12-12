using Microsoft.Extensions.Caching.Memory;

using Raffle.Core.Models;
using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Raffle.Core.Cache
{
    public class CacheRaffleItemRepository : IRaffleItemRepository
    {
        readonly IRaffleItemRepository repository;
        readonly IMemoryCache cache;

        public CacheRaffleItemRepository(IRaffleItemRepository repository, IMemoryCache cache)
        {
            this.cache = cache;
            this.repository = repository;
        }

        public IReadOnlyList<RaffleItem> GetAll()
        {
            var e = cache.Get(CacheKeys.RaffleItemsAll);
            if (!cache.TryGetValue(CacheKeys.RaffleItemsAll, out List<RaffleItem> cacheEntry))
            {
                cacheEntry = repository.GetAll().ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                cache.Set(CacheKeys.RaffleItemsAll, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        public RaffleItem GetById(int id)
        {
            return repository.GetById(id);
        }

        public IReadOnlyList<string> GetUsedCategories()
        {
            if (!cache.TryGetValue(CacheKeys.RaffleItemCategories, out List<string> cacheEntry))
            {
                cacheEntry = repository.GetUsedCategories().ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                cache.Set(CacheKeys.RaffleItemCategories, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }
    }
}
