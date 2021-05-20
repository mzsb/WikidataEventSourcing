using EventSourcing.Options;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class AsyncReferenceServiceBuilder : AsyncServiceBuilderBase
    {
        internal AsyncReferenceServiceBuilder(Database database,
                                              string containerId,
                                              string partitionKeyPath) :
            base(database, containerId, partitionKeyPath) { }

        public AsyncReferenceServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public async Task<ReferenceService> BuildAsync() =>
            new ReferenceService(await GetContainerAsync());
    }
}
