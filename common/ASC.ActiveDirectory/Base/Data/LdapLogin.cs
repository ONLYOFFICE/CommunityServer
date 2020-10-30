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


using System.Linq;

namespace ASC.ActiveDirectory.Base.Data
{
    public class LdapLogin
    {
        public string Username { get; private set; }
        public string Domain { get; private set; }

        public LdapLogin(string username, string domain)
        {
            Username = username;
            Domain = domain;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Domain) ? string.Format("{0}@{1}", Username, Domain) : Username;
        }

        public static LdapLogin ParseLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return null;

            string username;
            string domain = null;

            if (login.Contains("\\"))
            {
                var splited = login.Split('\\');

                if (!splited.Any() || splited.Length != 2)
                    return null;

                domain = splited[0];
                username = splited[1];

            }
            else if (login.Contains("@"))
            {
                var splited = login.Split('@');

                if (!splited.Any() || splited.Length != 2)
                    return null;

                username = splited[0];
                domain = splited[1];
            }
            else
            {
                username = login;
            }

            var result = new LdapLogin(username, domain);

            return result;
        }
    }
}
