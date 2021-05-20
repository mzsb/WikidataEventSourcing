using EventSourcing.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Model
{
    public class UpdateSegment
    {

        public string Path { get; set; } 

        public object Value { get; set; }

        public DateTime CreationDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ListUpdateType ListUpdateType { get; set; }
    }
}
