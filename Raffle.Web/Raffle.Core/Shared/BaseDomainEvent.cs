using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Shared
{
    public abstract class BaseDomainEvent : INotification
    {
        public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}
