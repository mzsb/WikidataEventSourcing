using EventSourcing.Builders;
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
    public class AsyncServiceProvider : ServiceProviderBase
    {
        internal AsyncServiceProvider(string databaseId,
                                      string connectionString) : base(databaseId, connectionString) { }

        public async Task<EventServiceBuilder> ProvideEventServiceAsync() =>
            new EventServiceBuilder(await GetDatabaseAsync());

        public async Task<DataServiceBuilder> ProvideDataServiceAsync<EntityType, IdType>() =>
            new DataServiceBuilder(await GetDatabaseAsync(), typeof(EntityType));

        public async Task<ReferenceServiceBuilder> ProvideReferenceServiceAsync() =>
            new ReferenceServiceBuilder(await GetDatabaseAsync());


        private async Task<Database> GetDatabaseAsync() =>
            _database ??= (await GetCosmosClient().CreateDatabaseIfNotExistsAsync(_databaseId)).Database;
    }
}
