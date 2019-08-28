/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using ASC.Core;
using ASC.Core.Users;
using ASC.Common.Threading.Progress;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using Constants = ASC.Core.Users.Constants;
using ASC.Mail.Utils;
using ASC.Mail.Data.Contracts;

namespace ASC.Web.People.Core.Import
{
    public enum Operation
    {
        Success = 1,
        Error = 0
    }

    [Serializable]
    public sealed class UserData
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ImportUsersTask : IProgressItem
    {
        private readonly string userList;
        private bool importUsersAsCollaborators;
        private readonly Dictionary<string, string> httpHeaders;

        public readonly List<UserResults> Data = new List<UserResults>();
        public object Id { get; set; }
        public Guid UserId { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public int GetUserCounter { get; set; }
        public bool IsCompleted { get; set; }

        public ImportUsersTask(string userList, bool importUsersAsCollaborators, Dictionary<string, string> httpHeaders)
        {
            this.userList = userList;
            this.importUsersAsCollaborators = importUsersAsCollaborators;
            this.httpHeaders = httpHeaders;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void RunJob()
        {
            Status = (int)Operation.Success;
            CoreContext.TenantManager.SetCurrentTenant((int)Id);
            SecurityContext.AuthenticateMe(UserId);

            if (!SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser))
            {
                Error = Resource.ErrorAccessDenied;
                IsCompleted = true;
                return;
            }

            try
            {
                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var ruleObj = jsSerializer.Deserialize<List<UserData>>(userList);
                var error = 0;
                Address address;
                GetUserCounter = ruleObj.Count;

                foreach (var userData in ruleObj)
                {
                    var isValidEmail = Parser.TryParseAddress(userData.Email, out address);

                    if (!isValidEmail || String.IsNullOrEmpty(userData.FirstName) || String.IsNullOrEmpty(userData.LastName))
                    {
                        Data.Add(new UserResults
                        {
                            Email = userData.Email,
                            Result = Resource.ImportContactsIncorrectFields,
                            Class = !isValidEmail ? "error3" : "error1"
                        });
                        error++;
                        Percentage++;
                        continue;
                    }

                    var us = CoreContext.UserManager.GetUserByEmail(userData.Email);

                    if (us.ID != Constants.LostUser.ID)
                    {
                        Data.Add(new UserResults
                        {
                            Email = userData.Email,
                            Result = CustomNamingPeople.Substitute<Resource>("ImportContactsAlreadyExists"),
                            Class = "error2"
                        });
                        error++;
                        Percentage++;
                        continue;
                    }

                    if (!importUsersAsCollaborators && TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers)
                    {
                        importUsersAsCollaborators = true;
                    }

                    var userInfo = new UserInfo
                    {
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName
                    };

                    UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), false, true, importUsersAsCollaborators);

                    var messageAction = importUsersAsCollaborators ? MessageAction.GuestImported : MessageAction.UserImported;
                    MessageService.Send(httpHeaders, messageAction, MessageTarget.Create(userInfo.ID), string.Format("{0} ({1})", userInfo.DisplayUserName(false), userInfo.Email));

                    Data.Add(new UserResults { Email = userData.Email, Result = String.Empty });
                    Percentage++;
                }
            }
            catch (Exception ex)
            {
                Status = (int)Operation.Error;
                Error = ex.Message;
            }

            IsCompleted = true;
        }
    }
}