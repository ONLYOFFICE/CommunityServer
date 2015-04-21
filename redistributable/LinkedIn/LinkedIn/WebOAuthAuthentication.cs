//-----------------------------------------------------------------------
// <copyright file="WebOAuthAuthentication.cs">
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
using System.Web;

using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;

namespace LinkedIn
{
  /// <summary>
  /// Provides authentication using OAuth to LinkedIn client web applications.
  /// </summary>
  [Serializable]
  public class WebOAuthAuthentication : WebOAuthAuthorization
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="WebOAuthAuthentication"/> class.
    /// </summary>
    /// <param name="tokenManager">The token manager.</param>
    /// <param name="accessToken">The access token, or null if the user doesn't have one yet.</param>
    public WebOAuthAuthentication(IConsumerTokenManager tokenManager, string accessToken)
      : base(tokenManager, accessToken, LinkedInAuthenticationServiceDescription)
    {
    }
  }
}
