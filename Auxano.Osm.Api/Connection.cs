using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Auxano.Osm.Api
{
    internal class Connection
                : IAuthorisation

    {
        private readonly string apiId;
        private readonly string secret;
        private readonly string token;
        private readonly string userId;

        public Connection(string apiId, string token, string userId, string secret)
        {
            this.apiId = apiId;
            this.token = token;
            this.secret = secret;
            this.userId = userId;
        }

        public bool IsAuthorised
        {
            get { return !string.IsNullOrEmpty(this.secret) && !string.IsNullOrEmpty(this.userId); }
        }

        public string Secret
        {
            get { return this.secret; }
        }

        public string UserId
        {
            get { return this.userId; }
        }

        public Connection AddAuthorisation(string newUserId, string newSecret)
        {
            return new Connection(this.apiId, this.token, newUserId, newSecret);
        }

        public async Task<string> PostAsync(string url, IDictionary<string, string> values)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://osm.scouts.org.nz");
                var baseValues = new Dictionary<string, string>
                {
                    ["token"] = this.token,
                    ["apiid"] = this.apiId
                };
                if (!string.IsNullOrEmpty(this.secret)) baseValues.Add("secret", this.secret);
                if (!string.IsNullOrEmpty(this.secret)) baseValues.Add("userid", this.userId);

                values = values ?? new Dictionary<string, string>(0);
                var data = string.Join("&",
                    Utils.EncodeQueryValues(baseValues).Concat(Utils.EncodeQueryValues(values)));
                var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }
    }
}