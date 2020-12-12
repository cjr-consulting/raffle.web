using System.Collections.Generic;
using System.Linq;

namespace Raffle.Core.Shared
{
    public class Result
    {
        private static readonly Result valid = new Result();

        public static Result Valid()
        {
            return valid;
        }

        public static Result Fail(List<string> errorMessages)
        {
            return new Result(errorMessages);
        }

        public IReadOnlyList<string> ErrorMessages { get; } = new List<string>();
        public bool Failed { get; } = false;

        protected Result()
        {
        }

        protected Result(List<string> errorMessages)
        {
            ErrorMessages = errorMessages;
            Failed = errorMessages.Any();
        }
    }
}
