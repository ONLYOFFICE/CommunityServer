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