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


using System;

namespace ASC.Mail.Autoreply.Utility
{
    public static class StringDistance
    {
        public static int LevenshteinDistance(string s, string t)
        {
            return LevenshteinDistance(s, t, true);
        }

        public static int LevenshteinDistance(string s, string t, bool ignoreCase)
        {
            if (String.IsNullOrEmpty(s))
            {
                return String.IsNullOrEmpty(t) ? 0 : t.Length;
            }
            if (String.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            if (ignoreCase)
            {
                s = s.ToLowerInvariant();
                t = t.ToLowerInvariant();
            }
            
            int n = s.Length;
            int m = t.Length;

            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}
