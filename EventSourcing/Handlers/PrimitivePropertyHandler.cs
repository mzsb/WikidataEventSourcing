using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using EventSourcing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Constant = EventSourcing.Constants.ReferenceServiceConstants;

namespace EventSourcing.Handlers
{
    public class PrimitivePropertyHandler<EntityType, IdType, ValueType> : PropertyHandlerBase<EntityType, ValueType>
    {
        public PrimitivePropertyHandler(string path,
                                        object propertyValue,
                                        object prevPropertyValue = null,
                                        PropertyInfo propertyInfo = null)
            : base(path, propertyValue, prevPropertyValue, propertyInfo) { }

        public virtual PrimitivePropertyHandler<EntityType, IdType, ReturnValueType> Path<ReturnValueType>(Expression<Func<ValueType, ReturnValueType>> property)
        {
            var pathSegment = GetValidPathSegment(property.Body.ToString());
            Setup(pathSegment);

            _path += $"{pathSegment.ToCamelCase()}{Constant.PathSegmentSeparator}";

            var propertyHandler = new PrimitivePropertyHandler<EntityType, IdType, ReturnValueType>(_path, _propertyValue, _prevPropertyValue, _propertyInfo);
            propertyHandler.SetEntityHandler(_entityHandler);
            Clean();

            return propertyHandler;
        }

        public virtual ListPropertyHandler<EntityType, IdType, ItemType> Path<ItemType>(Expression<Func<ValueType, List<ItemType>>> property) 
            where ItemType : IIdentifiable<IdType>
        {
            var pathSegment = GetValidPathSegment(property.Body.ToString());
            Setup(pathSegment);

            _path += $"{pathSegment.ToCamelCase()}{Constant.ListIndexSeparator}";

            var propertyHandler = new ListPropertyHandler<EntityType, IdType, ItemType>(_path, _propertyValue, _prevPropertyValue, _propertyInfo);
            propertyHandler.SetEntityHandler(_entityHandler);
            Clean();

            return propertyHandler;
        }

        public virtual void Set(ValueType value)
        {
            CheckValue(value);

            if(_propertyInfo.GetValue(_prevPropertyValue) is List<ValueType> list) 
            {
                var prevObject = (ValueType)_propertyValue;
                var prevObjectIndex = list.IndexOf(prevObject);
                list.Remove(prevObject);
                list.Insert(prevObjectIndex, value);
            }
            else
            {
                //if (_prevPropertyValue.GetType().GUID != _entityHandler.Entity.GetType().GUID && 
                //    _prevPropertyValue.GetType().IsImplementAny(typeof(IUploadable<>)))
                //{
                //    throw new Exception($"Use another {typeof(EntityHandler<,>).Name} to update {_propertyInfo.Name} of {_prevPropertyValue.GetType().Name} " +
                //                        $"because {_prevPropertyValue.GetType().Name} is an {typeof(IUploadable<>).Name}.");
                //}

                _propertyInfo.SetValue(_prevPropertyValue, value);
            }

            _entityHandler.CreateUpdateSegment(value, _path[0..^1]);

            Clean();
        }

        protected void CheckValue(ValueType value)
        {
            if (value is null)
            {
                throw new Exception("Value cannot be null.");
            }
        }
    }
}
