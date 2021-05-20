using EventSourcing.Events;
using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using EventSourcing.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace EventSourcing.Services
{
    public class DataService : ServiceBase
    {
        private readonly List<JsonConverter> _converters; 

        internal DataService(Container container, List<JsonConverter> converters) : base(container)
        {
            _converters = converters ?? new List<JsonConverter>();
        }

        internal async Task InsertAsync(InsertEvent insertEvent) 
        {
            var response = await _container.CreateItemAsync(JObject.Parse(insertEvent.JsonEntity), new PartitionKey(insertEvent.EntityPartitionKey));

            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Insert error: ({response.StatusCode})");
            }
        }

        internal async Task RemoveAsync(RemoveEvent removeEvent) 
        {
            var response = await _container.DeleteItemAsync<object>(
                removeEvent.EntityId,
                new PartitionKey(removeEvent.EntityPartitionKey));

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception($"Remove error: ({response.StatusCode})");
            }
        }

        internal async Task<JObject> ReadAsync(EventBase eventBase)
        {
            var response =
                await _container.ReadItemAsync<JObject>(eventBase.EntityId,
                    new PartitionKey(eventBase.EntityPartitionKey));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Resource;
            }

            throw new Exception($"Read error: ({response.StatusCode})");
        }

        internal async Task UpdateAsync(JObject entity, UpdateEvent updateEvent)
        {
            var updateResponse =
                await _container.ReplaceItemAsync(entity, updateEvent.EntityId,
                    new PartitionKey(updateEvent.EntityPartitionKey));

            if (updateResponse.StatusCode != HttpStatusCode.OK)
            {
                new Exception($"Update error: {updateResponse.StatusCode}");
            }
        }

        public async Task<List<EntityType>> GetEntitiesAsync<EntityType, IdType>() where EntityType : IUploadable<IdType>
        {
            var result = new List<EntityType>();
             
            using (var feedIterator = _container.GetItemLinqQueryable<object>().ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var @object in await feedIterator.ReadNextAsync()) 
                    {
                        result.Add(JsonConvert.DeserializeObject<EntityType>(@object.ToString(),
                            new JsonSerializerSettings() { Converters = _converters }));
                    }
                }
            }

            return result;
        }

        internal async Task<List<JObject>> GetEntitiesAsync()
        {
            var result = new List<JObject>();

            using (var feedIterator = _container.GetItemLinqQueryable<JObject>().ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var @object in await feedIterator.ReadNextAsync())
                    {
                        result.Add(@object);
                    }
                }
            }

            return result;
        }

        public async Task<EntityType> GetEntityByIdAsync<EntityType, IdType>(EntityType entity) where EntityType : IUploadable<IdType>
        {
            var result = await _container.ReadItemAsync<object>(
                entity.Id.ToString(),
                new PartitionKey(typeof(EntityType).GetPartitionKey(entity).value.ToString()));

            return JsonConvert.DeserializeObject<EntityType>(result.Resource.ToString(),
                            new JsonSerializerSettings() { Converters = _converters });
        }
    }
}
