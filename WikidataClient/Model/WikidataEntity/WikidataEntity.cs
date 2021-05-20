using EventSourcing.Attributes;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WikidataClient.Model.Property;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.WikidataEntity
{
    public abstract class WikidataEntity : IUploadable<string>
    {
        public string Id { get; set; }
        public int PageId { get; set; }
        public int Ns { get; set; }
        public int LastrevId { get; set; }
        public DateTime Modified { get; set; }
        [PartitionKey]
        public string Type { get; set; }
        public Redirects Redirects { get; set; }
        [ReferenceContainer]
        public List<Statement.Statement> Statements { get; set; } = new();
    }
}
