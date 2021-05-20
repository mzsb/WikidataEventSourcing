using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class MusicalNotation : Subject
    {
        public const string dataType = "musical-notation";

        public MusicalNotation() : base(dataType) { }

        public string Value { get; set; }
    }
}
