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
using System.Collections;
using System.Web;
using ASC.Data.Storage;
using ASC.Forum;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

namespace ASC.Web.UserControls.Forum.Common
{
    public class ForumManager
    {
        private class SecurityActionValidator : ISecurityActionView
        {
            #region ISecurityActionView Members

            public bool IsAccessible { get; set; }

            public event EventHandler<SecurityAccessEventArgs> ValidateAccess;

            #endregion

            public void OnValidate(SecurityAccessEventArgs e)
            {
                if (ValidateAccess != null)
                    ValidateAccess(this, e);
            }
        }

        internal static string ForumScriptKey { get { return "__forum_core_script"; } }
        internal static string SearchHelperScriptKey { get { return "__searchhelper_core_script"; } }

        private static object _syncObj = new object();

        private static Hashtable _settingsCollection;

        private SecurityActionValidator _securityValidator;

        public IPresenterFactory PresenterFactory { get; private set; }

        public Settings Settings { get; private set; }

        internal ForumManager(Settings settings)
        {
            SessionKeys = new ForumSessionKeys(settings.ID);
            Settings = settings;
            PresenterFactory = new ForumPresenterFactory();

            _securityValidator = new SecurityActionValidator();
            IPresenter securityPresenter = PresenterFactory.GetPresenter<ISecurityActionView>();
            securityPresenter.SetView(_securityValidator);
        }

        static ForumManager()
        {
            _settingsCollection = Hashtable.Synchronized(new Hashtable());
        }

        public static ForumManager GetForumManager(Guid settingsID)
        {
            lock (_syncObj)
            {
                if (_settingsCollection.ContainsKey(settingsID))
                    return (_settingsCollection[settingsID] as Settings).ForumManager;
            }

            return null;
        }


        public bool ValidateAccessSecurityAction(ForumAction forumAction, object targetObject)
        {
            _securityValidator.OnValidate(new SecurityAccessEventArgs(forumAction, targetObject));
            return _securityValidator.IsAccessible;
        }


        public string GetTopicImage(Topic topic)
        {
            var isNew = topic.IsNew();
            if (!topic.Sticky)
            {
                if (topic.Type == TopicType.Informational && isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_new_closed.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_new.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && !isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_closed.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && !isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_new_closed.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_new.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && !isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_closed.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && !isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll.png", Settings.ImageItemID);
            }
            else
            {
                if (topic.Type == TopicType.Informational && isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_new_closed_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_new_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && !isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_closed_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Informational && !isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("top_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_new_closed_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_new_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && !isNew && topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_closed_sticky.png", Settings.ImageItemID);

                else if (topic.Type == TopicType.Poll && !isNew && !topic.Closed)
                    return WebImageSupplier.GetAbsoluteWebPath("poll_sticky.png", Settings.ImageItemID);
            }

            return "";

        }

        internal IDataStore GetStore()
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), this.Settings.FileStoreModuleID);
        }

        #region Remove and Find Attachments

        public string GetAttachmentVirtualDirPath(Thread thread, Guid settingsID, Guid userID, out string offsetPhysicalPath)
        {
            offsetPhysicalPath = thread.CategoryID + "\\" + thread.ID + "\\" + userID.ToString();
            return thread.CategoryID + "/" + thread.ID + "/" + userID.ToString();
        }

        public string GetAttachmentWebPath(Attachment attachment)
        {
            return GetStore().GetUri(attachment.OffsetPhysicalPath).ToString();
        }

        public void RemoveAttachments(string offsetPhysicalPath)
        {
            try
            {
                var store = GetStore();
                store.Delete(offsetPhysicalPath);
            }
            catch { };
        }

        public void RemoveAttachments(Post post)
        {
            if (post.Attachments == null)
                return;

            foreach (var attachment in post.Attachments)
            {
                RemoveAttachments(attachment.OffsetPhysicalPath);
            }
        }

        public void RemoveAttachments(string[] attachmentPaths)
        {
            foreach (var offsetPath in attachmentPaths)
            {
                RemoveAttachments(offsetPath);
            }
        }

        public void RemoveAttachments(Thread thread)
        {
            string attachmentTopicDir = thread.CategoryID + "\\" + thread.ID;
            ClearDirectory(attachmentTopicDir);
        }

        public void RemoveAttachments(ThreadCategory threadCategory)
        {
            ClearDirectory(threadCategory.ID.ToString());
        }

        private void ClearDirectory(string directoryPath)
        {
            var store = GetStore();
            try
            {
                store.DeleteFiles(directoryPath, "*", true);
            }
            catch { };
        }

        #endregion


        public string GetHTMLImgUserAvatar(Guid userID)
        {
            return "<img alt=\"\" class='userPhoto' src=\"" + UserPhotoManager.GetBigPhotoURL(userID) + "\"/>";
        }



        public PageLocation CurrentPage
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation] != null)
                    return (PageLocation)HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation];

                return new PageLocation(ForumPage.Default, Settings.StartPageAbsolutePath);

            }
        }
        public PageLocation PreviousPage
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation] != null)
                    return (PageLocation)HttpContext.Current.Session[this.SessionKeys.PreviousPageLocation];

                return new PageLocation(ForumPage.Default, Settings.StartPageAbsolutePath);

            }
        }

        public void SetCurrentPage(ForumPage page)
        {
            if (HttpContext.Current != null)
            {
                PageLocation current = new PageLocation(ForumPage.Default, Settings.StartPageAbsolutePath);
                if (HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation] != null)
                    current = (PageLocation)HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation];

                PageLocation previous = (PageLocation)current.Clone();

                if (previous.Page != page)
                    HttpContext.Current.Session[this.SessionKeys.PreviousPageLocation] = previous;

                current = new PageLocation(page, HttpContext.Current.Request.GetUrlRewriter().AbsoluteUri);
                HttpContext.Current.Session[this.SessionKeys.CurrentPageLocation] = current;
            }
        }

        public ForumSessionKeys SessionKeys { get; private set; }

        public class ForumSessionKeys
        {
            private Guid _settingsID;

            public ForumSessionKeys(Guid settingsID)
            {
                _settingsID = settingsID;
            }

            public string CurrentPageLocation
            {
                get { return "forum_current_page_location" + _settingsID.ToString(); }
            }

            public string PreviousPageLocation
            {
                get { return "forum_previous_page_location" + _settingsID.ToString(); }
            }

        }

    }
}
