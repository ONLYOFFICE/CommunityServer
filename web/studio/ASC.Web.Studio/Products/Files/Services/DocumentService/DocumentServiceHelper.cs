/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Common.Caching;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Services.DocumentService
{
    public static class DocumentServiceHelper
    {
        public static File GetParams(object fileId, int version, string shareLinkKey, bool itsNew, bool editPossible, bool tryEdit, out DocumentServiceParams docServiceParams)
        {
            File file;

            var lastVersion = true;
            var rightToEdit = true;
            var checkLink = false;

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var fileShare = FileShareLink.Check(shareLinkKey, fileDao, out file);

                switch (fileShare)
                {
                    case FileShare.ReadWrite:
                        checkLink = true;
                        break;
                    case FileShare.Read:
                        editPossible = false;
                        rightToEdit = false;
                        checkLink = true;
                        break;
                }

                if (file == null)
                {
                    var curFile = fileDao.GetFile(fileId);

                    if (curFile != null && 0 < version && version < curFile.Version)
                    {
                        file = fileDao.GetFile(fileId, version);
                        lastVersion = false;
                    }
                    else
                    {
                        file = curFile;
                    }
                }
            }
            return GetParams(file, lastVersion, checkLink, itsNew, editPossible, rightToEdit, tryEdit, out docServiceParams);
        }

        public static File GetParams(File file, bool lastVersion, bool checkLink, bool itsNew, bool editPossible, bool rightToEdit, bool tryEdit, out DocumentServiceParams docServiceParams)
        {
            if (!TenantExtra.GetTenantQuota().DocsEdition) throw new Exception(FilesCommonResource.ErrorMassage_PayTariffDocsEdition);

            if (!checkLink && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                rightToEdit = false;
            }

            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);

            if (!string.IsNullOrEmpty(file.Error)) throw new Exception(file.Error);

            if (!checkLink)
            {
                rightToEdit = rightToEdit && Global.GetFilesSecurity().CanEdit(file);
                if (editPossible && !rightToEdit)
                {
                    editPossible = false;
                }

                if (!editPossible && !Global.GetFilesSecurity().CanRead(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
            }

            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            if (file.ContentLength > SetupInfo.AvailableFileSize) throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeEdit, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));

            rightToEdit = rightToEdit && !EntryManager.FileLockedForMe(file.ID);
            if (editPossible && !rightToEdit)
            {
                editPossible = false;
            }

            rightToEdit = rightToEdit && (!FileTracker.IsEditing(file.ID)
                || FileUtility.CanCoAuhtoring(file.Title) && !FileTracker.IsEditingAlone(file.ID));
            if (editPossible && !rightToEdit)
            {
                editPossible = false;
            }

            rightToEdit = rightToEdit && FileUtility.CanWebEdit(file.Title);
            if (editPossible && !rightToEdit)
            {
                editPossible = false;
            }

            if (!editPossible && !FileUtility.CanWebView(file.Title)) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

            var versionForKey = file.Version;

            //CreateNewDoc
            if ((itsNew || FileTracker.FixedVersion(file.ID)) && file.Version == 1 && file.CreateOn == file.ModifiedOn)
            {
                versionForKey = 0;
            }

            var docKey = GetDocKey(file.ID, versionForKey, file.ProviderEntry ? file.ModifiedOn : file.CreateOn);
            var modeWrite = editPossible && tryEdit;

            docServiceParams = new DocumentServiceParams
                {
                    File = file,
                    Key = docKey,
                    CanEdit = rightToEdit && lastVersion,
                    ModeWrite = modeWrite,
                };

            return file;
        }

        public static string GetDocKey(object fileId, int fileVersion, DateTime modified)
        {
            var str = String.Format("teamlab_{0}_{1}_{2}_{3}",
                                    fileId,
                                    fileVersion,
                                    modified.GetHashCode(),
                                    Global.GetDocDbKey());

            var keyDoc = Encoding.UTF8.GetBytes(str)
                                 .ToList()
                                 .Concat(MachinePseudoKeys.GetMachineConstant())
                                 .ToArray();

            return DocumentServiceConnector.GenerateRevisionId(Hasher.Base64Hash(keyDoc, HashAlg.SHA256));
        }

        public static void CheckUsersForDrop(File file, Guid userId)
        {
            //??? how distinguish auth user via sharelink
            if (Global.GetFilesSecurity().CanEdit(file, FileConstant.ShareLinkId)) return;

            var usersDrop = new List<Guid>();
            if (userId.Equals(Guid.Empty))
            {
                usersDrop = FileTracker.GetEditingBy(file.ID).Where(uid => !Global.GetFilesSecurity().CanEdit(file, uid)).ToList();
            }
            else
            {
                if (!FileTracker.GetEditingBy(file.ID).Contains(userId)) return;
                if (Global.GetFilesSecurity().CanEdit(file, userId)) return;

                usersDrop.Add(userId);
            }

            var versionForKey = file.Version;

            //NewDoc
            if (FileTracker.FixedVersion(file.ID) && file.Version == 1 && file.CreateOn == file.ModifiedOn)
            {
                versionForKey = 0;
            }

            var docKey = GetDocKey(file.ID, versionForKey, file.ProviderEntry ? file.ModifiedOn : file.CreateOn);
            DocumentServiceTracker.Drop(docKey, usersDrop, file.ID);
        }


        private static readonly ICache CacheUri = AscCache.Memory;

        public static string GetExternalUri(File file)
        {
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                using (var fileStream = fileDao.GetFileStream(file))
                {
                    var docKey = GetDocKey(file.ID, file.Version, file.ModifiedOn);

                    var uri = CacheUri.Get<string>(docKey);
                    if (string.IsNullOrEmpty(uri))
                    {
                        uri = DocumentServiceConnector.GetExternalUri(fileStream, MimeMapping.GetMimeMapping(file.Title), docKey);
                    }
                    CacheUri.Insert(docKey, uri, DateTime.UtcNow.Add(TimeSpan.FromSeconds(2)));
                    return uri;
                }
            }
            catch (Exception exception)
            {
                Global.Logger.Error("Get external uri: ", exception);
            }
            return null;
        }

        public static bool HaveExternalIP()
        {
            if (!CoreContext.Configuration.Standalone)
                return true;

            var checkExternalIp = FilesSettings.CheckHaveExternalIP;
            if (checkExternalIp.Value.AddDays(5) >= DateTime.UtcNow)
            {
                return checkExternalIp.Key;
            }

            string convertUri;
            try
            {
                const string toExtension = ".docx";
                var fileExtension = FileUtility.GetInternalExtension(toExtension);
                var storeTemplate = Global.GetStoreTemplate();
                var fileUri = storeTemplate.GetUri("", FileConstant.NewDocPath + "default/new" + fileExtension).ToString();

                DocumentServiceConnector.GetConvertedUri(CommonLinkUtility.GetFullAbsolutePath(fileUri), fileExtension, toExtension, Guid.NewGuid().ToString(), false, out convertUri);
            }
            catch
            {
                convertUri = string.Empty;
            }

            var result = !string.IsNullOrEmpty(convertUri);
            Global.Logger.Info("HaveExternalIP result " + result);
            FilesSettings.CheckHaveExternalIP = new KeyValuePair<bool, DateTime>(result, DateTime.UtcNow);
            return result;
        }
    }
}