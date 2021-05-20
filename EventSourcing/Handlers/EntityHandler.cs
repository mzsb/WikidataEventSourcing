using EventSourcing.Enums;
using EventSourcing.Events;
using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using EventSourcing.Model;
using EventSourcing.Options;
using EventSourcing.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcing.Handlers
{
    public class EntityHandler<EntityType, IdType> : PrimitivePropertyHandler<EntityType, IdType, EntityType>, IEntityHandler<EntityType> where EntityType : IUploadable<IdType>
    {
        private readonly Dictionary<Guid, UpdateSegment> _updateSegments = new Dictionary<Guid, UpdateSegment>();
        private readonly EventService _eventService;
        private readonly ReferenceService _referenceService;
        public EntityType Entity { get; set; }
        private PropertyInfo _firstPropertyInfo = null;

        public EntityHandler(EventService eventService,
                             ReferenceService referenceService,
                             EntityType entity) : base(string.Empty, entity)
        {
            _eventService = eventService;
            _referenceService = referenceService;
            Entity = entity;
            SetEntityHandler(this);
        }

        public override PrimitivePropertyHandler<EntityType, IdType, ReturnValueType> Path<ReturnValueType>(Expression<Func<EntityType, ReturnValueType>> property)
        {
            _firstPropertyInfo = typeof(EntityType).GetProperty(GetValidPathSegment(property.Body.ToString()));
            return base.Path(property);
        }

        public override ListPropertyHandler<EntityType, IdType, ItemType> Path<ItemType>(Expression<Func<EntityType, List<ItemType>>> property)
        {
            _firstPropertyInfo = typeof(EntityType).GetProperty(GetValidPathSegment(property.Body.ToString()));
            return base.Path(property);
        }
        public override void Set(EntityType value)
        {
            CheckValue(value);
            Entity = value;
            Clean();
        }

        public async Task CreateAsync()
        {
            var partitionKey = GetPartitionKeyOfValidEntity(Entity);
            await _eventService.PublishEventAsync(new InsertEvent
            {
                Id = Guid.NewGuid(),
                JsonEntity = new JObject
                {
                    ["id"] = Entity.Id.ToString(),
                    [partitionKey.Name.ToCamelCase()] = partitionKey.Value.ToString()
                }.ToString(Formatting.None),
                EntityId = Entity.Id.ToString(),
                EntityPartitionKey = partitionKey.Value.ToString(),
                CreationDate = DateTime.Now
            });
        }

        public async Task RemoveAsync()
        {
            var partitionKey = GetPartitionKeyOfValidEntity(Entity);

            await _eventService.PublishEventAsync(new RemoveEvent
            {
                Id = Guid.NewGuid(),
                EntityId = Entity.Id.ToString(),
                EntityPartitionKey = partitionKey.Value.ToString(),
                CreationDate = DateTime.Now
            });
        }

        Guid IEntityHandler<EntityType>.CreateUpdateSegment(object value, string path, ListUpdateType listUpdateType)
        {
            Guid segmentId = CreateSegmentId();

            _updateSegments.Add(segmentId,
                new UpdateSegment
                {
                    Path = path,
                    Value = value,
                    CreationDate = DateTime.Now,
                    ListUpdateType = listUpdateType
                });

            _referenceService?.AddReference(segmentId, _firstPropertyInfo.GetValue(Entity), _firstPropertyInfo);

            return segmentId;
        }

        public async Task UpdateAsync()
        {
            if (_updateSegments.Count > 0)
            {
                var partitionKey = GetPartitionKeyOfValidEntity(Entity);

                var id = Guid.NewGuid();

                _referenceService?.AddToUpdate(id, _updateSegments.Select(s => s.Key));

                await _eventService.PublishEventAsync(new UpdateEvent
                {
                    Id = id,
                    EntityId = Entity.Id.ToString(),
                    EntityPartitionKey = partitionKey.Value.ToString(),
                    UpdateSegments = _updateSegments.Values.ToList(),
                    CreationDate = DateTime.Now
                });
            }
        }

        public void RemoveUpdate(Guid segmentId)
        {
            _updateSegments.Remove(segmentId);
            _referenceService?.RemoveReference(segmentId);
        }

        private (string Name, object Value) GetPartitionKeyOfValidEntity(EntityType entity)
        {
            var partitionKey = GetPartitionKey(entity);

            if(entity.Id is null)
            {
                throw new Exception($"Id of {typeof(EntityType).Name} cannot be null");
            }

            return partitionKey;
        }

        private (string Name, object Value) GetPartitionKey(EntityType entity)
        {
            var entityType = typeof(EntityType);

            var partitionKey = entityType.GetPartitionKey(entity);

            return (partitionKey.path.GetPartitionKeyNameFromPath(), partitionKey.value);
        }

        private Guid CreateSegmentId()
        {
            Guid newSegmentId;
            do
            {
                newSegmentId = Guid.NewGuid();
            } while (_updateSegments.ContainsKey(newSegmentId));

            return newSegmentId;
        }
    }
}
