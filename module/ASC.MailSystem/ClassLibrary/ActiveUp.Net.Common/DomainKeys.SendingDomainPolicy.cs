// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Security
{
    public class SendingDomainPolicy
    {
        public static SendingDomainPolicy Parse(string input)
        {
            SendingDomainPolicy policy = new SendingDomainPolicy();
            MatchCollection matches = Regex.Matches(input, @"[a-zA-Z]+=[^;]+(?=(;|\Z))");
            foreach (Match m in matches)
            {
                string tag = m.Value.Substring(0, m.Value.IndexOf('='));
                string value = m.Value.Substring(m.Value.IndexOf('=') + 1);
                if (tag.Equals("n")) policy._n = value;
                else if (tag.Equals("r")) policy._r = Parser.ParseAddress(value);
                else if (tag.Equals("o"))
                {
                    if (value.Equals("~")) policy._o = OutboundSigningPolicy.Some;
                    else if (value.Equals("-")) policy._o = OutboundSigningPolicy.All;
                    else policy._o = OutboundSigningPolicy.OtherOrNoPolicy;
                }
                else if (tag.Equals("t")) policy._t = value.Equals("y");
            }
            return policy;
        }
        
        private string _n;
        private OutboundSigningPolicy _o = OutboundSigningPolicy.Some;
        private bool _t;
        private Address _r = new Address();

        public string Notes
        {
            get { return this._n; }
            set { this._n = value; }
        }
        public OutboundSigningPolicy OutboundSigningPolicy
        {
            get { return this._o; }
            set { this._o = value; }
        }
        public Address ReportTo
        {
            get { return this._r; }
            set { this._r = value; }
        }
    }
}
