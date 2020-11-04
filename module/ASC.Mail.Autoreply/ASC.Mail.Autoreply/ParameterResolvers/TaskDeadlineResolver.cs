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
using System.Text.RegularExpressions;
using ASC.Mail.Net.Mail;

namespace ASC.Mail.Autoreply.ParameterResolvers
{
    internal class TaskDeadlineResolver : IParameterResolver
    {
        public static readonly Regex Pattern = new Regex(@"\^(?'year'\d{4})-(?'month'\d{1,2})-(?'day'\d{1,2})", RegexOptions.Compiled);

        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            if (!Pattern.IsMatch(mailMessage.Subject))
                return null;

            var match = Pattern.Match(mailMessage.Subject);
            return new DateTime(Convert.ToInt32(match.Groups["year"].Value), Convert.ToInt32(match.Groups["month"].Value), Convert.ToInt32(match.Groups["day"].Value));
        }
    }
}
