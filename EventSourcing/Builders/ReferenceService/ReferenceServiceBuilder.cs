using EventSourcing.Constants;
using EventSourcing.Helpers;
using EventSourcing.Model;
using EventSourcing.Options;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public class ReferenceServiceBuilder : SyncServiceBuilderBase
    {
        internal ReferenceServiceBuilder(Database database) : 
            base(database, ReferenceServiceConstants.DefaultContainerId, typeof(Reference).GetPartitionKey().path) { }

        public ReferenceServiceBuilder WithContainerId(string containerId)
        {
            _containerId = containerId;
            return this;
        }

        public AsyncReferenceServiceBuilder CreateContainerIfNotExist()
        {
            return new AsyncReferenceServiceBuilder(_database, _containerId, _partitionKeyPath);
        }

        public ReferenceService Build() =>
            new ReferenceService(GetContainer());
    }
}
