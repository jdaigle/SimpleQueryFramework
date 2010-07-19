using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleQuery.Example.ViewModel {
    public class CustomerReport {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
    }

    public class Address {
        public string Street { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
    }
}
