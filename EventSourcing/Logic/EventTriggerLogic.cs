using EventSourcing.Enums;
using EventSourcing.Events;
using EventSourcing.Model;
using EventSourcing.Services;
using EventSourcing.Helpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Constant = EventSourcing.Constants.ReferenceServiceConstants;
using Newtonsoft.Json.Serialization;
using EventSourcing.Interfaces;
using EventSourcing.Converters;

namespace EventSourcing.Logic
{
    public class EventTriggerLogic : IEventTriggerLogic
    {
        private readonly ILogger _logger;
        private readonly DataService _dataService;
        private readonly ReferenceService _referenceService;

        public bool TestState { get; set; } = false;
        public List<JObject> JsonEntities { get; set; } = new(); 
        public List<JObject> UpdatedJsonEntities { get; set; } = new();  
        public List<JObject> UpdatedReferencedJsonEntities { get; set; } = new(); 

        public EventTriggerLogic(DataService dataService, ReferenceService referenceService = null, ILogger logger = null)
        {
            _dataService = dataService;
            _referenceService = referenceService;
            _logger = logger;
        }

        public async Task RunAsync(IEnumerable<string> jsonEvents) 
        {
            var converters = new List<JsonConverter>() { new EventConverter() };
            await RunAsync(jsonEvents.Select(e =>
                JsonConvert.DeserializeObject<
                    EventBase>(e.ToString(),
                    new JsonSerializerSettings() { Converters = converters }))
            );
        }

        public async Task RunAsync(IEnumerable<EventBase> events)
        {
            var updateEvents = new List<UpdateEvent>();
            var removeEvents = new List<RemoveEvent>();
            var insertEvents = new List<InsertEvent>();

            foreach (var e in events)
            {
                switch (e.Type)
                {
                    case EventType.Insert:

                        insertEvents.Add(e as InsertEvent);

                        break;

                    case EventType.Remove:

                        removeEvents.Add(e as RemoveEvent);

                        break;

                    case EventType.Update:

                        updateEvents.Add(e as UpdateEvent);

                        break;
                }
            }

            try
            {
                if (insertEvents.Count > 0)
                {
                    foreach (var insertEvent in insertEvents)
                    {
                        if (TestState)
                        {
                            JsonEntities.Add(JObject.Parse(insertEvent.JsonEntity));
                        }
                        else
                        {
                            await _dataService.InsertAsync(insertEvent);
                        }
                    }
                }

                if (removeEvents.Count > 0)
                {
                    foreach (var removeEvent in removeEvents)
                    {
                        if (TestState)
                        {
                            JsonEntities.Remove(JsonEntities.Single(e => e["id"].Value<string>() == removeEvent.EntityId));
                        }
                        else
                        {
                            await _dataService.RemoveAsync(removeEvent);
                        }
                    }
                }

                if (updateEvents.Count > 0)
                {
                    foreach (var updateEvent in updateEvents)
                    {
                        JObject entity = new();
                        if (TestState)
                        {
                            entity = JsonEntities.Single(e => e["id"].Value<string>() == updateEvent.EntityId);
                        }
                        else
                        {
                            entity = await _dataService.ReadAsync(updateEvent);
                        }

                        foreach (var updateSegment in updateEvent.UpdateSegments)
                        {
                            UpdateEntity(entity, updateSegment.Path, updateSegment.Value, updateSegment.ListUpdateType);
                        }

                        if (TestState)
                        {
                            UpdatedJsonEntities.Add(entity);
                        }
                        else
                        {
                            await _dataService.UpdateAsync(entity, updateEvent);
                        }
                    }
                }
            } catch(Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private async Task GetUpdatedReferencedEntities(UpdateEvent updateEvent)
        {
            var reference = await _referenceService.GetReferenceAsync(updateEvent.EntityId, updateEvent.EntityPartitionKey);

            var entities = await _dataService.GetEntitiesAsync();

            foreach (var pair in reference.References)
            {
                var entity = entities.Single(t => t.Value<string>("id") == pair.Key.ToPascalCase());

                foreach (var segment in updateEvent.UpdateSegments)
                {
                    foreach (var path in pair.Value)
                    {
                        UpdateEntity(entity, path, segment.Value, segment.ListUpdateType);
                    }
                }

                //await _dataService.UpdateAsync(entity, updateEvent);
            }
        }


        private void UpdateEntity(JObject entity, string path, object rawValue, ListUpdateType listUpdateType)
        {
            rawValue ??= new();
            var value = JToken.FromObject(rawValue, new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var pathSegments = path.Split(Constant.PathSegmentSeparator);
            string pathSegment = string.Empty;
            string[] listPathSegments = null;
            JContainer property = entity;
            int index = 0;
            while (index < pathSegments.Length - 1)
            {
                pathSegment = pathSegments[index];

                listPathSegments = GetListPathSegments(pathSegment);

                pathSegment = listPathSegments is { } ? listPathSegments[0] : pathSegment;

                if (property.Value<JContainer>(pathSegment) is null)
                {
                    property[pathSegment] = listPathSegments is { } ?
                        new JArray { JObject.FromObject(new { id = listPathSegments[1] }) } :
                        new JObject();
                }

                property = listPathSegments is { } ?
                    property.Value<JArray>(pathSegment).Single(t => t.Value<string>("id") == listPathSegments[1]) as JObject :
                    property.Value<JContainer>(pathSegment);

                index++;
            }

            pathSegment = pathSegments.Last();

            listPathSegments = GetListPathSegments(pathSegment);

            pathSegment = listPathSegments is { } ? listPathSegments[0] : pathSegment;

            if (listUpdateType == ListUpdateType.None)
            {
                if (listPathSegments is { })
                {
                    var l = property[pathSegment] as JArray;
                    var i = value;
                    l.Single(t => t.Value<string>("id") == listPathSegments[1]).Replace(i);
                }
                else
                {
                    property[pathSegment] = value;
                }
            }
            else if (property[pathSegment] is JArray listProperty)
            {
                if (listUpdateType == ListUpdateType.Add)
                {
                    listProperty.Add(value);
                }
                else if (listUpdateType == ListUpdateType.Remove)
                {
                    listProperty.Remove(listProperty.Single(t => t.Value<string>("id") == value.Value<string>("id")));
                }
                else if (listUpdateType == ListUpdateType.Clear)
                {
                    listProperty.Clear();
                }
            }
        }

        private string[] GetListPathSegments(string pathSegment) =>
            pathSegment.Split(Constant.ListIndexSeparator) is { } list && list.Length == 2 ? list : null;
    }
}
