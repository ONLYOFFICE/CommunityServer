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


using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterTemplateResources : ClientScriptTemplate
    {
        protected override string[] Links
        {
            get
            {
                return new[]
                {
                    "~/Templates/UserProfileCardTemplate.html",
                    "~/Templates/AdvansedUserProfileEditTemplate.html",
                    "~/Templates/AdvansedFilterTemplate.html",
                    "~/Templates/FeedListTemplate.html",
                    "~/Templates/DropFeedTemplate.html",
                    "~/Templates/DropMailTemplate.html",
                    "~/Templates/GroupSelectorTemplate.html",
                    "~/Templates/SharingSettingsTemplate.html",
                    "~/Templates/AdvansedSelectorTemplate.html",
                    "~/Templates/AdvansedEmailSelectorTemplate.html",
                    "~/Templates/CommonTemplates.html",
                    "~/Templates/ManagementTemplates.html",
                    "~/Templates/CommentsTemplates.html",
                    "~/Templates/ContainerTemplate.html",
                    "~/Templates/ProgressDialogTemplate.html"
                };
            }
        }
    }
}