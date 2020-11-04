// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

using System;
using System.Text;

namespace HtmlAgilityPack
{
    internal class EncodingFoundException : Exception
    {
        #region Fields

        private Encoding _encoding;

        #endregion

        #region Constructors

        internal EncodingFoundException(Encoding encoding)
        {
            _encoding = encoding;
        }

        #endregion

        #region Properties

        internal Encoding Encoding
        {
            get { return _encoding; }
        }

        #endregion
    }
}