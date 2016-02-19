using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for working with groups.
    /// </summary>
    public class GroupManager
    {
        private readonly Connection connection;

        internal GroupManager(Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Loads all the groups for the currently logged in user.
        /// </summary>
        /// <returns>The loaded groups.</returns>
        public async Task<IEnumerable<Group>> LoadForCurrentUserAsync()
        {
            var response = await this.connection.PostAsync("api.php?action=getUserRoles", null);
            var groupSections = JsonConvert.DeserializeObject<GroupSection[]>(response);
            var groups = from groupSection in groupSections
                         group groupSection by new { groupSection.groupid, groupSection.groupname } into groupDetails
                         select new Group(groupDetails.Key.groupname,
                         groupDetails.Key.groupid,
                         groupDetails.Select(s => new Section(s.sectionname, s.sectionid, s.section, null)));
            return groups;
        }

        private class GroupSection
        {
            public string groupid { get; set; }

            public string groupname { get; set; }

            public string section { get; set; }

            public string sectionid { get; set; }

            public string sectionname { get; set; }
        }
    }
}