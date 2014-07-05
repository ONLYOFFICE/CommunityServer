//-----------------------------------------------------------------------
// <copyright file="WebOAuthAuthorization.cs">
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
  /// Provides OAuth authorization to LinkedIn client web applications.
  /// </summary>
  [Serializable]
  public class WebOAuthAuthorization : OAuthAuthorization
  {
    /// <summary>
    /// Private member to hold the access token.
    /// </summary>
    private string accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebOAuthAuthorization"/> class.
    /// </summary>
    /// <param name="tokenManager">The token manager.</param>
    /// <param name="accessToken">The access token, or null if the user doesn't have one yet.</param>
    public WebOAuthAuthorization(IConsumerTokenManager tokenManager, string accessToken)
      : this(tokenManager, accessToken, LinkedInServiceDescription)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebOAuthAuthorization"/> class.
    /// </summary>
    /// <param name="tokenManager">The token manager.</param>
    /// <param name="accessToken">The access token, or null if the user doesn't have one yet.</param>
    /// <param name="serviceProviderDescription">The service provider description.</param>
    public WebOAuthAuthorization(IConsumerTokenManager tokenManager, string accessToken, ServiceProviderDescription serviceProviderDescription) :
      base(new WebConsumer(serviceProviderDescription, tokenManager))
    {
      this.accessToken = accessToken;
    }

    /// <summary>
    /// Gets the access token.
    /// </summary>
    /// <value>The access token.</value>
    public override string AccessToken
    {
      get { return this.accessToken; }
    }

    /// <summary>
    /// Gets the consumer.
    /// </summary>
    /// <value>The consumer.</value>
    protected new WebConsumer Consumer
    {
      get { return (WebConsumer)base.Consumer; }
    }

    /// <summary>
    /// Requests LinkedIn to authorize this client.
    /// </summary>
    public void BeginAuthorize()
    {
      this.Consumer.Channel.Send(this.Consumer.PrepareRequestUserAuthorization());
    }

    /// <summary>
    /// Requests LinkedIn to authorize this client.
    /// </summary>
    public void BeginAuthorize(Uri callback)
    {
      this.Consumer.Channel.Send(this.Consumer.PrepareRequestUserAuthorization(callback, null, null));
    }

    /// <summary>
    /// Exchanges an authorized request token for an access token.
    /// </summary>
    /// <returns>The newly acquired access token, or <c>null</c> if no authorization complete message was in the HTTP request.</returns>
    public string CompleteAuthorize()
    {
      var response = this.Consumer.ProcessUserAuthorization();
      if (response != null)
      {
        this.accessToken = response.AccessToken;
        return response.AccessToken;
      }

      return null;
    }
  }
}
