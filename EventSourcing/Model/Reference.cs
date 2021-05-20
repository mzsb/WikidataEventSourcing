using EventSourcing.Attributes;
using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventSourcing.Model
{
    public class Reference : IUploadable<string>
    {
        public string Id { get; set; }
        [PartitionKey]
        public string EntityPartitionKey { get; set; }
        public Dictionary<string, List<string>> References { get; set; }
    }
}
