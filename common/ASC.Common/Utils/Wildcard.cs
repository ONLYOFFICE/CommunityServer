/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Common.Utils
{
    public static class Wildcard
    {
        public static bool WildcardMatch(this string input, string pattern)
        {
            return WildcardMatch(input, pattern, true);
        }

        public static bool WildcardMatch(this string input, string pattern, bool ignoreCase)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return IsMatch(pattern, input, ignoreCase);
            }
            return false;
        }

        public static bool IsMatch(string pattern, string input)
        {
            return IsMatch(pattern, input, true);
        }

        public static bool IsMatch(string pattern, string input, bool ignoreCase)
        {
            int offsetInput = 0;
            bool isAsterix = false;
            int i;
            while (true)
            {
                for (i = 0; i < pattern.Length;)
                {
                    switch (pattern[i])
                    {
                        case '?':
                            isAsterix = false;
                            offsetInput++;
                            break;
                        case '*':
                            isAsterix = true;
                            while (i < pattern.Length &&
                                   pattern[i] == '*')
                            {
                                i++;
                            }
                            if (i >= pattern.Length)
                                return true;
                            continue;
                        default:
                            if (offsetInput >= input.Length)
                                return false;
                            if ((ignoreCase
                                     ? char.ToLower(input[offsetInput])
                                     : input[offsetInput])
                                !=
                                (ignoreCase
                                     ? char.ToLower(pattern[i])
                                     : pattern[i]))
                            {
                                if (!isAsterix)
                                    return false;
                                offsetInput++;
                                continue;
                            }
                            offsetInput++;
                            break;
                    }
                    i++;
                }

                if (i > input.Length)
                    return false;

                while (i < pattern.Length && pattern[i] == '*')
                    ++i;

                return (offsetInput == input.Length);
            }
        }
    }
}