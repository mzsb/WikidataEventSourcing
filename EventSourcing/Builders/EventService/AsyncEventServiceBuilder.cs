using EventSourcing.Options;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class AsyncEventServiceBuilder : AsyncServiceBuilderBase
    {
        private EventServiceOptions _options;

        internal AsyncEventServiceBuilder(Database database,
                                          string containerId,
                                          string partitionKeyPath,
                                          EventServiceOptions options) 
            : base(database, containerId, partitionKeyPath) 
        {
            _options = options;
        }

        public AsyncEventServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public AsyncEventServiceBuilder WithOptions(EventServiceOptions options)
        {
            _options = options;
            return this;
        }


        public async Task<EventService> BuildAsync() =>
            new EventService(await GetContainerAsync(),
                             _options);
    }
}
