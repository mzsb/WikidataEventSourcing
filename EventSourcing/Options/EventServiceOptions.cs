using EventSourcing.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Options
{
    public class EventServiceOptions
    {
        public ReferenceService ReferenceService { get; set; }

        public EventServiceState State { get; set; }
    }

    public enum EventServiceState
    {
        Upload,
        Test
    }
}
