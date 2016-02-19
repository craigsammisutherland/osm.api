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
        private readonly Dictionary<int, CachedDataSet<BadgesStructureResponse>> caches = new Dictionary<int, CachedDataSet<BadgesStructureResponse>>();
        private readonly CacheSettings cacheSettings;
        private readonly Connection connection;

        internal BadgeManager(Connection connection, CacheSettings cacheSettings)
        {
            this.connection = connection;
            this.cacheSettings = cacheSettings;
        }

        /// <summary>
        /// Lists the badges for a section.
        /// </summary>
        /// <param name="section">The section to list the badges for.</param>
        /// <param name="term">The term to list the badges for.</param>
        /// <param name="badgeType">The type of badge to list.</param>
        /// <returns>A list of all the badges in the section.</returns>
        public async Task<IEnumerable<Badge>> ListForSectionAsync(Section section, int badgeType, Term term)
        {
            var values = new Dictionary<string, string>
            {
                ["a"] = "1",
                ["section"] = section.Type,
                ["type_id"] = badgeType.ToString(),
                ["term_id"] = term.Id,
                ["section_id"] = section.Id
            };
            var query = string.Join("&", Utils.EncodeQueryValues(values));
            var badgeData = await this.RetrieveBadgeDataFromServer(section, badgeType, term, query);
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
                Utils.ParseDateTime(b.details.lastupdated).Value,
                ParseTasks(b.structure)));
            return badges;
        }

        private static IEnumerable<BadgeTask> ParseTasks(BadgeStructure[] structure)
        {
            var rows = structure
                .Skip(1)
                .SelectMany(s => s.Rows);
            var tasks = rows.Select(r => new BadgeTask(r.field, r.name, r.tooltip, null));
            return tasks;
        }

        private async Task<BadgesStructureResponse> RetrieveBadgeDataFromServer(Section section, int badgeType, Term term, string query)
        {
            CachedDataSet<BadgesStructureResponse> cache;
            if (!this.caches.TryGetValue(badgeType, out cache))
            {
                cache = new CachedDataSet<BadgesStructureResponse>(
                    () => new CachedData<BadgesStructureResponse>("ext/badges/records/?action=getBadgeStructureByType", this.cacheSettings));
                this.caches[badgeType] = cache;
            }

            return await cache
                .GetForSectionAndTerm(section, term)
                .GetAsync(this.connection, query, null);
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