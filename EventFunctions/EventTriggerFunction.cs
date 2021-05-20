using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventFunctions.Helpers;
using EventSourcing.Converters;
using EventSourcing.Enums;
using EventSourcing.Events;
using EventSourcing.Interfaces;
using EventSourcing.Logic;
using EventSourcing.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Constant = EventSourcing.Constants.EventServiceConstants;

namespace EventFunctions
{
    public class EventTriggerFunction
    {
        private readonly IEventTriggerLogic _eventTriggerLogic;

        private const string _databaseName = "wikidata";

        private const string _eventContainerId = Constant.DefaultContainerId; 

        private const string _collectionString = "wikidatacosmosdb_connection";

        private const string _leaseContainerId = "leases"; 

        public EventTriggerFunction(IEventTriggerLogic eventTriggerLogic) 
        {
            _eventTriggerLogic = eventTriggerLogic;
        }

        [FunctionName("EventTrigger")]
        public async Task Run([CosmosDBTrigger(
        databaseName: _databaseName,
        collectionName: _eventContainerId,
        ConnectionStringSetting = _collectionString,
        LeaseCollectionName = _leaseContainerId)] IReadOnlyList<Document> input, ILogger log)
        {
            if (input is { } events && events.Count > 0)
            {
                await _eventTriggerLogic.RunAsync(input.Select(e => e.ToString()));
            }
        }
    }
}
