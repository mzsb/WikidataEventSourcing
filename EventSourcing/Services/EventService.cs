using EventSourcing.Enums;
using EventSourcing.Events;
using EventSourcing.Handlers;
using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using EventSourcing.Model;
using EventSourcing.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventSourcing.Services
{
    public class EventService : ServiceBase
    {
        private readonly ReferenceService _referenceService;

        private readonly List<EventBase> _events = new();
        private readonly EventServiceOptions _options;

        internal EventService(Container container,
                              EventServiceOptions options) : base(container)
        {
            _options = options;
            _referenceService = _options.ReferenceService;
        }

        internal async Task PublishEventAsync(EventBase eventBase)
        {
            object @event = eventBase.Type switch
            {
                EventType.Insert => eventBase as InsertEvent,
                EventType.Remove => eventBase as RemoveEvent,
                EventType.Update => eventBase as UpdateEvent,
                _ => throw new Exception("Invalid event type.")
            };

            if (_options.State == EventServiceState.Upload)
            {
                var response = await _container.CreateItemAsync(@event, new PartitionKey(eventBase.EntityId));

                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception($"Event publish error: ({response.StatusCode})");
                }
            }
            
            _events.Add(eventBase);

            if(_referenceService is not null)
            {
                await _referenceService.UploadReferencesAsync(eventBase.EntityId, eventBase.Id);
            }
        }

        public List<EventBase> GetEvents() => _events;

        public void ClearEvents() => _events.Clear(); 

        public async Task ClearAsync()
        {
            using (var feedIterator = _container.GetItemLinqQueryable<UpdateEvent>()
                .Where(e => e.Type == EventType.Update)
                .ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var e in await feedIterator.ReadNextAsync())
                    {
                        await _container.DeleteItemAsync<UpdateEvent>(e.Id.ToString(), new PartitionKey(e.EntityId));
                    }
                }
            }

            using (var feedIterator = _container.GetItemLinqQueryable<InsertEvent>()
                .Where(e => e.Type == EventType.Insert)
                .ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var e in await feedIterator.ReadNextAsync())
                    {
                        await _container.DeleteItemAsync<InsertEvent>(e.Id.ToString(), new PartitionKey(e.EntityId));
                    }
                }
            }

            using (var feedIterator = _container.GetItemLinqQueryable<RemoveEvent>()
                .Where(e => e.Type == EventType.Remove)
                .ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var e in await feedIterator.ReadNextAsync())
                    {
                        await _container.DeleteItemAsync<RemoveEvent>(e.Id.ToString(), new PartitionKey(e.EntityId));
                    }
                }
            }
        }

        public EntityHandler<EntityType, IdType> GetHandler<EntityType, IdType>(EntityType entity) 
            where EntityType : IUploadable<IdType>
        {
            _referenceService?.CheckStructure(typeof(EntityType));
            return new EntityHandler<EntityType, IdType>(this, _referenceService, entity);
        }
    }
}
