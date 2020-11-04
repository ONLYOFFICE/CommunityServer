// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

#if !METRO
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a fragment of code in a mixed code document.
    /// </summary>
    public class MixedCodeDocumentCodeFragment : MixedCodeDocumentFragment
    {
#region Fields

        private string _code;

#endregion

#region Constructors

        internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc)
            :
                base(doc, MixedCodeDocumentFragmentType.Code)
        {
        }

#endregion

#region Properties

        /// <summary>
        /// Gets the fragment code text.
        /// </summary>
        public string Code
        {
            get
            {
                if (_code == null)
                {
                    _code = FragmentText.Substring(Doc.TokenCodeStart.Length,
                                                   FragmentText.Length - Doc.TokenCodeEnd.Length -
                                                   Doc.TokenCodeStart.Length - 1).Trim();
                    if (_code.StartsWith("="))
                    {
                        _code = Doc.TokenResponseWrite + _code.Substring(1, _code.Length - 1);
                    }
                }
                return _code;
            }
            set { _code = value; }
        }

#endregion
    }
}
#endif