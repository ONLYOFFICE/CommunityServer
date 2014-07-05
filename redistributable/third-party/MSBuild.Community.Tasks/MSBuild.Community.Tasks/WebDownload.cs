#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

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
using System.Net;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;




namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Downloads a resource with the specified URI to a local file. 
    /// </summary>
    /// <example>Download the Microsoft.com home page.
    /// <code><![CDATA[
    /// <WebDownload FileUri="http://www.microsoft.com/default.aspx" 
    ///     FileName="microsoft.html" />
    /// ]]></code>
    /// </example>
    /// <example>Download a page from your local intranet protected by Windows Authentication
    /// <code><![CDATA[
    /// <WebDownload FileUri="http://intranet/default.aspx" FileName="page.html" UseDefaultCredentials="True" />
    /// ]]></code>
    /// </example>
    /// <example>Download a page from a password protected website
    /// <code><![CDATA[
    /// <WebDownload FileUri="http://example.com/myscore.aspx" FileName="page.html" Username="joeuser" Password="password123" />
    /// ]]></code>
    /// </example>
    public class WebDownload : Task
    {
        #region Properties
        private string _fileName;

        /// <summary>
        /// Gets or sets the name of the local file that is to receive the data.
        /// </summary>
        /// <value>The name of the file.</value>
        [Required]
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        private string _fileUri;
        private bool useDefaultCredentials;
        private string username;
        private string password;
        private string domain;

        /// <summary>
        /// Gets or sets the URI from which to download data.
        /// </summary>
        /// <value>The file URI.</value>
        [Required]
        public string FileUri
        {
            get { return _fileUri; }
            set { _fileUri = value; }
        }

        /// <summary>
        /// When true, the current user's credentials are used to authenticate against the remote web server
        /// </summary>
        /// <remarks>
        /// This value is ignored if the <see cref="Username"/> property is set to a non-empty value.</remarks>
        public bool UseDefaultCredentials
        {
            get { return useDefaultCredentials; }
            set { useDefaultCredentials = value; }
        }

        /// <summary>
        /// The username used to authenticate against the remote web server
        /// </summary>
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        /// <summary>
        /// The password used to authenticate against the remote web server. A value for <see cref="Username"/> must also be provided.
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// The domain of the user being used to authenticate against the remote web server. A value for <see cref="Username"/> must also be provided.
        /// </summary>
        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        #endregion


        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            Log.LogMessage("Downloading File \"{0}\" from \"{1}\".", _fileName, _fileUri);

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = GetConfiguredCredentials();
                    client.DownloadFile(_fileUri, _fileName);
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            Log.LogMessage("Successfully Downloaded File \"{0}\" from \"{1}\"", _fileName, _fileUri);
            return true;
        }

        /// <summary>
        /// Determines which credentials to pass with the web request
        /// </summary>
        /// <returns></returns>
        public ICredentials GetConfiguredCredentials()
        {
            if (!String.IsNullOrEmpty(username))
            {
                return new NetworkCredential(username, password, domain);
            }
            if (useDefaultCredentials)
            {
                return CredentialCache.DefaultCredentials;
            }
            return null;
        }
    }
}
