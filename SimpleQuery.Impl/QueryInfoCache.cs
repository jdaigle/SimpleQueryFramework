using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SimpleQuery.Impl {
    public static class QueryInfoCache {
        private static Dictionary<Type, Dictionary<string, QueryInfo>> queries = new Dictionary<Type, Dictionary<string, QueryInfo>>();

        private static void EnsureType(Type type) {
            if (!queries.ContainsKey(type)) {
                lock (queries) {
                    if (!queries.ContainsKey(type)) {
                        queries.Add(type, new Dictionary<string, QueryInfo>());
                        ExtractQueries(type, type.FullName + ".xml");
                    }
                }
            }
        }

        private static void ExtractQueries(Type type, string queryFile) {
            using (var stream = type.Assembly.GetManifestResourceStream(queryFile)) {
                if (stream != null) {
                    var loadedQueries = new XmlSerializer(typeof(QueryInfoCollection)).Deserialize(stream) as QueryInfoCollection;
                    foreach (var query in loadedQueries.QueryInfo) {
                        queries[type].Add(query.Name, query);
                    }
                }
            }
        }

        public static IDictionary<string, QueryInfo> Queries(QueriesBase dtoQueries) {
            EnsureType(dtoQueries.GetType());
            return queries[dtoQueries.GetType()];
        }
    }
}
