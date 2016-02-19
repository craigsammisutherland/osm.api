namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details about a task for a badge.
    /// </summary>
    public class BadgeTask
    {
        private readonly Badge badge;
        private readonly string description;
        private readonly string id;
        private readonly string name;

        /// <summary>
        /// Initialises a new instance of a <see cref="BadgeTask"/>.
        /// </summary>
        /// <param name="id">The id of the task.</param>
        /// <param name="name">The name of the task.</param>
        /// <param name="description">A description of the task.</param>
        /// <param name="badge">The badge this task is for.</param>
        public BadgeTask(string id, string name, string description, Badge badge)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.badge = badge;
        }

        /// <summary>
        /// The badge this task is for.
        /// </summary>
        public Badge Badge
        {
            get { return this.badge; }
        }

        /// <summary>
        /// The description of the task for the badge.
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// The internal identifier of the task for the badge.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// The name of the task for the badge.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        internal BadgeTask SetBadge(Badge newBadge)
        {
            return new BadgeTask(this.id, this.name, this.description, newBadge);
        }
    }
}