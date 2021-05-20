using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Builders
{
    public abstract class SyncServiceBuilderBase : ServiceBuilderBase
    {
        public SyncServiceBuilderBase(Database database,
                             string defaultContainerId,
                             string defaultpartitionKeyPath) 
            : base(database, defaultContainerId, defaultpartitionKeyPath) { }

        protected Container GetContainer() =>
                _database.GetContainer(_containerId);
    }
}
