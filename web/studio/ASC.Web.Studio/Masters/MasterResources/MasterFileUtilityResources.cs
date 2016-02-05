/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Client;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Files;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterFileUtilityResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Files.Utility.Resource"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(23)
                   {
                       RegisterObject("ExtsImagePreviewed", FileUtility.ExtsImagePreviewed),
                       RegisterObject("ExtsWebPreviewed", FileUtility.ExtsWebPreviewed),
                       RegisterObject("ExtsWebEdited", FileUtility.ExtsWebEdited),
                       RegisterObject("ExtsCoAuthoring", FileUtility.ExtsCoAuthoring),
                       RegisterObject("ExtsMustConvert", FileUtility.ExtsMustConvert),
                       RegisterObject("ExtsConvertible", FileUtility.ExtsConvertible),
                       RegisterObject("ExtsUploadable", FileUtility.ExtsUploadable),
                       RegisterObject("ExtsArchive", FileUtility.ExtsArchive),
                       RegisterObject("ExtsVideo", FileUtility.ExtsVideo),
                       RegisterObject("ExtsAudio", FileUtility.ExtsAudio),
                       RegisterObject("ExtsImage", FileUtility.ExtsImage),
                       RegisterObject("ExtsSpreadsheet", FileUtility.ExtsSpreadsheet),
                       RegisterObject("ExtsPresentation", FileUtility.ExtsPresentation),
                       RegisterObject("ExtsDocument", FileUtility.ExtsDocument),
                       RegisterObject("InternalFormats", FileUtility.InternalExtension),
                       RegisterObject("ParamVersion", FilesLinkUtility.Version),
                       RegisterObject("ParamOutType", FilesLinkUtility.OutType),
                       RegisterObject("FileViewUrlString", FilesLinkUtility.FileViewUrlString),
                       RegisterObject("FileDownloadUrlString", FilesLinkUtility.FileDownloadUrlString),
                       RegisterObject("FileWebViewerUrlString", FilesLinkUtility.FileWebViewerUrlString),
                       RegisterObject("FileWebViewerExternalUrlString", FilesLinkUtility.FileWebViewerExternalUrlString),
                       RegisterObject("FileWebEditorUrlString", FilesLinkUtility.FileWebEditorUrlString),
                       RegisterObject("FileWebEditorExternalUrlString", FilesLinkUtility.FileWebEditorExternalUrlString)
                   };
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey;
        }
    }
}