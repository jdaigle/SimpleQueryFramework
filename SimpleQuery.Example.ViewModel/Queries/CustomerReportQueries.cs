using SimpleQuery.Impl;

namespace SimpleQuery.Example.ViewModel.Queries {
    public class CustomerReportQueries : QueriesBase {
        public CustomerReportQueries(IPersistenceManager persistenceManager) : base(persistenceManager) { }

        public IQuery<CustomerReport> All() {
            return ByNamedQuery<CustomerReport>("All", null);
        }

        public IQuery<CustomerReport> ById(string id) {
            return ByNamedQuery<CustomerReport>("ById", new { Id = id });
        }

        public IQuery<CustomerReport> ByCustomerName(string name) {
            return ByNamedQuery<CustomerReport>("ByCustomerName", new { Name = name });
        }
    }
}
