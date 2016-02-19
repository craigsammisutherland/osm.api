using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for working with badges.
    /// </summary>
    public class BadgeManager
    {
        private readonly CachedData<BadgesStructureResponse> cache;
        private readonly Connection connection;

        internal BadgeManager(Connection connection)
        {
            this.connection = connection;
            this.cache = new CachedData<BadgesStructureResponse>("ext/badges/records/?action=getBadgeStructureByType", TimeSpan.FromMinutes(0));
        }

        /// <summary>
        /// Lists the badges for a section.
        /// </summary>
        /// <param name="section">The section to list the badges for.</param>
        /// <param name="term">The term to list the badges for.</param>
        /// <param name="badgeType">The type of badge to list.</param>
        /// <returns>A list of all the badges in the section.</returns>
        public async Task<IEnumerable<Badge>> ListForSectionAsync(Section section, BadgeType badgeType, Term term)
        {
            var values = new Dictionary<string, string>
            {
                ["a"] = "1",
                ["section"] = section.Type,
                ["type_id"] = ((int)badgeType).ToString(),
                ["term_id"] = term.Id,
                ["section_id"] = section.Id
            };
            var query = string.Join("&", Utils.EncodeQueryValues(values));
            var badgeData = await cache.GetAsync(this.connection, query, null);
            var fullData = from details in badgeData.details
                           join structure in badgeData.structure on details.Key equals structure.Key
                           select new { details = details.Value, structure = structure.Value };
            var badges = fullData.Select(b => new Badge(
                b.details.badge_id,
                b.details.badge_version,
                b.details.name,
                b.details.description,
                b.details.group_name,
                b.details.picture,
                section,
                Utils.ParseDateTime(b.details.created_at).Value,
                Utils.ParseDateTime(b.details.lastupdated).Value));
            return badges;
        }

        private class BadgeDetails
        {
            public string badge_id { get; set; }
            public string badge_version { get; set; }
            public string created_at { get; set; }
            public string description { get; set; }
            public string group_name { get; set; }
            public string lastupdated { get; set; }
            public string name { get; set; }
            public string picture { get; set; }
        }

        private class BadgesStructureResponse
        {
            public string badgeOrder { get; set; }
            public Dictionary<string, BadgeDetails> details { get; set; }
            public Dictionary<string, BadgeStructure[]> structure { get; set; }
        }

        private class BadgeStructure
        {
            public StructureDetails[] Rows { get; set; }
        }

        private class StructureDetails
        {
            public string field { get; set; }
            public string name { get; set; }
            public string tooltip { get; set; }
        }
    }
}