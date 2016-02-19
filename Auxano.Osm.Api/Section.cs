namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details on a section.
    /// </summary>
    public class Section
    {
        private readonly Group group;
        private readonly string id;
        private readonly string name;
        private readonly string type;

        /// <summary>
        /// Initialises a new section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="id">The identifier of the section.</param>
        /// <param name="type">The type f the section.</param>
        /// <param name="group">The <see cref="Group"/> this section belongs to.</param>
        public Section(string name, string id, string type, Group group)
        {
            this.name = name;
            this.id = id;
            this.type = type;
            this.group = group;
        }

        /// <summary>
        /// The <see cref="Group"/> this section is part of.
        /// </summary>
        public Group Group
        {
            get { return this.group; }
        }

        /// <summary>
        /// The internal identifier of the section.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// The name of the section.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// The type of section.
        /// </summary>
        public string Type
        {
            get { return this.type; }
        }

        internal Section AddGroup(Group group)
        {
            return new Section(this.name, this.id, this.type, group);
        }
    }
}