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


using ASC.Core;
using ASC.Web.Core.Mail;
using ASC.Web.Mail.Controls;
using ASC.Web.Studio.Core;

namespace ASC.Web.Mail.Configuration
{
    public class Settings
    {
        public static bool IsAdministrationPageAvailable()
        {
            return SetupInfo.IsVisibleSettings<AdministrationPage>() &&
                   (!CoreContext.Configuration.Standalone || MailServiceHelper.IsMailServerAvailable());
        }

        public static bool IsMailCommonDomainAvailable()
        {
            return !CoreContext.Configuration.Standalone &&
                   SetupInfo.IsVisibleSettings("MailCommonDomain") &&
                   IsAdministrationPageAvailable();
        }

    }
}
