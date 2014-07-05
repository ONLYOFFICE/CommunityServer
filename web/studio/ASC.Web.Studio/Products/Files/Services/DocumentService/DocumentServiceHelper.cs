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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Caching;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;
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

                    if (0 < version && version < curFile.Version)
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
            if (!string.IsNullOrEmpty(file.Error))
            {
                throw new Exception(file.Error);
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

            rightToEdit = rightToEdit && !EntryManager.FileLockedForMe(file.ID);
            if (editPossible && !rightToEdit)
            {
                editPossible = false;
            }

            rightToEdit = rightToEdit && ((file.FileStatus & FileStatus.IsEditing) != FileStatus.IsEditing || FileUtility.CanCoAuhtoring(file.Title));
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

            if (!FileTracker.FixedVersion(file.ID))
                versionForKey++;

            //CreateNewDoc
            if (itsNew && file.Version == 1 && file.ConvertedType != null && file.CreateOn == file.ModifiedOn)
            {
                versionForKey = 1;
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


        private static readonly ICache CacheUri = new AspCache();

        public static string GetExternalUri(File file)
        {
            try
            {
                using (var fileDao = Global.DaoFactory.GetFileDao())
                using (var fileStream = fileDao.GetFileStream(file))
                {
                    var docKey = GetDocKey(file.ID, file.Version, file.ModifiedOn);

                    var uri = CacheUri.Get(docKey) as string;
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
                const string toExtension = ".xlsx";
                var fileExtension = FileUtility.GetInternalExtension(toExtension);
                var store = Global.GetStoreTemplate();
                var fileUri = store.GetUri("", "new" + fileExtension).ToString();

                DocumentServiceConnector.GetConvertedUri(CommonLinkUtility.GetFullAbsolutePath(fileUri), fileExtension, toExtension, Guid.NewGuid().ToString(), false, out convertUri);
            }
            catch
            {
                convertUri = string.Empty;
            }

            var result = !string.IsNullOrEmpty(convertUri);
            FilesSettings.CheckHaveExternalIP = new KeyValuePair<bool, DateTime>(result, DateTime.UtcNow);
            return result;
        }
    }
}