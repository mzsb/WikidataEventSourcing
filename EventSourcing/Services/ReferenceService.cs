using EventSourcing.Attributes;
using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using EventSourcing.Model;
using EventSourcing.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Constant = EventSourcing.Constants.ReferenceServiceConstants;

namespace EventSourcing.Services
{
    public class ReferenceService : ServiceBase
    {
        private readonly Dictionary<Guid, PropertyHolder> _referencesBySegment = new Dictionary<Guid, PropertyHolder>();  
        private readonly Dictionary<Guid, List<PropertyHolder>> _referencesByUpdate = new Dictionary<Guid, List<PropertyHolder>>();
        private readonly Dictionary<(string id, string partitionKeyValue), List<string>> _concreteReferences = new Dictionary<(string, string), List<string>>();

        public ReferenceService(Container container) : base(container) { }

        public async Task<Reference> GetReferenceAsync(string id, string partitionKey)
            => await _container.ReadItemAsync<Reference>(
                id,
                new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));

        public void CheckStructure(Type entityType)
        {
            if (entityType.GetCustomAttribute(typeof(ReferenceBase)) is ReferenceBase referenceBaseAttribute && 
                !referenceBaseAttribute.AllowDifferentStructure)
            {
                var propertyNames = referenceBaseAttribute.Type.GetProperties().Select(p => p.Name);
                foreach (var propertyName in entityType.GetProperties().Select(p => p.Name))
                {
                    if (!propertyNames.Contains(propertyName))
                    {
                        throw new Exception($"Structure of {entityType.FullName} differs from structure of {referenceBaseAttribute.Type.FullName} " +
                                            $"because of {propertyName} property");
                    }
                }
            }
        }

        public void AddReference(Guid updateSegmentId, object propertyValue, PropertyInfo propertyInfo) 
        {
            if(Attribute.IsDefined(propertyInfo, typeof(ReferenceContainer)) || propertyInfo.PropertyType.IsImplementAny(typeof(IUploadable<>)))
            {
                _referencesBySegment.Add(updateSegmentId, new PropertyHolder
                {
                    Value = propertyValue,
                    PropertyName = propertyInfo.Name
                });
            }
        }

        public void RemoveReference(Guid updateSegmentId) 
        {
            if (_referencesBySegment.ContainsKey(updateSegmentId))
            {
                _referencesBySegment.Remove(updateSegmentId);
            }
        }

        public void AddToUpdate(Guid eventId, IEnumerable<Guid> segmentIds)
        {
            _referencesByUpdate.Add(eventId,
            _referencesBySegment.Where(pair => segmentIds.Contains(pair.Key)).Select(pair => pair.Value).ToList());
        }


        public async Task UploadReferencesAsync(string entityId, Guid eventId)
        {
            if (_referencesByUpdate.ContainsKey(eventId))
            {
                foreach (var reference in _referencesByUpdate[eventId])
                {
                    List<(object @object, string path)> paths = new List<(object, string)> { (reference.Value, reference.PropertyName.ToCamelCase()) };
                    int index = 0;
                    do
                    {
                        while (paths[index].@object is null)
                        {
                            index++;

                            if (index == paths.Count)
                            {
                                return;
                            }
                        }

                        var type = paths[index].@object.GetType();

                        if (type.IsImplementAny(typeof(IUploadable<>)))
                        {
                            var path = paths[index].path;
                            if (path.Contains(Constant.InvalidPathTag))
                            {
                                throw new Exception($"{path.Split(Constant.InvalidPathTag)[1]} must implement IIdentifiable interface.");
                            }

                            var key = (type.GetProperty("Id").GetValue(paths[index].@object) as string, type.GetPartitionKey(paths[index].@object).value.ToString());

                            if (_concreteReferences.ContainsKey(key))
                            {
                                _concreteReferences[key].Add(path);
                            }
                            else
                            {
                                _concreteReferences.Add(key, new List<string> { path });
                            }

                            foreach (var prop in type.GetProperties())
                            {
                                paths.Add((prop.GetValue(paths[index].@object), $"{paths[index].path}{Constant.PathSegmentSeparator}{prop.Name.ToCamelCase()}"));
                            }
                        }
                        else
                        {
                            if (type.IsImplementAny(typeof(IEnumerable<>)))
                            {
                                if (paths[index].@object as IEnumerable<object> is { } list)
                                {
                                    foreach (var item in list)
                                    {
                                        var itemType = item.GetType();
                                        paths.Add((item,
                                            @$"{paths[index].path}{Constant.ListIndexSeparator}{(itemType.IsImplementAny(typeof(IIdentifiable<>), typeof(IUploadable<>)) ?
                                                itemType.GetProperty("Id").GetValue(item) is { } id ? 
                                                    id : 
                                                    throw new Exception($"Id of {item.GetType().Name} cannot be null.") :
                                                Constant.InvalidPathTag + itemType + Constant.InvalidPathTag)}")
                                        );
                                    }
                                }
                            }
                            else
                            {
                                foreach (var prop in type.GetProperties())
                                {
                                    paths.Add((prop.GetValue(paths[index].@object), $"{paths[index].path}{Constant.PathSegmentSeparator}{prop.Name.ToCamelCase()}"));
                                }
                            }
                        }

                        index++;
                    } while (index < paths.Count);
                }

                //await ClearAsync();

                foreach (var pair in _concreteReferences)
                {
                    try
                    {
                        var reference = (await _container.ReadItemAsync<Reference>(
                                pair.Key.id,
                                new Microsoft.Azure.Cosmos.PartitionKey(pair.Key.partitionKeyValue))).Resource;

                        if (reference.References.TryGetValue(entityId.ToCamelCase(), out List<string> paths))
                        {
                            paths = pair.Value;
                        }
                        else
                        {
                            reference.References.Add(entityId, pair.Value);
                        }

                        var response = await _container.ReplaceItemAsync(
                            reference,
                            pair.Key.id,
                            new Microsoft.Azure.Cosmos.PartitionKey(pair.Key.partitionKeyValue));

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Reference update error: ({response.StatusCode})");
                        }
                    }
                    catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var response = await _container.CreateItemAsync(new Reference
                        {
                            Id = pair.Key.id,
                            EntityPartitionKey = pair.Key.partitionKeyValue,
                            References = new Dictionary<string, List<string>> { { entityId, pair.Value } }
                        });

                        if (response.StatusCode != System.Net.HttpStatusCode.Created)
                        {
                            throw new Exception($"Reference insert error: ({response.StatusCode})");
                        }
                    }
                }
            }
        }

        private class PropertyHolder
        {
            public object Value { get; set; }
            public string PropertyName { get; set; }
        }

        public async Task ClearAsync()
        {
            using (var feedIterator = _container.GetItemLinqQueryable<Reference>().ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var e in await feedIterator.ReadNextAsync())
                    {
                        await _container.DeleteItemAsync<Reference>(e.Id, new Microsoft.Azure.Cosmos.PartitionKey(e.EntityPartitionKey));
                    }
                }
            }
        }
    }
}
