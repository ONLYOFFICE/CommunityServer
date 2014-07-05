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

namespace ActiveUp.Net.Mail
{
#region UsenetXrefList Object
    /// <summary>
    /// Represents a parsed Xref Header field.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class UsenetXrefList
    {
        string _host;
        System.Collections.Specialized.NameValueCollection _groups = new System.Collections.Specialized.NameValueCollection();

        /// <summary>
        /// The host where the message resides.
        /// </summary>
        public string Host
        {
            get
            {
                return this._host;
            }
            set
            {
                this._host = value;
            }
        }
        /// <summary>
        /// Name/Value collection with newsgroups as Keys and message indexes as Value.
        /// </summary>
        public System.Collections.Specialized.NameValueCollection Groups
        {
            get
            {
                return this._groups;
            }
            set
            {
                this._groups = value;
            }
        }
    }
    #endregion
}