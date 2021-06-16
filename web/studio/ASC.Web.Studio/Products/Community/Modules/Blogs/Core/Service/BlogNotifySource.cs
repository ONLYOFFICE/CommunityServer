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


using ASC.Core.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Community.Modules.Blogs.Core.Service;

namespace ASC.Blogs.Core.Service
{
    public class BlogNotifySource : NotifySource
    {
        public BlogNotifySource()
            : base(Constants.ModuleId)
        {
        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                    Constants.NewPost,
                    Constants.NewPostByAuthor,
                    Constants.NewComment
                );
        }
        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(BlogPatternsResource.patterns);
        }
    }
}
