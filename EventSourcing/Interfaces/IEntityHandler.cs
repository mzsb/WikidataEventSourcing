using EventSourcing.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Interfaces
{
    public interface IEntityHandler<EntityType>
    {
        public EntityType Entity { get; set; }

        internal Guid CreateUpdateSegment(object value, string path, ListUpdateType listUpdateType = ListUpdateType.None);
    }
}
