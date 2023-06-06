using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleContracts.v1
{
    public class YearSummary
    {
        public class Request
        {
            public Guid OrganizationId { get; set; }
        }

        public class Response
        {
            public IEnumerable<int> Years { get; set; }
            public Dictionary<int, int> PaymentsPerYear { get; set; }
        }
    }
}
