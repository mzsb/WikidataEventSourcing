using EventSourcing.Enums;
using EventSourcing.Interfaces;
using EventSourcing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Constant = EventSourcing.Constants.ReferenceServiceConstants;

namespace EventSourcing.Handlers
{
    public class ListPropertyHandler<EntityType, IdType, ItemType> : PropertyHandlerBase<EntityType, ItemType> where ItemType : IIdentifiable<IdType>
    {
        public ListPropertyHandler(string path,
                                   object propertyValue,
                                   object prevPropertyValue,
                                   PropertyInfo propertyInfo)
            : base(path, propertyValue, prevPropertyValue, propertyInfo) { }

        public void Set(List<ItemType> list)
        {
            _propertyInfo.SetValue(_prevPropertyValue, list);

            _entityHandler.CreateUpdateSegment(list, _path[0..^1]);

            Clean();
        }

        public PrimitivePropertyHandler<EntityType, IdType, ItemType> At(IdType id)
        {
            if (id is null)
            {
                throw new Exception($"Id of {typeof(ItemType).Name} cannot be null.");
            }

            Setup(id.ToString());

            _path += $"{id}{Constant.PathSegmentSeparator}";

            var propertyHandler = new PrimitivePropertyHandler<EntityType, IdType, ItemType>(_path, _propertyValue, _prevPropertyValue, _propertyInfo);
            propertyHandler.SetEntityHandler(_entityHandler);
            Clean();

            return propertyHandler;
        }

        public PrimitivePropertyHandler<EntityType, IdType, ItemType> this[IdType index]
        {
            get => At(index);
        }

        public PrimitivePropertyHandler<EntityType, IdType, ItemType> First() =>
            At((_propertyValue as List<ItemType>).First().Id);

        public PrimitivePropertyHandler<EntityType, IdType, ItemType> Last() =>
            At((_propertyValue as List<ItemType>).Last().Id);

        public PrimitivePropertyHandler<EntityType, IdType, ItemType> ElementAt(int index) =>
            At((_propertyValue as List<ItemType>).ElementAt(index).Id);

        public void Add(ItemType item)
        {
            CheckItem(item);
            (_propertyValue as List<ItemType>).Add(item);
            _entityHandler.CreateUpdateSegment(item, _path[0..^1], ListUpdateType.Add);

            Clean();
        }

        public ItemType Remove(ItemType item)
        {
            CheckItem(item);
            (_propertyValue as List<ItemType>).Remove(item);
            _entityHandler.CreateUpdateSegment(item, _path[0..^1], ListUpdateType.Remove);
            Clean();

            return item;
        }

        public void Clear()
        {
            (_propertyValue as List<ItemType>).Clear();
            _entityHandler.CreateUpdateSegment(null, _path[0..^1], ListUpdateType.Clear);
            Clean();
        }

        private void CheckItem(ItemType item)
        {
            if (item is null)
            {
                throw new Exception("Item cannot be null.");
            }
        }

        protected override void Setup(params string[] parameters)
        {
            var value = _propertyValue as List<ItemType>;
            _propertyValue = value.Single(i => i.Id.ToString() == parameters[0]);
        }
    }
}

