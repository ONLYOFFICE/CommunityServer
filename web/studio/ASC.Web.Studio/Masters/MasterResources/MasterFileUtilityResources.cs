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
            yield return RegisterObject("ExtsImagePreviewed", FileUtility.ExtsImagePreviewed);
            yield return RegisterObject("ExtsWebPreviewed", FileUtility.ExtsWebPreviewed);
            yield return RegisterObject("ExtsWebEdited", FileUtility.ExtsWebEdited);
            yield return RegisterObject("ExtsCoAuthoring", FileUtility.ExtsCoAuthoring);

            yield return RegisterObject("ExtsMustConvert", FileUtility.ExtsMustConvert);
            yield return RegisterObject("ExtsConvertible", FileUtility.ExtsConvertible);
            yield return RegisterObject("ExtsUploadable", FileUtility.ExtsUploadable);

            yield return RegisterObject("ExtsArchive", FileUtility.ExtsArchive);
            yield return RegisterObject("ExtsVideo", FileUtility.ExtsVideo);
            yield return RegisterObject("ExtsAudio", FileUtility.ExtsAudio);
            yield return RegisterObject("ExtsImage", FileUtility.ExtsImage);
            yield return RegisterObject("ExtsSpreadsheet", FileUtility.ExtsSpreadsheet);
            yield return RegisterObject("ExtsPresentation", FileUtility.ExtsPresentation);
            yield return RegisterObject("ExtsDocument", FileUtility.ExtsDocument);

            yield return RegisterObject("InternalFormats", FileUtility.InternalExtension);

            yield return RegisterObject("ParamVersion", FilesLinkUtility.Version);
            yield return RegisterObject("ParamOutType", FilesLinkUtility.OutType);

            yield return RegisterObject("FileViewUrlString", FilesLinkUtility.FileViewUrlString);
            yield return RegisterObject("FileDownloadUrlString", FilesLinkUtility.FileDownloadUrlString);
            yield return RegisterObject("FileWebViewerUrlString", FilesLinkUtility.FileWebViewerUrlString);
            yield return RegisterObject("FileWebViewerExternalUrlString", FilesLinkUtility.FileWebViewerExternalUrlString);
            yield return RegisterObject("FileWebEditorUrlString", FilesLinkUtility.FileWebEditorUrlString);
            yield return RegisterObject("FileWebEditorExternalUrlString", FilesLinkUtility.FileWebEditorExternalUrlString);
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey;
        }
    }
}