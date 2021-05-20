using EventSourcing.Attributes;
using EventSourcing.Helpers;
using EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EventSourcing.Helpers
{
    public static class PartitionKeyHelper
    {
        public static string GetPartitionKeyNameFromPath(this string partitionKeyPath) =>
            partitionKeyPath.Split("/").Last().ToPascalCase();

        public static (string path, object value) GetPartitionKey(this Type type, object value = null)
        {
            if (!type.IsImplementAny(typeof(IUploadable<>)))
            {
                throw new Exception($"{type.Name} must implement {typeof(IUploadable<>).Name} interface.");
            }

            var classTree = new List<(Type classType, PropertyInfo[] properties, string path, object value)>() { (type, type.GetProperties(), "", value) };  
            int index = 0;
            int partitionkeyCount = 0;
            (string path, object value) partitionKey = (string.Empty, null);
            (string parentTypeName, string partitionKeyName) prevPartitionKey = (type.Name, string.Empty);
            do
            {
                var classNode = classTree.ElementAt(index);

                foreach (var property in classNode.properties) 
                {
                    var propertyType = property.PropertyType;
                    var propertyName = property.Name;
                    if (property.GetCustomAttribute<PartitionKey>() is { } partitionKeyAttribute)
                    {
                        if(index == 0 || type.GetBases(true).Contains(partitionKeyAttribute.Type))
                        {
                            if (!propertyType.IsPrimitive())
                            {
                                throw new Exception($"{propertyName} is not valid partition key in {classNode.classType.FullName}.");
                            }

                            partitionKey.path = $"{classNode.path}/{propertyName.ToCamelCase()}";

                            partitionKey.value = classNode.value is { } v ? property.GetValue(v) : null;

                            partitionkeyCount++;

                            prevPartitionKey = (classNode.classType.FullName, partitionKey.path.GetPartitionKeyNameFromPath());
                        }

                        if (partitionkeyCount > 1)
                        {
                            throw new Exception($"{type.Name} has more than one partition key. " +
                                                $"{partitionKey.path.GetPartitionKeyNameFromPath()} in {classNode.classType.FullName} and " +
                                                $"{prevPartitionKey.partitionKeyName} in {prevPartitionKey.parentTypeName}.");
                        }
                    }

                    if (!propertyType.IsPrimitive() && !propertyType.IsSystem() && !propertyType.IsImplementAny(typeof(ICollection<>)))
                    {
                        classTree.Add((propertyType, propertyType.GetProperties(), $"{classNode.path}/{propertyName.ToCamelCase()}", classNode.value is { } v ? property.GetValue(v) : null));
                    }
                }

                index++;
            } while (index < classTree.Count);


            if (string.IsNullOrEmpty(partitionKey.path))
            {
                throw new Exception($"{type.FullName} must have a partition key in the data structure. " +
                                    $"Use {typeof(PartitionKey).FullName} attribute to determine the partition key.");
            }

            if(value is not null && partitionKey.value is null)
            {
                throw new Exception($"{partitionKey.path} partition key cannot be null.");
            }

            return partitionKey;
        }
    }
}
