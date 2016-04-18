using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for working with badges.
    /// </summary>
    public class BadgeManager
    {
        private static string[] progressConstants = new[]
        {
            "awarded",
            "awardeddate",
            "completed",
            "eligibilty",
            "firstname",
            "lastname",
            "scoutid"
        };

        private readonly Connection connection;

        internal BadgeManager(Connection connection)
        {
            this.connection = connection;
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
            var badgeData = await connection.PostAsync<BadgesStructureResponse>(
                "ext/badges/records/?action=getBadgeStructureByType", query, null);
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

        /// <summary>
        /// Lists the progress towards a badge for all members in a section.
        /// </summary>
        /// <param name="section">The section to list the progress for.</param>
        /// <param name="badge">The badge to list.</param>
        /// <param name="term">The current term for the list.</param>
        /// <returns>A list of the progress for all the members in the section for the badge.</returns>
        public async Task<IEnumerable<BadgeProgress>> ListProgressForBadgeAsync(Section section, Badge badge, Term term)
        {
            var values = new Dictionary<string, string>
            {
                ["badge_id"] = badge.Id,
                ["section"] = section.Type,
                ["badge_version"] = badge.Version,
                ["term_id"] = term.Id,
                ["section_id"] = section.Id
            };
            var query = string.Join("&", Utils.EncodeQueryValues(values));
            var badgeData = await connection.PostAsync<BadgeReportResponse>(
                "ext/badges/records/?action=getBadgeRecords", query, null);
            var fullData = (from item in badgeData.items
                            select ParseProgress(item, badge, section)).ToArray();
            return fullData;
        }

        private static string GetProgressValue(Dictionary<string, string> item, string key)
        {
            string value;
            return item.TryGetValue(key, out value) ? value : null;
        }

        private static DateTime? ParseDate(string date)
        {
            if (string.IsNullOrEmpty(date)) return null;
            if (date == "0000-00-00") return null;
            return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static BadgeProgress ParseProgress(Dictionary<string, string> item, Badge badge, Section section)
        {
            var member = new Member(GetProgressValue(item, "scoutid"),
                GetProgressValue(item, "lastname"),
                GetProgressValue(item, "firstname"),
                section);
            var completed = GetProgressValue(item, "completed") == "1";
            var awarded = ParseDate(GetProgressValue(item, "awardeddate"));
            foreach (var constant in progressConstants)
            {
                if (item.ContainsKey(constant)) item.Remove(constant);
            }

            var progress = new BadgeProgress(
                member,
                badge,
                completed,
                awarded,
                item);

            return progress;
        }

        private static IEnumerable<BadgeTask> ParseTasks(BadgeStructure[] structure)
        {
            var rows = structure
                .Skip(1)
                .SelectMany(s => s.Rows);
            var tasks = rows.Select(r => new BadgeTask(r.field, r.name, r.tooltip, null));
            return tasks;
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

        private class BadgeReportResponse
        {
            public List<Dictionary<string, string>> items { get; set; }
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