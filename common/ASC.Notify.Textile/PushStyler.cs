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
using ASC.Common.Notify.Patterns;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;

namespace ASC.Notify.Textile
{
    public class PushStyler : IPatternStyler
    {
        private static readonly Regex VelocityArgumentsRegex = new Regex(NVelocityPatternFormatter.NoStylePreffix + "(?'arg'.*?)" + NVelocityPatternFormatter.NoStyleSuffix, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public void ApplyFormating(NoticeMessage message)
        {
            if (!string.IsNullOrEmpty(message.Subject))
            {
                message.Subject = VelocityArgumentsRegex.Replace(message.Subject, m => m.Groups["arg"].Value);
                message.Subject = message.Subject.Replace(Environment.NewLine, " ").Trim();
            }
            if (!string.IsNullOrEmpty(message.Body))
            {
                message.Body = VelocityArgumentsRegex.Replace(message.Body, m => m.Groups["arg"].Value);
                message.Body = message.Body.Replace(Environment.NewLine, " ").Trim();
            }
        }
    }
}
