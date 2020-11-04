//-----------------------------------------------------------------------
// <copyright file="TwitterizerException.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net/)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
#if !SILVERLIGHT
    using System.Runtime.Serialization;
#endif
    using Core;

    /// <summary>
    /// The Twitterizer Exception
    /// </summary>
    /// <seealso cref="System.Net.WebException"/>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterizerException : WebException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterizerException"/> class.
        /// </summary>
        public TwitterizerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterizerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TwitterizerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterizerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TwitterizerException(string message, Exception innerException) :
            base(message, innerException)
        {
            if (innerException.GetType() == typeof(WebException))
            {
                HttpWebResponse response = (HttpWebResponse)((WebException)innerException).Response;

                if (response == null)
                    return;

                Stream responseStream = response.GetResponseStream();
                if (responseStream == null)
                    return;

                byte[] responseData = ConversionUtility.ReadStream(responseStream);

                this.ResponseBody = Encoding.UTF8.GetString(responseData, 0, responseData.Length);

                this.ParseRateLimitHeaders(response);

                if (response.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    this.ErrorDetails = SerializationHelper<TwitterErrorDetails>.Deserialize(responseData, null);
                }
#if !SILVERLIGHT
                else if (response.ContentType.StartsWith("text/xml", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to deserialize as XML (specifically OAuth requests)
                    System.Xml.Serialization.XmlSerializer ds =
                        new System.Xml.Serialization.XmlSerializer(typeof(TwitterErrorDetails));

                    this.ErrorDetails = ds.Deserialize(new MemoryStream(responseData)) as TwitterErrorDetails;
                }
#endif
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterizerException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected TwitterizerException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
#endif
        #endregion

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        public RequestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        /// <value>The response body.</value>
        public string ResponseBody { get; protected set; }

        /// <summary>
        /// Gets or sets the rate limits.
        /// </summary>
        /// <value>The rate limits.</value>
        public RateLimiting RateLimiting { get; protected set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        /// <value>The error details.</value>
        public TwitterErrorDetails ErrorDetails { get; protected set; }

        /// <summary>
        /// Gets the response that the remote host returned.
        /// </summary>
        /// <value></value>
        /// <returns>If a response is available from the Internet resource, a <see cref="T:System.Net.WebResponse"/> instance that contains the error response from an Internet resource; otherwise, null.</returns>
        public new WebResponse Response
        {
            get
            {
                return InnerException == null ? null : ((WebException)this.InnerException).Response;
            }
        }

        /// <summary>
        /// Gets the bug report.
        /// </summary>
        /// <value>The bug report.</value>
        public string BugReport
        {
            get
            {
                StringBuilder reportBuilder = new StringBuilder();
                reportBuilder.AppendFormat(
@"
--------------- ERROR MESSAGE ---------------
{0}

--------------- STACK TRACE -----------------
{1}

--------------- RESPONSE BODY ---------------
{2}
",
                    this.Message,
                    this.StackTrace,
                    this.ResponseBody);

                reportBuilder.Append("--------------- HTTP HEADERS ----------------");
                for (int i = 0; i < this.Response.Headers.Count; i++)
                {
                    reportBuilder.AppendFormat(
                        "{0} = \"{1}\"",
                        this.Response.Headers.AllKeys[i],
                        this.Response.Headers[this.Response.Headers.AllKeys[i]]);
                }

                return reportBuilder.ToString();
            }
        }

        /// <summary>
        /// Parses the rate limit headers.
        /// </summary>
        /// <param name="response">The response.</param>
        protected void ParseRateLimitHeaders(WebResponse response)
        {
            this.RateLimiting = new RateLimiting();

            if (response.Headers.AllKeys.Contains("X-Rate-Limit-Limit") && !string.IsNullOrEmpty(response.Headers["X-Rate-Limit-Limit"]))
            {
                this.RateLimiting.Total = int.Parse(response.Headers["X-Rate-Limit-Limit"], CultureInfo.InvariantCulture);
            }

            if (response.Headers.AllKeys.Contains("X-Rate-Limit-Remaining") && !string.IsNullOrEmpty(response.Headers["X-Rate-Limit-Remaining"]))
            {
                this.RateLimiting.Remaining = int.Parse(response.Headers["X-Rate-Limit-Remaining"], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(response.Headers["X-Rate-Limit-Reset"]) && !string.IsNullOrEmpty(response.Headers["X-Rate-Limit-Reset"]))
            {
                this.RateLimiting.ResetDate = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0)
                    .AddSeconds(double.Parse(response.Headers["X-Rate-Limit-Reset"], CultureInfo.InvariantCulture)), DateTimeKind.Utc);
            }
        }
    }
}
