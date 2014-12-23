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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using ASC.Common.Web;
using ASC.Core;
using ASC.Data.Storage.S3;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;
using System.Diagnostics;
using MimeMapping = System.Web.MimeMapping;

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
                context.Response.Redirect(TenantExtra.GetTariffPageLink());
            }

            try
            {
                switch ((context.Request[FilesLinkUtility.Action] ?? "").ToLower())
                {
                    case "view":
                        DownloadFile(context, true);
                        break;
                    case "download":
                        DownloadFile(context, false);
                        break;
                    case "bulk":
                        BulkDownloadFile(context);
                        break;
                    case "stream":
                        StreamFile(context);
                        break;
                    case "create":
                        CreateFile(context);
                        break;
                    case "redirect":
                        Redirect(context);
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

            if (store is S3Storage)
            {
                var url = store.GetPreSignedUri(FileConstant.StorageDomainTmp, path, TimeSpan.FromHours(1), null).ToString();
                context.Response.Redirect(url);
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/zip";
            context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(FileConstant.DownloadTitle + ".zip"));

            using (var readStream = store.IronReadStream(FileConstant.StorageDomainTmp, path, 40))
            {
                context.Response.AddHeader("Content-Length", readStream.Length.ToString());
                readStream.StreamCopyTo(context.Response.OutputStream);
            }
            try
            {
                context.Response.Flush();
                context.Response.End();
            }
            catch (HttpException)
            {
            }
        }

        private static void DownloadFile(HttpContext context, bool inline)
        {
            try
            {
                var id = context.Request[FilesLinkUtility.FileId];
                var shareLinkKey = context.Request[FilesLinkUtility.DocShareKey] ?? "";

                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    File file;
                    var readLink = FileShareLink.Check(shareLinkKey, true, fileDao, out file);
                    if (!readLink && file == null)
                    {
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

                    if (!fileDao.IsExistOnStorage(file))
                    {
                        Global.Logger.ErrorFormat("Download file error. File is not exist on storage. File id: {0}.", file.ID);
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                        return;
                    }

                    FileMarker.RemoveMarkAsNew(file);

                    context.Response.Clear();
                    context.Response.ContentType = MimeMapping.GetMimeMapping(file.Title);
                    context.Response.Charset = "utf-8";

                    var title = file.Title.Replace(',', '_');

                    var ext = FileUtility.GetFileExtension(file.Title);

                    var outType = string.Empty;
                    var curQuota = TenantExtra.GetTenantQuota();
                    if (curQuota.DocsEdition)
                        outType = context.Request[FilesLinkUtility.OutType];

                    if (!string.IsNullOrEmpty(outType) && !inline)
                    {
                        outType = outType.Trim();
                        if (FileUtility.ExtsConvertible[ext].Contains(outType))
                        {
                            ext = outType;

                            title = FileUtility.ReplaceFileExtension(title, ext);
                        }
                    }

                    context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(title, inline));

                    if (inline && string.Equals(context.Request.Headers["If-None-Match"], GetEtag(file)))
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
                            if (file.ContentLength <= SetupInfo.AvailableFileSize)
                            {
                                if (file.ConvertedType == null && (string.IsNullOrEmpty(outType) || inline))
                                {
                                    if (fileDao.IsSupportedPreSignedUri(file))
                                    {
                                        context.Response.Redirect(fileDao.GetPreSignedUri(file, TimeSpan.FromHours(1)).ToString(), true);

                                        return;
                                    }

                                    fileStream = fileDao.GetFileStream(file);
                                    context.Response.AddHeader("Content-Length", file.ContentLength.ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    fileStream = FileConverter.Exec(file, ext);
                                }

                                fileStream.StreamCopyTo(context.Response.OutputStream);

                                if (!context.Response.IsClientConnected)
                                {
                                    Global.Logger.Error(String.Format("Download file error {0} {1} Connection is lost. Too long to buffer the file", file.Title, file.ID));
                                }

                                FilesMessageService.Send(file, context.Request, MessageAction.FileDownloaded, file.Title);

                                context.Response.Flush();
                            }
                            else
                            {
                                long offset = 0;

                                if (context.Request.Headers["Range"] != null)
                                {
                                    context.Response.StatusCode = 206;
                                    var range = context.Request.Headers["Range"].Split(new[] { '=', '-' });
                                    offset = Convert.ToInt64(range[1]);
                                }

                                if (offset > 0)
                                    Global.Logger.Info("Starting file download offset is " + offset);

                                context.Response.AddHeader("Connection", "Keep-Alive");
                                context.Response.AddHeader("Accept-Ranges", "bytes");

                                if (offset > 0)
                                {
                                    context.Response.AddHeader("Content-Range", String.Format(" bytes {0}-{1}/{2}", offset, file.ContentLength - 1, file.ContentLength));
                                }

                                var dataToRead = file.ContentLength;
                                const int bufferSize = 1024;
                                var buffer = new Byte[bufferSize];

                                if (file.ConvertedType == null && (string.IsNullOrEmpty(outType) || inline))
                                {
                                    if (fileDao.IsSupportedPreSignedUri(file))
                                    {
                                        context.Response.Redirect(fileDao.GetPreSignedUri(file, TimeSpan.FromHours(1)).ToString(), true);

                                        return;
                                    }

                                    fileStream = fileDao.GetFileStream(file, offset);
                                    context.Response.AddHeader("Content-Length", (file.ContentLength - offset).ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    fileStream = FileConverter.Exec(file, ext);

                                    if (offset > 0)
                                    {
                                        var startBytes = offset;

                                        while (startBytes > 0)
                                        {
                                            long readCount;

                                            if (bufferSize >= startBytes)
                                            {
                                                readCount = startBytes;
                                            }
                                            else
                                            {
                                                readCount = bufferSize;
                                            }

                                            var length = fileStream.Read(buffer, 0, (int)readCount);

                                            startBytes -= length;
                                        }
                                    }
                                }

                                while (dataToRead > 0)
                                {
                                    int length;

                                    try
                                    {
                                        length = fileStream.Read(buffer, 0, bufferSize);
                                    }
                                    catch (HttpException exception)
                                    {
                                        Global.Logger.Error(
                                            String.Format("Read from stream is error. Download file {0} {1}. Maybe Connection is lost.?? Error is {2} ",
                                                          file.Title,
                                                          file.ID,
                                                          exception
                                                ));

                                        throw;
                                    }

                                    if (context.Response.IsClientConnected)
                                    {
                                        context.Response.OutputStream.Write(buffer, 0, length);
                                        dataToRead = dataToRead - length;
                                    }
                                    else
                                    {
                                        dataToRead = -1;
                                        Global.Logger.Error(String.Format("IsClientConnected is false. Why? Download file {0} {1} Connection is lost. ", file.Title, file.ID));
                                    }
                                }
                            }
                        }
                        catch (ThreadAbortException)
                        {
                        }
                        catch (HttpException e)
                        {
                            throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
                        }
                        finally
                        {
                            if (fileStream != null)
                            {
                                fileStream.Flush();
                                fileStream.Close();
                                fileStream.Dispose();
                            }
                        }

                        try
                        {
                            context.Response.End();
                        }
                        catch (HttpException)
                        {
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
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
                context.Response.StatusCode = 400;
                context.Response.Write(HttpUtility.HtmlEncode(ex.Message));
            }
        }

        private static void StreamFile(HttpContext context)
        {
            try
            {
                var id = context.Request[FilesLinkUtility.FileId];
                var auth = context.Request[FilesLinkUtility.AuthKey];
                int version;
                int.TryParse(context.Request[FilesLinkUtility.Version], out version);

                int validateTimespan;
                int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validateTimespan);
                if (validateTimespan <= 0) validateTimespan = 5;

                var validateResult = EmailValidationKeyProvider.ValidateEmailKey(id + version, auth, TimeSpan.FromMinutes(validateTimespan));
                if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                {
                    var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                    Global.Logger.Error(string.Format("{0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url), exc);

                    throw exc;
                }

                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var file = version > 0
                                   ? fileDao.GetFile(id, version)
                                   : fileDao.GetFile(id);
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
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Write(ex.Message);
                Global.Logger.Error("Error for: " + context.Request.Url, ex);
            }
            try
            {
                context.Response.Flush();
                context.Response.End();
            }
            catch (HttpException)
            {
            }
        }

        private static string GetEtag(File file)
        {
            return file.ID + ":" + file.Version + ":" + file.Title.GetHashCode();
        }

        private static void CreateFile(HttpContext context)
        {
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

            File file = null;
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
                    var template = context.Request["template"];
                    var docType = context.Request["doctype"];
                    file = CreateFileFromTemplate(folder, template, fileTitle, docType);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error(ex);
                context.Response.Redirect(PathProvider.StartURL + "#error/" + ex.Message);
            }

            FileMarker.MarkAsNew(file);

            context.Response.Redirect(
                (context.Request["openfolder"] ?? "").Equals("true")
                    ? PathProvider.GetFolderUrl(file.FolderID)
                    : FilesLinkUtility.GetFileWebEditorUrl(file.ID));
        }

        private static File CreateFileFromTemplate(Folder folder, string template, string fileTitle, string docType)
        {
            var storeTemp = Global.GetStoreTemplate();

            var lang = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();

            var fileExt = FileUtility.InternalExtension[FileType.Document];
            if (!string.IsNullOrEmpty(docType))
            {
                var tmpFileType = DocumentServiceParams.DocType.FirstOrDefault(r => r.Value.Equals(docType, StringComparison.OrdinalIgnoreCase));
                string tmpFileExt;
                FileUtility.InternalExtension.TryGetValue(tmpFileType.Key, out tmpFileExt);
                if (!string.IsNullOrEmpty(tmpFileExt))
                    fileExt = tmpFileExt;
            }

            string templatePath;
            string templateName;
            if (string.IsNullOrEmpty(template))
            {
                templateName = "new" + fileExt;

                templatePath = FileConstant.NewDocPath + lang + "/";
                if (!storeTemp.IsDirectory(templatePath))
                    templatePath = FileConstant.NewDocPath + "default/";
                templatePath += templateName;
            }
            else
            {
                templateName = template + fileExt;

                templatePath = FileConstant.TemplateDocPath + lang + "/";
                if (!storeTemp.IsDirectory(templatePath))
                    templatePath = FileConstant.TemplateDocPath + "default/";
                templatePath += templateName;

                if (!storeTemp.IsFile(templatePath))
                {
                    templatePath = FileConstant.TemplateDocPath + "default/";
                    templatePath += templateName;
                }
            }

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
                    ContentLength = storeTemp.GetFileSize(templatePath),
                    FolderID = folder.ID
                };

            file.ConvertedType = FileUtility.GetInternalExtension(file.Title);

            using (var fileDao = Global.DaoFactory.GetFileDao())
            using (var stream = storeTemp.IronReadStream("", templatePath, 10))
            {
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
                    FolderID = folder.ID
                };

            var req = (HttpWebRequest)WebRequest.Create(fileUri);

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
            int id;
            var folderId = context.Request[FilesLinkUtility.FolderId];
            if (!string.IsNullOrEmpty(folderId) && int.TryParse(folderId, out id))
            {
                try
                {
                    urlRedirect = PathProvider.GetFolderUrl(id);
                }
                catch (ArgumentNullException e)
                {
                    throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
                }
            }

            var fileId = context.Request[FilesLinkUtility.FileId];
            if (!string.IsNullOrEmpty(fileId) && int.TryParse(fileId, out id))
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var file = fileDao.GetFile(id);
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
            var vkey = context.Request["vkey"];
            var fileId = Common.Utils.Signature.Read<string>(vkey, StudioKeySettings.GetSKey());
            if (fileId == null)
            {
                Global.Logger.ErrorFormat("DocService track vkey error");
                throw new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);
            }

            var isNew = (context.Request["new"] ?? "") == "true";

            Global.Logger.Debug("DocService track fileid: " + fileId);
            string body;
            try
            {
                using (var receiveStream = context.Request.InputStream)
                using (var readStream = new StreamReader(receiveStream))
                {
                    body = readStream.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService track error read body", e);
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }

            try
            {
                Global.Logger.Debug("DocService track body: " + body);
                DocumentServiceTracker.ProcessData(fileId, isNew, body);
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService track:", e);
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }
        }
    }
}