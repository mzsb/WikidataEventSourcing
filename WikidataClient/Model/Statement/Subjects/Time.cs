using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Statement.Subjects
{
    public class Time : Subject, IUploadable<string>
    {
        public const string dataType = "time";

        public Time() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "itme";
        public string Value { get; set; }
        public int Timezone { get; set; }
        public int Precision { get; set; }
        public int Befor { get; set; }
        public int After { get; set; }
        public Label Label { get; set; }
    }
}
