using EventSourcing.Interfaces;
using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Property
{
    public class Alias : IIdentifiable<string>
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public List<string> Values { get; set; } = new();
    }
}
