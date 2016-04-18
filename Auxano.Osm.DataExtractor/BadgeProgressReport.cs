using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;
using SpreadsheetLight;

namespace Auxano.Osm.DataExtractor
{
    public class BadgeProgressReport
        : Report
    {
        public BadgeProgressReport()
            : base("Badge Progress (Excel)")
        {
        }

        public async override Task<string> Generate(ViewModel model, Manager manager)
        {
            var loadTasks = new[] { 1, 2 }
                .Select(i => manager.Badge.ListForSectionAsync(model.SelectedSection, i, model.SelectedTerm))
                .ToArray();
            await Task.WhenAll(loadTasks);
            var badges = loadTasks.SelectMany(t => t.Result);

            var progressTasks = badges
                .Select(b => manager.Badge.ListProgressForBadgeAsync(model.SelectedSection, b, model.SelectedTerm))
                .ToArray();
            await Task.WhenAll(progressTasks);
            var progressReports = progressTasks.ToDictionary(r => r.Result.Badge.Id, r => r.Result);

            var document = new SLDocument();
            var dateFormat = document.CreateStyle();
            dateFormat.FormatCode = "d mmm, yyyy";
            var isFirst = true;
            foreach (var badge in badges)
            {
                if (isFirst)
                {
                    isFirst = false;
                    document.RenameWorksheet(SLDocument.DefaultFirstSheetName, badge.Name);
                }
                else
                {
                    document.AddWorksheet(badge.Name);
                }

                document.SelectWorksheet(badge.Name);
                WriteHeader(document, badge);
                Api.BadgeProgressReport report;
                if (!progressReports.TryGetValue(badge.Id, out report)) continue;

                var row = 1;
                foreach (var progress in report)
                {
                    document.SetCellStyle(row, 4, dateFormat);
                    WriteProgress(document, badge, progress, ++row);
                }
            }

            var fileName = GetSafeFileName(model);
            document.SaveAs(fileName);
            return fileName;
        }

        private static void WriteHeader(SLDocument document, Badge badge, int row = 1)
        {
            var column = 0;
            document.SetCellValue(row, ++column, "Family Name");
            document.SetCellValue(row, ++column, "First Name");
            document.SetCellValue(row, ++column, "Completed");
            document.SetCellValue(row, ++column, "Date Awarded");
            foreach (var task in badge.Tasks)
            {
                document.SetCellValue(row, ++column, task.Name);
            }
        }

        private void WriteProgress(SLDocument document, Badge badge, BadgeProgress progress, int row)
        {
            var column = 0;
            document.SetCellValue(row, ++column, progress.Member.FamilyName);
            document.SetCellValue(row, ++column, progress.Member.FirstName);
            document.SetCellValue(row, ++column, progress.IsCompleted ? "Yes" : "No");
            ++column;
            if (progress.IsAwarded) document.SetCellValue(row, column, progress.WhenAwarded.Value);
            foreach (var task in badge.Tasks)
            {
                ++column;
                string status;
                if (progress.TaskStatus.TryGetValue(task.Id, out status)) document.SetCellValue(row, column, status);
            }
        }
    }
}