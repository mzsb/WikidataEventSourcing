using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Property
{
    public class SiteLink : IIdentifiable<string>
    {
        public string Id { get; set; }
        public string Site { get; set; }
        public string Title { get; set; }
        public List<string> Badges { get; set; } = new();
    }
}
