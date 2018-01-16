/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Web.Core.Client;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using Global = ASC.Web.Files.Classes.Global;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files
{
    public partial class DocEditor : MainPage
    {
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

        private bool RequestEmbedded
        {
            get
            {
                return
                    (Request[FilesLinkUtility.Action] ?? "").Equals("embedded", StringComparison.InvariantCultureIgnoreCase)
                    && !string.IsNullOrEmpty(RequestShareLinkKey);
            }
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

            var refererURL = Request.Url.AbsoluteUri;
            Session["refererURL"] = refererURL;
            Response.Redirect("~/auth.aspx");
        }

        protected override void OnLoad(EventArgs e)
        {
            IsMobile = MobileDetector.IsMobile;
            PageLoad();
            InitScript();

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            DocServiceApiUrl += (DocServiceApiUrl.Contains("?") ? "&" : "?") + "ver=" + ClientSettings.ResetCacheKey;

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
                        var ver = string.IsNullOrEmpty(Request[FilesLinkUtility.Version]) ? -1 : Convert.ToInt32(Request[FilesLinkUtility.Version]);
                        file = DocumentServiceHelper.GetParams(RequestFileId, ver, RequestShareLinkKey, editPossible, !RequestView, true, out _configuration);
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

                    if (CoreContext.Configuration.Standalone)
                    {
                        try
                        {
                            var webRequest = (HttpWebRequest)WebRequest.Create(RequestFileUrl);

                            // hack. http://ubuntuforums.org/showthread.php?t=1841740
                            if (WorkContext.IsMono)
                            {
                                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                            }

                            using (var response = webRequest.GetResponse())
                            using (var responseStream = new ResponseStream(response))
                            {
                                var externalFileKey = DocumentServiceConnector.GenerateRevisionId(RequestFileUrl);
                                fileUri = DocumentServiceConnector.GetExternalUri(responseStream, MimeMapping.GetMimeMapping(fileTitle), externalFileKey);
                            }
                        }
                        catch (Exception error)
                        {
                            Global.Logger.Error("Cannot receive external url for \"" + RequestFileUrl + "\"", error);
                        }
                    }

                    file = new File
                        {
                            ID = RequestFileUrl,
                            Title = Global.ReplaceInvalidCharsAndTruncate(fileTitle)
                        };

                    file = DocumentServiceHelper.GetParams(file, true, FileShare.Read, false, false, false, false, false, out _configuration);
                    _configuration.Document.Permissions.Edit = editPossible && !CoreContext.Configuration.Standalone;
                    _configuration.Document.Permissions.Rename = false;
                    _configuration.Document.Permissions.Review = false;
                    _configuration.Document.Permissions.ChangeHistory = false;
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
                    file = FileConverter.ExecDuplicate(file, RequestShareLinkKey);
                }
                catch (Exception ex)
                {
                    _configuration = null;
                    Global.Logger.Error("DocEditor", ex);
                    ErrorMessage = ex.Message;
                    return;
                }

                var comment = "#message/" + HttpUtility.UrlEncode(FilesCommonResource.CopyForEdit);

                Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(file.ID) + comment);
                return;
            }

            Title = file.Title;

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

                if (FileSharing.CanSetAccess(file))
                {
                    _configuration.EditorConfig.SharingSettingsUrl = CommonLinkUtility.GetFullAbsolutePath(Share.Location + "?" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(file.ID.ToString()));
                }
            }

            if (!isExtenral)
            {
                _docKeyForTrack = DocumentServiceHelper.GetDocKey(file.ID, -1, DateTime.MinValue);

                FileMarker.RemoveMarkAsNew(file);
            }

            if (_configuration.EditorConfig.ModeWrite)
            {
                _tabId = FileTracker.Add(file.ID);
                if (SecurityContext.IsAuthenticated)
                {
                    _configuration.EditorConfig.FileChoiceUrl = CommonLinkUtility.GetFullAbsolutePath(FileChoice.Location) + "?" + FileChoice.ParamFilterExt + "=xlsx&" + FileChoice.MailMergeParam + "=true";
                    _configuration.EditorConfig.MergeFolderUrl = CommonLinkUtility.GetFullAbsolutePath(MailMerge.GetUrl);
                }
            }
            else
            {
                _linkToEdit = _editByUrl
                                  ? CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorExternalUrl(fileUri, file.Title))
                                  : CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID));

                if (FileConverter.MustConvert(_configuration.Document.Info.File)) _editByUrl = true;
            }
        }

        private void InitScript()
        {
            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("\nASC.Files.Constants.URL_WCFSERVICE = \"{0}\";",
                                      PathProvider.GetFileServicePath);

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
                    OpeninigDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    ShareLinkParam = string.IsNullOrEmpty(RequestShareLinkKey) ? string.Empty : "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey,
                    ServerErrorMessage = ErrorMessage,
                    TabId = _tabId.ToString(),
                    ThirdPartyApp = _thirdPartyApp,
                };

            if (_configuration != null)
            {
                docServiceParams.FileId = _configuration.Document.Info.File.ID.ToString();
                docServiceParams.FileProviderKey = _configuration.Document.Info.File.ProviderKey;
                docServiceParams.FileVersion = _configuration.Document.Info.File.Version;

                _configuration.Token = DocumentServiceHelper.GetSignature(_configuration);
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

        #endregion
    }
}