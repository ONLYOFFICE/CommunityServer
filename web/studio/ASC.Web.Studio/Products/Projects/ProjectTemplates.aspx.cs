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


using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects
{
    public partial class ProjectTemplates : BasePage
    {
        protected override bool CheckSecurity { get { return ProjectSecurity.CanCreate<Project>(null); } }

        protected override void PageLoad()
        {
            var action = UrlParameters.ActionType;

            if (UrlParameters.EntityID >= 0)
            {
                if (action.HasValue && action.Value == UrlAction.Edit)
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Templates/EditTemplate.ascx")));
                    return;
                }
            }
            else
            {
                if (action.HasValue && action.Value == UrlAction.Add)
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Templates/EditTemplate.ascx")));
                    return;
                }
            }

            Title = HeaderStringHelper.GetPageTitle(ProjectTemplatesResource.AllProjectTmpl);
        }
    }
}