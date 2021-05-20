using EventSourcing.Builders;
using EventSourcing.Interfaces;
using EventSourcing.Options;
using EventSourcing.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Providers
{
    public class ServiceProvider : ServiceProviderBase
    {
        public ServiceProvider(string databaseId, 
                               string connectionString) : base(databaseId, connectionString) { }

        public AsyncServiceProvider CreateDatabaseIfNotExist()
        {
            return new AsyncServiceProvider(_databaseId, _connectionString);
        }

        public EventServiceBuilder ProvideEventService()
            => new EventServiceBuilder(GetDatabase());

        public DataServiceBuilder ProvideDataService<EntityType, IdType>() where EntityType : IUploadable<IdType>
            => new DataServiceBuilder(GetDatabase(), typeof(EntityType));

        public ReferenceServiceBuilder ProvideReferenceService()
            => new ReferenceServiceBuilder(GetDatabase());


        private Database GetDatabase() =>
            _database ??= GetCosmosClient().GetDatabase(_databaseId);

    }
}
