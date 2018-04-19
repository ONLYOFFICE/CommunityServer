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
using System.Globalization;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Exceptions;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;

namespace ASC.Web.Mail.HttpHandlers
{
    public class FilesUploader : FileUploadHandler
    {
        private static readonly MailBoxManager MailBoxManager = new MailBoxManager();

        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        private CultureInfo CurrentCulture
        {
            get
            {
                var u = CoreContext.UserManager.GetUsers(new Guid(Username));

                var culture = !string.IsNullOrEmpty(u.CultureName) ? u.GetCulture() : CoreContext.TenantManager.GetCurrentTenant().GetCulture();

                return culture;
            }
        }

        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileName = string.Empty;
            MailAttachment attachment = null;
            try
            {
                if (FileToUpload.HasFilesToUpload(context))
                {
                    try
                    {
                        Thread.CurrentThread.CurrentCulture = CurrentCulture;
                        Thread.CurrentThread.CurrentUICulture = CurrentCulture;

                        var mailId = Convert.ToInt32(context.Request["messageId"]);
                        var copyToMy = Convert.ToInt32(context.Request["copyToMy"]);

                        if (mailId < 1) throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not yet saved!");

                        var item = MailBoxManager.GetMailInfo(TenantId, Username, mailId, new MailMessage.Options());
                        if (item == null)
                            throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

                        if (string.IsNullOrEmpty(item.StreamId)) throw new AttachmentsException(AttachmentsException.Types.BadParams, "Have no stream");

                        var postedFile = new FileToUpload(context);
                        fileName = context.Request["name"];

                        if (copyToMy == 1)
                        {
                            var uploadedFile = FileUploader.Exec(Global.FolderMy.ToString(), fileName, postedFile.ContentLength, postedFile.InputStream, true);
                            return new FileUploadResult
                                {
                                    Success = true,
                                    FileName = uploadedFile.Title,
                                    FileURL = FilesLinkUtility.GetFileWebPreviewUrl(uploadedFile.Title, uploadedFile.ID),
                                    Data = new MailAttachment
                                        {
                                            fileId = Convert.ToInt32(uploadedFile.ID),
                                            fileName = uploadedFile.Title,
                                            size = uploadedFile.ContentLength,
                                            contentType = uploadedFile.ConvertedType,
                                            attachedAsLink = true,
                                            tenant = TenantId,
                                            user = Username
                                        }
                                };
                        }

                        attachment = MailBoxManager.AttachFileToDraft(TenantId, Username, mailId, fileName, postedFile.InputStream);

                        return new FileUploadResult
                            {
                                Success = true,
                                FileName = attachment.fileName,
                                FileURL = attachment.storedFileUrl,
                                Data = attachment
                            };
                    }
                    catch(AttachmentsException e)
                    {
                        string errorMessage;

                        switch (e.ErrorType)
                        {
                            case AttachmentsException.Types.BadParams:
                                errorMessage = MailScriptResource.AttachmentsBadInputParamsError;
                                break;
                            case AttachmentsException.Types.EmptyFile:
                                errorMessage = MailScriptResource.AttachmentsEmptyFileNotSupportedError;
                                break;
                            case AttachmentsException.Types.MessageNotFound:
                                errorMessage = MailScriptResource.AttachmentsMessageNotFoundError;
                                break;
                            case AttachmentsException.Types.TotalSizeExceeded:
                                errorMessage = MailScriptResource.AttachmentsTotalLimitError;
                                break;
                            case AttachmentsException.Types.DocumentNotFound:
                                errorMessage = MailScriptResource.AttachmentsDocumentNotFoundError;
                                break;
                            case AttachmentsException.Types.DocumentAccessDenied:
                                errorMessage = MailScriptResource.AttachmentsDocumentAccessDeniedError;
                                break;
                            default:
                                errorMessage = MailScriptResource.AttachmentsUnknownError;
                                break;
                        }
                        throw new Exception(errorMessage);
                    }
                    catch(TenantQuotaException)
                    {
                        throw;
                    }
                    catch(Exception)
                    {
                        throw new Exception(MailScriptResource.AttachmentsUnknownError);
                    }
                }
                throw new Exception(MailScriptResource.AttachmentsBadInputParamsError);
            }
            catch(Exception ex)
            {
                return new FileUploadResult
                    {
                        Success = false,
                        FileName = fileName,
                        Data = attachment,
                        Message = ex.Message,
                    };
            }
        }
    }
}