using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Builders
{
    public abstract class ServiceBuilderBase
    {
        protected readonly Database _database;

        protected string _containerId;
        protected string _partitionKeyPath;

        protected ServiceBuilderBase(Database database, 
                                     string defaultContainerId,
                                     string defaultpartitionKeyPath)
        {
            _database = database;
            _containerId = defaultContainerId;
            _partitionKeyPath = defaultpartitionKeyPath;
        }
    }
}
