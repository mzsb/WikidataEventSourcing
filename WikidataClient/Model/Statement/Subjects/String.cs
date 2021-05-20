using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikidataClient.Model.Statement.Subjects;

namespace WikidataClient.Model.Statement.Subjects
{
    public class String : Subject
    {
        public const string dataType = "string";

        public String() : base(dataType) { }

        public string Value { get; set; }
    }
}
