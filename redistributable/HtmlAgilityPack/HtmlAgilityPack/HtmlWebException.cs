// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

using System;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an exception thrown by the HtmlWeb utility class.
    /// </summary>
    public class HtmlWebException : Exception
    {
#region Constructors

        /// <summary>
        /// Creates an instance of the HtmlWebException.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public HtmlWebException(string message)
            : base(message)
        {
        }

#endregion
    }
}