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
