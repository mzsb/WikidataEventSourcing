using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventSourcing.Interfaces
{
    public interface IIdentifiable<IdType>
    {
        public IdType Id { get; set; }
    }
}
