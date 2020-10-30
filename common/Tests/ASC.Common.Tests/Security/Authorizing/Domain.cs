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


#if DEBUG
using System;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using AzAction = ASC.Common.Security.Authorizing.Action;

namespace ASC.Common.Tests.Security.Authorizing
{

    static class Domain
    {
        public static readonly Guid accountAlientID = Guid.NewGuid();
        public static readonly IAccount accountAlient = new UserAccount(accountAlientID, "Djohn Doo");

        public static readonly Guid accountLevID = Guid.NewGuid();
        public static readonly IAccount accountLev = new UserAccount(accountLevID, "Lev");

        public static readonly Guid accountAntonID = Guid.NewGuid();
        public static readonly IAccount accountAnton = new UserAccount(accountAntonID, "anton");

        public static readonly Guid accountNikID = Guid.NewGuid();
        public static readonly IAccount accountNik = new UserAccount(accountNikID, "nikolay");

        public static readonly Guid accountValeryID = Guid.NewGuid();
        public static readonly IAccount accountValery = new UserAccount(accountValeryID, "Schumaher");

        public static readonly Guid accountKatID = Guid.NewGuid();
        public static readonly IAccount accountKat = new UserAccount(accountKatID, "Kat");

        public static readonly Guid accountMessangerServiceID = Guid.NewGuid();
        public static readonly IAccount accountMessangerService = new AccountS(accountMessangerServiceID, "Messanger Service");


        public static readonly Guid roleAVSID = Guid.NewGuid();
        public static readonly IRole roleAVS = new Role(roleAVSID, "AVS");

        public static readonly Guid roleAdministrationID = Guid.NewGuid();
        public static readonly IRole roleAdministration = new Role(roleAdministrationID, "administration");

        public static readonly Guid roleNETID = Guid.NewGuid();
        public static readonly IRole roleNET = new Role(roleNETID, ".NET Deparment");

        public static readonly Guid roleHRID = Guid.NewGuid();
        public static readonly IRole roleHR = new Role(roleHRID, "hr");


        public static readonly RoleFactory RoleProvider = new RoleFactory();

        public static readonly Guid actionAddUserID = Guid.NewGuid();
        public static readonly AzAction actionAddUser = new AzAction(actionAddUserID, "add user");

        public static readonly Guid actionDeleteUserID = Guid.NewGuid();
        public static readonly AzAction actionDeleteUser = new AzAction(actionDeleteUserID, "delete user");

        public static readonly Guid actionChangeSelfContactInfoID = Guid.NewGuid();
        public static readonly AzAction actionChangeSelfContactInfo = new AzAction(actionChangeSelfContactInfoID, "change self contact info");

        public static readonly Guid actionViewInfoID = Guid.NewGuid();
        public static readonly AzAction actionViewInfo = new AzAction(actionViewInfoID, "view user info");

        public static readonly Guid categoryUserManagerID = Guid.NewGuid();
        public static readonly IAction[] categoryUserManager = { actionAddUser, actionDeleteUser, actionChangeSelfContactInfo, actionViewInfo };

        public static readonly Guid actionHREditInfoID = Guid.NewGuid();
        public static readonly AzAction actionHREditInfo = new AzAction(actionHREditInfoID, "edit HR info");

        public static readonly Guid actionHRViewInfoID = Guid.NewGuid();
        public static readonly AzAction actionHRViewInfo = new AzAction(actionHRViewInfoID, "view HR info");

        public static readonly Guid categoryHRID = Guid.NewGuid();
        public static readonly IAction[] categoryHR = new[] { actionHREditInfo, actionHRViewInfo };

        public static readonly Guid actionNETCommitID = Guid.NewGuid();
        public static readonly AzAction actionNETCommit = new AzAction(actionNETCommitID, "SVN Commit");

        public static readonly Guid actionNETViewID = Guid.NewGuid();
        public static readonly AzAction actionNETView = new AzAction(actionNETViewID, "SVN View");

        public static readonly Guid categoryNETID = Guid.NewGuid();
        public static readonly IAction[] categoryNET = new[] { actionNETCommit, actionNETView };

        public static readonly PermissionFactory PermissionProvider = new PermissionFactory();


        static Domain()
        {
            //++ explicit allow, + ancestor of ++
            //-- explicit deny, - ancestor of --
            //aa - explicit audit, a ancestor of aa

            //                           action

            //Owner						  ++

            //Self						  ++

            //EveryOne                     

            //User                                  

            //roleAVS                     ++        

            //    roleHR                   +aa         
            //        accountKat           +a          
            //        accountLev           -a        

            //    roleNET                  +         
            //        accountAnton         +         
            //        accountNik           +         
            //        accountValery       --         

            //    roleAdministration      --          
            //        accountLev           -         

            //    accountLev               -a          
            //    accountAnton             +          
            //    accountNik               +          
            //    accountValery            -         
            //    accountKat               +a          

            RoleProvider.AddAccountInRole(accountLev, roleAVS);
            RoleProvider.AddAccountInRole(accountAnton, roleAVS);
            RoleProvider.AddAccountInRole(accountNik, roleAVS);
            RoleProvider.AddAccountInRole(accountValery, roleAVS);
            RoleProvider.AddAccountInRole(accountKat, roleAVS);

            RoleProvider.AddAccountInRole(roleHR, roleAVS);
            RoleProvider.AddAccountInRole(roleNET, roleAVS);
            RoleProvider.AddAccountInRole(roleAdministration, roleAVS);

            RoleProvider.AddAccountInRole(accountLev, roleAdministration);

            RoleProvider.AddAccountInRole(accountKat, roleHR);
            RoleProvider.AddAccountInRole(accountLev, roleHR);

            RoleProvider.AddAccountInRole(accountAnton, roleNET);
            RoleProvider.AddAccountInRole(accountNik, roleNET);
            RoleProvider.AddAccountInRole(accountValery, roleNET);

            PermissionProvider.AddAce(Domain.roleAVS, actionAddUser, AceType.Allow);
            PermissionProvider.AddAce(Domain.roleAdministration, actionAddUser, AceType.Deny);
            PermissionProvider.AddAce(Domain.accountValery, actionAddUser, AceType.Deny);

            PermissionProvider.AddAce(Constants.Owner, actionAddUser, AceType.Allow);
            PermissionProvider.AddAce(Constants.Self, actionAddUser, AceType.Allow);
        }
    }
}
#endif