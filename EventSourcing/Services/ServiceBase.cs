using EventSourcing.Interfaces;
using EventSourcing.Model;
using EventSourcing.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EventSourcing.Services
{
    public abstract class ServiceBase
    {
        protected readonly Container _container;

        protected ServiceBase(Container container)
        {
            _container = container;
        }
    }
}
