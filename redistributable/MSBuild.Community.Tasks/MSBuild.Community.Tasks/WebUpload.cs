#region Copyright © 2014 Paul Welter. All rights reserved.
/*
Copyright © 2014 Paul Welter. All rights reserved.

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
    /// Upload a local file to a remote URI. 
    /// </summary>
    /// <example>Upload the xml file.
    /// <code><![CDATA[
    /// <WebUpload RemoteUri="http://intranet/upload" FileName="page.xml" />
    /// ]]></code>
    /// </example>
    /// <example>Upload the xml file to a protected by Windows Authentication
    /// <code><![CDATA[
    /// <WebUpload RemoteUri="http://intranet/upload" FileName="page.xml" UseDefaultCredentials="True" />
    /// ]]></code>
    /// </example>
    /// <example>Upload the xml file to a password protected website
    /// <code><![CDATA[
    /// <WebUpload RemoteUri="http://intranet/upload" FileName="page.xml" Username="joeuser" Password="password123" />
    /// ]]></code>
    /// </example>
    public class WebUpload : Task
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the local file to upload to the specified URI.
        /// </summary>
        /// <value>The name of the file.</value>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the URI to which to upload data to.
        /// </summary>
        /// <value>The remote URI.</value>
        [Required]
        public string RemoteUri { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method to use when uploading data to the speficied URI.
        /// </summary>
        /// <value>The HTTP method to use. If not specified, it defaults to POST.</value>
        public string Method { get; set; } = "POST";

        /// <summary>
        /// When true, the current user's credentials are used to authenticate against the remote web server
        /// </summary>
        /// <remarks>
        /// This value is ignored if the <see cref="Username"/> property is set to a non-empty value.
        /// </remarks>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// The username used to authenticate against the remote web server
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password used to authenticate against the remote web server. A value for <see cref="Username"/> must also be provided.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The domain of the user being used to authenticate against the remote web server. A value for <see cref="Username"/> must also be provided.
        /// </summary>
        public string Domain { get; set; }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            Log.LogMessage("Uploading File \"{0}\" to \"{1}\" using method \"{2}\".", FileName, RemoteUri, Method);

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = GetConfiguredCredentials();
                    byte[] buffer = client.UploadFile(RemoteUri, Method, FileName);
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            Log.LogMessage("Successfully Upload File \"{0}\" to \"{1}\"", FileName, RemoteUri);
            return true;
        }

        /// <summary>
        /// Determines which credentials to pass with the web request
        /// </summary>
        /// <returns></returns>
        public ICredentials GetConfiguredCredentials()
        {
            if (!String.IsNullOrEmpty(Username))
            {
                return new NetworkCredential(Username, Password, Domain);
            }
            if (UseDefaultCredentials)
            {
                return CredentialCache.DefaultCredentials;
            }
            return null;
        }

    }
}
