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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using ASC.Common.Threading.Progress;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Import;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Resources;
using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio.UserControls.Users
{
    [AjaxNamespace("ImportUsersController")]
    public partial class ImportUsers : System.Web.UI.UserControl
    {
        protected bool EnableInviteLink = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

        private static readonly ProgressQueue progressQueue = new ProgressQueue(1, TimeSpan.FromMinutes(5), true);

        private static Dictionary<string, string> GetHttpHeaders(HttpRequest httpRequest)
        {
            if (httpRequest == null) return null;

            var di = (from object k in httpRequest.Headers.Keys select k.ToString()).ToDictionary(key => key, key => httpRequest.Headers[key]);
            return di;
        }

        public enum Operation
        {
            Success = 1,
            Error = 0
        }

        public static string Location
        {
            get { return "~/UserControls/Users/ImportUsers/ImportUsers.ascx"; }
        }

        protected int PeopleLimit { get; set; }

        protected bool FreeTariff { get; set; }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var quota = TenantExtra.GetTenantQuota();

            PeopleLimit = Math.Min(quota.ActiveUsers - TenantStatisticsProvider.GetUsersCount(), 0);
            FreeTariff = (quota.Free || quota.NonProfit || quota.Trial) && !quota.Open;
            HelpLink = CommonLinkUtility.GetHelpLink();

            icon.Options.IsPopup = true;
            icon.Options.PopupContainerCssClass = "okcss popupContainerClass";
            icon.Options.OnCancelButtonClick = "ImportUsersManager.HideInfoWindow('okcss');";

            limitPanel.Options.IsPopup = true;
            limitPanel.Options.OnCancelButtonClick = "ImportUsersManager.HideImportUserLimitPanel();";

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            RegisterScript();
        }

        private void RegisterScript()
        {
            Page.RegisterStyle("~/usercontrols/users/importusers/css/import.less")
                .RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                "~/usercontrols/users/ImportUsers/js/ImportUsers.js");

            var script = new StringBuilder();

            script.AppendFormat("ImportUsersManager.FName = '{0}';", Resource.ImportContactsFirstName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.EmptyFName = '{0}';", Resource.ImportContactsEmptyFirstName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.LName = '{0}';", Resource.ImportContactsLastName.ReplaceSingleQuote().Replace("\n", ""));
            script.AppendFormat("ImportUsersManager.EmptyLName = '{0}';", Resource.ImportContactsEmptyLastName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.Email = '{0}';", Resource.ImportContactsEmail.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager._errorImport = '{0}';", String.Format(Resource.ImportContactsFromFileError.ReplaceSingleQuote(), "<br />"));
            script.AppendFormat("ImportUsersManager._errorEmail = '{0}';", Resource.ImportContactsIncorrectFields.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager._emptySocImport = '{0}';", String.Format(Resource.ImportContactsEmptyData.ReplaceSingleQuote().Replace("\n", ""), "<br />"));
            script.AppendFormat("ImportUsersManager._portalLicence.maxUsers = '{0}';", TenantExtra.GetTenantQuota().ActiveUsers);
            script.AppendFormat("ImportUsersManager._portalLicence.currectUsers = '{0}';", TenantStatisticsProvider.GetUsersCount());

            script.Append("jq(document).click(function(event) {");
            script.Append("jq.dropdownToggle({rightPos: true}).registerAutoHide(event, '.file', '.fileSelector');");
            script.Append("jq('#upload img').attr('src', StudioManager.GetImage('loader_16.gif'));");
            script.Append("});");

            Page.RegisterInlineScript(script.ToString());
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void SaveUsers(string userList, bool importUsersAsCollaborators)
        {
            lock (progressQueue.SynchRoot)
            {
                var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                if (task != null && task.IsCompleted)
                {
                    progressQueue.Remove(task);
                    task = null;
                }
                if (task == null)
                {
                    progressQueue.Add(new ImportUsersTask(userList, importUsersAsCollaborators, GetHttpHeaders(HttpContext.Current.Request))
                    {
                        Id = TenantProvider.CurrentTenantID,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Percentage = 0
                    });
                }
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.None)]
        public object GetStatus()
        {
            lock (progressQueue.SynchRoot)
            {
                var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                if (task == null) return null;

                return new
                {
                    Completed = task.IsCompleted,
                    Percents = (int)task.Percentage,
                    Status = (int)task.Status,
                    Error = (string)task.Error,
                    task.Data
                };
            }
        }

        internal class UserResults
        {
            public string Email { get; set; }
            public string Result { get; set; }
            public string Class { get; set; }
        }

        private class ImportUsersTask : IProgressItem
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

                    var percentStep = 100.0 / ruleObj.Count;
                    foreach (var userData in ruleObj)
                    {
                        var validateEmail = UserManagerWrapper.ValidateEmail(userData.Email);
                        if (!validateEmail || String.IsNullOrEmpty(userData.FirstName) || String.IsNullOrEmpty(userData.LastName))
                        {
                            Data.Add(new UserResults
                                {
                                    Email = userData.Email,
                                    Result = Resource.ImportContactsIncorrectFields,
                                    Class = !validateEmail ? "error3" : "error1"
                                });
                            error++;
                            Percentage += percentStep;
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
                            Percentage += percentStep;
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
                        MessageService.Send(httpHeaders, messageAction, MessageTarget.Create(userInfo.ID), userInfo.DisplayUserName(false));

                        Data.Add(new UserResults { Email = userData.Email, Result = String.Empty });
                        Percentage += percentStep;
                    }
                }
                catch(Exception ex)
                {
                    Status = (int)Operation.Error;
                    Error = ex.Message;
                }

                IsCompleted = true;
            }
        }
    }

    internal class ContactsUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser);

                if (context.Request.Files.Count != 0)
                {
                    var logo = context.Request.Files[0];
                    var ext = FileUtility.GetFileExtension(logo.FileName);

                    if (ext != ".csv")
                    {
                        result.Success = false;
                        result.Message = Resource.ErrorEmptyUploadFileSelected;
                        return result;
                    }

                    IUserImporter importer = context.Request["obj"] == "txt"
                                                 ? new TextFileUserImporter(logo.InputStream) { DefaultHeader = "Email;FirstName;LastName", }
                                                 : new OutlookCSVUserImporter(logo.InputStream);

                    var users = importer.GetDiscoveredUsers();

                    result.Success = true;
                    result.Message = JsonContacts(users);
                }
                else
                {
                    result.Success = false;
                    result.Message = Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        private static string JsonContacts(IEnumerable<ContactInfo> contacts)
        {
            var serializer = new DataContractJsonSerializer(contacts.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, contacts);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }
}