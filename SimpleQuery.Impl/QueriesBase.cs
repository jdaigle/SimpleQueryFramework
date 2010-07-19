using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace SimpleQuery.Impl {
    public abstract class QueriesBase {


        private IPersistenceManager persistenceManager;

        public QueriesBase(IPersistenceManager persistenceManager) {
            this.persistenceManager = persistenceManager;
        }

        protected IQuery<T> ByNamedQuery<T>(
            string name,
            object parameters) where T : new() {
            if (!Queries(this).ContainsKey(name)) {
                throw new ArgumentException(string.Format(
                    "Query with the name '{0}' can not be found. Make sure the query xml file is present and is an embedded resource.",
                    name));
            }

            var query = Queries(this)[name];

            return new SqlServerQuery<T>(persistenceManager,
                query.CountQuery.Trim(),
                query.SelectQuery.Trim(),
                parameters.ToDictionary(),
                query.DefaultSort);
        }


        #region QueryCache

        private static Dictionary<Type, Dictionary<string, QueryInfo>> queries = new Dictionary<Type, Dictionary<string, QueryInfo>>();

        private static void EnsureType(Type type) {
            if (!queries.ContainsKey(type)) {
                lock (queries) {
                    if (!queries.ContainsKey(type)) {
                        queries.Add(type, new Dictionary<string, QueryInfo>());

                        try {
                            ExtractQueries(type, type.FullName + ".xml");
                        } catch { }
                    }
                }
            }
        }
        private static void ExtractQueries(Type type, string queryFile) {
            using (var stream = type.Assembly.GetManifestResourceStream(queryFile)) {
                if (stream != null) {
                    using (var reader = new XmlTextReader(stream)) {
                        if (MoveToNextElement(reader)) {
                            // For each query element
                            while (MoveToNextElement(reader)) {
                                // Get the name attribute
                                reader.MoveToFirstAttribute();
                                string name = reader.Value;

                                // Get the default sort attribute
                                reader.MoveToNextAttribute();
                                string defaultSort = reader.Value;

                                // Move to the count element
                                MoveToNextElement(reader);
                                reader.Read(); // Move to text or CDATA node
                                string countQuery = reader.Value;

                                // Move to the select element
                                MoveToNextElement(reader);
                                reader.Read(); // Move to text or CDATA node
                                string selectQuery = reader.Value;

                                queries[type].Add(name, new QueryInfo(countQuery, selectQuery, defaultSort));
                            }
                        }
                    }
                }
            }
        }

        private static bool MoveToNextElement(XmlTextReader reader) {
            bool resVal = true;
            while ((resVal = reader.Read()) && reader.NodeType != XmlNodeType.Element) ;
            return resVal;
        }

        public static IDictionary<string, QueryInfo> Queries(QueriesBase dtoQueries) {
            EnsureType(dtoQueries.GetType());
            return queries[dtoQueries.GetType()];
        }
        #endregion
    }

    public class QueryInfo {
        public readonly string CountQuery;
        public readonly string SelectQuery;
        public readonly string DefaultSort;

        public QueryInfo(string countQuery, string selectQuery, string defaultSort) {
            CountQuery = countQuery;
            SelectQuery = selectQuery;
            DefaultSort = defaultSort;
        }
    }

    public static class ObjectExtensions {
        /// <summary>
        /// Converts the given anonymous object to a dictionary.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object o) {
            Dictionary<string, object> resVal = new Dictionary<string, object>();

            if (o != null) {
                foreach (System.ComponentModel.PropertyDescriptor prop in TypeDescriptor.GetProperties(o)) {
                    resVal.Add(prop.Name, prop.GetValue(o));
                }
            }

            return resVal;
        }

    }
}
