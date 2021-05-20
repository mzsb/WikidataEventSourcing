using EventSourcing.Constants;
using EventSourcing.Events;
using EventSourcing.Helpers;
using EventSourcing.Options;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class EventServiceBuilder : SyncServiceBuilderBase
    {
        private EventServiceOptions _options = new EventServiceOptions
        {
            State = EventServiceState.Upload
        };

        internal EventServiceBuilder(Database database) 
            : base(database, EventServiceConstants.DefaultContainerId, typeof(EventBase).GetPartitionKey().path) { }

        public EventServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public EventServiceBuilder WithOptions(EventServiceOptions options)
        {
            _options = options;
            return this;
        }

        public AsyncEventServiceBuilder CreateContainerIfNotExist()
        {
            return new AsyncEventServiceBuilder(_database, _containerId, _partitionKeyPath, _options);
        }

        public EventService Build() => 
            new EventService(GetContainer(),
                             _options);
    }
}
