using EventSourcing.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;

namespace WikidataClient.Model.WikidataEntity
{
    public class WikidataSense : WikidataEntity
    {

        public List<Gloss> Glosses { get; set; } = new();
    }
}
