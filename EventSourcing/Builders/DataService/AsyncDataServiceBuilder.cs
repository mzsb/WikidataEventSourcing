using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class AsyncDataServiceBuilder : AsyncServiceBuilderBase
    {
        private readonly List<JsonConverter> _converters;

        internal AsyncDataServiceBuilder(Database database, 
                                         string containerId, 
                                         string partitionKeyPath,
                                         List<JsonConverter> converters) 
            : base(database, containerId, partitionKeyPath) 
        {
            _converters = converters;
        }

        public AsyncDataServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public AsyncDataServiceBuilder AddConverters(params JsonConverter[] converters)
        {
            _converters.AddRange(converters);
            return this;
        }

        public async Task<DataService> BuildAsync() => _partitionKeyPath is not null ?
            new DataService(await GetContainerAsync(), _converters) :
            throw new Exception($"{GetType().Name} must have custom partition key path.");
    }
}
