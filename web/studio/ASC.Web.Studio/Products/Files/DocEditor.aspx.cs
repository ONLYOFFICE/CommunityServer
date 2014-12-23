/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using File = ASC.Files.Core.File;
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

        private DocumentServiceParams _docParams;
        private string _docKeyForTrack;
        private Guid _tabId = Guid.Empty;
        private bool _fileNew;
        private string _errorMessage;
        private bool _fixedVersion;
        private bool _editByUrl;
        private bool _newScheme;

        protected bool IsMobile;

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
                    Global.EnableEmbedded
                    && (Request[FilesLinkUtility.Action] ?? "").Equals("embedded", StringComparison.InvariantCultureIgnoreCase)
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

            if (!TenantExtra.GetTenantQuota().DocsEdition)
                Response.Redirect(FilesLinkUtility.FileHandlerPath + "?" + Context.Request.QueryString
                                  + (string.IsNullOrEmpty(Context.Request[FilesLinkUtility.Action]) ? "&" + FilesLinkUtility.Action + "=view" : string.Empty));

            if (CoreContext.Configuration.PartnerHosted && WebConfigurationManager.AppSettings["files.onlyauthorized"] != "false")
            {
                var hostedPartner = CoreContext.PaymentManager.GetApprovedPartner();
                if (hostedPartner == null || string.IsNullOrEmpty(hostedPartner.AuthorizedKey))
                {
                    Response.Redirect(FilesLinkUtility.FileHandlerPath + "?" + Context.Request.QueryString
                                      + (string.IsNullOrEmpty(Context.Request[FilesLinkUtility.Action]) ? "&" + FilesLinkUtility.Action + "=view" : string.Empty));
                }
            }
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
        }

        private void PageLoad()
        {
            var editPossible = !RequestEmbedded;
            var isExtenral = false;

            File file;
            var fileUri = string.Empty;
            IThirdPartyApp app = null;

            try
            {
                if (string.IsNullOrEmpty(RequestFileUrl))
                {
                    _fileNew = (Request["new"] ?? "") == "true";

                    app = ThirdPartySelector.GetAppByFileId(RequestFileId);
                    if (app == null)
                    {
                        var ver = string.IsNullOrEmpty(Request[FilesLinkUtility.Version]) ? -1 : Convert.ToInt32(Request[FilesLinkUtility.Version]);

                        file = DocumentServiceHelper.GetParams(RequestFileId, ver, RequestShareLinkKey, _fileNew, editPossible, !RequestView, out _docParams);

                        _fileNew = _fileNew && file.Version == 1 && file.ConvertedType != null && file.CreateOn == file.ModifiedOn;
                    }
                    else
                    {
                        isExtenral = true;

                        bool editable;
                        _thirdPartyApp = true;
                        file = app.GetFile(RequestFileId, out editable);
                        file = DocumentServiceHelper.GetParams(file, true, true, true, editable, editable, editable, out _docParams);

                        _docParams.FileUri = app.GetFileStreamUrl(file);
                        _docParams.FolderUrl = string.Empty;
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
                            var webRequest = WebRequest.Create(RequestFileUrl);
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

                    file = DocumentServiceHelper.GetParams(file, true, true, true, false, false, false, out _docParams);
                    _docParams.CanEdit = editPossible && !CoreContext.Configuration.Standalone;
                    _editByUrl = true;

                    _docParams.FileUri = fileUri;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return;
            }

            if (_docParams.ModeWrite && FileConverter.MustConvert(file))
            {
                try
                {
                    file = FileConverter.ExecDuplicate(file, RequestShareLinkKey);
                }
                catch (Exception e)
                {
                    _docParams = null;
                    _errorMessage = e.Message;
                    return;
                }

                var comment = "#message/" + HttpUtility.UrlEncode(FilesCommonResource.CopyForEdit);

                Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(file.ID) + comment);
                return;
            }

            Title = file.Title;

            _newScheme = FileUtility.ExtsNewService.Contains(FileUtility.GetFileExtension(file.Title))
                         && (string.IsNullOrEmpty(RequestShareLinkKey)
                                 ? OnlineEditorsSettings.NewScheme
                                 : OnlineEditorsSettings.NewSchemeFor(file.CreateBy))
                         && app == null;
            if (_newScheme)
            {
                DocServiceApiUrl = FilesLinkUtility.DocServiceApiUrlNew;
            }

            if (string.IsNullOrEmpty(_docParams.FolderUrl))
            {
                _docParams.FolderUrl = Request[FilesLinkUtility.FolderUrl] ?? "";
            }
            if (MobileDetector.IsRequestMatchesMobile(true))
            {
                _docParams.FolderUrl = string.Empty;
            }

            if (RequestEmbedded)
            {
                _docParams.Type = DocumentServiceParams.EditorType.Embedded;

                var shareLinkParam = "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey;
                _docParams.ViewerUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=view" + shareLinkParam);
                _docParams.DownloadUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath + "?" + FilesLinkUtility.Action + "=download" + shareLinkParam);
                _docParams.EmbeddedUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FilesBaseAbsolutePath + FilesLinkUtility.EditorPage + "?" + FilesLinkUtility.Action + "=embedded" + shareLinkParam);
            }
            else
            {
                _docParams.Type = IsMobile ? DocumentServiceParams.EditorType.Mobile : DocumentServiceParams.EditorType.Desktop;

                if (FileSharing.CanSetAccess(file))
                {
                    _docParams.SharingSettingsUrl = CommonLinkUtility.GetFullAbsolutePath(Share.Location + "?" + FilesLinkUtility.FileId + "=" + file.ID);
                }
            }

            if (!isExtenral)
            {
                _docKeyForTrack = DocumentServiceHelper.GetDocKey(file.ID, -1, DateTime.MinValue);

                FileMarker.RemoveMarkAsNew(file);
            }

            if (_docParams.ModeWrite)
            {
                _tabId = FileTracker.Add(file.ID, _fileNew);
                _fixedVersion = FileTracker.FixedVersion(file.ID);
            }
            else
            {
                _docParams.LinkToEdit = _editByUrl
                                            ? CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorExternalUrl(fileUri, file.Title))
                                            : FileConverter.MustConvert(_docParams.File) || _newScheme
                                                  ? CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID))
                                                  : string.Empty;
            }
        }

        private void InitScript()
        {
            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("\nASC.Files.Constants.URL_WCFSERVICE = \"{0}\";" +
                                      "ASC.Files.Constants.URL_FILES_START = \"{1}\";",
                                      PathProvider.GetFileServicePath,
                                      FilesLinkUtility.FilesBaseAbsolutePath);

            if (SecurityContext.IsAuthenticated && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                inlineScript.AppendFormat("\nASC.Files.Constants.URL_HANDLER_CREATE = \"{0}\";",
                                          CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath));
            }

            inlineScript.AppendFormat("\nASC.Files.Editor.docKeyForTrack = \"{0}\";" +
                                      "ASC.Files.Editor.shareLinkParam = \"{1}\";" +
                                      "ASC.Files.Editor.serverErrorMessage = \"{2}\";" +
                                      "ASC.Files.Editor.editByUrl = ({3} == true);" +
                                      "ASC.Files.Editor.fixedVersion = ({4} == true);" +
                                      "ASC.Files.Editor.tabId = \"{5}\";" +
                                      "ASC.Files.Editor.FileWebEditorExternalUrlString = \"{6}\";" +
                                      "ASC.Files.Editor.thirdPartyApp = ({7} == true);" +
                                      "ASC.Files.Editor.openinigDate = \"{8}\";" +
                                      "ASC.Files.Editor.newScheme = ({9} == true);",
                                      _docKeyForTrack,
                                      string.IsNullOrEmpty(RequestShareLinkKey) ? string.Empty : "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey,
                                      _errorMessage.HtmlEncode(),
                                      _editByUrl.ToString().ToLower(),
                                      _fixedVersion.ToString().ToLower(),
                                      _tabId,
                                      FilesLinkUtility.FileWebEditorExternalUrlString,
                                      _thirdPartyApp.ToString().ToLower(),
                                      DateTime.UtcNow,
                                      _newScheme.ToString().ToLower());

            inlineScript.Append(BuildOptions());

            inlineScript.AppendFormat("\nASC.Files.Editor.docServiceParams = {0};",
                                      DocumentServiceParams.Serialize(_docParams));

            InlineScripts.Scripts.Add(new Tuple<string, bool>(inlineScript.ToString(), false));
        }

        private string BuildOptions()
        {
            var options = new
                {
                    isEmpty = _fileNew,
                    asNew = _fileNew,
                };

            var opts = JsonConvert.SerializeObject(options);

            var optsRequest = Request["options"];
            if (!string.IsNullOrEmpty(optsRequest))
            {
                opts = opts.TrimEnd('}') + "," + optsRequest.TrimStart('{');
            }

            return string.Format("\nASC.Files.Editor.options = {0};", opts);
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