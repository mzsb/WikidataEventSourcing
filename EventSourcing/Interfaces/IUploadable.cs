using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Interfaces
{
    public interface IUploadable<IdType> : IIdentifiable<IdType> { }
}
