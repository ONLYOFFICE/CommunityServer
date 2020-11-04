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


using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.HttpHandlers
{
    public class ChunkedUploaderHandler : AbstractHttpAsyncHandler
    {
        public override void OnProcessRequest(HttpContext context)
        {
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.Response.End();
                return;
            }

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
                    return;
                }

                switch (request.Type)
                {
                    case ChunkedRequestType.Abort:
                        FileUploader.AbortUpload(request.UploadId);
                        WriteSuccess(context, null);
                        return;

                    case ChunkedRequestType.Initiate:
                        var createdSession = FileUploader.InitiateUpload(request.FolderId, request.FileId, request.FileName, request.FileSize, request.Encrypted);
                        WriteSuccess(context, ToResponseObject(createdSession, true));
                        return;

                    case ChunkedRequestType.Upload:
                        var resumedSession = FileUploader.UploadChunk(request.UploadId, request.ChunkStream, request.ChunkSize);
                        
                        if (resumedSession.BytesUploaded == resumedSession.BytesTotal)
                        {
                            WriteSuccess(context, ToResponseObject(resumedSession.File), (int) HttpStatusCode.Created);
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
            catch (FileNotFoundException error)
            {
                Global.Logger.Error(error);
                WriteError(context, FilesCommonResource.ErrorMassage_FileNotFound);
            }
            catch (Exception error)
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
                if (request.CultureInfo != null)
                    Thread.CurrentThread.CurrentUICulture = request.CultureInfo;
                return true;
            }

            if (!string.IsNullOrEmpty(request.UploadId))
            {
                var uploadSession = ChunkedUploadSessionHolder.GetSession(request.UploadId);
                if (uploadSession != null)
                {
                    CoreContext.TenantManager.SetCurrentTenant(uploadSession.TenantId);
                    SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(uploadSession.UserId));
                    var culture = SetupInfo.EnabledCulturesPersonal.Find(c => String.Equals(c.Name, uploadSession.CultureName, StringComparison.InvariantCultureIgnoreCase));
                    if (culture != null)
                        Thread.CurrentThread.CurrentUICulture = culture;
                    return true;
                }
            }

            return false;
        }

        private static void WriteError(HttpContext context, string message)
        {
            WriteResponse(context, false, null, message, (int) HttpStatusCode.OK);
        }

        private static void WriteSuccess(HttpContext context, object data, int statusCode = (int) HttpStatusCode.OK)
        {
            WriteResponse(context, true, data, string.Empty, statusCode);
        }

        private static void WriteResponse(HttpContext context, bool success, object data, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Write(JsonConvert.SerializeObject(new {success, data, message}));
            context.Response.ContentType = "application/json";
        }

        public static object ToResponseObject(ChunkedUploadSession session, bool appendBreadCrumbs = false)
        {
            var pathFolder = appendBreadCrumbs
                                 ? EntryManager.GetBreadCrumbs(session.FolderId).Select(f =>
                                     {
                                         //todo: check how?
                                         if (f == null)
                                         {
                                             Global.Logger.ErrorFormat("GetBreadCrumbs {0} with null", session.FolderId);
                                             return string.Empty;
                                         }
                                         return f.ID;
                                     })
                                 : new List<object> {session.FolderId};

            return new
                {
                    id = session.Id,
                    path = pathFolder,
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
                    folderId = file.FolderID,
                    version = file.Version,
                    title = file.Title,
                    provider_key = file.ProviderKey,
                    uploaded = true
                };
        }

        private enum ChunkedRequestType
        {
            None,
            Initiate,
            Abort,
            Upload
        }

        [DebuggerDisplay("{Type} ({UploadId})")]
        private class ChunkedRequestHelper
        {
            private readonly HttpRequest _request;
            private HttpPostedFileBase _file;
            private int? _tenantId;
            private long? _fileContentLength;
            private Guid? _authKey;
            private CultureInfo _cultureInfo;

            public ChunkedRequestType Type
            {
                get
                {
                    if (_request["initiate"] == "true" && IsAuthDataSet() && IsFileDataSet())
                        return ChunkedRequestType.Initiate;

                    if (_request["abort"] == "true" && !string.IsNullOrEmpty(UploadId))
                        return ChunkedRequestType.Abort;

                    return !string.IsNullOrEmpty(UploadId)
                               ? ChunkedRequestType.Upload
                               : ChunkedRequestType.None;
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
                get { return _request[FilesLinkUtility.FolderId]; }
            }

            public string FileId
            {
                get { return _request[FilesLinkUtility.FileId]; }
            }

            public string FileName
            {
                get { return _request[FilesLinkUtility.FileTitle]; }
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

            public CultureInfo CultureInfo
            {
                get
                {
                    if (_cultureInfo != null)
                        return _cultureInfo;

                    var culture = _request["culture"];
                    if (string.IsNullOrEmpty(culture)) culture = "en-US";

                    return _cultureInfo = SetupInfo.EnabledCulturesPersonal.Find(c => String.Equals(c.Name, culture, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            public bool Encrypted
            {
                get { return _request["encrypted"] == "true"; }
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