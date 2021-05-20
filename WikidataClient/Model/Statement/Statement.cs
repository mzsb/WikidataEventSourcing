using EventSourcing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.Statement.Subjects;
using EventSourcing.Interfaces;

namespace WikidataClient.Model.Statement
{
    public class Statement : IIdentifiable<string>
    {
        public string Id { get; set; }

        public Predicate Predicate { get; set; }
        public List<Subject> Subjects { get; set; } = new();
    }
}
