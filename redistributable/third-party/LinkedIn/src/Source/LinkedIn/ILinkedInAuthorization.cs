//-----------------------------------------------------------------------
// <copyright file="ILinkedInAuthorization.cs">
//     Copyright (c) Andrew Arnott. All rights reserved.
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace LinkedIn
{
  /// <summary>
  /// Provides steps to authorize a LinkedIn client and send authorized HTTP requests.
  /// </summary>
  public interface ILinkedInAuthorization
  {
    #region Non-authorization HTTP-specific details
    /// <summary>
    /// Gets or sets the read write timeout.
    /// </summary>
    /// <value>The read write timeout.</value>
    TimeSpan ReadWriteTimeout { get; set; }

    /// <summary>
    /// Gets or sets the timeout.
    /// </summary>
    /// <value>The timeout.</value>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Gets or sets the user agent.
    /// </summary>
    /// <value>The user agent.</value>
    string UserAgent { get; set; }
    #endregion
        
    /// <summary>
    /// Prepares an authorized HTTP GET request.
    /// </summary>
    /// <param name="requestUrl">The request URL.</param>
    /// <returns>The <see cref="HttpWebRequest"/> object that may be further customized.</returns>
    HttpWebRequest InitializeGetRequest(Uri requestUrl);

    /// <summary>
    /// Prepares an authorized HTTP POST request without sending a POST entity stream.
    /// </summary>
    /// <param name="requestUrl">The request URL.</param>
    /// <returns>The <see cref="HttpWebRequest"/> object that may be further customized.</returns>
    HttpWebRequest InitializePostRequest(Uri requestUrl);

    /// <summary>
    /// Prepares an authorized HTTP PUT request.
    /// </summary>
    /// <param name="requestUrl">The request URL.</param>
    /// <returns>The <see cref="HttpWebRequest"/> object that may be further customized.</returns>
    HttpWebRequest InitializePutRequest(Uri requestUrl);

    /// <summary>
    /// Prepares an authorized HTTP DELETE request.
    /// </summary>
    /// <param name="requestUrl">The request URL.</param>
    /// <returns>The <see cref="HttpWebRequest"/> object that may be further customized.</returns>
    HttpWebRequest InitializeDeleteRequest(Uri requestUrl);
  }
}
