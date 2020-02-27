/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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

        public static readonly ISystemAccount CoreSystem = new SystemAccount(new Guid("A37EE56E-3302-4a7b-B67E-DDBEA64CD032"), "asc system", true);

        public static readonly ISystemAccount Guest = new SystemAccount(new Guid("712D9EC3-5D2B-4b13-824F-71F00191DCCA"), "guest", false);

        public static readonly IPrincipal Anonymous = new GenericPrincipal(Guest, new[] {Role.Everyone});

        public static readonly ISystemAccount[] SystemAccounts = new[] {CoreSystem, Guest};

        public static readonly int DefaultTrialPeriod = 30;
    }
}