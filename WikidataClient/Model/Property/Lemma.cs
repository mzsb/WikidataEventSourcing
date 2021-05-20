using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Property
{
    public class Lemma : IIdentifiable<string>
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
    }
}
