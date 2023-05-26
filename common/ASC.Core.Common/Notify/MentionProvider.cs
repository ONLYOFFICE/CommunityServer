/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Linq;
using System.Text.RegularExpressions;

using ASC.Core.Users;

namespace ASC.Core.Common.Notify
{
    public static class MentionProvider
    {
        /// <summary>
        /// Returns array of users that were mentioned in comments
        /// </summary>
        /// <param name="content">Comment content</param>
        /// <returns>Array of users</returns>
        public static UserInfo[] GetMentionedUsers(string content)
        {
            var mentionRegex = new Regex("<a.*? mention=\"true\".*?>@(.*?)</a>");

            var matchCollection = mentionRegex.Matches(content);

            if (matchCollection.Count > 0)
            {
                return matchCollection.Cast<Match>()
                         .Select(match => CoreContext.UserManager.GetUserByEmail(match.Groups[1].Value))
                         .Where(user => user.ID != Constants.LostUser.ID)
                         .ToArray();
            }

            return new UserInfo[] { };
        }

    }
}
