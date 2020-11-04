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


namespace ASC.Data.Storage
{
    public class Wildcard
    {
        public static bool IsMatch(string pattern, string input)
        {
            return IsMatch(pattern, input, false);
        }

        public static bool IsMatch(string pattern, string input, bool caseInsensitive)
        {
            var offsetInput = 0;
            var isAsterix = false;

            while (true)
            {
                int i;
                for (i = 0; i < pattern.Length; )
                {
                    switch (pattern[i])
                    {
                        case '?':
                            isAsterix = false;
                            offsetInput++;
                            break;
                        case '*':
                            isAsterix = true;
                            while (i < pattern.Length && pattern[i] == '*')
                            {
                                i++;
                            }
                            if (i >= pattern.Length)
                            {
                                return true;
                            }
                            continue;
                        default:
                            if (offsetInput >= input.Length)
                            {
                                return false;
                            }
                            if ((caseInsensitive ? char.ToLower(input[offsetInput]) : input[offsetInput]) != (caseInsensitive ? char.ToLower(pattern[i]) : pattern[i]))
                            {
                                if (!isAsterix)
                                {
                                    return false;
                                }
                                offsetInput++;
                                continue;
                            }
                            offsetInput++;
                            break;
                    } // end switch
                    i++;
                } // end for

                // have we finished parsing our input?
                if (i > input.Length)
                {
                    return false;
                }
                // do we have any lingering asterixes we need to skip?
                while (i < pattern.Length && pattern[i] == '*')
                {
                    ++i;
                }
                // final evaluation. The index should be pointing at the
                // end of the string.
                return (offsetInput == input.Length);
            }
        }
    }
}