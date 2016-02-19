using System;
using System.Collections.Generic;

namespace Auxano.Osm.Api
{
    internal class CachedDataSet<TData>
    {
        private readonly Dictionary<string, CachedData<TData>> caches = new Dictionary<string, CachedData<TData>>();
        private readonly Func<CachedData<TData>> initialise;

        public CachedDataSet(Func<CachedData<TData>> initialise)
        {
            this.initialise = initialise;
        }

        public CachedData<TData> Get(string key)
        {
            CachedData<TData> cache;
            if (!this.caches.TryGetValue(key, out cache))
            {
                cache = initialise();
                this.caches[key] = cache;
            }

            return cache;
        }

        public CachedData<TData> GetForSection(Section section)
        {
            return Get("s" + section.Id);
        }

        public CachedData<TData> GetForSectionAndTerm(Section section, Term term)
        {
            return Get("s" + section.Id + "~t" + term.Id);
        }
    }
}