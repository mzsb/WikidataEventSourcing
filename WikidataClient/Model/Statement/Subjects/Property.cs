using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcing.Interfaces;
using WikidataClient.Model.Property;
using WikidataClient.Model.WikidataEntity;

namespace WikidataClient.Model.Statement.Subjects
{
    [ReferenceBase(typeof(WikidataProperty), true)]
    public class Property : Subject, IUploadable<string>
    {
        public const string dataType = "wikibase-property";

        public Property() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "property";
        public int NumericId { get; set; }
        public List<Description> Descriptions { get; set; } = new();
        public List<Label> Labels { get; set; } = new();
    }
}
