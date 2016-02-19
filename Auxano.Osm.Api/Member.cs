using System;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Details on a member.
    /// </summary>
    public class Member
    {
        private readonly string familyName;
        private readonly string firstName;
        private readonly string id;
        private readonly bool isActive;
        private readonly string patrol;
        private readonly string role;
        private readonly Section section;
        private readonly DateTime? whenBorn;
        private readonly DateTime? whenEnded;
        private readonly DateTime? whenJoined;
        private readonly DateTime? whenStarted;

        /// <summary>
        /// Initialises a new member.
        /// </summary>
        /// <param name="id">The identifier of the member.</param>
        /// <param name="familyName">The member's family name.</param>
        /// <param name="firstName">The member's first name.</param>
        /// <param name="patrol">Which patrol the member belongs to.</param>
        /// <param name="role">Which role the member has.</param>
        /// <param name="isActive">Whether the member is currently active or not.</param>
        /// <param name="whenBorn">When the member was born.</param>
        /// <param name="whenJoined">When the member joined the section.</param>
        /// <param name="whenStarted">When the member started at the section.</param>
        /// <param name="whenEnded">When the member ended at thge section.</param>
        /// <param name="section">The section the member belongs to.</param>
        public Member(string id, string familyName, string firstName, string patrol, string role, bool isActive, DateTime? whenBorn, DateTime? whenJoined, DateTime? whenStarted, DateTime? whenEnded, Section section)
        {
            this.id = id;
            this.familyName = familyName;
            this.firstName = firstName;
            this.patrol = patrol;
            this.role = role;
            this.whenBorn = whenBorn;
            this.whenJoined = whenJoined;
            this.whenStarted = whenStarted;
            this.whenEnded = whenEnded;
            this.isActive = isActive;
            this.section = section;
        }

        /// <summary>
        /// The member's family name.
        /// </summary>
        public string FamilyName
        {
            get { return this.familyName; }
        }

        /// <summary>
        /// The member's first name.
        /// </summary>
        public string FirstName
        {
            get { return this.firstName; }
        }

        /// <summary>
        /// The internal identifier of the member.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Whether this member is currently active or not.
        /// </summary>
        public bool IsActive
        {
            get { return this.isActive; }
        }

        /// <summary>
        /// The patrol this member belongs to.
        /// </summary>
        public string Patrol
        {
            get { return this.patrol; }
        }

        /// <summary>
        /// The role of the member.
        /// </summary>
        public string Role
        {
            get { return this.role; }
        }

        /// <summary>
        /// The <see cref="Section"/> this member belongs to.
        /// </summary>
        public Section Section
        {
            get { return this.section; }
        }

        /// <summary>
        /// When this member was born.
        /// </summary>
        public DateTime? WhenBorn
        {
            get { return this.whenBorn; }
        }

        /// <summary>
        /// When this member ended at the section.
        /// </summary>
        public DateTime? WhenEnded
        {
            get { return this.whenEnded; }
        }

        /// <summary>
        /// When this member joined the section.
        /// </summary>
        public DateTime? WhenJoined
        {
            get { return this.whenJoined; }
        }

        /// <summary>
        /// When this member started at the section.
        /// </summary>
        public DateTime? WhenStarted
        {
            get { return this.whenStarted; }
        }
    }
}