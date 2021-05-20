using EventSourcing.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;
using WikidataClient.Model.Statement.Subjects;

namespace WikidataClient.Model.WikidataEntity
{
    public class WikidataLexeme : WikidataEntity
    {
        public Item LexicalCategory { get; set; }
        public Item Language { get; set; }

        public List<Lemma> Lemmas { get; set; } = new();
        [ReferenceContainer]
        public List<Form> Forms { get; set; } = new();
        [ReferenceContainer]
        public List<Sense> Senses { get; set; } = new();
    }
}
