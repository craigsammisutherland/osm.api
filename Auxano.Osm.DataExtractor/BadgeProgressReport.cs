using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;

namespace Auxano.Osm.DataExtractor
{
    public class BadgeProgressReport
        : Report
    {
        public BadgeProgressReport()
            : base("Badge Progress (Excel)")
        {
        }

        public override Task Generate(ViewModel model, Manager manager)
        {
            var loadTasks = new[] { 1, 2 }
                .Select(i => manager.Badge.ListForSectionAsync(model.SelectedSection, i, model.SelectedTerm))
                .ToArray();
            Task.WaitAll(loadTasks);
            var badges = loadTasks.SelectMany(t => t.Result);
            return Task.FromResult(0);
        }
    }
}