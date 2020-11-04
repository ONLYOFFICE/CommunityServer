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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Core;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;

using Autofac;

using Constants = ASC.Core.Configuration.Constants;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Classes
{
    public class Global
    {
        private static readonly object Locker = new object();
        private static bool isInit;
        public static readonly ICacheNotify Notify = AscCache.Notify;

        static Global()
        {
            Init();
        }

        internal static void Init()
        {
            try
            {
                if (isInit) return;

                lock (Locker)
                {
                    if (isInit) return;

                    DIHelper.Register();

                    var container = DIHelper.Resolve();

                    IDaoFactory factory;
                    if (!container.TryResolve(out factory))
                    {
                        factory = new DaoFactory();
                        Logger.Fatal("Could not resolve IDaoFactory instance. Using default DaoFactory instead.");
                    }

                    IFileStorageService storageService;
                    if (!container.TryResolve(out storageService))
                    {
                        storageService = new FileStorageServiceController();
                        Logger.Fatal("Could not resolve IFileStorageService instance. Using default FileStorageServiceController instead.");
                    }

                    DaoFactory = factory;
                    FileStorageService = storageService;
                    SocketManager = new SocketManager();
                    if (CoreContext.Configuration.Standalone)
                    {
                        ClearCache();
                    }

                    isInit = true;
                }
            }
            catch (Exception error)
            {
                Logger.Fatal("Could not resolve IDaoFactory instance. Using default DaoFactory instead.", error);
                DaoFactory = new DaoFactory();
                FileStorageService = new FileStorageServiceController();
            }
        }

        private static void ClearCache()
        {
            try
            {
                Notify.Subscribe<AscCacheItem>((item, action) =>
                {
                    try
                    {
                        ProjectsRootFolderCache.Clear();
                        UserRootFolderCache.Clear();
                        CommonFolderCache.Clear();
                        ShareFolderCache.Clear();
                        RecentFolderCache.Clear();
                        FavoritesFolderCache.Clear();
                        TemplatesFolderCache.Clear();
                        PrivacyFolderCache.Clear();
                        TrashFolderCache.Clear();
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal("ClearCache action", e);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Fatal("ClearCache subscribe", e);
            }
        }

        #region Property

        public const int MaxTitle = 170;

        public static readonly Regex InvalidTitleChars = new Regex("[\t*\\+:\"<>?|\\\\/\\p{Cs}]");

        public static bool EnableUploadFilter
        {
            get { return Boolean.TrueString.Equals(ConfigurationManagerExtension.AppSettings["files.upload-filter"] ?? "false", StringComparison.InvariantCultureIgnoreCase); }
        }

        public static TimeSpan StreamUrlExpire
        {
            get
            {
                int validateTimespan;
                int.TryParse(ConfigurationManagerExtension.AppSettings["files.stream-url-minute"], out validateTimespan);
                if (validateTimespan <= 0) validateTimespan = 16;
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
            protected internal set
            {
                var cacheKey = string.Format("my/{0}/{1}", TenantProvider.CurrentTenantID, value);
                UserRootFolderCache.Remove(cacheKey);
            }
        }

        public static bool IsFirstVisit()
        {
            var cacheKey = string.Format("my/{0}/{1}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID);

            object myFolderId;

            if (!UserRootFolderCache.TryGetValue(cacheKey, out myFolderId))
            {
                using (var folderDao = DaoFactory.GetFolderDao())
                {
                    myFolderId = folderDao.GetFolderIDUser(false);

                    if (Equals(myFolderId, 0))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private static readonly IDictionary<int, object> RecentFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderRecent
        {
            get
            {
                if (!SecurityContext.IsAuthenticated) return 0;
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return 0;

                object recentFolderId;
                if (!RecentFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out recentFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                    {
                        recentFolderId = folderDao.GetFolderIDRecent(true);
                    }

                    if (!recentFolderId.Equals(0))
                        RecentFolderCache[TenantProvider.CurrentTenantID] = recentFolderId;
                }

                return recentFolderId;
            }
        }

        private static readonly IDictionary<int, object> FavoritesFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderFavorites
        {
            get
            {
                if (!SecurityContext.IsAuthenticated) return 0;
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return 0;

                object favoriteFolderId;
                if (!FavoritesFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out favoriteFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                    {
                        favoriteFolderId = folderDao.GetFolderIDFavorites(true);
                    }

                    if (!favoriteFolderId.Equals(0))
                        FavoritesFolderCache[TenantProvider.CurrentTenantID] = favoriteFolderId;
                }

                return favoriteFolderId;
            }
        }

        private static readonly IDictionary<int, object> TemplatesFolderCache =
            new ConcurrentDictionary<int, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderTemplates
        {
            get
            {
                if (!SecurityContext.IsAuthenticated) return 0;
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return 0;

                object templatesFolderId;
                if (!TemplatesFolderCache.TryGetValue(TenantProvider.CurrentTenantID, out templatesFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                    {
                        templatesFolderId = folderDao.GetFolderIDTemplates(true);
                    }

                    if (!templatesFolderId.Equals(0))
                        TemplatesFolderCache[TenantProvider.CurrentTenantID] = templatesFolderId;
                }

                return templatesFolderId;
            }
        }

        private static readonly IDictionary<string, object> PrivacyFolderCache =
            new ConcurrentDictionary<string, object>(); /*Use SYNCHRONIZED for cross thread blocks*/

        public static object FolderPrivacy
        {
            get
            {
                if (!SecurityContext.IsAuthenticated) return 0;
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) return 0;

                var cacheKey = string.Format("privacy/{0}/{1}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID);

                object privacyFolderId;
                if (!PrivacyFolderCache.TryGetValue(cacheKey, out privacyFolderId))
                {
                    using (var folderDao = DaoFactory.GetFolderDao())
                    {
                        privacyFolderId = folderDao.GetFolderIDPrivacy(true);
                    }

                    if (!Equals(privacyFolderId, 0))
                        PrivacyFolderCache[cacheKey] = privacyFolderId;
                }
                return privacyFolderId;
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
            protected internal set
            {
                var cacheKey = string.Format("trash/{0}/{1}", TenantProvider.CurrentTenantID, value);
                TrashFolderCache.Remove(cacheKey);
            }
        }

        #endregion

        #endregion

        public static ILog Logger
        {
            get { return LogManager.GetLogger("ASC.Files"); }
        }

        public static IDaoFactory DaoFactory { get; private set; }

        public static IFileStorageService FileStorageService { get; private set; }

        public static SocketManager SocketManager { get; private set; }

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

        public static string GetUserName(Guid userId, bool alive = false)
        {
            if (userId.Equals(SecurityContext.CurrentAccount.ID)) return FilesCommonResource.Author_Me;
            if (userId.Equals(Constants.Guest.ID)) return FilesCommonResource.Guest;

            var userInfo = CoreContext.UserManager.GetUsers(userId);
            if (userInfo.Equals(ASC.Core.Users.Constants.LostUser)) return alive ? FilesCommonResource.Guest : CustomNamingPeople.Substitute<FilesCommonResource>("ProfileRemoved");

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

                return id;
            }
        }

        private static void SaveStartDocument(IFolderDao folderDao, IFileDao fileDao, object folderId, string path, IDataStore storeTemplate)
        {
            foreach (var file in storeTemplate.ListFilesRelative("", path, "*", false))
            {
                SaveFile(fileDao, folderId, path + file, storeTemplate);
            }

            foreach (var folderName in storeTemplate.ListDirectoriesRelative(path, false))
            {
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
                    ContentLength = stream.CanSeek ? stream.Length : storeTemp.GetFileSize("", filePath),
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


        public static long GetUserUsedSpace()
        {
            return GetUserUsedSpace(SecurityContext.CurrentAccount.ID);
        }

        public static long GetUserUsedSpace(Guid userId)
        {
            var spaceUsageManager = new FilesSpaceUsageStatManager() as IUserSpaceUsage;

            return spaceUsageManager.GetUserSpaceUsage(userId);
        }
    }
}