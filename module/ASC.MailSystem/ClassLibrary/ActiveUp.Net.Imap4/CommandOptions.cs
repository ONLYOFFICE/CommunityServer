using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Mail
{
    public class CommandOptions
    {
        public bool IsPlusCmdAllowed { get; set; }

        public CommandOptions()
        {
            this.IsPlusCmdAllowed = true;
        }
    }
}
