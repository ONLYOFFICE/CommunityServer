//-----------------------------------------------------------------------
// <copyright file="NetworkUpdateType.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// The different types of network updates.
  /// </summary>
  [Flags]
  public enum NetworkUpdateTypes
  {
    /// <summary>
    /// A connection has answered a question posed on LinkedIn Answers.
    /// </summary>
    [Description("ANSW")]
    AnswerUpdate = 1,

    /// <summary>
    /// An action that occurred in a partner application either by a connection or by an application itself.
    /// </summary>
    [Description("APPS")]
    ApplicationUpdate = 2,

    /// <summary>
    /// These updates cover aspects of connections made on LinkedIn.
    /// </summary>
    [Description("CONN")]
    ConnectionUpdate = 4,

    /// <summary>
    /// A connection has posted a job posting on LinkedIn.
    /// </summary>
    [Description("JOBS")]
    PostedAJob = 8,

    /// <summary>
    /// A connection has joined a group.
    /// </summary>
    [Description("JGRP")]
    JoinedAGroup = 16,

    /// <summary>
    /// A connection has updated their profile picture.
    /// </summary>
    [Description("PICT")]
    ChangedAPicture = 32,

    /// <summary>
    /// A connection was recommended.
    /// </summary>
    [Description("RECU")]
    Recommendations = 64,

    /// <summary>
    /// A connection has updated their profile. This does not include picture updates.
    /// </summary>
    [Description("PRFU")]
    ChangedProfile = 128,

    /// <summary>
    /// A connection has asked a question on LinkedIn Answers.
    /// </summary>
    [Description("QSTN")]
    QuestionUpdate = 256,

    /// <summary>
    /// A connection has updated their status.
    /// </summary>
    [Description("STAT")]
    [Obsolete]
    StatusUpdate = 512,

    /// <summary>
    /// A connection has shared an update or link.
    /// </summary>
    [Description("SHAR")]
    SharedItem = 1024,

    /// <summary>
    /// All the Network Updates.
    /// </summary>
    [Description("")]
    All = 2048
  }
}
