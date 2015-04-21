//-----------------------------------------------------------------------
// <copyright file="LinkedInException.cs" company="Beemway">
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
  /// Represents an error returned by the LinkedIn API.
  /// </summary>
  [Serializable]
  public class LinkedInException : Exception
  {
    /// <summary>
    /// Gets or sets the http status returned by LinkedId.
    /// </summary>
    public int HttpStatus { get; set; }

    /// <summary>
    /// Gets or sets the specific error code returned by LinkedIn.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInException"/> class.
    /// </summary>
    public LinkedInException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInException"/> class.
    /// </summary>
    /// <param name="message">The error message returned by LinkedIn.</param>
    public LinkedInException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInException"/> class.
    /// </summary>
    /// <param name="message">The error message returned by LinkedIn.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public LinkedInException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInException"/> class.
    /// </summary>
    /// <param name="httpStatus">The http status returned by LinkedIn.</param>
    /// <param name="errorCode">The specific error code returned by LinkedIn.</param>
    /// <param name="message">The error message returned by LinkedIn.</param>
    public LinkedInException(int httpStatus, string errorCode, string message)
      : base(message)
    {
      this.HttpStatus = httpStatus;
      this.ErrorCode = errorCode;
    }
  }
}
