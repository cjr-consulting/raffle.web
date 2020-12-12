using System.Collections.Generic;
using System.Linq;

namespace Raffle.Core.Shared
{
    public class Result<T> : Result
    {
        public static Result<T> Valid(T data)
        {
            return new Result<T>(data);
        }

        public T Data { get; }

        protected Result(T data)
        {
            Data = data;
        }

        protected Result(List<string> errorMessages)
            : base(errorMessages)
        {

        }
    }
}
