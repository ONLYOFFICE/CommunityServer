#region Copyright © 2007 Pinal Patel. All rights reserved.
/*
Copyright © 2007 Pinal Patel. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion



using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Provides information about the build user.
    /// </summary>
    /// <example>Get build user information.
    /// <code><![CDATA[
    /// <User>
    ///   <Output TaskParameter="UserNameWithDomain" PropertyName="BuildUserID" />
    ///   <Output TaskParameter="FullName" PropertyName="BuildUserName" />
    ///   <Output TaskParameter="Email" PropertyName="BuildUserEmail" />
    ///   <Output TaskParameter="Phone" PropertyName="BuildUserPhone" />
    /// </User>    
    /// ]]></code>
    /// </example>
    /// <remarks>
    /// The following output parameters are set only if information about the build user can be retrieved
    /// from the Active Directory if one exists:
    /// <list type="bullet">
    ///   <item><c>FirstName</c></item>
    ///   <item><c>LastName</c></item>
    ///   <item><c>MiddleInitial</c></item>
    ///   <item><c>FullName</c></item>
    ///   <item><c>Email</c></item>
    ///   <item><c>Phone</c></item>
    /// </list>
    /// </remarks>
    public class User : Task
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:User"/> class.
        /// </summary>  
        public User()
        {
        }
        #endregion Constructor

        #region Output Parameters
        private string _userName = "";
        /// <summary>
        /// Gets the user name of the build user.
        /// </summary>
        [Output]
        public string UserName
        {
            get { return _userName; }
        }

        private string _domainName = "";
        /// <summary>
        /// Gets the domain name of the build user.
        /// </summary>
        [Output]
        public string DomainName
        {
            get { return _domainName; }
        }

        private string _firstName = "";
        /// <summary>
        /// Gets the first name of the build user.
        /// </summary>
        [Output]
        public string FirstName
        {
            get { return _firstName; }
        }

        private string _lastName = "";
        /// <summary>
        /// Gets the last name of the build user.
        /// </summary>
        [Output]
        public string LastName
        {
            get { return _lastName; }
        }

        private string _middleInitial = "";
        /// <summary>
        /// Gets the middle initial of the build user.
        /// </summary>
        [Output]
        public string MiddleInitial
        {
            get { return _middleInitial; }
        }

        private string _email = "";
        /// <summary>
        /// Gets the email address of the build user.
        /// </summary>
        [Output]
        public string Email
        {
            get { return _email; }
        }

        private string _phone = "";
        /// <summary>
        /// Gets the phone number of the build user.
        /// </summary>
        [Output]
        public string Phone
        {
            get { return _phone; }
        }

        /// <summary>
        /// Gets the username and domain name of the build user in "[Domain name]\[User name]" format.
        /// </summary>
        [Output]
        public string UserNameWithDomain
        {
            get { return string.Concat(_domainName, @"\", _userName); }
        }

        /// <summary>
        /// Gets the full name of the build user in "[First name] [Middle initial]. [Last name]" format .
        /// </summary>
        [Output]
        public string FullName
        {
            get { return string.Concat(_firstName, " ", _middleInitial, ". ", _lastName); }
        }
        #endregion Output Parameters

        #region Task Overrides
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the task ran successfully; otherwise <see langword="false"/>.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                _userName = Environment.UserName;
                _domainName = Environment.UserDomainName;

                DirectoryEntry userEntry = null;
                DirectorySearcher searcher = new DirectorySearcher();
                try
                {
                    // Try to retrieve user information from Active Directory if possible.
                    searcher.Filter = string.Format("(SAMAccountName={0})", _userName);
                    userEntry = searcher.FindOne().GetDirectoryEntry();

                    if (userEntry != null)
                    {
                        _firstName = userEntry.Properties["givenName"][0].ToString();
                        _lastName = userEntry.Properties["sn"][0].ToString();
                        _middleInitial = userEntry.Properties["initials"][0].ToString();
                        _email = userEntry.Properties["mail"][0].ToString();
                        _phone = userEntry.Properties["telephoneNumber"][0].ToString();
                    }
                }
                catch
                {
                    Log.LogWarning(Properties.Resources.ActiveDirectoryLookupException, UserNameWithDomain);
                }
                finally
                {
                    searcher.Dispose();
                    if (userEntry != null) { userEntry.Dispose(); }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
        #endregion Task Overrides
    }
}
