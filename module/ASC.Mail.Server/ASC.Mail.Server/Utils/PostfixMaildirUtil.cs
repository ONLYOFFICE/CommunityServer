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

namespace ASC.Mail.Server.Utils
{
    public static class PostfixMaildirUtil
    {
        public static string GenerateMaildirPath(string domain, string localpart, DateTime creationdate)
        {
            var maildir = domain + "/";

            if (localpart.Length >= 3)
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[1], localpart[2]);
            }
            else if (localpart.Length == 2)
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[1], localpart[1]);
            }
            else
            {
                maildir += string.Format("{0}/{1}/{2}/", localpart[0], localpart[0], localpart[0]);
            }

            maildir += string.Format("{0}-{1}/", localpart, creationdate.ToString("yyyy.MM.dd.HH.mm.ss"));

            return maildir.ToLower();
        }
    }
}
