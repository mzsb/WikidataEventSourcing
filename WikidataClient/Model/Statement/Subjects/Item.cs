using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;
using WikidataClient.Model.WikidataEntity;

namespace WikidataClient.Model.Statement.Subjects
{
    [ReferenceBase(typeof(WikidataItem), true)]
    public class Item : Subject, IUploadable<string>
    {
        public const string dataType = "wikibase-item";

        public Item() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "item";
        public int NumericId { get; set; }
        public List<Description> Descriptions { get; set; } = new();
        public List<Label> Labels { get; set; } = new();
    }
}
