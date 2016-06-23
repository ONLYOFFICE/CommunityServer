/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Autoreply.Utility;
using ASC.Mail.Net.Mail;

namespace ASC.Mail.Autoreply.ParameterResolvers
{
    internal class TaskResponsiblesResolver : IParameterResolver
    {
        public static readonly Regex Pattern = new Regex(@"@\w+\s+\w+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            if (!Pattern.IsMatch(mailMessage.Subject))
                return null;

            var users = new List<object>();
            foreach (Match match in Pattern.Matches(mailMessage.Subject))
            {
                var words = match.Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                Guid user;
                if (TryGetUser(words[0], words[1], out user))
                {
                    users.Add(user);
                }
            }

            return users;
        }

        private static bool TryGetUser(string word1, string word2, out Guid userId)
        {
            userId = Guid.Empty;

            var users = CoreContext.UserManager.GetUsers()
                                  .GroupBy(u => GetDistance(u, word1, word2))
                                  .OrderBy(g => g.Key)
                                  .FirstOrDefault(g => g.Key < 3);

            if (users != null && users.Count() == 1)
            {
                userId = users.First().ID;
                return true;
            }

            return false;
        }

        private static int GetDistance(UserInfo user, string word1, string word2)
        {
            var distance1 = StringDistance.LevenshteinDistance(user.FirstName, word1) + StringDistance.LevenshteinDistance(user.LastName, word2);
            var distance2 = StringDistance.LevenshteinDistance(user.FirstName, word2) + StringDistance.LevenshteinDistance(user.LastName, word1);
            return Math.Min(distance1, distance2);
        }
    }

    internal class TaskResponsibleResolver : IParameterResolver
    {
        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            var responsibles = new TaskResponsiblesResolver().ResolveParameterValue(mailMessage) as List<object>;

            return responsibles != null ? responsibles.FirstOrDefault() : null;
        }
    }
}
