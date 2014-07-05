/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
