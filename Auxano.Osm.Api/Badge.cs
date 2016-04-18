using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details of a badge.
    /// </summary>
    public class Badge
    {
        private readonly int category;
        private readonly string description;
        private readonly string group;
        private readonly string id;
        private readonly string name;
        private readonly string pictureUrl;
        private readonly Section section;
        private readonly ImmutableArray<BadgeTask> tasks;
        private readonly string version;
        private readonly DateTime whenAdded;
        private readonly DateTime whenUpdated;

        /// <summary>
        /// Initialises a new instance of a <see cref="Badge"/>.
        /// </summary>
        /// <param name="id">The identifier of the badge.</param>
        /// <param name="category">The category of the badge.</param>
        /// <param name="name">The name of the badge.</param>
        /// <param name="description">A description of the badge.</param>
        /// <param name="group">Which group of badges this badges belongs to.</param>
        /// <param name="pictureUrl">An URL to the picture.</param>
        /// <param name="section">The section this badge belongs to.</param>
        /// <param name="version">The version of this badge.</param>
        /// <param name="whenAdded">When the badge was added.</param>
        /// <param name="whenUpdated">When the badge was last updated.</param>
        /// <param name="tasks">The tasks required to earn this badge.</param>
        public Badge(string id, string version, int category, string name, string description, string group, string pictureUrl, Section section, DateTime whenAdded, DateTime whenUpdated, IEnumerable<BadgeTask> tasks)
        {
            this.id = id;
            this.category = category;
            this.name = name;
            this.description = description;
            this.group = group;
            this.pictureUrl = pictureUrl;
            this.section = section;
            this.version = version;
            this.whenAdded = whenAdded;
            this.whenUpdated = whenUpdated;
            this.tasks = tasks
                .Select(t => t.SetBadge(this))
                .ToImmutableArray();
        }

        /// <summary>
        /// The category of the badge.
        /// </summary>
        public int Category
        {
            get { return this.category; }
        }

        /// <summary>
        /// The description of the badge.
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// The full internal identifier of the badge.
        /// </summary>
        /// <remarks>
        /// The full identifier consists of the id and the version, separated by an underscore.</remarks>
        public string FullId
        {
            get { return this.id + "_" + this.version; }
        }

        /// <summary>
        /// The group of badges this badge belongs to.
        /// </summary>
        public string Group
        {
            get { return this.group; }
        }

        /// <summary>
        /// The internal identifier of the badge.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// The name of the badge.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// A URL to the image to use for a picture of this badge.
        /// </summary>
        public string PictureUrl
        {
            get { return this.pictureUrl; }
        }

        /// <summary>
        /// The section this badge is for.
        /// </summary>
        public Section Section
        {
            get { return this.section; }
        }

        /// <summary>
        /// The tasks to complete the badge.
        /// </summary>
        public IEnumerable<BadgeTask> Tasks
        {
            get { return this.tasks; }
        }

        /// <summary>
        /// The version of the badge.
        /// </summary>
        public string Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// When this badge was added.
        /// </summary>
        public DateTime WhenAdded
        {
            get { return this.whenAdded; }
        }

        /// <summary>
        /// When this badge was last updated.
        /// </summary>
        public DateTime WhenUpdated
        {
            get { return this.whenUpdated; }
        }
    }
}