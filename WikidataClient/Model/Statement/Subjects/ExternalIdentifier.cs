using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class ExternalIdentifier : Subject
    {
        public const string dataType = "external-id";

        public ExternalIdentifier() : base(dataType) { }

        public string Value { get; set; }
    }
}
