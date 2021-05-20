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
    [ReferenceBase(typeof(WikidataSense), true)]
    public class Sense : Subject, IUploadable<string>
    {
        public const string dataType = "wikibase-sense";
        public Sense() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "sense";
        public List<Gloss> Glosses { get; set; } = new();
    }
}
