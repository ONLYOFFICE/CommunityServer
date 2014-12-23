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
using System.Web;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Utils;
using Newtonsoft.Json;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.HttpHandlers
{
    public class ChunkedUploaderHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            try
            {
                var request = new ChunkedRequestHelper(context.Request);

                if (!TryAuthorize(request))
                {
                    WriteError(context, "Can't authorize given initiate session request or session with specified upload id already expired");
                    return;
                }

                if (CoreContext.TenantManager.GetCurrentTenant().Status != TenantStatus.Active)
                {
                    WriteError(context, "Can't perform upload for deleted or transfering portals");
                }

                switch (request.Type)
                {
                    case ChunkedRequestType.Abort:
                        FileUploader.AbortUpload(request.UploadId);
                        WriteSuccess(context, null);
                        return;

                    case ChunkedRequestType.Initiate:
                        ChunkedUploadSession createdSession = FileUploader.InitiateUpload(request.FolderId, request.FileId, request.FileName, request.FileSize);
                        WriteSuccess(context, ToResponseObject(createdSession));
                        return;

                    case ChunkedRequestType.Upload:
                        ChunkedUploadSession resumedSession = FileUploader.UploadChunk(request.UploadId, request.ChunkStream, request.ChunkSize);

                        if (resumedSession.BytesUploaded == resumedSession.BytesTotal)
                        {
                            WriteSuccess(context, ToResponseObject(resumedSession.File), statusCode: 201);
                            FilesMessageService.Send(resumedSession.File, context.Request, MessageAction.FileUploaded, resumedSession.File.Title);
                        }
                        else
                        {
                            WriteSuccess(context, ToResponseObject(resumedSession));
                        }

                        return;

                    default:
                        WriteError(context, "Unknown request type.");
                        return;
                }
            }
            catch(Exception error)
            {
                Global.Logger.Error(error);
                WriteError(context, error.Message);
            }
        }

        private static bool TryAuthorize(ChunkedRequestHelper request)
        {
            if (request.Type == ChunkedRequestType.Initiate)
            {
                CoreContext.TenantManager.SetCurrentTenant(request.TenantId);
                SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(request.AuthKey));
                return true;
            }

            if (!string.IsNullOrEmpty(request.UploadId))
            {
                var uploadSession = ChunkedUploadSessionHolder.GetSession(request.UploadId);
                if (uploadSession != null)
                {
                    CoreContext.TenantManager.SetCurrentTenant(uploadSession.TenantId);
                    SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(uploadSession.UserId));
                    return true;
                }
            }

            return false;
        }

        private static void WriteError(HttpContext context, string message, int statusCode = 200)
        {
            WriteResponse(context, statusCode, false, message.HtmlEncode(), "");
        }

        private static void WriteSuccess(HttpContext context, object data, int statusCode = 200)
        {
            WriteResponse(context, statusCode, true, string.Empty, data);
        }

        private static void WriteResponse(HttpContext context, int statusCode, bool success, string message, object data)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Write(JsonConvert.SerializeObject(new {success, data, message}));
        }

        private static object ToResponseObject(ChunkedUploadSession session)
        {
            return new
                {
                    id = session.Id,
                    created = session.Created,
                    expired = session.Expired,
                    location = session.Location,
                    bytes_uploaded = session.BytesUploaded,
                    bytes_total = session.BytesTotal
                };
        }

        private static object ToResponseObject(File file)
        {
            return new
                {
                    id = file.ID,
                    version = file.Version,
                    title = file.Title,
                    provider_key = file.ProviderKey
                };
        }

        private enum ChunkedRequestType
        {
            None,
            Initiate,
            Abort,
            Upload
        }

        private class ChunkedRequestHelper
        {
            private readonly HttpRequest _request;
            private HttpPostedFileBase _file;
            private int? _tenantId;
            private long? _fileContentLength;
            private Guid? _authKey;

            public ChunkedRequestType Type
            {
                get
                {
                    if (_request["initiate"] == "true" && IsAuthDataSet() && IsFileDataSet())
                        return ChunkedRequestType.Initiate;

                    if (_request["abort"] == "true" && !string.IsNullOrEmpty(UploadId))
                        return ChunkedRequestType.Abort;

                    if (!string.IsNullOrEmpty(UploadId))
                        return ChunkedRequestType.Upload;

                    return ChunkedRequestType.None;
                }
            }

            public string UploadId
            {
                get { return _request["uid"]; }
            }

            public int TenantId
            {
                get
                {
                    if (!_tenantId.HasValue)
                    {
                        int v;
                        if (int.TryParse(_request["tid"], out v))
                            _tenantId = v;
                        else
                            _tenantId = -1;
                    }
                    return _tenantId.Value;
                }
            }

            public Guid AuthKey
            {
                get
                {
                    if (!_authKey.HasValue)
                    {
                        _authKey = !string.IsNullOrEmpty(_request["userid"])
                                       ? new Guid(InstanceCrypto.Decrypt(_request["userid"]))
                                       : Guid.Empty;
                    }
                    return _authKey.Value;
                }
            }

            public string FolderId
            {
                get { return _request["folderid"]; }
            }

            public string FileId
            {
                get { return _request["fileid"]; }
            }

            public string FileName
            {
                get { return _request["name"]; }
            }

            public long FileSize
            {
                get
                {
                    if (!_fileContentLength.HasValue)
                    {
                        long v;
                        long.TryParse(_request["fileSize"], out v);
                        _fileContentLength = v;
                    }
                    return _fileContentLength.Value;
                }
            }

            public long ChunkSize
            {
                get { return File.ContentLength; }
            }

            public Stream ChunkStream
            {
                get { return File.InputStream; }
            }

            private HttpPostedFileBase File
            {
                get
                {
                    if (_file != null)
                        return _file;

                    if (_request.Files.Count > 0)
                        return _file = new HttpPostedFileWrapper(_request.Files[0]);

                    throw new Exception("HttpRequest.Files is empty");
                }
            }

            public ChunkedRequestHelper(HttpRequest request)
            {
                if (request == null) throw new ArgumentNullException("request");
                _request = request;
            }

            private bool IsAuthDataSet()
            {
                return TenantId > -1 && AuthKey != Guid.Empty;
            }

            private bool IsFileDataSet()
            {
                return !string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(FolderId);
            }
        }
    }
}