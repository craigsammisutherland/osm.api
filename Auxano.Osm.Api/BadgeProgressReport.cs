using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// A report on the progress of a section toward a badge.
    /// </summary>
    public class BadgeProgressReport
        : IEnumerable<BadgeProgress>
    {
        private readonly Badge badge;
        private readonly ImmutableArray<BadgeProgress> progress;

        /// <summary>
        /// Initialises a new instance of <see cref="BadgeProgressReport"/>.
        /// </summary>
        /// <param name="badge">The badge this report is for.</param>
        /// <param name="progress">The progress of each individual member.</param>
        public BadgeProgressReport(Badge badge, IEnumerable<BadgeProgress> progress)
        {
            this.badge = badge;
            this.progress = progress.ToImmutableArray();
        }

        /// <summary>
        /// The badge this report is for.
        /// </summary>
        public Badge Badge
        {
            get { return this.badge; }
        }

        /// <summary>
        /// Gets the enumerator for iterating through the list of badge progress.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<BadgeProgress> GetEnumerator()
        {
            return this.progress
                .AsEnumerable()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.progress
                .AsQueryable()
                .GetEnumerator();
        }
    }
}