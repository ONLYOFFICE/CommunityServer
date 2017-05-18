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


using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ASC.Web.Files.Services.NotifyService
{
    public static class NotifyClient
    {
        public static INotifyClient Instance { get; private set; }

        static NotifyClient()
        {
            Instance = WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource.Instance);
        }

        public static void SendDocuSignComplete(File file)
        {
            var recipient = NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());

            Instance.SendNoticeAsync(
                NotifyConstants.Event_DocuSignComplete,
                file.UniqID,
                recipient,
                true,
                new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID))),
                new TagValue(NotifyConstants.Tag_DocumentTitle, file.Title)
                );
        }

        public static void SendDocuSignStatus(string subject, string status)
        {
            var recipient = NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());

            Instance.SendNoticeAsync(
                NotifyConstants.Event_DocuSignStatus,
                null,
                recipient,
                true,
                new TagValue(NotifyConstants.Tag_DocumentTitle, subject),
                new TagValue(NotifyConstants.Tag_Message, status)
                );
        }

        public static void SendMailMergeEnd(Guid userId, int countMails, int countError)
        {
            var recipient = NotifySource.Instance.GetRecipientsProvider().GetRecipient(userId.ToString());

            Instance.SendNoticeAsync(
                NotifyConstants.Event_MailMergeEnd,
                null,
                recipient,
                true,
                new TagValue(NotifyConstants.Tag_MailsCount, countMails),
                new TagValue(NotifyConstants.Tag_Message, countError > 0 ? string.Format(FilesCommonResource.ErrorMassage_MailMergeCount, countError) : string.Empty)
                );
        }

        public static void SendLinkToEmail(File file, String url, String message, List<String> addressRecipients)
        {
            if (file == null || String.IsNullOrEmpty(url))
                throw new ArgumentException();

            foreach (var recipients in addressRecipients
                .Select(addressRecipient => (IRecipient) (new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), String.Empty, new[] {addressRecipient}, false))))
            {
                Instance.SendNoticeToAsync(
                    NotifyConstants.Event_LinkToEmail,
                    null,
                    new[] {recipients},
                    new[] {ASC.Core.Configuration.Constants.NotifyEMailSenderSysName},
                    null,
                    new TagValue(NotifyConstants.Tag_DocumentTitle, file.Title),
                    new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(url)),
                    new TagValue(NotifyConstants.Tag_AccessRights, GetAccessString(file.Access, CultureInfo.CurrentUICulture)),
                    new TagValue(NotifyConstants.Tag_Message, message.HtmlEncode()),
                    new TagValue(NotifyConstants.Tag_UserEmail, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email),
                    new TagValue(CommonTags.WithPhoto, CoreContext.Configuration.Personal ? "personal" : ""),
                    new TagValue(CommonTags.IsPromoLetter, CoreContext.Configuration.Personal ? "true" : "false"),
                    new TagValue("noUnsubscribeLink", "true")
                    );
            }
        }

        public static void SendShareNotice(FileEntry fileEntry, Dictionary<Guid, FileShare> recipients, string message)
        {
            if (fileEntry == null || recipients.Count == 0) return;

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                if (fileEntry is File
                    && folderDao.GetFolder(((File)fileEntry).FolderID) == null) return;

                String url;
                if (fileEntry is File)
                {
                    url = FilesLinkUtility.GetFileWebPreviewUrl(fileEntry.Title, fileEntry.ID);
                }
                else
                    url = PathProvider.GetFolderUrl(((Folder)fileEntry));

                var recipientsProvider = NotifySource.Instance.GetRecipientsProvider();

                foreach (var recipientPair in recipients)
                {
                    var u = CoreContext.UserManager.GetUsers(recipientPair.Key);
                    var culture = string.IsNullOrEmpty(u.CultureName)
                                      ? CoreContext.TenantManager.GetCurrentTenant().GetCulture()
                                      : CultureInfo.GetCultureInfo(u.CultureName);

                    var aceString = GetAccessString(recipientPair.Value, culture);
                    var recipient = recipientsProvider.GetRecipient(u.ID.ToString());

                    Instance.SendNoticeAsync(
                        fileEntry is File ? NotifyConstants.Event_ShareDocument : NotifyConstants.Event_ShareFolder,
                        fileEntry.UniqID,
                        recipient,
                        true,
                        new TagValue(NotifyConstants.Tag_DocumentTitle, fileEntry.Title),
                        new TagValue(NotifyConstants.Tag_FolderID, fileEntry.ID),
                        new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(url)),
                        new TagValue(NotifyConstants.Tag_AccessRights, aceString),
                        new TagValue(NotifyConstants.Tag_Message, message.HtmlEncode())
                        );
                }
            }
        }

        private static String GetAccessString(FileShare fileShare, CultureInfo cultureInfo)
        {
            switch (fileShare)
            {
                case FileShare.Read:
                    return FilesCommonResource.ResourceManager.GetString("AceStatusEnum_Read", cultureInfo);
                case FileShare.ReadWrite:
                    return FilesCommonResource.ResourceManager.GetString("AceStatusEnum_ReadWrite", cultureInfo);
                case FileShare.Review:
                    return FilesCommonResource.ResourceManager.GetString("AceStatusEnum_Review", cultureInfo);
                default:
                    return String.Empty;
            }
        }
    }
}