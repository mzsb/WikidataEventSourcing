using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Statement.Subjects
{
    public class Quantity : Subject, IUploadable<string>
    {
        public const string dataType = "quantity";

        public Quantity() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "item";
        public string Amount { get; set; }
        public Label Label { get; set; }
    }
}
