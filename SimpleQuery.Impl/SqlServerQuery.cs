using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace SimpleQuery.Impl {
    [Serializable]
    public class SqlServerQuery<T> : IQuery<T> where T : new() {

        private string countQuery;
        private string selectQuery;
        private string defaultSort;
        private IDictionary<string, object> parameters;
        private IDictionary<string, int> parameterSizes;
        private IPersistenceManager persistenceManager;

        private static readonly Regex ORDER_BY_REGEX = new Regex(@"\sorder\sby.*", RegexOptions.IgnoreCase);
        private static readonly Regex SELECT_REGEX = new Regex("^select", RegexOptions.IgnoreCase);

        public SqlServerQuery(IPersistenceManager persistenceManager,
                            string countQuery,
                            string selectQuery,
                            IDictionary<string, object> parameters,
                            IDictionary<string, int> parameterSizes,
                            string defaultSort) {
            this.persistenceManager = persistenceManager;
            this.countQuery = countQuery;
            this.selectQuery = selectQuery;
            this.parameters = parameters;
            this.parameterSizes = parameterSizes;
            this.defaultSort = defaultSort;
        }

        public int Count() {
            var needToOpen = !persistenceManager.IsOpened();
            if (needToOpen) persistenceManager.Open();
            try {
                var query = GetBaseQuery(countQuery);
                var count = (int)query.ExecuteScalar();
                if (needToOpen) persistenceManager.Commit();
                return count;
            } catch (Exception) {
                if (needToOpen) persistenceManager.Rollback();
                throw;
            }
        }

        public T ExecuteSingle() {
            var needToOpen = !persistenceManager.IsOpened();
            if (needToOpen) persistenceManager.Open();
            try {
                var query = GetBaseQuery(countQuery);
                T result = default(T);
                using (var reader = GetBaseQuery(selectQuery).ExecuteReader()) {
                    result = DTOMapper.Map<T>(reader);
                }
                if (needToOpen) persistenceManager.Commit();
                return result;
            } catch (Exception) {
                if (needToOpen) persistenceManager.Rollback();
                throw;
            }
        }

        public IEnumerable<T> Execute() {
            var needToOpen = !persistenceManager.IsOpened();
            if (needToOpen) persistenceManager.Open();
            try {
                var query = GetBaseQuery(countQuery);
                IEnumerable<T> results = null;
                using (var reader = GetBaseQuery(selectQuery).ExecuteReader()) {
                    results = DTOMapper.MapSet<T>(reader);
                }
                if (needToOpen) persistenceManager.Commit();
                return results;
            } catch (Exception) {
                if (needToOpen) persistenceManager.Rollback();
                throw;
            }
        }

        public IEnumerable<T> Execute(int skip, int take) {
            return Execute(skip, take, null, null);
        }

        public IEnumerable<T> Execute(string sortColumn, SortDirection? sortDirection) {
            return Execute(0, 0, sortColumn, sortDirection);
        }

        public IEnumerable<T> Execute(int skip, int take, string sortColumn, SortDirection? sortDirection) {
            if (string.IsNullOrEmpty(sortColumn)) {
                sortColumn = defaultSort;
            }

            var sorts = string.Format("{0} {1}", sortColumn, sortDirection.GetValueOrDefault() == SortDirection.Asc ? "ASC" : "DESC");
            return DoListPage(GetBaseQuery(selectQuery), skip, take, sorts);
        }

        private IDbCommand GetBaseQuery(string query) {
            var cmd = persistenceManager.CreateCommand();
            cmd.CommandText = query;

            if (parameters != null) {
                foreach (KeyValuePair<string, object> kvp in parameters) {
                    var param = cmd.CreateParameter();
                    param.ParameterName = kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key;
                    if (parameterSizes.ContainsKey(kvp.Key))
                        param.Size = parameterSizes[kvp.Key];
                    param.Value = kvp.Value;
                    cmd.Parameters.Add(param);
                }
            }

            return cmd;
        }

        private IEnumerable<T> DoListPage(IDbCommand query, int offset, int limit, string sorts) {
            if (offset > 0 && limit > 0) {
                AddPaginationToQuery(query, limit, offset, sorts);
            } else if (offset > 0) {
                AddOffsetToQuery(query, offset, sorts);
            } else if (limit > 0) {
                AddTopToQuery(query, limit, sorts);
            } else {
                AddSortsToQuery(query, sorts);
            }

            var needToOpen = !persistenceManager.IsOpened();
            if (needToOpen) persistenceManager.Open();
            try {
                using (var reader = query.ExecuteReader()) {
                    var results = DTOMapper.MapSet<T>(reader);
                    if (needToOpen) persistenceManager.Commit();
                    return results;
                }
            } catch (Exception) {
                if (needToOpen) persistenceManager.Rollback();
                throw;
            }
        }

        private void AddSortsToQuery(IDbCommand query, string sorts) {
            query.CommandText += string.IsNullOrEmpty(sorts) ? string.Empty : " ORDER BY " + sorts;
        }

        private void AddTopToQuery(IDbCommand query, int top, string sorts) {
            query.CommandText = SELECT_REGEX.Replace(query.CommandText,
                string.Format("SELECT TOP {0}", top));
            AddSortsToQuery(query, sorts);
        }

        private void AddOffsetToQuery(IDbCommand query, int offset, string sorts) {
            query.CommandText = string.Format(
                "select * from ({0}) results  where RowNumber > {1}",
                SELECT_REGEX.Replace(
                    query.CommandText,
                    string.Format("select ROW_NUMBER() over (order by {0}) as RowNumber, ", sorts)),
                offset);
        }
        private void AddPaginationToQuery(IDbCommand query, int top, int offset, string sorts) {
            query.CommandText = string.Format(
                "select top {2} * from ({0}) results  where RowNumber > {1}",
                SELECT_REGEX.Replace(
                    query.CommandText,
                    string.Format("select ROW_NUMBER() over (order by {0}) as RowNumber, ", sorts)),
                offset,
                top);
        }
    }
}
