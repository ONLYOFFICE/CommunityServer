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
using ASC.Api.Attributes;
using ASC.Web.Studio.UserControls.Common.HelpCenter;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns the string with html of help center page
        /// </summary>
        /// <returns>String with html of help center page</returns>
        /// <short>Get html of help center page</short> 
        /// <category>HelpCenter</category>
        [Read(@"helpcenter")]
        public string GetHelpCenterHtml()
        {
            return HelpCenter.RenderControlToString(new Guid("{2A923037-8B2D-487b-9A22-5AC0918ACF3F}"));
        }
    }
}
