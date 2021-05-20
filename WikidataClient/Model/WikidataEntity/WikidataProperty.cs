using EventSourcing.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;

namespace WikidataClient.Model.WikidataEntity
{
    public class WikidataProperty : WikidataEntity
    {
        public string DataType { get; set; }

        public List<Label> Labels { get; set; } = new();

        public List<Description> Descriptions { get; set; } = new();

        public List<Alias> Aliases { get; set; } = new();
    }
}
