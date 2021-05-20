using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class GlobeCoordinate : Subject
    {
        public const string dataType = "globecoordinate";

        public GlobeCoordinate() : base(dataType) { }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public object Altitude { get; set; }
        public float Precision { get; set; }
        public string Globe { get; set; }
    }
}
