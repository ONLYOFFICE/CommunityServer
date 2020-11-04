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


using ASC.Bookmarking.Common;
using ASC.Bookmarking.Resources;
using ASC.Core.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.UserControls.Bookmarking.Common;

namespace ASC.Bookmarking.Business.Subscriptions
{
    internal class BookmarkingNotifySource : NotifySource
    {
        public static BookmarkingNotifySource Instance { get; private set; }

        static BookmarkingNotifySource()
        {
            Instance = new BookmarkingNotifySource();
        }

        private BookmarkingNotifySource() : base(BookmarkingSettings.ModuleId)
        {
        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(BookmarkingBusinessConstants.NotifyActionNewBookmark, BookmarkingBusinessConstants.NotifyActionNewComment);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(BookmarkingSubscriptionPatterns.BookmarkingPatterns);
        }
    }
}