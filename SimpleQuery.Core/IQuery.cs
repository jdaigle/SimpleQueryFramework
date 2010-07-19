using System.Collections.Generic;

namespace SimpleQuery {
    public interface IQuery<T> {
        int Count();
        T ExecuteSingle();
        IEnumerable<T> Execute();
        IEnumerable<T> Execute(int skip, int take);
        IEnumerable<T> Execute(string sortColumn, SortDirection? sortDirection);
        IEnumerable<T> Execute(int skip, int take, string sortColumn, SortDirection? sortDirection);
    }
}
