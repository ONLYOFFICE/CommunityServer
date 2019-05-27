using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Net;
using System.IO;

namespace MSBuild.Community.Tasks.Net
{
    /// <summary>
    /// Makes an HTTP request, optionally validating the result and writing it to a file.
    /// </summary>
    /// <remarks>
    /// Execute a http request to hit the database update.  
    /// Target attributes to set:
    ///     Url (required),
    ///     FailOnNon2xxResponse (200 responses generally means successful http request. default=true), 
    ///     EnsureResponseContains (string to check for),
    ///     WriteResponseTo (file name to write to),
    /// </remarks>
    /// <example>
    /// Example of a update request ensuring "Database upgrade check completed successfully." was returned.
    /// <code><![CDATA[
    ///     <HttpRequest Url="http://mydomain.com/index.php?checkdb=1" 
    ///         EnsureResponseContains="Database upgrade check completed successfully." 
    ///         FailOnNon2xxResponse="true" />
    /// ]]></code>
    /// </example>
    public class HttpRequest : Task
    {

        /// <summary>
        /// The URL to make an HTTP request against.
        /// </summary>
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// Optional: if set then the task fails if the response text doesn't contain the text specified.
        /// </summary>
        public string EnsureResponseContains { get; set; }

        /// <summary>
        /// Default is true.  When true, if the web server returns a status code less than 200 or greater than 299 then the task fails.
        /// </summary>
        public bool FailOnNon2xxResponse { get; set; }

        /// <summary>
        /// Optional, default is GET. The HTTP method to use for the request.
        /// </summary>
        public string Method{ get; set; }

        /// <summary>
        /// Optional. The username to use with basic authentication.
        /// </summary>
        public string Username{ get; set; }

        /// <summary>
        /// Optional. The password to use with basic authentication.
        /// </summary>
        public string Password{ get; set; }

        /// <summary>
        /// Optional; the name of the file to reqd the request from.
        /// </summary>
        public string ReadRequestFrom { get; set; }

        /// <summary>
        /// Optional; the name of the file to write the response to.
        /// </summary>
        public string WriteResponseTo { get; set; }

        /// <summary>
        /// Constructor to set the default parameters for http request
        /// </summary>
        public HttpRequest()
            : base()
        {
            FailOnNon2xxResponse = true;
            EnsureResponseContains = null;
        }

        private bool CheckResponseContents
        {
            get
            {
                return !string.IsNullOrEmpty(EnsureResponseContains);
            }
        }


        private bool ReadRequestFromFile
        {
            get
            {
                return !string.IsNullOrEmpty(ReadRequestFrom);
            }
        }

        private bool WriteResponseToFile
        {
            get
            {
                return !string.IsNullOrEmpty(WriteResponseTo);
            }
        }

        /// <summary>
        /// Entry Point inherited from Task
        /// </summary>
        public override bool Execute()
        {
            Log.LogMessage("Requesting {0}", Url);
            HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
            if (request == null)
            {
                Log.LogError("Url \"{0}\" did not create an HttpRequest.", Url);
                return false;
            }

	    if (!string.IsNullOrEmpty(Method))
	    {
 		request.Method = Method;
	    }

	    if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
	    {
		request.Credentials = new NetworkCredential(Username, Password);
	    }
	
	    if (ReadRequestFromFile) {
		request.SendChunked = true;
		using (FileStream source = File.Open(ReadRequestFrom, FileMode.Open))
		{
			using (Stream requestStream = request.GetRequestStream())
			{
				byte[] buffer = new byte[16 * 1024];
				int bytesRead;
				while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
				{
					requestStream.Write(buffer, 0, bytesRead);
				}
			}
		}
	    }


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                int code = (int)response.StatusCode;
                Log.LogMessage("HTTP RESPONSE: {0}, {1}", code, response.StatusDescription);
                if (FailOnNon2xxResponse)
                {
                    if (code < 200 || code > 299)
                    {
                        Log.LogError("Status code not in Successful 2xx range.");
                        return false;
                    }
                }
                if (CheckResponseContents || WriteResponseToFile)
                {
                    StreamReader responseReader = new StreamReader(response.GetResponseStream());
                    string responseString = responseReader.ReadToEnd();
                    if (WriteResponseToFile)
                    {
                        using (TextWriter tw = new StreamWriter(WriteResponseTo))
                        {
                            tw.Write(responseString);
                            tw.Close();
                        }
                    }
                    if (CheckResponseContents)
                    {
                        if (!responseString.Contains(EnsureResponseContains))
                        {
                            int length = System.Math.Min(100, responseString.Length);
                            Log.LogError("Response did not contain the specified text.  Started with: " + responseString.Substring(0, length));
                            return false;
                        }
                    }
                }
                response.Close();
            }
            return true;
        }

    }
}
