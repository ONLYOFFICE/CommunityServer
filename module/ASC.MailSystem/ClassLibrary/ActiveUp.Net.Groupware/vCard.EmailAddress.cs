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

namespace ActiveUp.Net.Groupware.vCard
{
    /// <summary>
    /// Description résumée de vCard.
    /// </summary>
    #if !PocketPC
    [System.Serializable]
    #endif
    public class EmailAddress
    {
        public EmailAddress()
        {
            //
            // TODO : ajoutez ici la logique du constructeur
            //
        }

        public EmailAddress(string address)
        {
            _address = address;
            _isInternet = true;
        }

        public EmailAddress(string address, bool isInternet, bool isPrefered)
        {
            _address = address;
            _isInternet = isInternet;
            _isPrefered = IsPrefered;
        }
        
        private string _address;
        private bool _isInternet,_isX400,_isPrefered;

        public string Address
        {
            get
            {
                return this._address;
            }
            set
            {
                this._address = value;
            }
        }
        public bool IsInternet
        {
            get
            {
                return this._isInternet;
            }
            set
            {
                this._isInternet = value;
            }
        }
        public bool IsX400
        {
            get
            {
                return this._isX400;
            }
            set
            {
                this._isX400 = value;
            }
        }
        public bool IsPrefered
        {
            get
            {
                return this._isPrefered;
            }
            set
            {
                this._isPrefered = value;
            }
        }

        public string GetFormattedLine()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("EMAIL;");
            if(this.IsInternet || this.IsPrefered || this.IsX400) sb.Append("TYPE=");
            if(this.IsInternet) sb.Append("internet,");
            if(this.IsPrefered) sb.Append("pref,");
            if(this.IsX400) sb.Append("x400,");
            sb.Remove(sb.Length-1,1);
            sb.Append(":");
            if(this.Address!=null) sb.Append(this.Address);
            return sb.ToString();
        }
    }
}
