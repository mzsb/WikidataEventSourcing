using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReferenceBase : UploadableAttribute
    {
        public bool AllowDifferentStructure { get; }
        public ReferenceBase(Type type, bool allowDifferentStructure = false) : base(type) 
        {
            AllowDifferentStructure = allowDifferentStructure;
        }
    }
}
