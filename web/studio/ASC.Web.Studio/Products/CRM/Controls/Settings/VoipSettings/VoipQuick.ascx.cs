/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.IO;
using System.Web;
using ASC.CRM.Core;
using ASC.VoipService;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using Global = ASC.Web.CRM.Classes.Global;
using StudioResources = Resources;

namespace ASC.Web.CRM.Controls.Settings
{
    public class VoipUploadHandler : IFileUploadHandler
    {
        private const long maxFileSize = 5;

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            if (!VoipNumberData.Allowed || !CRMSecurity.IsAdmin)
                throw CRMSecurity.CreateSecurityException();

            if (context.Request.Files.Count == 0)
            {
                return Error("No files.");
            }

            var file = context.Request.Files[0];

            if (file.ContentLength <= 0 || file.ContentLength > maxFileSize * 1024L * 1024L)
            {
                return Error(StudioResources.Resource.FileSizeMaxExceed);
            }

            try
            {
                var path = file.FileName;

                AudioType audioType;
                if (Enum.TryParse(context.Request["audioType"], true, out audioType))
                {
                    path = Path.Combine(audioType.ToString().ToLower(), path);
                }

                var uri = Global.GetStore().Save("voip", path, file.InputStream, MimeMapping.GetMimeMapping(file.FileName), ContentDispositionUtil.GetHeaderValue(file.FileName, withoutBase: true));
                return Success(new VoipUpload
                    {
                        AudioType = audioType,
                        Name = file.FileName,
                        Path = CommonLinkUtility.GetFullAbsolutePath(uri.ToString())
                    });
            }
            catch(Exception error)
            {
                return Error(error.Message);
            }
        }

        private static FileUploadResult Success(VoipUpload file)
        {
            return new FileUploadResult
                {
                    Success = true,
                    Data = file
                };
        }

        private static FileUploadResult Error(string messageFormat, params object[] args)
        {
            return new FileUploadResult
                {
                    Success = false,
                    Message = string.Format(messageFormat, args)
                };
        }
    }

    public partial class VoipQuick : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/VoIPSettings/VoipQuick.ascx"); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!VoipNumberData.Allowed || !CRMSecurity.IsAdmin)
            {
                Response.Redirect(PathProvider.StartURL() + "settings.aspx?type=voip.common");
            }

            buyNumberContainer.Options.IsPopup = true;
            linkNumberContainer.Options.IsPopup = true;
            deleteNumberContainer.Options.IsPopup = true;
            Page.RegisterBodyScripts("~/js/asc/core/voip.countries.js");
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("voip.quick.js"));
        }
    }
}