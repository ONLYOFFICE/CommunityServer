/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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