using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Property
{
    public class Redirects
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}
