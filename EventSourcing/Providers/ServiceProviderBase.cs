using EventSourcing.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Providers
{
    public abstract class ServiceProviderBase
    {
        protected readonly string _connectionString;
        protected readonly string _databaseId;
        protected Database _database;

        public ServiceProviderBase(string databaseId,
                               string connectionString)
        {
            _connectionString = connectionString;
            _databaseId = databaseId;
        }

        protected CosmosClient GetCosmosClient() =>
            new CosmosClientBuilder(_connectionString)
                .WithSerializerOptions(
                    new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    })
                .WithBulkExecution(false)
                .Build();
    }
}
