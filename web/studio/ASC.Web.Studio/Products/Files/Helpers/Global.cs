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
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Data.Storage.S3;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using Constants = ASC.Core.Configuration.Constants;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Classes
{
    public class Global
    {
        static Global()
        {
            try
            {
                var container = new UnityContainer();
                container.LoadConfiguration("files");
                DaoFactory = container.Resolve<IDaoFactory>() ?? new DaoFactory();
            }
            catch (Exception error)
            {
                Logger.Fatal("Could not resolve IDaoFactory instance. Using default DaoFactory instead.", error);
                DaoFactory = new DaoFactory();
            }
        }

        #region Property

        public const int MaxTitle = 170;

        public static readonly Regex InvalidTitleChars = new Regex("[\t*\\+:\"<>?|\\\\/]");

        public static bool EnableUploadFilter
        {
            get { return Boolean.TrueString.Equals(WebConfigurationManager.AppSettings["files.upload-filter"] ?? "false", StringComparison.InvariantCultureIgnoreCase); }
        }

        public static TimeSpan StreamUrlExpire
        {
            get
            {
                int validateTimespan;
                int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validateTimespan);
                if (validateTimespan <= 0) validateTimespan = 5;
                return TimeSpan.FromMinutes(validateTimespan);
            }
        }

        public static bool IsAdministrator
        {
            get { return FileSecurity.IsAdministrator(SecurityContext.CurrentAccount.ID); }
        }

        public static bool IsOutsider
        {
            get { return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider(); }
        }

        public static string GetDocDbKey()
        {
            const string dbKey = "UniqueDocument";
            var resultKey = CoreContext.Configuration.GetSetting(dbKey);

            if (!String.IsNullOrEmpty(resultKey)) return resultKey;

            resultKey = Guid.NewGuid().ToString();
            CoreContext.Configuration.SaveSetting(dbKey, resultKey);

            return resultKey;
        }

        #region GlobalFolderID

        private static readonly IDictionary<int, object> ProjectsRootFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderProjects
        {
            get
            {
                if (CoreContext.Configuration.Personal) return null;

                if (WebItemManager.Instance[WebItemManager.ProjectsProductID].IsDisabled()) return null;

                using (var folderDao = DaoFactory.GetFolderDao())
                {
                    object result;
                    if (!ProjectsRootFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out result))
                    {
                        result = folderDao.GetFolderIDProjects(true);

                        ProjectsRootFolderCache[TenantProvider.CurrentTenantID] = result;
                    }

                    return result;
                }
            }
        }

        private static readonly IDictionary<string, object> UserRootFolderCache =
            new ConcurrentDictionary<string, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderMy
        {
            get
            {
                if (!SecurityContext.IsAuthenticated) return 0;
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return 0;

                var cacheKey = string.Format("my/{0}/{1}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID);

                object myFolderId;
                if (!UserRootFolderCache.TryGetValue(cacheKey, out myFolderId))
                {
                    myFolderId = GetFolderIdAndProccessFirstVisit(true);
                    if (!Equals(myFolderId, 0))
                        UserRootFolderCache[cacheKey] = myFolderId;
                }
                return myFolderId;
            }
        }

        private static readonly IDictionary<int, object> CommonFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderCommon
        {
            get
            {
                if (CoreContext.Configuration.Personal) return null;

                object commonFolderId;
                if (!CommonFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out commonFolderId))
                {
                    commonFolderId = GetFolderIdAndProccessFirstVisit(false);
                    if (!Equals(commonFolderId, 0))
                        CommonFolderCache[TenantProvider.CurrentTenantID] = commonFolderId;
                }
                return commonFolderId;
            }
        }

        private static readonly IDictionary<int, object> ShareFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderShare
        {
            get
            {
                if (CoreContext.Configuration.Personal) return null;
                if (IsOutsider) return null;

                object sharedFolderId;
                if (!ShareFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out sharedFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                    {
                        sharedFolderId = folderDao.GetFolderIDShare(true);
                    }

                    if (!sharedFolderId.Equals(0))
                        ShareFolderCache[TenantProvider.CurrentTenantID] = sharedFolderId;
                }

                return sharedFolderId;
            }
        }

        private static readonly IDictionary<string, object> TrashFolderCache =
            new ConcurrentDictionary<string, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderTrash
        {
            get
            {
                if (IsOutsider) return null;

                var cacheKey = string.Format("trash/{0}/{1}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID);

                object trashFolderId;
                if (!TrashFolderCache.TryGetValue(cacheKey, out trashFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                        trashFolderId = SecurityContext.IsAuthenticated ? folderDao.GetFolderIDTrash(true) : 0;
                    TrashFolderCache[cacheKey] = trashFolderId;
                }
                return trashFolderId;
            }
        }

        #endregion

        #endregion

        public static ILog Logger
        {
            get { return LogManager.GetLogger("ASC.Files"); }
        }

        public static IDaoFactory DaoFactory { get; private set; }

        public static IDataStore GetStore(bool currentTenant = true)
        {
            return StorageFactory.GetStorage(currentTenant ? TenantProvider.CurrentTenantID.ToString() : string.Empty, FileConstant.StorageModule);
        }

        public static IDataStore GetStoreTemplate()
        {
            return StorageFactory.GetStorage(String.Empty, FileConstant.StorageTemplate);
        }

        public static FileSecurity GetFilesSecurity()
        {
            return new FileSecurity(DaoFactory);
        }

        public static string ReplaceInvalidCharsAndTruncate(string title)
        {
            if (String.IsNullOrEmpty(title)) return title;
            title = title.Trim();
            if (MaxTitle < title.Length)
            {
                var pos = title.LastIndexOf('.');
                if (MaxTitle - 20 < pos)
                {
                    title = title.Substring(0, MaxTitle - (title.Length - pos)) + title.Substring(pos);
                }
                else
                {
                    title = title.Substring(0, MaxTitle);
                }
            }
            return InvalidTitleChars.Replace(title, "_");
        }

        public static string GetUserName(Guid userId)
        {
            if (userId.Equals(SecurityContext.CurrentAccount.ID)) return FilesCommonResource.Author_Me;
            if (userId.Equals(Constants.Guest.ID)) return FilesCommonResource.Guest;

            var userInfo = CoreContext.UserManager.GetUsers(userId);
            if (userInfo.Equals(ASC.Core.Users.Constants.LostUser)) return CustomNamingPeople.Substitute<FilesCommonResource>("ProfileRemoved");

            return userInfo.DisplayUserName(false);
        }

        #region Generate start documents

        private static object GetFolderIdAndProccessFirstVisit(bool my)
        {
            using (var folderDao = DaoFactory.GetFolderDao())
            using (var fileDao = DaoFactory.GetFileDao())
            {
                var id = my ? folderDao.GetFolderIDUser(false) : folderDao.GetFolderIDCommon(false);

                if (Equals(id, 0)) //TODO: think about 'null'
                {
                    var isWarmup = HttpContext.Current != null && HttpContext.Current.Request.QueryString["warmup"] == "true" && !WarmUpController.Instance.CheckCompleted();

                    if ((!CoreContext.Configuration.Standalone || !isWarmup))
                {
                    id = my ? folderDao.GetFolderIDUser(true) : folderDao.GetFolderIDCommon(true);

                    //Copy start document
                    if (AdditionalWhiteLabelSettings.Instance.StartDocsEnabled)
                    {
                        try
                        {
                            var storeTemplate = GetStoreTemplate();

                            var culture = my ? CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture() : CoreContext.TenantManager.GetCurrentTenant().GetCulture();
                            var path = FileConstant.StartDocPath + culture + "/";

                            if (!storeTemplate.IsDirectory(path))
                                path = FileConstant.StartDocPath + "default/";
                            path += my ? "my/" : "corporate/";

                            SaveStartDocument(folderDao, fileDao, id, path, storeTemplate);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
                }

                return id;
            }
        }

        private static void SaveStartDocument(IFolderDao folderDao, IFileDao fileDao, object folderId, string path, IDataStore storeTemplate)
        {
            foreach (var file in storeTemplate.ListFilesRelative("", path, "*", false))
            {
                SaveFile(fileDao, folderId, path + file, storeTemplate);
            }

            if (storeTemplate is S3Storage) return;

            foreach (var folderUri in storeTemplate.List(path, false))
            {
                var folderName = Path.GetFileName(folderUri.ToString());

                var subFolderId = folderDao.SaveFolder(new Folder
                    {
                        Title = folderName,
                        ParentFolderID = folderId
                    });

                SaveStartDocument(folderDao, fileDao, subFolderId, path + folderName + "/", storeTemplate);
            }
        }

        private static void SaveFile(IFileDao fileDao, object folder, string filePath, IDataStore storeTemp)
        {
            using (var stream = storeTemp.GetReadStream("", filePath))
            {
                var fileName = Path.GetFileName(filePath);
                var file = new File
                    {
                        Title = fileName,
                        ContentLength = stream.Length,
                        FolderID = folder,
                        Comment = FilesCommonResource.CommentCreate,
                    };
                stream.Position = 0;
                try
                {
                    file = fileDao.SaveFile(file, stream);

                    FileMarker.MarkAsNew(file);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        #endregion
    }
}