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


using System.Web;
using ASC.Core.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;

namespace ASC.Web.UserControls.Wiki
{
    public class WikiNotifySource : NotifySource
    {
        private string defPageHref;


        public static WikiNotifySource Instance
        {
            get;
            private set;
        }


        static WikiNotifySource()
        {
            Instance = new WikiNotifySource();
        }

        public WikiNotifySource()
            : base(WikiManager.ModuleId)
        {
            defPageHref = VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath);
        }


        public string GetDefPageHref()
        {
            return defPageHref;
        }


        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(Constants.NewPage, Constants.EditPage);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(Patterns.WikiPatternsResource.patterns, ChoosePattern);
        }


        private IPattern ChoosePattern(INotifyAction action, string senderName, ASC.Notify.Engine.NotifyRequest request)
        {
            if (action == Constants.EditPage)
            {
                var tag = request.Arguments.Find(t => t.Tag == "ChangeType");
                if (tag != null && tag.Value.ToString() == "new wiki page comment")
                {
                    return PatternProvider.GetPattern(new NotifyAction(tag.Value.ToString()), senderName);
                }
            }
            return null;
        }
    }
}
