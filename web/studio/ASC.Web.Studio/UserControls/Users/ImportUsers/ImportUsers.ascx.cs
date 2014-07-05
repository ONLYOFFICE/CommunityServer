/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Import;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio.UserControls.Users
{
    internal class ContactsUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    var logo = context.Request.Files[0];
                    var ext = FileUtility.GetFileExtension(logo.FileName);

                    if (ext != ".csv")
                    {
                        result.Success = false;
                        result.Message = Resources.Resource.ErrorEmptyUploadFileSelected;
                        return result;
                    }

                    IUserImporter importer = context.Request["obj"] == "txt"
                                                 ? new TextFileUserImporter(logo.InputStream) {DefaultHeader = "Email;FirstName;LastName",}
                                                 : new OutlookCSVUserImporter(logo.InputStream);

                    var users = importer.GetDiscoveredUsers();

                    result.Success = true;
                    result.Message = JsonContacts(users);
                }
                else
                {
                    result.Success = false;
                    result.Message = Resources.Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch(Exception ex)
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

        #endregion
    }

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

        protected void Page_Load(object sender, EventArgs e)
        {
            var quota = TenantExtra.GetTenantQuota().ActiveUsers - TenantStatisticsProvider.GetUsersCount();
            PeopleLimit = quota > 0 ? quota : 0;

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
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/users/importusers/css/import.less"));

            Page.RegisterBodyScripts(ResolveUrl("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/js/third-party/zeroclipboard.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/ImportUsers/js/ImportUsers.js"));

            var script = new StringBuilder();

            script.AppendFormat("ImportUsersManager.FName = '{0}';", Resources.Resource.ImportContactsFirstName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.EmptyFName = '{0}';", Resources.Resource.ImportContactsEmptyFirstName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.LName = '{0}';", Resources.Resource.ImportContactsLastName.ReplaceSingleQuote().Replace("\n", ""));
            script.AppendFormat("ImportUsersManager.EmptyLName = '{0}';", Resources.Resource.ImportContactsEmptyLastName.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager.Email = '{0}';", Resources.Resource.ImportContactsEmail.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager._errorImport = '{0}';", String.Format(Resources.Resource.ImportContactsFromFileError.ReplaceSingleQuote(), "<br />"));
            script.AppendFormat("ImportUsersManager._errorEmail = '{0}';", Resources.Resource.ImportContactsIncorrectFields.ReplaceSingleQuote());
            script.AppendFormat("ImportUsersManager._emptySocImport = '{0}';", String.Format(Resources.Resource.ImportContactsEmptyData.ReplaceSingleQuote().Replace("\n", ""), "<br />"));
            script.AppendFormat("ImportUsersManager._portalLicence.maxUsers = '{0}';", TenantExtra.GetTenantQuota().ActiveUsers);
            script.AppendFormat("ImportUsersManager._portalLicence.currectUsers = '{0}';", TenantStatisticsProvider.GetUsersCount());

            script.Append("jq(document).click(function(event) {");
            script.Append("jq.dropdownToggle().registerAutoHide(event, '.file', '.fileSelector');");
            script.Append("jq('#upload img').attr('src', StudioManager.GetImage('loader_16.gif'));");
            script.Append("});");

            Page.RegisterInlineScript(script.ToString());

            var sb = new StringBuilder();

            sb.AppendFormat(@"ZeroClipboard.setMoviePath('{0}');",
                            CommonLinkUtility.ToAbsolute("~/js/flash/zeroclipboard/ZeroClipboard10.swf")
                );

            Page.RegisterInlineScript(sb.ToString(), true);
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
                    Error = Resources.Resource.ErrorAccessDenied;
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
                                    Result = Resources.Resource.ImportContactsIncorrectFields,
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
                                    Result = Resources.Resource.ImportContactsAlreadyExists,
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
                        MessageService.Send(httpHeaders, messageAction, userInfo.DisplayUserName(false));

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
}