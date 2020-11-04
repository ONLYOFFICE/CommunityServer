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


using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Client;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Files;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterFileUtilityResources : ClientScript
    {
        protected override bool CheckAuth
        {
            get { return false; }
        }

        protected override string BaseNamespace
        {
            get { return "ASC.Files.Utility.Resource"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(new
                        {
                            FileUtility.ExtsImagePreviewed,
                            FileUtility.ExtsMediaPreviewed,
                            FileUtility.ExtsWebPreviewed,
                            FileUtility.ExtsWebEdited,
                            FileUtility.ExtsWebEncrypt,
                            FileUtility.ExtsWebReviewed,
                            FileUtility.ExtsWebCustomFilterEditing,
                            FileUtility.ExtsWebRestrictedEditing,
                            FileUtility.ExtsWebCommented,
                            FileUtility.ExtsWebTemplate,
                            FileUtility.ExtsCoAuthoring,
                            FileUtility.ExtsMustConvert,
                            FileUtility.ExtsConvertible,
                            FileUtility.ExtsUploadable,
                            FileUtility.ExtsArchive,
                            FileUtility.ExtsVideo,
                            FileUtility.ExtsAudio,
                            FileUtility.ExtsImage,
                            FileUtility.ExtsSpreadsheet,
                            FileUtility.ExtsPresentation,
                            FileUtility.ExtsDocument,
                            InternalFormats = FileUtility.InternalExtension,
                            ParamVersion = FilesLinkUtility.Version,
                            ParamOutType = FilesLinkUtility.OutType,
                            FilesLinkUtility.FileDownloadUrlString,
                            FilesLinkUtility.FileWebViewerUrlString,
                            FilesLinkUtility.FileWebViewerExternalUrlString,
                            FilesLinkUtility.FileWebEditorUrlString,
                            FilesLinkUtility.FileWebEditorExternalUrlString,
                            FilesLinkUtility.FileRedirectPreviewUrlString
                        })
                };
        }

        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey
                   + string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl);
        }
    }
}