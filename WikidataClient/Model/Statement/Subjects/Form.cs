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
    [ReferenceBase(typeof(WikidataForm), true)]
    public class Form : Subject, IUploadable<string>
    {
        public const string dataType = "wikibase-form";

        public Form() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "form";
        public List<Representation> Representations { get; set; } = new();
    }
}
