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


using ASC.Core.Notify;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;

namespace ASC.Forum.Module
{
    class ForumNotifyClient
    {
        public static INotifyClient NotifyClient { get; private set; }

        static ForumNotifyClient()
        {
            NotifyClient = ASC.Core.WorkContext.NotifyContext.NotifyService.RegisterClient(ForumNotifySource.Instance);
        }
    }

    class ForumNotifySource : NotifySource
    {
        public static ForumNotifySource Instance
        {
            get;
            private set;
        }


        static ForumNotifySource()
        {
            Instance = new ForumNotifySource();
        }


        private ForumNotifySource()
            : base(ASC.Web.Community.Forum.ForumManager.ModuleID)
        {

        }


        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                    Constants.NewPostByTag,
                    Constants.NewPostInThread,
                    Constants.NewPostInTopic,
                    Constants.NewTopicInForum
                );
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(ASC.Forum.Module.Patterns.forum_patterns);
        }
    }
}
