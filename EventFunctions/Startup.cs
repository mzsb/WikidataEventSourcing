using EventSourcing.Interfaces;
using EventSourcing.Logic;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.WikidataEntity;
using WikidataEventTrigger;

[assembly: FunctionsStartup(typeof(Startup))]
namespace WikidataEventTrigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvide = new EventSourcing.Providers.ServiceProvider("wikidata", Environment.GetEnvironmentVariable("wikidatacosmosdb_connection"));
            builder.Services.AddSingleton(serviceProvide.ProvideDataService<WikidataEntity, string>().Build());
            builder.Services.AddTransient<IEventTriggerLogic, EventTriggerLogic>();
        }
    }

}
