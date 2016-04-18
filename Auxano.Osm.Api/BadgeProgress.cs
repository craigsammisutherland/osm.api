using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// The current progress of a member towards a badge.
    /// </summary>
    public class BadgeProgress
    {
        private readonly Badge badge;
        private readonly bool isCompleted;
        private readonly Member member;
        private readonly ImmutableDictionary<string, string> progress;
        private readonly DateTime? whenAwarded;

        /// <summary>
        /// Initializes a new instance of a <see cref="BadgeProgress"/>.
        /// </summary>
        /// <param name="member">The member the progress is for.</param>
        /// <param name="badge">The badge the progress is for.</param>
        /// <param name="isCompleted">Whether the badge has been completed or not.</param>
        /// <param name="whenAwarded">When the badge was awarded; <c>null</c> if not awarded yet.</param>
        /// <param name="taskStatus">The status for each individual task.</param>
        public BadgeProgress(Member member, Badge badge, bool isCompleted, DateTime? whenAwarded, IDictionary<string, string> taskStatus)
        {
            this.member = member;
            this.badge = badge;
            this.isCompleted = isCompleted;
            this.whenAwarded = whenAwarded;
            this.progress = taskStatus.ToImmutableDictionary();
        }

        /// <summary>
        /// The badge this progress record is for.
        /// </summary>
        public Badge Badge
        {
            get { return this.badge; }
        }

        /// <summary>
        /// A flag indicating whether the badge has been awarded or not.
        /// </summary>
        public bool IsAwarded
        {
            get { return this.whenAwarded.HasValue; }
        }

        /// <summary>
        /// A flag indicating whether the badge has been completed or not.
        /// </summary>
        public bool IsCompleted
        {
            get { return this.isCompleted; }
        }

        /// <summary>
        /// The member this progress record is for.
        /// </summary>
        public Member Member
        {
            get { return this.member; }
        }

        /// <summary>
        /// The status for each individual task.
        /// </summary>
        public ImmutableDictionary<string, string> TaskStatus
        {
            get { return this.progress; }
        }

        /// <summary>
        /// The date when this badge was awarded.
        /// </summary>
        /// <value>
        /// If this property is <c>null</c> then the badge has not been awarded yet.
        /// </value>
        public DateTime? WhenAwarded
        {
            get { return this.whenAwarded; }
        }
    }
}