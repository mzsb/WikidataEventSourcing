using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class TabularData : Subject
    {
        public const string dataType = "tabular-data";

        public TabularData() : base(dataType) { }

        public string Value { get; set; }
    }
}
