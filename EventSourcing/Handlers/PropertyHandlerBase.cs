using EventSourcing.Interfaces;
using EventSourcing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EventSourcing.Handlers
{
    public abstract class PropertyHandlerBase<EntityType, ValueType>
    {
        protected IEntityHandler<EntityType> _entityHandler;
        protected string _path;
        protected object _propertyValue;
        protected object _prevPropertyValue;
        protected PropertyInfo _propertyInfo;

        public PropertyHandlerBase(string path,
                                   object propertyValue,
                                   object prevPropertyValue,
                                   PropertyInfo propertyInfo)
        {
            _path = path;
            _propertyValue = propertyValue;
            _prevPropertyValue = prevPropertyValue;
            _propertyInfo = propertyInfo;
        }

        internal void SetEntityHandler(IEntityHandler<EntityType> entityHandler) =>
            _entityHandler ??= entityHandler;

        protected string GetValidPathSegment(string body)
        {
            var splittedBody = body.Split(".");

            if (splittedBody.Length != 2)
            {
                throw new Exception($"Invalid path for {typeof(ValueType).Name}.");
            }

            return splittedBody[1];
        }

        protected virtual void Setup(params string[] parameters)
        {
            _propertyValue ??= _entityHandler.Entity;
            _propertyInfo = _propertyValue.GetType().GetProperty(parameters[0]);
            _prevPropertyValue = _propertyValue;
            _propertyValue = _propertyInfo.GetValue(_propertyValue);
        }

        protected void Clean()
        {
            _path = string.Empty;
            _propertyValue = null;
            _prevPropertyValue = null;
            _propertyInfo = null;
            _path = string.Empty;
        }
    }
}
