using System;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Settings for caching data.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Initialise a new instance of <see cref="CacheSettings"/>.
        /// </summary>
        public CacheSettings()
        {
            this.CacheDuration = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// The duration that items are cached for.
        /// </summary>
        public TimeSpan CacheDuration { get; set; }
    }
}