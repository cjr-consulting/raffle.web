using System;
using System.Collections.Generic;

namespace Raffle.Web.Services
{
    public class FlagManager
    {
        readonly Dictionary<string, string> flags;
        public FlagManager(Dictionary<string, string> flags)
        {
            this.flags = flags;
        }

        public string Get(string key)
        {
            if (!flags.TryGetValue(key, out string value))
            {
                return "Nothing";
            }

            return value;
        }
    }
}
