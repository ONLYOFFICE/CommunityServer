//-----------------------------------------------------------------------
// <copyright file="LinkedInNotAuthorizedException.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;

namespace LinkedIn.Utility
{
  /// <summary>
  /// Represents an not authorized error returned by the LinkedIn API.
  /// </summary>
  [Serializable]
  public class LinkedInNotAuthorizedException : LinkedInException
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInNotAuthorizedException"/> class.
    /// </summary>
    public LinkedInNotAuthorizedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInNotAuthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message returned by LinkedIn.</param>
    public LinkedInNotAuthorizedException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInNotAuthorizedException"/> class.
    /// </summary>
    /// <param name="httpStatus">The http status returned by LinkedIn.</param>
    /// <param name="errorCode">The specific error code returned by LinkedIn.</param>
    /// <param name="message">The error message returned by LinkedIn.</param>
    public LinkedInNotAuthorizedException(int httpStatus, string errorCode, string message)
      : base(httpStatus, errorCode, message)
    {
    }
  }
}
