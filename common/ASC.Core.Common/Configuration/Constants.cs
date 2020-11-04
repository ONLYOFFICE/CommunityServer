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
using System.Security.Principal;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;

namespace ASC.Core.Configuration
{
    public sealed class Constants
    {
        public static readonly string NotifyEMailSenderSysName = "email.sender";

        public static readonly string NotifyMessengerSenderSysName = "messanger.sender";

        public static readonly string NotifyPushSenderSysName = "push.sender";

        public static readonly string NotifyTelegramSenderSysName = "telegram.sender";

        public static readonly ISystemAccount CoreSystem = new SystemAccount(new Guid("A37EE56E-3302-4a7b-B67E-DDBEA64CD032"), "asc system", true);

        public static readonly ISystemAccount Guest = new SystemAccount(new Guid("712D9EC3-5D2B-4b13-824F-71F00191DCCA"), "guest", false);

        public static readonly IPrincipal Anonymous = new GenericPrincipal(Guest, new[] {Role.Everyone});

        public static readonly ISystemAccount[] SystemAccounts = new[] {CoreSystem, Guest};

        public static readonly int DefaultTrialPeriod = 30;
    }
}