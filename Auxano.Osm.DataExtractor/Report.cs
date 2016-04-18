using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auxano.Osm.Api;

namespace Auxano.Osm.DataExtractor
{
    public abstract class Report
    {
        protected Report(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public static IEnumerable<Report> All()
        {
            var reportType = typeof(Report);
            var assembly = reportType.Assembly;
            var reports = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(reportType))
                .Select(t => (Report)Activator.CreateInstance(t))
                .ToArray();
            return reports;
        }

        public abstract Task Generate(ViewModel model, Manager manager);
    }
}