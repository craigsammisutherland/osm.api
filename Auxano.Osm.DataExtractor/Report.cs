using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;

namespace Auxano.Osm.DataExtractor
{
    public abstract class Report
    {
        protected Report(string name, string extension = ".xlsx")
        {
            this.Name = name;
            this.Extension = extension;
        }

        public string Extension { get; private set; }

        public string Name { get; private set; }

        public static IEnumerable<Report> All()
        {
            var reportType = typeof(Report);
            var assembly = reportType.Assembly;
            var reports = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(reportType))
                .Select(t => (Report)Activator.CreateInstance(t))
                .OrderBy(r => r.Name)
                .ToArray();
            return reports;
        }

        public abstract Task<string> Generate(ViewModel model, Manager manager);

        protected static string GetSafeFileName(ViewModel model)
        {
            var fileName = model.SelectedFile;
            var copy = 0;
            var extension = Path.GetExtension(fileName);
            var basePath = fileName.Remove(fileName.Length - extension.Length);
            while (File.Exists(fileName))
            {
                fileName = basePath + " (Copy " + ++copy + ")" + extension;
            }

            return fileName;
        }
    }
}