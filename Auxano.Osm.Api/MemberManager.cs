using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for working with members.
    /// </summary>
    public class MemberManager
    {
        private readonly CachedData<MembersResponse> cache;
        private readonly Connection connection;

        internal MemberManager(Connection connection)
        {
            this.connection = connection;
            this.cache = new CachedData<MembersResponse>("ext/members/contact/grid/?action=getMembers", TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Lists all the members for a section.
        /// </summary>
        /// <param name="section">The section to list the members for.</param>
        /// <param name="term">The term to list the members for.</param>
        /// <returns>A list of all the members in the section.</returns>
        public async Task<IEnumerable<Member>> ListForSectionAsync(Section section, Term term)
        {
            var values = new Dictionary<string, string>
            {
                ["section_id"] = section.Id,
                ["term_id"] = term.Id
            };
            var memberData = await cache.GetAsync(this.connection, values);
            var members = memberData.data
                .Values
                .Select(m => new Member(
                    m.member_id,
                    m.last_name,
                    m.first_name,
                    m.patrol,
                    m.patrol_role_level_label,
                    m.active,
                    Utils.ParseDate(m.date_of_birth),
                    Utils.ParseDate(m.joined),
                    Utils.ParseDate(m.started),
                    Utils.ParseDate(m.end_date),
                    section));
            return members;
        }

        private class MemberResponse
        {
            public bool active { get; set; }
            public string date_of_birth { get; set; }
            public string end_date { get; set; }
            public string first_name { get; set; }
            public string joined { get; set; }
            public string last_name { get; set; }
            public string member_id { get; set; }
            public string patrol { get; set; }
            public string patrol_role_level_label { get; set; }
            public string started { get; set; }
        }

        private class MembersResponse
        {
            public Dictionary<string, MemberResponse> data { get; set; }
        }
    }
}