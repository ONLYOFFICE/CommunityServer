//-----------------------------------------------------------------------
// <copyright file="SortCriteria.cs" company="Beemway">
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
  /// The different options for sorting the response.
  /// </summary>
  public enum SortCriteria
  {
    /// <summary>
    /// Orders the results by number of connections.
    /// </summary>
    [Description("connections")]
    Connections = 0,

    /// <summary>
    /// Orders the results by number of recommenders.
    /// </summary>
    [Description("recommenders")]
    Recommenders = 1,

    /// <summary>
    /// Orders the results based on the ascending degree of separation within a member's network, with first degree connections first.
    /// </summary>
    [Description("distance")]
    Distance = 2,

    /// <summary>
    /// Orders the results based on relevance for the keywords provided.
    /// </summary>
    [Description("relevance")]
    Relevance = 3
  }
}
