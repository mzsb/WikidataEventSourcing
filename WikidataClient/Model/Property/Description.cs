using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcing.Interfaces;
using EventSourcing.Attributes;

namespace WikidataClient.Model.Property
{
    public class Description : IIdentifiable<string>
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
    }
}
