using System;

namespace Raffle.Core.Shared
{
    public interface IQuery<TOut>
    {
    }

    public interface IQueryHandler<T, TOut> where T : IQuery<TOut>
    {
        TOut Handle(T query);
    }
}
