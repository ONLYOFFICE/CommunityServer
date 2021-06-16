/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Text.RegularExpressions;
using ASC.Mail.Net.Mail;

namespace ASC.Mail.Autoreply.ParameterResolvers
{
    internal class TitleResolver : IParameterResolver
    {
        private readonly Regex[] _ignorePatterns;

        public TitleResolver(params Regex[] ignorePatterns)
        {
            _ignorePatterns = ignorePatterns;
        }

        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            var subject = mailMessage.Subject;

            foreach (var pattern in _ignorePatterns)
            {
                subject = pattern.Replace(subject, "");
            }

            return Regex.Replace(subject, @"\s+", " ").Trim(' ');
        }
    }
}
