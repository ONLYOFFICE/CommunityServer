/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using JWT;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Services.DocumentService
{
    public static class DocumentServiceHelper
    {
        public static File GetParams(object fileId, int version, string doc, bool editPossible, bool tryEdit, bool tryCoauth, out Configuration configuration)
        {
            File file;

            var lastVersion = true;
            FileShare linkRight;

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                linkRight = FileShareLink.Check(doc, fileDao, out file);

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
            return GetParams(file, lastVersion, linkRight, true, true, editPossible, tryEdit, tryCoauth, out configuration);
        }

        public static File GetParams(File file, bool lastVersion, FileShare linkRight, bool rightToRename, bool rightToEdit, bool editPossible, bool tryEdit, bool tryCoauth, out Configuration configuration)
        {
            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            if (!string.IsNullOrEmpty(file.Error)) throw new Exception(file.Error);

            var rightToReview = rightToEdit;
            var reviewPossible = editPossible;

            var rightToFillForms = rightToEdit;
            var fillFormsPossible = editPossible;

            var rightToComment = rightToEdit;
            var commentPossible = editPossible;

            if (linkRight == FileShare.Restrict && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                rightToEdit = false;
                rightToReview = false;
                rightToFillForms = false;
                rightToComment = false;
            }

            var fileSecurity = Global.GetFilesSecurity();
            rightToEdit = rightToEdit
                          && (linkRight == FileShare.ReadWrite
                              || fileSecurity.CanEdit(file));
            if (editPossible && !rightToEdit)
            {
                editPossible = false;
            }

            rightToRename = rightToRename && rightToEdit && fileSecurity.CanEdit(file);

            rightToReview = rightToReview
                            && (linkRight == FileShare.Review || linkRight == FileShare.ReadWrite
                                || fileSecurity.CanReview(file));
            if (reviewPossible && !rightToReview)
            {
                reviewPossible = false;
            }

            rightToFillForms = rightToFillForms
                            && (linkRight == FileShare.FillForms || linkRight == FileShare.Review || linkRight == FileShare.ReadWrite
                                || fileSecurity.CanFillForms(file));
            if (fillFormsPossible && !rightToFillForms)
            {
                fillFormsPossible = false;
            }

            rightToComment = rightToComment
                            && (linkRight == FileShare.Comment || linkRight == FileShare.Review || linkRight == FileShare.ReadWrite
                                || fileSecurity.CanComment(file));
            if (commentPossible && !rightToComment)
            {
                commentPossible = false;
            }

            if (linkRight == FileShare.Restrict
                && !(editPossible || reviewPossible || fillFormsPossible || commentPossible)
                && !fileSecurity.CanRead(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            if (file.ContentLength > SetupInfo.AvailableFileSize) throw new Exception(string.Format(FilesCommonResource.ErrorMassage_FileSizeEdit, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize)));

            string strError = null;
            if ((editPossible || reviewPossible || fillFormsPossible || commentPossible)
                && EntryManager.FileLockedForMe(file.ID))
            {
                if (tryEdit)
                {
                strError = FilesCommonResource.ErrorMassage_LockedFile;
                }
                rightToRename = false;
                rightToEdit = editPossible = false;
                rightToReview = reviewPossible = false;
                rightToFillForms = fillFormsPossible = false;
                rightToComment = commentPossible = false;
            }

            if (editPossible
                && !FileUtility.CanWebEdit(file.Title))
            {
                rightToEdit = editPossible = false;
            }

            if (!editPossible && !FileUtility.CanWebView(file.Title)) throw new Exception(string.Format("{0} ({1})", FilesCommonResource.ErrorMassage_NotSupportedFormat, FileUtility.GetFileExtension(file.Title)));

            if (reviewPossible &&
                !FileUtility.CanWebReview(file.Title))
            {
                rightToReview = reviewPossible = false;
            }

            if (fillFormsPossible &&
                !FileUtility.CanWebRestrictedEditing(file.Title))
            {
                rightToFillForms = fillFormsPossible = false;
            }

            if (commentPossible &&
                !FileUtility.CanWebComment(file.Title))
            {
                rightToComment = commentPossible = false;
            }

            var rightChangeHistory = rightToEdit;

            if (FileTracker.IsEditing(file.ID))
            {
                rightChangeHistory = false;

                bool coauth;
                if ((editPossible || reviewPossible || fillFormsPossible || commentPossible)
                    && tryCoauth
                    && (!(coauth = FileUtility.CanCoAuhtoring(file.Title)) || FileTracker.IsEditingAlone(file.ID)))
                {
                    if (tryEdit)
                    {
                        var editingBy = FileTracker.GetEditingBy(file.ID).FirstOrDefault();
                        strError = string.Format(!coauth
                                                     ? FilesCommonResource.ErrorMassage_EditingCoauth
                                                     : FilesCommonResource.ErrorMassage_EditingMobile,
                                                 Global.GetUserName(editingBy, true));
                    }
                    rightToEdit = editPossible = reviewPossible = fillFormsPossible = commentPossible = false;
                }
            }

            var docKey = GetDocKey(file);
            var modeWrite = (editPossible || reviewPossible || fillFormsPossible || commentPossible) && tryEdit;

            configuration = new Configuration(file)
                {
                    Document =
                        {
                            Key = docKey,
                            Permissions =
                                {
                                    Edit = rightToEdit && lastVersion,
                                    Rename = rightToRename && lastVersion && !file.ProviderEntry,
                                    Review = rightToReview && lastVersion,
                                    FillForms= rightToFillForms && lastVersion,
                                    Comment = rightToComment && lastVersion,
                                    ChangeHistory = rightChangeHistory,
                                }
                        },
                    EditorConfig =
                        {
                            ModeWrite = modeWrite,
                        },
                    ErrorMessage = strError,
                };

            if (!lastVersion)
            {
                configuration.Document.Title += string.Format(" ({0})", file.CreateOnString);
            }

            return file;
        }


        public static string GetSignature(object payload)
        {
            if (string.IsNullOrEmpty(FileUtility.SignatureSecret)) return null;

            JsonWebToken.JsonSerializer = new Web.Core.Files.DocumentService.JwtSerializer();
            return JsonWebToken.Encode(payload, FileUtility.SignatureSecret, JwtHashAlgorithm.HS256);
        }


        public static string GetDocKey(File file)
        {
            return GetDocKey(file.ID, file.Version, file.ProviderEntry ? file.ModifiedOn : file.CreateOn);
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


        public static void CheckUsersForDrop(File file)
        {
            var fileSecurity = Global.GetFilesSecurity();
            var sharedLink =
                fileSecurity.CanEdit(file, FileConstant.ShareLinkId)
                || fileSecurity.CanReview(file, FileConstant.ShareLinkId)
                || fileSecurity.CanFillForms(file, FileConstant.ShareLinkId)
                || fileSecurity.CanComment(file, FileConstant.ShareLinkId);

            var usersDrop = FileTracker.GetEditingBy(file.ID)
                                       .Where(uid =>
                                           {
                                               if (!CoreContext.UserManager.UserExists(uid))
                                               {
                                                   return !sharedLink;
                                               }
                                               return !fileSecurity.CanEdit(file, uid) && !fileSecurity.CanReview(file, uid) && !fileSecurity.CanFillForms(file, uid) && !fileSecurity.CanComment(file, uid);
                                           })
                                       .Select(u => u.ToString()).ToArray();

            if (!usersDrop.Any()) return;
            var docKey = GetDocKey(file);
            DropUser(docKey, usersDrop, file.ID);
        }

        public static bool DropUser(string docKeyForTrack, string[] users, object fileId = null)
        {
            return DocumentServiceConnector.Command(Web.Core.Files.DocumentService.CommandMethod.Drop, docKeyForTrack, fileId, null, users);
        }

        public static bool RenameFile(File file)
        {
            if (!FileUtility.CanWebView(file.Title) && !FileUtility.CanWebEdit(file.Title) && !FileUtility.CanWebReview(file.Title) && !FileUtility.CanWebRestrictedEditing(file.Title) && !FileUtility.CanWebComment(file.Title)) return true;
            var docKeyForTrack = GetDocKey(file);
            var meta = new Web.Core.Files.DocumentService.MetaData { Title = file.Title };
            return DocumentServiceConnector.Command(Web.Core.Files.DocumentService.CommandMethod.Meta, docKeyForTrack, file.ID, meta: meta);
        }
    }
}