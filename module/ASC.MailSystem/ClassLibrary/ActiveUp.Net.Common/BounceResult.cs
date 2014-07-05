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

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Represent a mail bounce status. Contains all the information needed to determine if the email is a mail server error bounce.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class BounceResult
    {
        private int _level = 0;
        private string _email = string.Empty;


        /// <summary>
        /// The default constructor.
        /// </summary>
        public BounceResult()
        {
            _level = 0;
            _email = string.Empty;
        }

        /// <summary>
        /// Create the object with specified default value.
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="email">The email</param>
        public BounceResult(int level, string email)
        {
            _level = level;
            _email = email;
        }

        /// <summary>
        /// The level of revelance. <b>0</b> mean probably not a bounce. <b>1</b> mean this email is suspicious. 
        /// <b>2</b> mean this is a potential bounce email. <b>3</b> mean we are quite sure that is a bounce email.
        /// </summary>
        /// <remarks>If the Level is <b>3</b>, the erroneous email (if available) is contained in the <see cref="Email"/> property.</remarks>
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }    

        /// <summary>
        /// Contains the erroneous email if the revelance level is 3.
        /// </summary>
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
            }    
        }
    }
}
