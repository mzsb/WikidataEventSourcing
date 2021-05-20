using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public abstract class AsyncServiceBuilderBase : ServiceBuilderBase
    {
        public AsyncServiceBuilderBase(Database database,
                                       string defaultContainerId,
                                       string defaultpartitionKeyPath) 
            : base(database, defaultContainerId, defaultpartitionKeyPath) { }

        protected async Task<Container> GetContainerAsync() =>
                (await _database.DefineContainer(_containerId, _partitionKeyPath).CreateIfNotExistsAsync()).Container;
    }
}
