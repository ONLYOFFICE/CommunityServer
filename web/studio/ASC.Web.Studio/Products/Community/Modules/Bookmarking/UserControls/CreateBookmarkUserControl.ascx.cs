/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using AjaxPro;

using ASC.Bookmarking;
using ASC.Bookmarking.Common;
using ASC.Web.Community.Modules.Bookmarking.UserControls.Resources;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;

namespace ASC.Web.UserControls.Bookmarking
{
    public partial class CreateBookmarkUserControl : System.Web.UI.UserControl
    {
        public bool IsNewBookmark { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(BookmarkingUserControl));
            Utility.RegisterTypeForAjax(typeof(SingleBookmarkUserControl));

            InitActionButtons();
        }

        #region Init Action Buttons

        private void InitActionButtons()
        {
            SaveBookmarkButton.ButtonText = BookmarkingUCResource.Save;
            SaveBookmarkButton.AjaxRequestText = BookmarkingUCResource.BookmarkCreationIsInProgressLabel;

            SaveBookmarkButtonCopy.ButtonText = BookmarkingUCResource.AddToFavourite;
            SaveBookmarkButtonCopy.AjaxRequestText = BookmarkingUCResource.BookmarkCreationIsInProgressLabel;

            AddToFavouritesBookmarkButton.ButtonText = BookmarkingUCResource.AddToFavourite;
            AddToFavouritesBookmarkButton.AjaxRequestText = BookmarkingUCResource.BookmarkCreationIsInProgressLabel;

            CheckBookmarkUrlLinkButton.ButtonText = BookmarkingUCResource.CheckBookmarkUrlButton;
            CheckBookmarkUrlLinkButton.AjaxRequestText = BookmarkingUCResource.CheckingUrlIsInProgressLabel;
        }

        #endregion

        public string NavigateToMainPage
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                return BookmarkDisplayMode.CreateBookmark.Equals(displayMode).ToString().ToLower();
            }
        }

        public bool CreateBookmarkMode
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                return BookmarkDisplayMode.CreateBookmark.Equals(displayMode);
            }
        }

        public bool IsEditMode
        {
            get
            {
                var displayMode = BookmarkingBusinessFactory.GetDisplayMode();

                var serviceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

                return BookmarkDisplayMode.SelectedBookmark.Equals(displayMode) && serviceHelper.IsCurrentUserBookmark();
            }
        }
    }
}