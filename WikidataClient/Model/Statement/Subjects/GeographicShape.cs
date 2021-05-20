using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class GeographicShape : Subject
    {
        public const string dataType = "geo-shape";

        public GeographicShape() : base(dataType) { }

        public string Value { get; set; }
    }
}
