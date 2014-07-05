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

using System;
using System.Web;
using ASC.Mail.Aggregator.Common;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Core;
using ASC.Web.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Web.Mail.Resources;

namespace ASC.Web.Mail.HttpHandlers
{
    public class FilesUploader : FileUploadHandler
    {
        private static readonly MailBoxManager _mailBoxManager = new MailBoxManager(25);

        private String ModuleName
        {
            get { return "mailaggregator"; }
        }

        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var file_name = string.Empty;
            MailAttachment attachment = null;
            try
            {
                if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
                    throw new UnauthorizedAccessException(MailResource.AttachemntsUnauthorizedError);

                if (FileToUpload.HasFilesToUpload(context))
                {
                    try
                    {
                        var stream_id = context.Request["stream"];
                        var mail_id = Convert.ToInt32(context.Request["messageId"]);

                        if (mail_id < 1)
                            throw new AttachmentsException(AttachmentsException.Types.MESSAGE_NOT_FOUND,
                                                           "Message not yet saved!");

                        if (String.IsNullOrEmpty(stream_id))
                            throw new AttachmentsException(AttachmentsException.Types.BAD_PARAMS, "Have no stream");

                        var posted_file = new FileToUpload(context);

                        file_name = context.Request["name"];

                        attachment = new MailAttachment
                            {
                                fileId = -1,
                                size = posted_file.ContentLength,
                                fileName = file_name,
                                streamId = stream_id,
                                tenant = TenantId,
                                user = Username
                            };

                        attachment = _mailBoxManager.AttachFile(TenantId, Username, mail_id,
                                                                    file_name, posted_file.InputStream, stream_id);

                        return new FileUploadResult
                            {
                                Success = true,
                                FileName = attachment.fileName,
                                FileURL = attachment.storedFileUrl,
                                Data = attachment
                            };
                    }
                    catch (AttachmentsException e)
                    {
                        string error_message;

                        switch (e.ErrorType)
                        {
                            case AttachmentsException.Types.BAD_PARAMS:
                                error_message = MailScriptResource.AttachmentsBadInputParamsError;
                                break;
                            case AttachmentsException.Types.EMPTY_FILE:
                                error_message = MailScriptResource.AttachmentsEmptyFileNotSupportedError;
                                break;
                            case AttachmentsException.Types.MESSAGE_NOT_FOUND:
                                error_message = MailScriptResource.AttachmentsMessageNotFoundError;
                                break;
                            case AttachmentsException.Types.TOTAL_SIZE_EXCEEDED:
                                error_message = MailScriptResource.AttachmentsTotalLimitError;
                                break;
                            case AttachmentsException.Types.DOCUMENT_NOT_FOUND:
                                error_message = MailScriptResource.AttachmentsDocumentNotFoundError;
                                break;
                            case AttachmentsException.Types.DOCUMENT_ACCESS_DENIED:
                                error_message = MailScriptResource.AttachmentsDocumentAccessDeniedError;
                                break;
                            default:
                                error_message = MailScriptResource.AttachmentsUnknownError;
                                break;
                        }
                        throw new Exception(error_message);
                    }
                    catch (ASC.Core.Tenants.TenantQuotaException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        throw new Exception(MailScriptResource.AttachmentsUnknownError);
                    }
                }
                throw new Exception(MailScriptResource.AttachmentsBadInputParamsError);
            }
            catch (Exception ex)
            {
                return new FileUploadResult
                    {
                        Success = false,
                        FileName = file_name,
                        Data = attachment,
                        Message = ex.Message,
                    };
            }
        }
    }
}