using System.Collections.Generic;

namespace ExampleContracts.v1
{
    public class DtoWithDictionary
    {
        public string Name { get; set; }
        public Dictionary<string, int> Values { get; set; }
    }
}
