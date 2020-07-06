using System;

namespace Raffle.Core.Shared
{
    public interface ICommandHandler<T> where T : ICommand
    {
        void Handle(T command);
    }
}
