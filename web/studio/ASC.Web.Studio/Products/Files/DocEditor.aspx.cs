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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core.Client;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

using Resources;

using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using Global = ASC.Web.Files.Classes.Global;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files
{
    public partial class DocEditor : MainPage
    {
        private static readonly string ResetCacheKey = ConfigurationManagerExtension.AppSettings["web.client.cache.resetkey.ds"];

        protected override bool MayNotAuth
        {
            get { return !string.IsNullOrEmpty(Request[FilesLinkUtility.DocShareKey]); }
            set { }
        }

        protected string DocServiceApiUrl = FilesLinkUtility.DocServiceApiUrl;

        #region Member

        private Services.DocumentService.Configuration _configuration;
        private string _docKeyForTrack;
        private Guid _tabId = Guid.Empty;
        private bool _editByUrl;
        private string _linkToEdit;
        protected bool IsMobile;
        protected string Favicon = TenantLogoManager.GetFavicon(true, true);

        private List<string> _errorMessage;

        private string ErrorMessage
        {
            get { return string.Join("\\n", (_errorMessage ?? new List<string>()).Select(s => s.Replace("\n", "\\n").Replace("\r", "").Replace("\"", "\\\""))); }
            set { if (!string.IsNullOrEmpty(value)) (_errorMessage = (_errorMessage ?? new List<string>())).Add(value); }
        }

        #endregion

        #region RequestParams

        private string RequestFileId
        {
            get { return Request[FilesLinkUtility.FileId]; }
        }

        private string RequestShareLinkKey
        {
            get { return Request[FilesLinkUtility.DocShareKey] ?? string.Empty; }
        }

        private bool _valideShareLink;

        private string RequestFileUrl
        {
            get { return Request[FilesLinkUtility.FileUri]; }
        }

        private bool RequestView
        {
            get { return (Request[FilesLinkUtility.Action] ?? "").Equals("view", StringComparison.InvariantCultureIgnoreCase); }
        }

        private int RequestVersion
        {
            get { return string.IsNullOrEmpty(Request[FilesLinkUtility.Version]) ? -1 : Convert.ToInt32(Request[FilesLinkUtility.Version]); }
        }

        private bool RequestEmbedded
        {
            get
            {
                return
                    (Request[FilesLinkUtility.Action] ?? "").Equals("embedded", StringComparison.InvariantCultureIgnoreCase)
                    && !string.IsNullOrEmpty(RequestShareLinkKey);
            }
        }

        private bool RequestHistoryClose
        {
            get { return (Request["history"] ?? "").Equals("close", StringComparison.InvariantCultureIgnoreCase); }
        }

        private bool _thirdPartyApp;

        #endregion

        #region Event

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            _valideShareLink = !string.IsNullOrEmpty(FileShareLink.Parse(RequestShareLinkKey));
            CheckAuth();
        }

        private void CheckAuth()
        {
            if (SecurityContext.IsAuthenticated)
                return;
            if (_valideShareLink)
                return;

            var refererURL = Request.GetUrlRewriter().AbsoluteUri;
            Session["refererURL"] = refererURL;
            Response.Redirect("~/Auth.aspx");
        }

        protected override void OnLoad(EventArgs e)
        {
            IsMobile = MobileDetector.IsMobile;
            PageLoad();
            InitScript();

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            DocServiceApiUrl += (DocServiceApiUrl.Contains("?") ? "&" : "?") + "ver=" + HttpUtility.UrlEncode(ClientSettings.ResetCacheKey + ResetCacheKey);

            if (_configuration != null && !string.IsNullOrEmpty(_configuration.DocumentType))
            {
                Favicon = WebImageSupplier.GetAbsoluteWebPath("onlyoffice_logo/" + _configuration.DocumentType + ".ico");
            }
        }

        private void PageLoad()
        {
            var editPossible = !RequestEmbedded;
            var isExtenral = false;

            File file;
            var fileUri = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(RequestFileUrl))
                {
                    var app = ThirdPartySelector.GetAppByFileId(RequestFileId);
                    if (app == null)
                    {
                        file = DocumentServiceHelper.GetParams(RequestFileId, RequestVersion, RequestShareLinkKey, editPossible, !RequestView, true, out _configuration);
                        if (_valideShareLink)
                        {
                            _configuration.Document.SharedLinkKey += RequestShareLinkKey;

                            if (CoreContext.Configuration.Personal && !SecurityContext.IsAuthenticated)
                            {
                                var user = CoreContext.UserManager.GetUsers(file.CreateBy);
                                var culture = CultureInfo.GetCultureInfo(user.CultureName);
                                Thread.CurrentThread.CurrentCulture = culture;
                                Thread.CurrentThread.CurrentUICulture = culture;
                            }
                        }
                    }
                    else
                    {
                        isExtenral = true;

                        bool editable;
                        _thirdPartyApp = true;
                        file = app.GetFile(RequestFileId, out editable);
                        file = DocumentServiceHelper.GetParams(file, true, editPossible ? FileShare.ReadWrite : FileShare.Read, false, editable, editable, editable, true, out _configuration);

                        _configuration.Document.Url = app.GetFileStreamUrl(file);
                        _configuration.EditorConfig.Customization.GobackUrl = string.Empty;
                    }
                }
                else
                {
                    isExtenral = true;

                    fileUri = RequestFileUrl;
                    var fileTitle = Request[FilesLinkUtility.FileTitle];
                    if (string.IsNullOrEmpty(fileTitle))
                        fileTitle = Path.GetFileName(HttpUtility.UrlDecode(fileUri)) ?? "";

                    file = new File
                    {
                        ID = RequestFileUrl,
                        Title = Global.ReplaceInvalidCharsAndTruncate(fileTitle)
                    };

                    file = DocumentServiceHelper.GetParams(file, true, FileShare.Read, false, false, false, false, false, out _configuration);
                    _configuration.Document.Permissions.Edit = editPossible && !CoreContext.Configuration.Standalone;
                    _configuration.Document.Permissions.Rename = false;
                    _configuration.Document.Permissions.Review = false;
                    _configuration.Document.Permissions.FillForms = false;
                    _configuration.Document.Permissions.ChangeHistory = false;
                    _configuration.Document.Permissions.ModifyFilter = false;
                    _editByUrl = true;

                    _configuration.Document.Url = fileUri;
                }
                ErrorMessage = _configuration.ErrorMessage;
            }
            catch (Exception ex)
            {
                Global.Logger.Warn("DocEditor", ex);
                ErrorMessage = ex.Message;
                return;
            }

            if (_configuration.EditorConfig.ModeWrite && FileConverter.MustConvert(file))
            {
                try
                {
                    file = FileConverter.ExecSync(file, RequestShareLinkKey);
                }
                catch (Exception ex)
                {
                    _configuration = null;
                    Global.Logger.Error("DocEditor", ex);
                    ErrorMessage = ex.Message;
                    return;
                }

                var comment = "#message/" + HttpUtility.UrlEncode(string.Format(FilesCommonResource.ConvertForEdit, file.Title));

                Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(file.ID) + comment);
                return;
            }

            Title = file.Title + GetPageTitlePostfix();

            if (_configuration.EditorConfig.Customization.Goback == null || string.IsNullOrEmpty(_configuration.EditorConfig.Customization.Goback.Url))
            {
                _configuration.EditorConfig.Customization.GobackUrl = Request[FilesLinkUtility.FolderUrl] ?? "";
            }

            _configuration.EditorConfig.Customization.IsRetina = TenantLogoManager.IsRetina(Request);

            if (RequestEmbedded)
            {
                _configuration.Type = Services.DocumentService.Configuration.EditorType.Embedded;

                _configuration.EditorConfig.Embedded.ShareLinkParam = string.IsNullOrEmpty(RequestShareLinkKey) ? string.Empty : "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey;
            }
            else
            {
                _configuration.Type = IsMobile ? Services.DocumentService.Configuration.EditorType.Mobile : Services.DocumentService.Configuration.EditorType.Desktop;

                if (FileSharing.CanSetAccess(file)
                    && !(file.Encrypted
                         && (!Request.DesktopApp()
                             || CoreContext.Configuration.Personal)))
                {
                    _configuration.EditorConfig.SharingSettingsUrl = CommonLinkUtility.GetFullAbsolutePath(
                        Share.Location
                        + "?" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString())
                        + (Request.DesktopApp() ? "&desktop=true" : string.Empty));
                }

                if (file.RootFolderType == FolderType.Privacy)
                {
                    if (!PrivacyRoomSettings.Enabled)
                    {
                        _configuration = null;
                        ErrorMessage = FilesCommonResource.ErrorMassage_FileNotFound;
                        return;
                    }
                    else
                    {
                        if (Request.DesktopApp())
                        {
                            var keyPair = EncryptionKeyPair.GetKeyPair();
                            if (keyPair != null)
                            {
                                _configuration.EditorConfig.EncryptionKeys = new Services.DocumentService.Configuration.EditorConfiguration.EncryptionKeysConfig
                                {
                                    PrivateKeyEnc = keyPair.PrivateKeyEnc,
                                    PublicKey = keyPair.PublicKey,
                                };
                            }
                        }
                    }
                }
            }

            if (!isExtenral)
            {
                _docKeyForTrack = DocumentServiceHelper.GetDocKey(file.ID, -1, DateTime.MinValue);

                FileMarker.RemoveMarkAsNew(file);
                if (!file.Encrypted && !file.ProviderEntry) EntryManager.MarkAsRecent(file);
            }

            if (SecurityContext.IsAuthenticated)
            {
                _configuration.EditorConfig.SaveAsUrl = CommonLinkUtility.GetFullAbsolutePath(SaveAs.GetUrl);
            }

            if (_configuration.EditorConfig.ModeWrite)
            {
                _tabId = FileTracker.Add(file.ID);

                Global.SocketManager.FilesChangeEditors(file.ID);

                if (SecurityContext.IsAuthenticated)
                {
                    _configuration.EditorConfig.FileChoiceUrl = CommonLinkUtility.GetFullAbsolutePath(FileChoice.GetUrlForEditor);
                }
            }
            else
            {
                _linkToEdit = _editByUrl
                                  ? CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorExternalUrl(fileUri, file.Title))
                                  : CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID));
                if (Request.DesktopApp()) _linkToEdit += "&desktop=true";

                if (FileConverter.MustConvert(_configuration.Document.Info.File)) _editByUrl = true;
            }

            var actionAnchor = Request[FilesLinkUtility.Anchor];
            if (!string.IsNullOrEmpty(actionAnchor))
            {
                _configuration.EditorConfig.ActionLinkString = actionAnchor;
            }
        }

        private void InitScript()
        {
            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("\nASC.Files.Constants.URL_WCFSERVICE = \"{0}\";" +
                                      "ASC.Files.Constants.DocsAPIundefined = \"{1}\";",
                                      PathProvider.GetFileServicePath,
                                      FilesCommonResource.DocsAPIundefined);

            if (!CoreContext.Configuration.Personal)
            {
                inlineScript.AppendFormat("\nASC.Files.Constants.URL_MAIL_ACCOUNTS = \"{0}\";",
                                          CommonLinkUtility.GetFullAbsolutePath("~/addons/mail/#accounts"));
            }

            var docServiceParams = new DocumentServiceParams
            {
                DocKeyForTrack = _docKeyForTrack,
                EditByUrl = _editByUrl,
                LinkToEdit = _linkToEdit,
                OpenHistory = RequestVersion != -1 && RequestView && !RequestHistoryClose && _configuration.Document.Info.File.Forcesave == ForcesaveType.None && !_configuration.Document.Info.File.Encrypted,
                OpeninigDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                ShareLinkParam = string.IsNullOrEmpty(RequestShareLinkKey) ? string.Empty : "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey,
                ServerErrorMessage = ErrorMessage,
                TabId = _tabId.ToString(),
                ThirdPartyApp = _thirdPartyApp,
                CanGetUsers = SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal,
                PageTitlePostfix = GetPageTitlePostfix()
            };

            if (_configuration != null)
            {
                docServiceParams.FileId = _configuration.Document.Info.File.ID.ToString();
                docServiceParams.FileProviderKey = _configuration.Document.Info.File.ProviderKey;
                docServiceParams.FileVersion = _configuration.Document.Info.File.Version;

                _configuration.Token = DocumentServiceHelper.GetSignature(_configuration);

                if (!string.IsNullOrEmpty(_configuration.Token))
                {
                    _configuration.EditorConfig.CallbackUrl = DocumentServiceTracker.GetCallbackUrl(_configuration.Document.Info.File.ID.ToString());
                }
            }

            if (Request.DesktopApp() && SecurityContext.IsAuthenticated)
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                docServiceParams.DisplayName = DisplayUserSettings.GetFullUserName(user);
                docServiceParams.Email = user.Email;
            }

            inlineScript.AppendFormat("\nASC.Files.Editor.docServiceParams = {0};",
                                      DocumentServiceParams.Serialize(docServiceParams));

            inlineScript.AppendFormat("\nASC.Files.Editor.configurationParams = {0};",
                                      Services.DocumentService.Configuration.Serialize(_configuration));

            InlineScripts.Scripts.Add(new Tuple<string, bool>(inlineScript.ToString(), false));
        }

        protected string RenderCustomScript()
        {
            var sb = new StringBuilder();
            //custom scripts
            foreach (var script in SetupInfo.CustomScripts.Where(script => !String.IsNullOrEmpty(script)))
            {
                sb.AppendFormat("<script language=\"javascript\" src=\"{0}\" type=\"text/javascript\"></script>", script);
            }

            return sb.ToString();
        }

        private string GetPageTitlePostfix()
        {
            return Request.DesktopApp() ? string.Empty : string.Format(" - {0}", Resource.WebStudioName);
        }

        #endregion
    }
}