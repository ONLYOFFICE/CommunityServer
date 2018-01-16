/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.ReferralInfo.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This class encapsulates the combination of LdapReferral URL and
    ///     the connection opened to service this URL
    /// </summary>
    public class ReferralInfo
    {
        /// <summary>
        ///     Returns the referral URL
        /// </summary>
        /// <returns>
        ///     the Referral URL
        /// </returns>
        public virtual LdapUrl ReferralUrl
        {
            get { return referralUrl; }
        }

        /// <summary>
        ///     Returns the referral Connection
        /// </summary>
        /// <returns>
        ///     the Referral Connection
        /// </returns>
        public virtual LdapConnection ReferralConnection
        {
            get { return conn; }
        }

        /// <summary>
        ///     Returns the referral list
        /// </summary>
        /// <returns>
        ///     the Referral list
        /// </returns>
        public virtual string[] ReferralList
        {
            get { return referralList; }
        }

//		private DirectoryEntry conn;
        private readonly LdapConnection conn;
        private readonly LdapUrl referralUrl;
        private readonly string[] referralList;

        /// <summary>
        ///     Construct the ReferralInfo class
        /// </summary>
        /// <param name="lc">
        ///     The DirectoryEntry opened to process this referral
        /// </param>
        /// <param name="refUrl">
        ///     The URL string associated with this connection
        /// </param>
        public ReferralInfo(LdapConnection lc, string[] refList, LdapUrl refUrl)
        {
            conn = lc;
            referralUrl = refUrl;
            referralList = refList;
        }
    }
}