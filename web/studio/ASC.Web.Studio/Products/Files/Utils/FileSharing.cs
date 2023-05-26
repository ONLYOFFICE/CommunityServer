/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.Linq;
using System.Security;

using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public static class FileSharing
    {
        public static bool CanSetAccess(FileEntry entry)
        {
            return
                entry != null
                && (entry.RootFolderType == FolderType.COMMON && Global.IsAdministrator
                    || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                        && (entry.RootFolderType == FolderType.USER
                            && (Equals(entry.RootFolderId, Global.FolderMy) || Global.GetFilesSecurity().CanShare(entry))
                            || entry.RootFolderType == FolderType.Privacy
                                && entry is File
                                && (Equals(entry.RootFolderId, Global.FolderPrivacy) || Global.GetFilesSecurity().CanShare(entry))));
        }

        public static List<AceWrapper> GetSharedInfo(FileEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(FilesCommonResource.ErrorMassage_BadRequest);
            if (!CanSetAccess(entry))
            {
                Global.Logger.ErrorFormat("User {0} can't get shared info for {1} {2}", SecurityContext.CurrentAccount.ID, (entry.FileEntryType == FileEntryType.File ? "file" : "folder"), entry.ID);
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
            }

            var linkAccess = FileShare.Restrict;
            var result = new List<AceWrapper>();

            var fileSecurity = Global.GetFilesSecurity();

            var records = fileSecurity
                .GetShares(entry)
                .GroupBy(r => r.Subject)
                .Select(g => g.OrderBy(r => r.Level)
                              .ThenBy(r => r.Level)
                              .ThenByDescending(r => r.Share, new FileShareRecord.ShareComparer()).FirstOrDefault());

            var unknownSubjects = new List<Guid>();

            foreach (var r in records)
            {
                if (r.Subject == FileConstant.ShareLinkId)
                {
                    linkAccess = r.Share;
                    continue;
                }

                if (r.SubjectType == SubjectType.Restriction)
                {
                    continue;
                }

                if (r.SubjectType == SubjectType.ExternalLink)
                {
                    var options = r.Options ?? new FileShareOptions();
                    var expired = options.IsExpired();
                    var lw = new AceWrapper
                    {
                        SubjectId = r.Subject,
                        SubjectName = options.Title,
                        SubjectGroup = true,
                        Share = expired ? FileShare.Restrict : r.Share,
                        Link = FilesSettings.ExternalShare ? entry is File ? FileShareLink.GetLink((File)entry, true, r.Subject) 
                            : FileShareLink.GetLink((Folder)entry, r.Subject) : string.Empty,
                        LinkSettings = new LinkSettingsWrapper
                        {
                            Password = options.Password,
                            ExpirationDate = options.GetExpirationDateStr(),
                            AutoDelete = options.AutoDelete,
                            Expired = expired
                        },
                        EntryType = r.EntryType,
                        Inherited = (entry is File && r.EntryType == FileEntryType.Folder) || (entry is Folder && r.Level > 0)
                    };

                    // only editable format on SaaS trial/startup
                    if (lw.Inherited && entry is File && !FileUtility.CanWebView(entry.Title) && TenantExtra.Saas &&
                        (TenantExtra.GetTenantQuota().Trial || TenantExtra.GetTenantQuota().Free))
                    {
                        continue;
                    }

                    result.Add(lw);
                    continue;
                }

                var u = CoreContext.UserManager.GetUsers(r.Subject);
                var isgroup = false;
                var title = u.DisplayUserName(false);
                var share = r.Share;

                if (u.ID == Constants.LostUser.ID)
                {
                    var g = CoreContext.UserManager.GetGroupInfo(r.Subject);
                    isgroup = true;
                    title = g.Name;

                    if (g.ID == Constants.GroupAdmin.ID)
                        title = FilesCommonResource.Admin;
                    if (g.ID == Constants.GroupEveryone.ID)
                        title = FilesCommonResource.Everyone;

                    if (g.ID == Constants.LostGroupInfo.ID)
                    {
                        unknownSubjects.Add(r.Subject);
                        continue;
                    }
                }
                else if (u.IsVisitor()
                    && new FileShareRecord.ShareComparer().Compare(FileShare.Read, share) > 0)
                {
                    share = FileShare.Read;
                }

                var w = new AceWrapper
                {
                    SubjectId = r.Subject,
                    SubjectName = title,
                    SubjectGroup = isgroup,
                    Share = share,
                    Owner =
                            entry.RootFolderType == FolderType.USER
                                ? entry.RootFolderCreator == r.Subject
                                : entry.CreateBy == r.Subject,
                    LockedRights = r.Subject == SecurityContext.CurrentAccount.ID
                };
                result.Add(w);
            }

            if (unknownSubjects.Any())
            {
                fileSecurity.RemoveSubjects(unknownSubjects);
            }

            if (entry.FileEntryType == FileEntryType.File
                && !((File)entry).Encrypted
                && result.All(w => w.SubjectId != FileConstant.ShareLinkId)
                && (linkAccess != FileShare.Restrict || CoreContext.Configuration.Standalone || !TenantExtra.GetTenantQuota().Trial || FileUtility.CanWebView(entry.Title)))
            {
                var w = new AceWrapper
                {
                    SubjectId = FileConstant.ShareLinkId,
                    Link = FilesSettings.ExternalShare ? FileShareLink.GetLink((File)entry) : string.Empty,
                    SubjectGroup = true,
                    Share = linkAccess,
                    Owner = false
                };
                result.Add(w);
            }

            if (!result.Any(w => w.Owner))
            {
                var ownerId = entry.RootFolderType == FolderType.USER ? entry.RootFolderCreator : entry.CreateBy;
                var w = new AceWrapper
                {
                    SubjectId = ownerId,
                    SubjectName = Global.GetUserName(ownerId),
                    SubjectGroup = false,
                    Share = FileShare.ReadWrite,
                    Owner = true
                };
                result.Add(w);
            }

            if (result.Any(w => w.SubjectId == SecurityContext.CurrentAccount.ID))
            {
                result.Single(w => w.SubjectId == SecurityContext.CurrentAccount.ID).LockedRights = true;
            }

            if (entry.RootFolderType == FolderType.COMMON)
            {
                if (result.All(w => w.SubjectId != Constants.GroupAdmin.ID))
                {
                    var w = new AceWrapper
                    {
                        SubjectId = Constants.GroupAdmin.ID,
                        SubjectName = FilesCommonResource.Admin,
                        SubjectGroup = true,
                        Share = FileShare.ReadWrite,
                        Owner = false,
                        LockedRights = true,
                    };
                    result.Add(w);
                }
                var index = result.FindIndex(w => w.SubjectId == Constants.GroupEveryone.ID);
                if (index == -1)
                {
                    var w = new AceWrapper
                    {
                        SubjectId = Constants.GroupEveryone.ID,
                        SubjectName = FilesCommonResource.Everyone,
                        SubjectGroup = true,
                        Share = fileSecurity.DefaultCommonShare,
                        Owner = false,
                        DisableRemove = true
                    };
                    result.Add(w);
                }
                else
                {
                    result[index].DisableRemove = true;
                }
            }

            return result;
        }

        public static bool SetAceObject(List<AceWrapper> aceWrappers, FileEntry entry, bool notify, string message, AceAdvancedSettingsWrapper advancedSettings)
        {
            if (entry == null) throw new ArgumentNullException(FilesCommonResource.ErrorMassage_BadRequest);
            if (!CanSetAccess(entry)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            var fileSecurity = Global.GetFilesSecurity();
            var ownerId = entry.RootFolderType == FolderType.USER ? entry.RootFolderCreator : entry.CreateBy;
            var entryType = entry.FileEntryType;
            var recipients = new Dictionary<Guid, FileShare>();
            var usersWithoutRight = new List<Guid>();
            var changed = false;

            foreach (var w in aceWrappers.OrderByDescending(ace => ace.SubjectGroup))
            {
                var subjects = fileSecurity.GetUserSubjects(w.SubjectId, w.IsLink);

                if (entry.RootFolderType == FolderType.COMMON && subjects.Contains(Constants.GroupAdmin.ID)
                    || ownerId == w.SubjectId)
                    continue;

                var share = w.Share;

                if (w.IsLink)
                {
                    if (w.Share == FileShare.ReadWrite && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
                        throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

                    // only editable format on personal
                    if (CoreContext.Configuration.Personal && !FileUtility.CanWebView(entry.Title) && w.Share != FileShare.Restrict)
                        throw new SecurityException(FilesCommonResource.ErrorMassage_BadRequest);

                    // only editable format on SaaS trial/startup
                    if (w.Share != FileShare.Restrict && TenantExtra.Saas && (TenantExtra.GetTenantQuota().Trial || TenantExtra.GetTenantQuota().Free) 
                        && !FileUtility.CanWebView(entry.Title) && entry.FileEntryType == FileEntryType.File)
                        throw new SecurityException(FilesCommonResource.ErrorMassage_BadRequest);

                    share = w.Share != FileShare.None && !FilesSettings.ExternalShare
                                ? FileShare.Restrict
                                : w.Share;

                    share = w.SubjectId == FileConstant.ShareLinkId && share == FileShare.Restrict
                                ? FileShare.None
                                : share;
                }

                SubjectType subjectType = SubjectType.UserOrGroup;
                FileShareOptions options = null;

                if (w.IsLink)
                {
                    subjectType = SubjectType.ExternalLink;
                    if (w.LinkSettings != null)
                    {
                        DateTime.TryParse(w.LinkSettings.ExpirationDate, out DateTime expirationDate);
                        options = new FileShareOptions()
                        {
                            Title = w.SubjectName,
                            Password = w.LinkSettings.Password,
                            ExpirationDate = expirationDate,
                            AutoDelete = w.LinkSettings.AutoDelete
                        };
                    }
                }

                fileSecurity.Share(entry.ID, entryType, w.SubjectId, subjectType, share, options);
                changed = true;

                if (w.IsLink)
                {
                    continue;
                }

                entry.Access = share;

                var listUsersId = new List<Guid>();

                if (w.SubjectGroup)
                    listUsersId = CoreContext.UserManager.GetUsersByGroup(w.SubjectId).Select(ui => ui.ID).ToList();
                else
                    listUsersId.Add(w.SubjectId);
                listUsersId.Remove(SecurityContext.CurrentAccount.ID);

                if (entryType == FileEntryType.File)
                {
                    listUsersId.ForEach(uid => FileTracker.ChangeRight(entry.ID, uid, true));
                }

                var addRecipient = share == FileShare.Read
                                   || share == FileShare.CustomFilter
                                   || share == FileShare.ReadWrite
                                   || share == FileShare.Review
                                   || share == FileShare.FillForms
                                   || share == FileShare.Comment
                                   || share == FileShare.None && entry.RootFolderType == FolderType.COMMON;
                var removeNew = share == FileShare.None && entry.RootFolderType == FolderType.USER
                                || share == FileShare.Restrict;
                listUsersId.ForEach(id =>
                                        {
                                            recipients.Remove(id);
                                            if (addRecipient)
                                            {
                                                recipients.Add(id, share);
                                            }
                                            else if (removeNew)
                                            {
                                                usersWithoutRight.Add(id);
                                            }
                                        });
            }

            if (entryType == FileEntryType.File)
            {
                DocumentServiceHelper.CheckUsersForDrop((File)entry);
            }

            if (recipients.Any())
            {
                if (entryType == FileEntryType.File
                    || ((Folder)entry).TotalSubFolders + ((Folder)entry).TotalFiles > 0
                    || entry.ProviderEntry)
                {
                    FileMarker.MarkAsNew(entry, recipients.Keys.ToList());
                }

                if ((entry.RootFolderType == FolderType.USER
                    || entry.RootFolderType == FolderType.Privacy)
                    && notify)
                {
                    NotifyClient.SendShareNotice(entry, recipients, message);
                }
            }

            if (advancedSettings != null && entryType == FileEntryType.File && ownerId == SecurityContext.CurrentAccount.ID && FileUtility.CanWebView(entry.Title) && !entry.ProviderEntry)
            {
                fileSecurity.Share(entry.ID, entryType, FileConstant.DenyDownloadId, SubjectType.Restriction, advancedSettings.DenyDownload ? FileShare.Restrict : FileShare.None);
                fileSecurity.Share(entry.ID, entryType, FileConstant.DenySharingId, SubjectType.Restriction, advancedSettings.DenySharing ? FileShare.Restrict : FileShare.None);
            }

            usersWithoutRight.ForEach(userId => FileMarker.RemoveMarkAsNew(entry, userId));

            return changed;
        }

        public static void RemoveAce(List<FileEntry> entries)
        {
            var fileSecurity = Global.GetFilesSecurity();

            entries.ForEach(
                entry =>
                    {
                        if (entry.RootFolderType != FolderType.USER && entry.RootFolderType != FolderType.Privacy
                            || Equals(entry.RootFolderId, Global.FolderMy)
                            || Equals(entry.RootFolderId, Global.FolderPrivacy))
                            return;

                        var entryType = entry.FileEntryType;
                        fileSecurity.Share(entry.ID, entryType, SecurityContext.CurrentAccount.ID, SubjectType.UserOrGroup,
                            entry.RootFolderType == FolderType.USER
                            ? fileSecurity.DefaultMyShare
                            : fileSecurity.DefaultPrivacyShare);

                        if (entryType == FileEntryType.File)
                        {
                            DocumentServiceHelper.CheckUsersForDrop((File)entry);
                        }

                        FileMarker.RemoveMarkAsNew(entry);
                    });
        }
    }
}