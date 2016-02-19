using System;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details on a term.
    /// </summary>
    public class Term
    {
        private readonly DateTime endDate;
        private readonly string id;
        private readonly string name;
        private readonly Section section;
        private readonly DateTime startDate;

        /// <summary>
        /// Initialises a new instance of a <see cref="Term"/>.
        /// </summary>
        /// <param name="name">The name of the term.</param>
        /// <param name="id">The identifier of the term.</param>
        /// <param name="startDate">The starting date of the term.</param>
        /// <param name="endDate">The ending date of the term.</param>
        /// <param name="section">The <see cref="Section"/> this term is for.</param>
        public Term(string name, string id, DateTime startDate, DateTime endDate, Section section)
        {
            this.name = name;
            this.id = id;
            this.startDate = startDate;
            this.endDate = endDate;
            this.section = section;
        }

        /// <summary>
        /// The ending date of the term.
        /// </summary>
        public DateTime EndDate
        {
            get { return this.endDate; }
        }

        /// <summary>
        /// The internal identifier of the term.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// The name of the term.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// The <see cref="Section"/> this term is for.
        /// </summary>
        public Section Section
        {
            get { return this.section; }
        }

        /// <summary>
        /// The starting date of the term.
        /// </summary>
        public DateTime StartDate
        {
            get { return this.startDate; }
        }
    }
}