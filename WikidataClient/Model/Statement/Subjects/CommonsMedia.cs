using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class CommonsMedia : Subject
    {
        public const string dataType = "commonsMedia";

        public CommonsMedia() : base(dataType) { }

        public string Value { get; set; }
    }
}
