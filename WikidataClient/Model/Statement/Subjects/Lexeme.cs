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
    [ReferenceBase(typeof(WikidataLexeme), true)]
    public class Lexeme : Subject, IUploadable<string>
    {
        public const string dataType = "wikibase-lexeme";

        public Lexeme() : base(dataType) { }

        [PartitionKey]
        public string EntityType { get; set; } = "lexeme";
        public int NumericId { get; set; }
        public List<Lemma> Lemmas { get; set; } = new();  
    }
}
