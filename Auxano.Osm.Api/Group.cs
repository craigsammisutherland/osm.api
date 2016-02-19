using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details on a group.
    /// </summary>
    public class Group
    {
        private readonly string id;

        private readonly string name;

        private readonly ImmutableArray<Section> sections;

        /// <summary>
        /// Initialises a new group.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="id">The identifier of the group.</param>
        /// <param name="sections">The sections within the group.</param>
        public Group(string name, string id, IEnumerable<Section> sections)
        {
            this.name = name;
            this.id = id;
            this.sections = ImmutableArray.Create(sections.Select(s => s.AddGroup(this)).ToArray());
        }

        /// <summary>
        /// The internal identifier of the group.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// The sections within the group.
        /// </summary>
        public IEnumerable<Section> Sections
        {
            get { return this.sections; }
        }
    }
}