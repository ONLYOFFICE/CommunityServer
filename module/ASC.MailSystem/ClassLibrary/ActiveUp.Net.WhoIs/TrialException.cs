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

namespace ActiveUp.Net.WhoIs
{
    #region class TrialException

    /// <summary>
    /// Represents a trial specific error.
    /// </summary>
    public class TrialException : Exception
    {

        #region Variables

        /// <summary>
        /// Message contains the error message.
        /// </summary>
        private string _message;

        /// <summary>
        /// Default exception message.
        /// </summary>
        private readonly string _defaultMessage = "Only .org domain extention are available in the trial version. To use others domains extentions use the full version available at www.activeup.com";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor whith the default trial error.
        /// </summary>
        public TrialException()
        {
            _message = _defaultMessage;
        }

        /// <summary>
        /// Constructor, sets message to the specified value.
        /// </summary>
        /// <param name="message">Message contains the error.</param>
        public TrialException(string message)
        {
            _message = message;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the exception's message.
        /// </summary>
        public override string Message
        {
            get
            {
                return _message;
            }
        }

        #endregion
    }

    #endregion
}
