using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PartitionKey : UploadableAttribute
    {
        public PartitionKey(Type type = null) : base(type) { }
    }
}
