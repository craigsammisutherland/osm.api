using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;
using SpreadsheetLight;

namespace Auxano.Osm.DataExtractor
{
    public class BadgeAchievementReport
        : Report
    {
        public BadgeAchievementReport()
            : base("Badge Achievement (Excel)")
        {
        }

        public async override Task<string> Generate(ViewModel model, Manager manager)
        {
            var loadTasks = new[] { 1, 2 }
                .Select(i => manager.Badge.ListForSectionAsync(model.SelectedSection, i, model.SelectedTerm))
                .ToArray();
            await Task.WhenAll(loadTasks);
            var badges = loadTasks.SelectMany(t => t.Result);
            var members = await manager.Member.ListForSectionAsync(model.SelectedSection, model.SelectedTerm);

            var progressTasks = badges
                .Select(b => manager.Badge.ListProgressForBadgeAsync(model.SelectedSection, b, model.SelectedTerm))
                .ToArray();
            var progressReports = await Task.WhenAll(progressTasks);

            var document = new SLDocument();
            document.RenameWorksheet(SLDocument.DefaultFirstSheetName, "Badge Achievement");
            var column = 0;
            document.SetCellValue(1, ++column, "Family Name");
            document.SetCellValue(1, ++column, "First Name");
            document.SetCellValue(1, ++column, "Date of Birth");
            var orderedBadges = badges.OrderBy(b => b.Category).ThenBy(b => b.Name).ToArray();
            foreach (var badge in orderedBadges)
            {
                document.SetCellValue(1, ++column, badge.Name);
            }

            var dateFormat = document.CreateStyle();
            dateFormat.FormatCode = "d mmm, yyyy";
            var row = 1;
            var achievements = progressReports
                .SelectMany(r => r)
                .GroupBy(p => p.Member.Id)
                .ToDictionary(g => g.Key, g => g.ToDictionary(b => b.Badge.Id, b => b.IsCompleted));
            foreach (var member in members)
            {
                column = 0;
                ++row;
                document.SetCellValue(row, ++column, member.FamilyName);
                document.SetCellValue(1, ++column, member.FirstName);
                ++column;
                if (member.WhenBorn.HasValue) document.SetCellValue(1, column, member.WhenBorn.Value);
                document.SetCellStyle(row, column, dateFormat);
                Dictionary<string, bool> badgesCompleted;
                if (!achievements.TryGetValue(member.Id, out badgesCompleted)) continue;

                foreach (var badge in orderedBadges)
                {
                    bool isCompleted;
                    document.SetCellValue(1, ++column, badgesCompleted.TryGetValue(badge.Id, out isCompleted) && isCompleted ? "Yes" : null);
                }
            }

            var fileName = GetSafeFileName(model);
            document.SaveAs(fileName);
            return fileName;
        }
    }
}