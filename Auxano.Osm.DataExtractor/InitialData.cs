using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;

namespace Auxano.Osm.DataExtractor
{
    public class InitialData
    {
        public IEnumerable<Group> Groups { get; private set; }

        public TermArray Terms { get; private set; }

        public static async Task<InitialData> LoadAsync(Manager manager)
        {
            var groups = await manager.Group.LoadForCurrentUserAsync();
            var section = groups.FirstOrDefault()?.Sections.FirstOrDefault();
            var terms = section == null
                ? null
                : await manager.Term.ListForSectionAsync(section);
            return new InitialData
            {
                Groups = groups,
                Terms = terms
            };
        }
    }
}