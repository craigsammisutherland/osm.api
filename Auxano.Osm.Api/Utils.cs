using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Auxano.Osm.Api
{
    internal static class Utils
    {
        public static IEnumerable<string> EncodeQueryValues(IDictionary<string, string> values)
        {
            return values.Select(v => v.Key + "=" + Uri.EscapeDataString(v.Value));
        }

        public static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}