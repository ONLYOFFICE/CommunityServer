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
using System.Linq;
using System.Net;
using System.Web;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using Global = ASC.Web.Files.Classes.Global;

namespace ASC.Web.Files
{
    public partial class ShareLink : MainPage, IStaticBundle
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "sharelink.aspx"; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (string.IsNullOrEmpty(Request[FilesLinkUtility.FileId]))
            {
                Response.Redirect(PathProvider.StartURL
                                  + "#error/" +
                                  HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;
            Master.Master
                  .AddStaticStyles(GetStaticStyleSheet())
                  .AddStaticBodyScripts(GetStaticJavaScript());

            Page.Title = HeaderStringHelper.GetPageTitle(FilesCommonResource.ShareLinkMail);

            InitScript();
        }

        private void InitScript()
        {
            var fileId = Request[FilesLinkUtility.FileId];
            File file;
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    file = fileDao.GetFile(fileId);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("ShareLink", ex);

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (file == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            if (!FileSharing.CanSetAccess(file))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var shareRecord = Global.GetFilesSecurity().GetShares(file).FirstOrDefault(r => r.Subject == FileConstant.ShareLinkId);
            if (shareRecord == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            Link = FileShareLink.GetLink(file);

            if (BitlyLoginProvider.Enabled)
            {
                try
                {
                    Link = BitlyLoginProvider.GetShortenLink(Link);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Get shorten link", ex);
                }
            }
        }


        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("sharelink", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "common.js",
                                  "servicemanager.js",
                                  "sharelink.js",
                                  "templatemanager.js",
                                  "ui.js"
                       )
                       .AddSource(ResolveUrl,
                                  "~/js/third-party/autosize.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("sharelink", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "common.css",
                                  "sharelink.css");
        }


        protected string GetAbsoluteCompanyTopLogoPath()
        {
            var general = !TenantLogoManager.IsRetina(Request);
            return
                CoreContext.Configuration.Personal
                    ? WebImageSupplier.GetAbsoluteWebPath("personal_logo/logo_personal_auth.png")
                    : TenantLogoManager.WhiteLabelEnabled
                          ? TenantLogoManager.GetLogoDark(general)
                          : TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, general);
        }

        public string Link { get; set; }
    }
}