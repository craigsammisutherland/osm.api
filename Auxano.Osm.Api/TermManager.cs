using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for working with terms.
    /// </summary>
    public class TermManager
    {
        private readonly Connection connection;

        internal TermManager(Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Lists all the terms.
        /// </summary>
        /// <param name="section">The <see cref="Section"/> to list the terms for.</param>
        /// <returns>A list of all the terms.</returns>
        public async Task<TermArray> ListForSectionAsync(Section section)
        {
            var parsedTerms = await connection.PostAsync<Dictionary<string, TermResponse[]>>("api.php?action=getTerms", null);
            TermResponse[] termsToConvert;
            if (parsedTerms.TryGetValue(section.Id, out termsToConvert))
            {
                var terms = termsToConvert
                    .Select(t => new Term(t.name, t.termid, Utils.ParseDate(t.startdate).Value, Utils.ParseDate(t.enddate).Value, section));
                return new TermArray(terms);
            }

            return new TermArray(new Term[0]);
        }

        private class TermResponse
        {
            public string enddate { get; set; }
            public string name { get; set; }
            public string sectionid { get; set; }
            public string startdate { get; set; }
            public string termid { get; set; }
        }
    }
}