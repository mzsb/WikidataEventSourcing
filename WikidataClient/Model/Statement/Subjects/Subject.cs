using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WikidataClient.Converter;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Statement.Subjects
{
    public abstract class Subject : IIdentifiable<string>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DataType { get; set; }
        public string Rank { get; set; }

        public List<Statement> References { get; set; } = new();
        public List<Statement> Qualifiers { get; set; } = new();

        public Subject(string dataType)
        {
            DataType = dataType;
        }
    }
}
