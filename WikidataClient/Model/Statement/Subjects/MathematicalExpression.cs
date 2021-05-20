using System;
using System.Collections.Generic;
using System.Text;

namespace WikidataClient.Model.Statement.Subjects
{
    public class MathematicalExpression : Subject
    {
        public const string dataType = "math";

        public MathematicalExpression() : base(dataType) { }

        public string Value { get; set; }
    }
}
