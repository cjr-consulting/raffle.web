using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Shared
{
    public interface IQuery
    {
    }

    public interface IQueryHandler<T, O> where T : IQuery
    {
        O Handle(T query);
    }
}
