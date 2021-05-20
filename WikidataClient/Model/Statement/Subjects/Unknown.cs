using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikidataClient.Model.Statement.Subjects
{
    public class Unknown : Subject
    {
        public const string dataType = "unknown";
        public Unknown() : base(dataType) { }

        public string Value { get; set; }
    }
}
