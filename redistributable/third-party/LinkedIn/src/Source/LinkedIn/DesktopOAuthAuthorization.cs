//-----------------------------------------------------------------------
// <copyright file="DesktopOAuthAuthorization.cs">
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
using System.Diagnostics;
using System.Globalization;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace LinkedIn
{
  /// <summary>
  /// Provides OAuth authorization to installed LinkedIn desktop applications.
  /// </summary>
  [Serializable]
  public class DesktopOAuthAuthorization : OAuthAuthorization
  {
    /// <summary>
    /// The request token that is being authorized.
    /// </summary>
    private string requestToken;

    /// <summary>
    /// Private member to hold the access token.
    /// </summary>
    private string accessToken;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopOAuthAuthorization"/> class
    /// that uses a custom token manager.
    /// </summary>
    /// <param name="tokenManager">The token manager.</param>
    /// <param name="accessToken">The access token.</param>
    public DesktopOAuthAuthorization(IConsumerTokenManager tokenManager, string accessToken)
      : this(tokenManager, accessToken, LinkedInServiceDescription)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopOAuthAuthorization"/> class
    /// that uses a custom token manager.
    /// </summary>
    /// <param name="tokenManager">The token manager.</param>
    /// <param name="accessToken">The access token.</param>
    public DesktopOAuthAuthorization(IConsumerTokenManager tokenManager, string accessToken, ServiceProviderDescription serviceProviderDescription)
      : base(new DesktopConsumer(serviceProviderDescription, tokenManager))
    {
      this.accessToken = accessToken;
    }

    /// <summary>
    /// Gets the consumer.
    /// </summary>
    /// <value>The consumer.</value>
    private new DesktopConsumer Consumer
    {
      get { return (DesktopConsumer)base.Consumer; }
    }

    /// <summary>
    /// Gets or sets the function that will ask the user for the verifier code (PIN).
    /// </summary>
    public Func<string> GetVerifier { get; set; }
    
    /// <summary>
    /// Gets the access token.
    /// </summary>
    /// <value>The access token.</value>
    public override string AccessToken
    {
      get { return this.accessToken; }
    }

    /// <summary>
    /// Gets the token manager.
    /// </summary>
    private IConsumerTokenManager TokenManager
    {
      get { return base.Consumer.TokenManager; }
    }

    /// <summary>
    /// Generates the URI to direct the user to in order to complete authorization.
    /// </summary>
    public Uri BeginAuthorize()
    {
      return this.Consumer.RequestUserAuthorization(null, null, out this.requestToken);
    }

    /// <summary>
    /// Exchanges an authorized request token for an access token.
    /// </summary>
    /// <returns>The newly acquired access token, or <c>null</c> if no authorization complete message was in the HTTP request.</returns>
    /// <remarks>The <see cref="GetVerifier"/> property MUST be set before this call.</remarks>
    public string CompleteAuthorize()
    {
      if (this.GetVerifier == null)
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}.GetVerifier must be set first.", this.GetType().Name));
      }

      var response = this.Consumer.ProcessUserAuthorization(this.requestToken, this.GetVerifier());
      this.accessToken = response.AccessToken;
      return response.AccessToken;
    }

    /// <summary>
    /// Invokes the entire authorization flow and blocks until it is complete.
    /// </summary>
    /// <returns>The newly acquired access token, or <c>null</c> if no authorization complete message was in the HTTP request.</returns>
    /// <remarks>The <see cref="GetVerifier"/> property MUST be set before this call.</remarks>
    public string Authorize()
    {
      if (this.GetVerifier == null)
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}.GetVerifier must be set first.", this.GetType().Name));
      }

      Process.Start(this.BeginAuthorize().AbsoluteUri);
      return this.CompleteAuthorize();
    }
  }
}
