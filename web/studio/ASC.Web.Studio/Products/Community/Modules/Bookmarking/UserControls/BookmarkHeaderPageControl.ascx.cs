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
using System.Web.UI;
using ASC.Core;
using ASC.Web.Community.Product;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.Community.Bookmarking.UserControls
{
    public partial class BookmarkHeaderPageControl : UserControl
    {
        public string Title { get; set; }
        public long BookmarkID { get; set; }
        public Guid Author { get; set; }

        protected string SubscribeOnBookmarkComments { get { return BookmarkingUCResource.SubscribeOnBookmarkComments; } }
        protected string UnsubscribeOnBookmarkComments { get { return BookmarkingUCResource.UnSubscribeOnBookmarkComments; } }
        protected bool IsAdmin { get; set; }
        protected bool IsAuthor { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            IsAdmin = CommunitySecurity.IsAdministrator();
            IsAuthor = Author.Equals(SecurityContext.CurrentAccount.ID);
        }
    }
}