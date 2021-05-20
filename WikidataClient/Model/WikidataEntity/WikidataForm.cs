using EventSourcing.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;
using WikidataClient.Model.Statement.Subjects;

namespace WikidataClient.Model.WikidataEntity
{
    public class WikidataForm : WikidataEntity
    {
        public List<Representation> Representations { get; set; } = new();
        [ReferenceContainer]
        public List<Item> GrammaticalFeatures { get; set; } = new();
    }
}
