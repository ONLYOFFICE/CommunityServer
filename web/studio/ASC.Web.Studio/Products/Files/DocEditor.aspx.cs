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


using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Settings;
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
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
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

            try
            {
                if (string.IsNullOrEmpty(RequestFileUrl))
                {
                    _fileNew = (Request["new"] ?? "") == "true";

                    var app = ThirdPartySelector.GetAppByFileId(RequestFileId);
                    if (app == null)
                    {
                        var ver = string.IsNullOrEmpty(Request[FilesLinkUtility.Version]) ? -1 : Convert.ToInt32(Request[FilesLinkUtility.Version]);

                        file = DocumentServiceHelper.GetParams(RequestFileId, ver, RequestShareLinkKey, _fileNew, editPossible, !RequestView, out _docParams);

                        _fileNew = _fileNew && file.Version == 1 && file.CreateOn == file.ModifiedOn;
                    }
                    else
                    {
                        isExtenral = true;

                        bool editable;
                        _thirdPartyApp = true;
                        file = app.GetFile(RequestFileId, out editable);
                        file = DocumentServiceHelper.GetParams(file, true, true, true, editable, editable, editable, editable, out _docParams);

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

                    file = DocumentServiceHelper.GetParams(file, true, true, true, false, false, false, false, out _docParams);
                    _docParams.CanEdit = editPossible && !CoreContext.Configuration.Standalone;
                    _docParams.CanReview = _docParams.CanEdit;
                    _editByUrl = true;

                    _docParams.FileUri = fileUri;
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocEditor", ex);
                _errorMessage = ex.Message;
                return;
            }

            if (_docParams.ModeWrite && FileConverter.MustConvert(file))
            {
                try
                {
                    file = FileConverter.ExecDuplicate(file, RequestShareLinkKey);
                }
                catch (Exception ex)
                {
                    _docParams = null;
                    Global.Logger.Error("DocEditor", ex);
                    _errorMessage = ex.Message;
                    return;
                }

                var comment = "#message/" + HttpUtility.UrlEncode(FilesCommonResource.CopyForEdit);

                Response.Redirect(FilesLinkUtility.GetFileWebEditorUrl(file.ID) + comment);
                return;
            }

            Title = file.Title;

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
                if (SecurityContext.IsAuthenticated)
                {
                    _docParams.FileChoiceUrl = CommonLinkUtility.GetFullAbsolutePath(FileChoice.Location) + "?" + FileChoice.ParamFilterExt + "=xlsx&" + FileChoice.MailMergeParam + "=true";
                    _docParams.MergeFolderUrl = CommonLinkUtility.GetFullAbsolutePath(MailMerge.GetUrl);
                }
            }
            else
            {
                if (!RequestView && FileTracker.IsEditingAlone(file.ID))
                {
                    var editingBy = FileTracker.GetEditingBy(file.ID).FirstOrDefault();
                    _errorMessage = string.Format(FilesCommonResource.ErrorMassage_EditingMobile, Global.GetUserName(editingBy));
                }

                _docParams.LinkToEdit = _editByUrl
                                            ? CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorExternalUrl(fileUri, file.Title))
                                            : CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebEditorUrl(file.ID));

                if (FileConverter.MustConvert(_docParams.File)) _editByUrl = true;
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

            if (SecurityContext.IsAuthenticated && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                inlineScript.AppendFormat("ASC.Files.Constants.URL_HANDLER_CREATE = \"{0}\";" +
                                          "ASC.Files.Constants.TitleNewFileText = \"{1}\";" +
                                          "ASC.Files.Constants.TitleNewFileSpreadsheet = \"{2}\";" +
                                          "ASC.Files.Constants.TitleNewFilePresentation = \"{3}\";",
                                          CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.FileHandlerPath),
                                          FilesJSResource.TitleNewFileText,
                                          FilesJSResource.TitleNewFileSpreadsheet,
                                          FilesJSResource.TitleNewFilePresentation);
            }

            var isRetina = TenantLogoManager.IsRetina(Request);
            inlineScript.AppendFormat("\nASC.Files.Editor.brandingLogoUrl = \"{0}\";" +
                                      "ASC.Files.Editor.brandingLogoEmbeddedUrl = \"{1}\";" +
                                      "ASC.Files.Editor.brandingCustomerLogo = \"{2}\";" +
                                      "ASC.Files.Editor.brandingCustomer = \"{3}\";" +
                                      "ASC.Files.Editor.brandingSite = \"{4}\";",
                                      CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.DocsEditor, !isRetina)),
                                      CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.Dark, !isRetina)),
                                      CommonLinkUtility.GetFullAbsolutePath(TenantLogoHelper.GetLogo(WhiteLabelLogoTypeEnum.Dark, !isRetina)),
                                      (SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID).LogoText ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("/", "\\/"),
                                      CompanyWhiteLabelSettings.Instance.Site);

            inlineScript.AppendFormat("\nASC.Files.Editor.docKeyForTrack = \"{0}\";" +
                                      "ASC.Files.Editor.shareLinkParam = \"{1}\";" +
                                      "ASC.Files.Editor.serverErrorMessage = \"{2}\";" +
                                      "ASC.Files.Editor.editByUrl = ({3} == true);" +
                                      "ASC.Files.Editor.fixedVersion = ({4} == true);" +
                                      "ASC.Files.Editor.tabId = \"{5}\";" +
                                      "ASC.Files.Editor.thirdPartyApp = ({6} == true);" +
                                      "ASC.Files.Editor.openinigDate = \"{7}\";",
                                      _docKeyForTrack,
                                      string.IsNullOrEmpty(RequestShareLinkKey) ? string.Empty : "&" + FilesLinkUtility.DocShareKey + "=" + RequestShareLinkKey,
                                      (_errorMessage ?? "").Replace("\n", "\\n").Replace("\r", "").Replace("\"", "\\\""),
                                      _editByUrl.ToString().ToLower(),
                                      _fixedVersion.ToString().ToLower(),
                                      _tabId,
                                      _thirdPartyApp.ToString().ToLower(),
                                      DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

            if (!CoreContext.Configuration.Standalone)
            {
                inlineScript.AppendFormat("\nASC.Files.Editor.showAbout = true;" +
                                          "ASC.Files.Editor.feedbackUrl = \"{0}\";",
                                          AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportEnabled
                                              ? CommonLinkUtility.GetRegionalUrl(
                                                  AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportUrl,
                                                  CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                                              : string.Empty);
            }
            else if (_docParams != null)
            {
                inlineScript.AppendFormat("\nASC.Files.Editor.licenseUrl = \"{0}\";" +
                                          "ASC.Files.Editor.customerId = \"{1}\";",
                                          PathProvider.GetLicenseUrl(_docParams.File),
                                          LicenseReader.CustomerId);
            }

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