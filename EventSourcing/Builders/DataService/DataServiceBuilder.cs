using EventSourcing.Constants;
using EventSourcing.Helpers;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class DataServiceBuilder : SyncServiceBuilderBase
    {
        private readonly List<JsonConverter> _converters = new();

        internal DataServiceBuilder(Database database, Type entityType) 
            : base(database, DataServiceConstants.DefaultContainerId, entityType.GetPartitionKey().path) { }

        public DataServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public DataServiceBuilder AddConverters(params JsonConverter[] converters)
        {
            _converters.AddRange(converters);
            return this;
        }

        public AsyncServiceBuilderBase CreateContainerIfNotExist()
        {
            return new AsyncDataServiceBuilder(_database, _containerId, _partitionKeyPath, _converters);
        }

        public DataService Build() => _partitionKeyPath is not null ?
            new DataService(GetContainer(), _converters) :
            throw new Exception($"{GetType().Name} must have custom partition key path.");
    }
}
