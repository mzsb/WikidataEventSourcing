using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class URL : Subject
    {
        public const string dataType = "url";

        public URL() : base(dataType) { }

        public string Value { get; set; }
    }
}
