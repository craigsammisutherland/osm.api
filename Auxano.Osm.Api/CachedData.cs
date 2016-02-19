using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Auxano.Osm.Api
{
    internal class CachedData<TData>
    {
        private readonly TimeSpan expiryDuration;
        private readonly Func<Connection, string, IDictionary<string, string>, Task<TData>> getter;
        private TData cachedValue;
        private DateTime expiryTime = DateTime.MinValue;

        public CachedData(Func<Connection, string, IDictionary<string, string>, Task<TData>> getter, TimeSpan expiryDuration)
        {
            this.getter = getter;
            this.expiryDuration = expiryDuration;
        }

        public CachedData(string url, TimeSpan expiryDuration)
        {
            this.getter = async (connection, query, values) => await RetrieveFromServer(connection, url, query, values);
            this.expiryDuration = expiryDuration;
        }

        public async Task<TData> GetAsync(Connection connection, IDictionary<string, string> values)
        {
            var now = DateTime.UtcNow;
            if (now > this.expiryTime)
            {
                this.cachedValue = await this.getter(connection, null, values);
                this.expiryTime = now.Add(this.expiryDuration);
            }

            return this.cachedValue;
        }

        public async Task<TData> GetAsync(Connection connection, string query, IDictionary<string, string> values)
        {
            var now = DateTime.UtcNow;
            if (now > this.expiryTime)
            {
                this.cachedValue = await this.getter(connection, query, values);
                this.expiryTime = now.Add(this.expiryDuration);
            }

            return this.cachedValue;
        }

        private async static Task<TData> RetrieveFromServer(Connection connection, string url, string query, IDictionary<string, string> values)
        {
            var fullUrl = url +
                (string.IsNullOrEmpty(query)
                    ? string.Empty
                    : ((url.Contains("?") ? "&" : "?") + query));
            var response = await connection.PostAsync(fullUrl, values);
            var parsedTerms = JsonConvert.DeserializeObject<TData>(response);
            return parsedTerms;
        }
    }
}