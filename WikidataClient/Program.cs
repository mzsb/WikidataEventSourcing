using EventSourcing.Logic;
using EventSourcing.Model;
using EventSourcing.Services;
using EventSourcing.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WikidataClient.Converter;
using WikidataClient.Model;
using WikidataClient.Model.Statement;
using WikidataClient.Model.Statement.Subjects;
using WikidataClient.Model.WikidataEntity;
using WikidataEntityLoader;
using EventSourcing.Providers;
using EventSourcing.Events;
using EventSourcing.Interfaces;
using EventSourcing.Helpers;
using EventSourcing.Builders;
using Constant = EventSourcing.Constants.ReferenceServiceConstants;
using Newtonsoft.Json.Serialization;
using EventSourcing.Enums;

namespace WikidataClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string _connectionString = "AccountEndpoint=https://wikidatacosmosdb.documents.azure.com:443/;";
            const string _databaseId = "wikidata";

            var serviceProvider = new ServiceProvider(_databaseId, _connectionString);
            var eventService = serviceProvider.ProvideEventService().WithOptions(new EventServiceOptions() { State = EventServiceState.Test}).Build();

            var entityLoader = new EntityLoader(eventService, new HttpClient());

            var Q42 = await entityLoader.LoadEntity("Q42") as WikidataItem;

            var e = serviceProvider.ProvideEventService().Build();


            var Q42handler = e.GetHandler<WikidataItem, string>(Q42);

            await Q42handler.CreateAsync();

            Q42handler.Path(Q42 => Q42.Statements)["P31"]
                      .Path(P31 => P31.Subjects).First()
                      .Path(Q5 => Q5.Rank)
                      .Set("Test");

            await Q42handler.UpdateAsync();

            _ = 0;
        }
    }
}
