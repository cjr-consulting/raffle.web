using System;
using System.Collections.Generic;
using System.Text;

namespace Raffle.Core.Models
{
    public class EmailAddress
    {
        public string Email { get; }
        public string Name { get; }

        public EmailAddress(string email, string name)
        {
            Email = email;
            Name = name;
        }
    }
}
