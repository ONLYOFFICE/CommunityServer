// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

using System.Collections.Generic;

namespace HtmlAgilityPack
{
    internal static class Utilities
    {
        public static TValue GetDictionaryValueOrNull<TKey,TValue>(Dictionary<TKey,TValue> dict, TKey key) where TKey: class
        {
            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }
    }
}
