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


using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Services;
using ASC.Common.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.FFmpegService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using JWT;
using Newtonsoft.Json.Linq;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using MimeMapping = ASC.Common.Web.MimeMapping;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class FileHandler : AbstractHttpAsyncHandler
    {
        public static string FileHandlerPath
        {
            get { return FilesLinkUtility.FileHandlerPath; }
        }

        public override void OnProcessRequest(HttpContext context)
        {
            if (TenantStatisticsProvider.IsNotPaid())
            {
                context.Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                context.Response.StatusDescription = "Payment Required.";
                return;
            }

            try
            {
                switch ((context.Request[FilesLinkUtility.Action] ?? "").ToLower())
                {
                    case "view":
                    case "download":
                        DownloadFile(context);
                        break;
                    case "bulk":
                        BulkDownloadFile(context);
                        break;
                    case "stream":
                        StreamFile(context);
                        break;
                    case "empty":
                        EmptyFile(context);
                        break;
                    case "tmp":
                        TempFile(context);
                        break;
                    case "create":
                        CreateFile(context);
                        break;
                    case "redirect":
                        Redirect(context);
                        break;
                    case "diff":
                        DifferenceFile(context);
                        break;
                    case "track":
                        TrackFile(context);
                        break;
                    default:
                        throw new HttpException((int)HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest);
                }

            }
            catch (InvalidOperationException e)
            {
                throw new HttpException((int)HttpStatusCode.InternalServerError, FilesCommonResource.ErrorMassage_BadRequest, e);
            }
        }

        private static void BulkDownloadFile(HttpContext context)
        {
            if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var store = Global.GetStore();
            var path = string.Format(@"{0}\{1}.zip", SecurityContext.CurrentAccount.ID, FileConstant.DownloadTitle);
            if (!store.IsFile(FileConstant.StorageDomainTmp, path))
            {
                Global.Logger.ErrorFormat("BulkDownload file error. File is not exist on storage. UserId: {0}.", SecurityContext.CurrentAccount.ID);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            if (store.IsSupportedPreSignedUri)
            {
                var url = store.GetPreSignedUri(FileConstant.StorageDomainTmp, path, TimeSpan.FromHours(1), null).ToString();
                context.Response.Redirect(url);
                return;
            }

            context.Response.Clear();

            try
            {
                bool flushed = false;
                using (var readStream = store.GetReadStream(FileConstant.StorageDomainTmp, path))
                {
                    long offset = 0;
                    long length = readStream.Length;
                    if (readStream.CanSeek)
                    {
                        length = ProcessRangeHeader(context, readStream.Length, ref offset);
                        readStream.Seek(offset, SeekOrigin.Begin);
                    }

                    SendStreamByChunks(context, length, FileConstant.DownloadTitle + ".zip", readStream, ref flushed);
                }

                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception e)
            {
                Global.Logger.ErrorFormat("BulkDownloadFile failed for user {0} with error: ", SecurityContext.CurrentAccount.ID, e.Message);
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }
        }

        private static void DownloadFile(HttpContext context)
        {
            var flushed = false;
            try
            {
                var id = context.Request[FilesLinkUtility.FileId];
                var doc = context.Request[FilesLinkUtility.DocShareKey] ?? "";

                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    File file;
                    var readLink = FileShareLink.Check(doc, true, fileDao, out file);
                    if (!readLink && file == null)
                    {
                        fileDao.InvalidateCache(id);

                        int version;
                        file = int.TryParse(context.Request[FilesLinkUtility.Version], out version) && version > 0
                                   ? fileDao.GetFile(id, version)
                                   : fileDao.GetFile(id);
                    }

                    if (file == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                        return;
                    }

                    if (!readLink && !Global.GetFilesSecurity().CanRead(file))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }

                    if (!string.IsNullOrEmpty(file.Error)) throw new Exception(file.Error);

                    if (!fileDao.IsExistOnStorage(file))
                    {
                        Global.Logger.ErrorFormat("Download file error. File is not exist on storage. File id: {0}.", file.ID);
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                        return;
                    }

                    FileMarker.RemoveMarkAsNew(file);

                    context.Response.Clear();
                    context.Response.ClearHeaders();
                    context.Response.Charset = "utf-8";

                    FilesMessageService.Send(file, context.Request, MessageAction.FileDownloaded, file.Title);

                    if (string.Equals(context.Request.Headers["If-None-Match"], GetEtag(file)))
                    {
                        //Its cached. Reply 304
                        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                        context.Response.Cache.SetETag(GetEtag(file));
                    }
                    else
                    {
                        context.Response.CacheControl = "public";
                        context.Response.Cache.SetETag(GetEtag(file));
                        context.Response.Cache.SetCacheability(HttpCacheability.Public);

                        Stream fileStream = null;
                        try
                        {
                            var title = file.Title;

                            if (file.ContentLength <= SetupInfo.AvailableFileSize)
                            {
                                var ext = FileUtility.GetFileExtension(file.Title);

                                var outType = (context.Request[FilesLinkUtility.OutType] ?? "").Trim();
                                if (!string.IsNullOrEmpty(outType)
                                    && FileUtility.ExtsConvertible.Keys.Contains(ext)
                                    && FileUtility.ExtsConvertible[ext].Contains(outType))
                                {
                                    ext = outType;
                                }

                                long offset = 0;
                                long length;
                                if (!file.ProviderEntry
                                    && string.Equals(context.Request["convpreview"], "true", StringComparison.InvariantCultureIgnoreCase)
                                    && FFmpegService.IsConvertable(ext))
                                {
                                    const string mp4Name = "content.mp4";
                                    var mp4Path = FileDao.GetUniqFilePath(file, mp4Name);
                                    var store = Global.GetStore();
                                    if (!store.IsFile(mp4Path))
                                    {
                                        fileStream = fileDao.GetFileStream(file);

                                        Global.Logger.InfoFormat("Converting {0} (fileId: {1}) to mp4", file.Title, file.ID);
                                        var stream = FFmpegService.Convert(fileStream, ext);
                                        store.Save(string.Empty, mp4Path, stream, mp4Name);
                                    }

                                    var fullLength = store.GetFileSize(string.Empty, mp4Path);

                                    length = ProcessRangeHeader(context, fullLength, ref offset);
                                    fileStream = store.GetReadStream(string.Empty, mp4Path, (int)offset);

                                    title = FileUtility.ReplaceFileExtension(title, ".mp4");
                                }
                                else
                                {
                                    if (!FileConverter.EnableConvert(file, ext))
                                    {
                                        if (!readLink && fileDao.IsSupportedPreSignedUri(file))
                                        {
                                            context.Response.Redirect(fileDao.GetPreSignedUri(file, TimeSpan.FromHours(1)).ToString(), true);

                                            return;
                                        }

                                        fileStream = fileDao.GetFileStream(file); // getStream to fix file.ContentLength

                                        if (fileStream.CanSeek)
                                        {
                                            var fullLength = file.ContentLength;
                                            length = ProcessRangeHeader(context, fullLength, ref offset);
                                            fileStream.Seek(offset, SeekOrigin.Begin);
                                        }
                                        else
                                        {
                                            length = file.ContentLength;
                                        }
                                    }
                                    else
                                    {
                                        title = FileUtility.ReplaceFileExtension(title, ext);
                                        fileStream = FileConverter.Exec(file, ext);

                                        length = fileStream.Length;
                                    }
                                }

                                SendStreamByChunks(context, length, title, fileStream, ref flushed);
                            }
                            else
                            {
                                if (!readLink && fileDao.IsSupportedPreSignedUri(file))
                                {
                                    context.Response.Redirect(fileDao.GetPreSignedUri(file, TimeSpan.FromHours(1)).ToString(), true);

                                    return;
                                }

                                fileStream = fileDao.GetFileStream(file); // getStream to fix file.ContentLength

                                long offset = 0;
                                var length = file.ContentLength;
                                if (fileStream.CanSeek)
                                {
                                    length = ProcessRangeHeader(context, file.ContentLength, ref offset);
                                    fileStream.Seek(offset, SeekOrigin.Begin);
                                }

                                SendStreamByChunks(context, length, title, fileStream, ref flushed);
                            }
                        }
                        catch (ThreadAbortException tae)
                        {
                            Global.Logger.Error("DownloadFile", tae);
                        }
                        catch (HttpException e)
                        {
                            Global.Logger.Error("DownloadFile", e);
                            throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
                        }
                        finally
                        {
                            if (fileStream != null)
                            {
                                fileStream.Close();
                                fileStream.Dispose();
                            }
                        }

                        try
                        {
                            context.Response.Flush();
                            context.Response.SuppressContent = true;
                            context.ApplicationInstance.CompleteRequest();
                            flushed = true;
                        }
                        catch (HttpException ex)
                        {
                            Global.Logger.Error("DownloadFile", ex);
                        }
                    }
                }
            }
            catch (ThreadAbortException tae)
            {
                Global.Logger.Error("DownloadFile", tae);
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                Global.Logger.ErrorFormat("Url: {0} {1} IsClientConnected:{2}, line number:{3} frame:{4}", context.Request.Url, ex, context.Response.IsClientConnected, line, frame);
                if (!flushed && context.Response.IsClientConnected)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write(HttpUtility.HtmlEncode(ex.Message));
                }
            }
        }

        private static long ProcessRangeHeader(HttpContext context, long fullLength, ref long offset)
        {
            if (context == null) throw new ArgumentNullException();
            if (context.Request.Headers["Range"] == null) return fullLength;

            long endOffset = -1;

            var range = context.Request.Headers["Range"].Split(new[] { '=', '-' });
            offset = Convert.ToInt64(range[1]);
            if (range.Count() > 2 && !string.IsNullOrEmpty(range[2]))
            {
                endOffset = Convert.ToInt64(range[2]);
            }
            if (endOffset < 0 || endOffset >= fullLength)
            {
                endOffset = fullLength - 1;
            }

            var length = endOffset - offset + 1;

            if (length <= 0) throw new HttpException("Wrong Range header");

            Global.Logger.InfoFormat("Starting file download (chunk {0}-{1})", offset, endOffset);
            if (length < fullLength)
            {
                context.Response.StatusCode = (int)HttpStatusCode.PartialContent;
            }
            context.Response.AddHeader("Accept-Ranges", "bytes");
            context.Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", offset, endOffset, fullLength));

            return length;
        }

        private static void SendStreamByChunks(HttpContext context, long toRead, string title, Stream fileStream, ref bool flushed)
        {
            context.Response.Buffer = false;
            context.Response.AddHeader("Connection", "Keep-Alive");
            context.Response.AddHeader("Content-Length", toRead.ToString(CultureInfo.InvariantCulture));
            context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(title));
            context.Response.ContentType = MimeMapping.GetMimeMapping(title);

            const int bufferSize = 32 * 1024; // 32KB
            var buffer = new byte[bufferSize];
            while (toRead > 0)
            {
                var length = fileStream.Read(buffer, 0, bufferSize);

                if (context.Response.IsClientConnected)
                {
                    context.Response.OutputStream.Write(buffer, 0, length);
                    context.Response.Flush();
                    flushed = true;
                    toRead -= length;
                }
                else
                {
                    toRead = -1;
                    Global.Logger.Warn(string.Format("IsClientConnected is false. Why? Download file {0} Connection is lost. ", title));
                }
            }
        }

        private static void StreamFile(HttpContext context)
        {
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var id = context.Request[FilesLinkUtility.FileId];
                    int version;
                    if (!int.TryParse(context.Request[FilesLinkUtility.Version] ?? "", out version))
                    {
                        version = 0;
                    }
                    var doc = context.Request[FilesLinkUtility.DocShareKey];

                    fileDao.InvalidateCache(id);

                    File file;
                    var linkRight = FileShareLink.Check(doc, fileDao, out file);
                    if (linkRight == FileShare.Restrict && !SecurityContext.IsAuthenticated)
                    {
                        var auth = context.Request[FilesLinkUtility.AuthKey];
                        var validateResult = EmailValidationKeyProvider.ValidateEmailKey(id + version, auth ?? "", Global.StreamUrlExpire);
                        if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                        {
                            var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                            Global.Logger.Error(string.Format("{0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.Write(FilesCommonResource.ErrorMassage_SecurityException);
                            return;
                        }

                        if (!string.IsNullOrEmpty(FileUtility.SignatureSecret))
                        {
                            try
                            {
                                var header = context.Request.Headers[FileUtility.SignatureHeader];
                                if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                                {
                                    throw new Exception("Invalid header " + header);
                                }
                                header = header.Substring("Bearer ".Length);

                                JsonWebToken.JsonSerializer = new DocumentService.JwtSerializer();

                                var stringPayload = JsonWebToken.Decode(header, FileUtility.SignatureSecret);

                                Global.Logger.Debug("DocService StreamFile payload: " + stringPayload);
                                //var data = JObject.Parse(stringPayload);
                                //if (data == null)
                                //{
                                //    throw new ArgumentException("DocService StreamFile header is incorrect");
                                //}

                                //var signedStringUrl = data["url"] ?? (data["payload"] != null ? data["payload"]["url"] : null);
                                //if (signedStringUrl == null)
                                //{
                                //    throw new ArgumentException("DocService StreamFile header url is incorrect");
                                //}
                                //var signedUrl = new Uri(signedStringUrl.ToString());

                                //var signedQuery = signedUrl.Query;
                                //if (!context.Request.Url.Query.Equals(signedQuery))
                                //{
                                //    throw new SecurityException(string.Format("DocService StreamFile header id not equals: {0} and {1}", context.Request.Url.Query, signedQuery));
                                //}
                            }
                            catch (Exception ex)
                            {
                                Global.Logger.Error("Download stream header " + context.Request.Url, ex);
                                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                context.Response.Write(FilesCommonResource.ErrorMassage_SecurityException);
                                return;
                            }
                        }
                    }

                    if (file == null
                        || version > 0 && file.Version != version)
                    {
                        file = version > 0
                                   ? fileDao.GetFile(id, version)
                                   : fileDao.GetFile(id);
                    }

                    if (file == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    if (linkRight == FileShare.Restrict && SecurityContext.IsAuthenticated && !Global.GetFilesSecurity().CanRead(file))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }

                    if (!string.IsNullOrEmpty(file.Error))
                    {
                        context.Response.StatusDescription = file.Error;
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(file.Title));
                    context.Response.ContentType = MimeMapping.GetMimeMapping(file.Title);

                    using (var stream = fileDao.GetFileStream(file))
                    {
                        context.Response.AddHeader("Content-Length",
                                                   stream.CanSeek
                                                       ? stream.Length.ToString(CultureInfo.InvariantCulture)
                                                       : file.ContentLength.ToString(CultureInfo.InvariantCulture));
                        stream.StreamCopyTo(context.Response.OutputStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("Error for: " + context.Request.Url, ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write(ex.Message);
                return;
            }

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException he)
            {
                Global.Logger.ErrorFormat("StreamFile", he);
            }
        }

        private static void EmptyFile(HttpContext context)
        {
            try
            {
                var fileName = context.Request[FilesLinkUtility.FileTitle];
                if (!string.IsNullOrEmpty(FileUtility.SignatureSecret))
                {
                    try
                    {
                        var header = context.Request.Headers[FileUtility.SignatureHeader];
                        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                        {
                            throw new Exception("Invalid header " + header);
                        }
                        header = header.Substring("Bearer ".Length);

                        JsonWebToken.JsonSerializer = new DocumentService.JwtSerializer();

                        var stringPayload = JsonWebToken.Decode(header, FileUtility.SignatureSecret);

                        Global.Logger.Debug("DocService EmptyFile payload: " + stringPayload);
                        //var data = JObject.Parse(stringPayload);
                        //if (data == null)
                        //{
                        //    throw new ArgumentException("DocService EmptyFile header is incorrect");
                        //}

                        //var signedStringUrl = data["url"] ?? (data["payload"] != null ? data["payload"]["url"] : null);
                        //if (signedStringUrl == null)
                        //{
                        //    throw new ArgumentException("DocService EmptyFile header url is incorrect");
                        //}
                        //var signedUrl = new Uri(signedStringUrl.ToString());

                        //var signedQuery = signedUrl.Query;
                        //if (!context.Request.Url.Query.Equals(signedQuery))
                        //{
                        //    throw new SecurityException(string.Format("DocService EmptyFile header id not equals: {0} and {1}", context.Request.Url.Query, signedQuery));
                        //}
                    }
                    catch (Exception ex)
                    {
                        Global.Logger.Error("Download stream header " + context.Request.Url, ex);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.Write(FilesCommonResource.ErrorMassage_SecurityException);
                        return;
                    }
                }

                var toExtension = FileUtility.GetFileExtension(fileName);
                var fileExtension = FileUtility.GetInternalExtension(toExtension);
                fileName = "new" + fileExtension;
                var path = FileConstant.NewDocPath
                           + (CoreContext.Configuration.CustomMode ? "ru-RU/" : "default/")
                           + fileName;

                var storeTemplate = Global.GetStoreTemplate();
                if (!storeTemplate.IsFile("", path))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Write(FilesCommonResource.ErrorMassage_FileNotFound);
                    return;
                }

                context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(fileName));
                context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);

                using (var stream = storeTemplate.GetReadStream("", path))
                {
                    context.Response.AddHeader("Content-Length",
                                               stream.CanSeek
                                                   ? stream.Length.ToString(CultureInfo.InvariantCulture)
                                                   : storeTemplate.GetFileSize("", path).ToString(CultureInfo.InvariantCulture));
                    stream.StreamCopyTo(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("Error for: " + context.Request.Url, ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write(ex.Message);
                return;
            }

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException he)
            {
                Global.Logger.ErrorFormat("EmptyFile", he);
            }
        }

        private static void TempFile(HttpContext context)
        {
            var fileName = context.Request[FilesLinkUtility.FileTitle];
            var auth = context.Request[FilesLinkUtility.AuthKey];

            var validateResult = EmailValidationKeyProvider.ValidateEmailKey(fileName, auth ?? "", Global.StreamUrlExpire);
            if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
            {
                var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                Global.Logger.Error(string.Format("{0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.Write(FilesCommonResource.ErrorMassage_SecurityException);
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);
            context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(fileName));

            var store = Global.GetStore();

            var path = Path.Combine("temp_stream", fileName);

            if (!store.IsFile(FileConstant.StorageDomainTmp, path))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Write(FilesCommonResource.ErrorMassage_FileNotFound);
                return;
            }

            using (var readStream = store.GetReadStream(FileConstant.StorageDomainTmp, path))
            {
                context.Response.AddHeader("Content-Length", readStream.Length.ToString(CultureInfo.InvariantCulture));
                readStream.StreamCopyTo(context.Response.OutputStream);
            }

            store.Delete(FileConstant.StorageDomainTmp, path);

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException he)
            {
                Global.Logger.ErrorFormat("TempFile", he);
            }
        }

        private static void DifferenceFile(HttpContext context)
        {
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var id = context.Request[FilesLinkUtility.FileId];
                    int version;
                    int.TryParse(context.Request[FilesLinkUtility.Version] ?? "", out version);
                    var doc = context.Request[FilesLinkUtility.DocShareKey];

                    File file;
                    var linkRight = FileShareLink.Check(doc, fileDao, out file);
                    if (linkRight == FileShare.Restrict && !SecurityContext.IsAuthenticated)
                    {
                        var auth = context.Request[FilesLinkUtility.AuthKey];
                        var validateResult = EmailValidationKeyProvider.ValidateEmailKey(id + version, auth ?? "", Global.StreamUrlExpire);
                        if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                        {
                            var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                            Global.Logger.Error(string.Format("{0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.Write(FilesCommonResource.ErrorMassage_SecurityException);
                            return;
                        }
                    }

                    fileDao.InvalidateCache(id);

                    if (file == null
                        || version > 0 && file.Version != version)
                    {
                        file = version > 0
                                   ? fileDao.GetFile(id, version)
                                   : fileDao.GetFile(id);
                    }

                    if (file == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    if (linkRight == FileShare.Restrict && SecurityContext.IsAuthenticated && !Global.GetFilesSecurity().CanRead(file))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }

                    if (!string.IsNullOrEmpty(file.Error))
                    {
                        context.Response.StatusDescription = file.Error;
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(".zip"));
                    context.Response.ContentType = MimeMapping.GetMimeMapping(".zip");

                    using (var stream = fileDao.GetDifferenceStream(file))
                    {
                        context.Response.AddHeader("Content-Length", stream.Length.ToString(CultureInfo.InvariantCulture));
                        stream.StreamCopyTo(context.Response.OutputStream);
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write(ex.Message);
                Global.Logger.Error("Error for: " + context.Request.Url, ex);
                return;
            }

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException he)
            {
                Global.Logger.ErrorFormat("DifferenceFile", he);
            }
        }

        private static string GetEtag(File file)
        {
            return file.ID + ":" + file.Version + ":" + file.Title.GetHashCode() + ":" + file.ContentLength;
        }

        private static void CreateFile(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                var refererURL = context.Request.GetUrlRewriter().AbsoluteUri;

                context.Session["refererURL"] = refererURL;
                var authUrl = "~/Auth.aspx";
                context.Response.Redirect(authUrl, true);
                return;
            }

            var responseMessage = context.Request["response"] == "message";
            var folderId = context.Request[FilesLinkUtility.FolderId];
            if (string.IsNullOrEmpty(folderId))
                folderId = Global.FolderMy.ToString();
            Folder folder;

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                folder = folderDao.GetFolder(folderId);
            }
            if (folder == null) throw new HttpException((int)HttpStatusCode.NotFound, FilesCommonResource.ErrorMassage_FolderNotFound);
            if (!Global.GetFilesSecurity().CanCreate(folder)) throw new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException_Create);

            File file;
            var fileUri = context.Request[FilesLinkUtility.FileUri];
            var fileTitle = context.Request[FilesLinkUtility.FileTitle];
            try
            {
                if (!string.IsNullOrEmpty(fileUri))
                {
                    file = CreateFileFromUri(folder, fileUri, fileTitle);
                }
                else
                {
                    var docType = context.Request["doctype"];
                    file = CreateFileFromTemplate(folder, fileTitle, docType);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error(ex);
                if (responseMessage)
                {
                    context.Response.Write("error: " + ex.Message);
                    return;
                }
                context.Response.Redirect(PathProvider.StartURL + "#error/" + HttpUtility.UrlEncode(ex.Message), true);
                return;
            }

            FileMarker.MarkAsNew(file);

            if (responseMessage)
            {
                context.Response.Write("ok: " + string.Format(FilesCommonResource.MessageFileCreated, folder.Title));
                return;
            }

            context.Response.Redirect(
                (context.Request["openfolder"] ?? "").Equals("true")
                    ? PathProvider.GetFolderUrl(file.FolderID)
                    : (FilesLinkUtility.GetFileWebEditorUrl(file.ID) + "#message/" + HttpUtility.UrlEncode(string.Format(FilesCommonResource.MessageFileCreated, folder.Title))));
        }

        private static File CreateFileFromTemplate(Folder folder, string fileTitle, string docType)
        {
            var storeTemplate = Global.GetStoreTemplate();

            var lang = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();

            var fileExt = FileUtility.InternalExtension[FileType.Document];
            if (!string.IsNullOrEmpty(docType))
            {
                var tmpFileType = Services.DocumentService.Configuration.DocType.FirstOrDefault(r => r.Value.Equals(docType, StringComparison.OrdinalIgnoreCase));
                string tmpFileExt;
                FileUtility.InternalExtension.TryGetValue(tmpFileType.Key, out tmpFileExt);
                if (!string.IsNullOrEmpty(tmpFileExt))
                    fileExt = tmpFileExt;
            }

            var templateName = "new" + fileExt;

            var templatePath = FileConstant.NewDocPath + lang + "/";
            if (!storeTemplate.IsDirectory(templatePath))
                templatePath = FileConstant.NewDocPath + "default/";
            templatePath += templateName;

            if (string.IsNullOrEmpty(fileTitle))
            {
                fileTitle = templateName;
            }
            else
            {
                fileTitle = fileTitle + fileExt;
            }

            var file = new File
                {
                    Title = fileTitle,
                    FolderID = folder.ID,
                    Comment = FilesCommonResource.CommentCreate,
                };

            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var stream = storeTemplate.GetReadStream("", templatePath))
            {
                file.ContentLength = stream.CanSeek ? stream.Length : storeTemplate.GetFileSize(templatePath);
                return fileDao.SaveFile(file, stream);
            }
        }

        private static File CreateFileFromUri(Folder folder, string fileUri, string fileTitle)
        {
            if (string.IsNullOrEmpty(fileTitle))
                fileTitle = Path.GetFileName(HttpUtility.UrlDecode(fileUri));

            var file = new File
                {
                    Title = fileTitle,
                    FolderID = folder.ID,
                    Comment = FilesCommonResource.CommentCreate,
                };

            var req = (HttpWebRequest)WebRequest.Create(fileUri);

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var fileStream = new ResponseStream(req.GetResponse()))
            {
                file.ContentLength = fileStream.Length;

                return fileDao.SaveFile(file, fileStream);
            }
        }

        private static void Redirect(HttpContext context)
        {
            if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
            var urlRedirect = string.Empty;
            var folderId = context.Request[FilesLinkUtility.FolderId];
            if (!string.IsNullOrEmpty(folderId))
            {
                try
                {
                    urlRedirect = PathProvider.GetFolderUrl(folderId);
                }
                catch (ArgumentNullException e)
                {
                    throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
                }
            }

            var fileId = context.Request[FilesLinkUtility.FileId];
            if (!string.IsNullOrEmpty(fileId))
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var file = fileDao.GetFile(fileId);
                    if (file == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    urlRedirect = FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID);
                }
            }

            if (string.IsNullOrEmpty(urlRedirect))
                throw new HttpException((int)HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest);
            context.Response.Redirect(urlRedirect);
        }

        private static void TrackFile(HttpContext context)
        {
            var auth = context.Request[FilesLinkUtility.AuthKey];
            var fileId = context.Request[FilesLinkUtility.FileId];
            Global.Logger.Debug("DocService track fileid: " + fileId);

            var callbackSpan = TimeSpan.FromDays(128);
            var validateResult = EmailValidationKeyProvider.ValidateEmailKey(fileId, auth ?? "", callbackSpan);
            if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
            {
                Global.Logger.ErrorFormat("DocService track auth error: {0}, {1}: {2}", validateResult.ToString(), FilesLinkUtility.AuthKey, auth);
                throw new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);
            }

            DocumentServiceTracker.TrackerData fileData;
            try
            {
                string body;
                using (var receiveStream = context.Request.InputStream)
                using (var readStream = new StreamReader(receiveStream))
                {
                    body = readStream.ReadToEnd();
                }

                Global.Logger.Debug("DocService track body: " + body);
                if (string.IsNullOrEmpty(body))
                {
                    throw new ArgumentException("DocService request body is incorrect");
                }

                var data = JToken.Parse(body);
                if (data == null)
                {
                    throw new ArgumentException("DocService request is incorrect");
                }
                fileData = data.ToObject<DocumentServiceTracker.TrackerData>();
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService track error read body", e);
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }

            if (!string.IsNullOrEmpty(FileUtility.SignatureSecret))
            {
                JsonWebToken.JsonSerializer = new DocumentService.JwtSerializer();
                if (!string.IsNullOrEmpty(fileData.Token))
                {
                    try
                    {
                        var dataString = JsonWebToken.Decode(fileData.Token, FileUtility.SignatureSecret);
                        var data = JObject.Parse(dataString);
                        if (data == null)
                        {
                            throw new ArgumentException("DocService request token is incorrect");
                        }
                        fileData = data.ToObject<DocumentServiceTracker.TrackerData>();
                    }
                    catch (SignatureVerificationException ex)
                    {
                        Global.Logger.Error("DocService track header", ex);
                        throw new HttpException((int)HttpStatusCode.Forbidden, ex.Message);
                    }
                }
                else
                {
                    //todo: remove old scheme
                    var header = context.Request.Headers[FileUtility.SignatureHeader];
                    if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                    {
                        Global.Logger.Error("DocService track header is null");
                        throw new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);
                    }
                    header = header.Substring("Bearer ".Length);

                    try
                    {
                        var stringPayload = JsonWebToken.Decode(header, FileUtility.SignatureSecret);

                        Global.Logger.Debug("DocService track payload: " + stringPayload);
                        var jsonPayload = JObject.Parse(stringPayload);
                        var data = jsonPayload["payload"];
                        if (data == null)
                        {
                            throw new ArgumentException("DocService request header is incorrect");
                        }
                        fileData = data.ToObject<DocumentServiceTracker.TrackerData>();
                    }
                    catch (SignatureVerificationException ex)
                    {
                        Global.Logger.Error("DocService track header", ex);
                        throw new HttpException((int)HttpStatusCode.Forbidden, ex.Message);
                    }
                }
            }

            DocumentServiceTracker.TrackResponse result;
            try
            {
                result = DocumentServiceTracker.ProcessData(fileId, fileData);
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService track:", e);
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }
            result = result ?? new DocumentServiceTracker.TrackResponse();

            context.Response.Write(DocumentServiceTracker.TrackResponse.Serialize(result));
        }
    }
}