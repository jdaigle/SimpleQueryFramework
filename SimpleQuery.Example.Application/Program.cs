using System.Linq;
using SimpleQuery.Example.ViewModel.Queries;
using SimpleQuery.Impl;

namespace SimpleQuery.Example.Application {
    public class Program {
        public static void Main(string[] args) {
            var pm = new ADONETPersistenceManager(@"Data Source=.\SQLEXPRESS;Initial Catalog=Northwind;Trusted_Connection=True;");

            var queries = new CustomerReportQueries(pm);


            // Should all customers
            //var allCustomersCount = queries.All().Count();
            //var allCustomers = queries.All().Execute().ToArray();
            //var allSortedCustomers = queries.All().Execute("CustomerId", SortDirection.Desc).ToArray();
            //var someCustomers = queries.All().Execute(0, 20, "CustomerId", SortDirection.Desc).ToArray();
            //var someOtherCustomers = queries.All().Execute(20, 20, "CustomerId", SortDirection.Desc).ToArray();

            // Should get a specific customer
            var theOneCustomerCount = queries.ById("BONAP").Count();
            var singleCustomer = queries.ById("BONAP").ExecuteSingle();
            var theOneCustomer = queries.ById("BONAP").Execute().FirstOrDefault();
        }
    }
}
