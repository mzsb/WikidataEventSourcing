using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;
using WikidataClient.Model.WikidataEntity;

namespace WikidataClient.Model.Statement
{
    [ReferenceBase(typeof(WikidataProperty), true)]
    public class Predicate : IUploadable<string>
    {
        public string Id { get; set; }
        [PartitionKey]
        public string EntityType { get; set; } = "property";
        public List<Label> Labels { get; set; } = new();
    }
}
