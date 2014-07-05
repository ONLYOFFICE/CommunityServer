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
    /// <summary>
    /// Mechanisms available for authentication.
    /// </summary>
    public enum SaslMechanism
    {
        /// <summary>
        /// Authentication doesn't needed
        /// </summary>
        None = 0,
        /// <summary>
        /// The LOGIN mechanism (BASE64 encoded exchanges).
        /// </summary>
        Login = 1,
        /*/// <summary>
        /// The PLAIN mechanism (identity<NUL>username<NUL>password).
        /// </summary>
        Plain = 2,*/
        /*/// <summary>
        /// The DIGEST-MD5 mechanism. [RFC2831]
        /// </summary>
        DigestMd5 = 3,*/
        /// <summary>
        /// The CRAM-MD5 mechanism. [RFC2195]
        /// </summary>
        CramMd5 = 4,
        /// <summary>
        /// The OAuth 2.0 mechanism. [draft-ietf-oauth-v2-31]
        /// </summary>
        OAuth2 = 5
        /*/// <summary>
        /// The KERBEROS_V4 mechanism. [RFC2222]
        /// </summary>
        KerberosV4 = 6*/
    }
}
