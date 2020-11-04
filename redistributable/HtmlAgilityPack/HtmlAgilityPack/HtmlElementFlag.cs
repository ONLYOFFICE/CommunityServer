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
    /// Flags that describe the behavior of an Element node.
    /// </summary>
    [Flags]
    public enum HtmlElementFlag
    {
        /// <summary>
        /// The node is a CDATA node.
        /// </summary>
        CData = 1,

        /// <summary>
        /// The node is empty. META or IMG are example of such nodes.
        /// </summary>
        Empty = 2,

        /// <summary>
        /// The node will automatically be closed during parsing.
        /// </summary>
        Closed = 4,

        /// <summary>
        /// The node can overlap.
        /// </summary>
        CanOverlap = 8
    }
}