using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleQuery.Impl {
    public abstract class QueriesBase {


        private IPersistenceManager persistenceManager;

        public QueriesBase(IPersistenceManager persistenceManager) {
            this.persistenceManager = persistenceManager;
        }

        protected IQuery<T> ByNamedQuery<T>(
            string name,
            object parameters) where T : new() {
            if (!QueryInfoCache.Queries(this).ContainsKey(name)) {
                throw new ArgumentException(string.Format(
                    "Query with the name '{0}' can not be found. Make sure the query xml file is present and is an embedded resource.",
                    name));
            }

            var query = QueryInfoCache.Queries(this)[name];

            var countQuery = query.CountQuery.Trim();
            var selectQuery = query.SelectQuery.Trim();

            if (!string.IsNullOrEmpty(query.BaseQuery)) {
                // This query has a base query, so let's pull it out
                if (!QueryInfoCache.Queries(this).ContainsKey(query.BaseQuery)) {
                    throw new ArgumentException(string.Format(
                        "Query with the name '{0}' can not be found. Make sure the query xml file is present and is an embedded resource.",
                        query.BaseQuery));
                }
                var baseQuery = QueryInfoCache.Queries(this)[query.BaseQuery];
                countQuery = countQuery.Replace("#basequery", baseQuery.CountQuery.Trim());
                selectQuery = selectQuery.Replace("#basequery", baseQuery.SelectQuery.Trim());
            }

            return new SqlServerQuery<T>(persistenceManager,
                countQuery,
                selectQuery,
                parameters.ToDictionary(),
                query.DefaultSort);
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
