using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EventSourcing.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferenceContainer : Attribute { }
}
