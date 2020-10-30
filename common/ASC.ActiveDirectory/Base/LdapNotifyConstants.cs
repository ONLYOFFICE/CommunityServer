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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Patterns;

namespace ASC.ActiveDirectory.Base
{
    public static class NotifyConstants
    {
        public static string TagUserName = "UserName";
        public static string TagUserEmail = "UserEmail";
        public static string TagMyStaffLink = "MyStaffLink";

        public static INotifyAction ActionLdapActivation = new NotifyAction("user_ldap_activation");

        public static ITagValue TagGreenButton(string btnText, string btnUrl)
        {
            Func<string> action = () =>
            {
                return
                    string.Format(@"<table style=""height: 48px; width: 540px; border-collapse: collapse; empty-cells: show; vertical-align: middle; text-align: center; margin: 30px auto; padding: 0;""><tbody><tr cellpadding=""0"" cellspacing=""0"" border=""0"">{2}<td style=""height: 48px; width: 380px; margin:0; padding:0; background-color: #66b76d; -moz-border-radius: 2px; -webkit-border-radius: 2px; border-radius: 2px;""><a style=""{3}"" target=""_blank"" href=""{0}"">{1}</a></td>{2}</tr></tbody></table>",
                        btnUrl,
                        btnText,
                        "<td style=\"height: 48px; width: 80px; margin:0; padding:0;\">&nbsp;</td>",
                        "color: #fff; font-family: Helvetica, Arial, Tahoma; font-size: 18px; font-weight: 600; vertical-align: middle; display: block; padding: 12px 0; text-align: center; text-decoration: none; background-color: #66b76d;");
            };
            return new TagActionValue("GreenButton", action);
        }

        private class TagActionValue : ITagValue
        {
            private readonly Func<string> action;

            public string Tag
            {
                get;
                private set;
            }

            public object Value
            {
                get { return action(); }
            }

            public TagActionValue(string name, Func<string> action)
            {
                Tag = name;
                this.action = action;
            }
        }
    }

    public static class NotifyCommonTags
    {
        public static string Footer = "Footer";

        public static string MasterTemplate = "MasterTemplate";

        public static string WithoutUnsubscribe = "WithoutUnsubscribe";
    }
}
