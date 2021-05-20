using EventSourcing.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Property;

namespace WikidataClient.Model.WikidataEntity
{
    public class WikidataItem : WikidataEntity
    {

        public List<Label> Labels { get; set; } = new();

        public List<Description> Descriptions { get; set; } = new();

        public List<Alias> Aliases { get; set; } = new();

        public List<SiteLink> SiteLinks { get; set; } = new();
    }
}
