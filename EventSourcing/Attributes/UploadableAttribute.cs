using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Attributes
{
    public abstract class UploadableAttribute : Attribute
    {
        public Type Type { get; }

        public UploadableAttribute(Type type)
        {
            if (type is not null && !type.IsImplementAny(typeof(IUploadable<>)))
            {
                throw new Exception($"Type parameter of {GetType().Name} must implement {typeof(IUploadable<>).Name} interface.");
            }

            Type = type;
        }
    }
}
