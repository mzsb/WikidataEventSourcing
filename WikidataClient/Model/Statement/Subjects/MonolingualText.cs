using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;

namespace WikidataClient.Model.Statement.Subjects
{
    public class MonolingualText : Subject
    {
        public const string dataType = "monolingualtext";

        public MonolingualText() : base(dataType) { }

        public string Text { get; set; }
        public string Language { get; set; }
    }
}
