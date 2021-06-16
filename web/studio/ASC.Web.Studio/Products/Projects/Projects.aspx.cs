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


using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;

namespace ASC.Web.Projects
{
    public partial class Projects : BasePage
    {
        protected override bool CheckSecurity
        {
            get
            {
                var action = UrlParameters.ActionType;
                if (!action.HasValue) return true;

                if (RequestContext.IsInConcreteProject)
                {
                    if (action.Value == UrlAction.Edit)
                    {
                        return ProjectSecurity.CanEdit(Project);
                    }
                }
                else
                {
                    if (action.Value == UrlAction.Add)
                    {
                        return ProjectSecurity.CanCreate<Project>(null);
                    }
                }

                return true;
            }
        }

        protected override void PageLoad()
        {
            var action = UrlParameters.ActionType;

            if (RequestContext.IsInConcreteProject)
            {
                if (action.HasValue && action.Value == UrlAction.Edit)
                {
                    Master.AddControl(LoadControl(PathProvider.GetFileStaticRelativePath("Projects/ProjectAction.ascx")));
                }
            }
            else
            {
                if (action.HasValue && action.Value == UrlAction.Add)
                {
                    Master.AddControl(LoadControl(PathProvider.GetFileStaticRelativePath("Projects/ProjectAction.ascx")));
                }
            }
        }
    }
}