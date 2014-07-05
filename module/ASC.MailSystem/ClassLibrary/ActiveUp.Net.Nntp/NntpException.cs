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
#region NntpException Object
    /// <summary>
    /// Custom exception.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class NntpException : System.Exception
    {
        internal NntpException(string servermessage)
        {
            this.NntpError = servermessage;
        }
        internal string NntpError = "Unhandled exception occured";
        /// <summary>
        /// Message returned by the server or error description.
        /// </summary>
        public override string Message
        {
            get
            {
                return this.NntpError;
            }
        }
    }
    #endregion
}