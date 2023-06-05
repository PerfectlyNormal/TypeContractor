using System;
using System.Collections.Generic;

namespace ExampleContracts.v1
{
    public class ComplexDictionary
    {
        public Dictionary<Guid, CustomClass> KeyValues { get; set; }
    }
}
