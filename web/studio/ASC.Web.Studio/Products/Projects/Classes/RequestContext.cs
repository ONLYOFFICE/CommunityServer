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
using System.Collections.Generic;

using ASC.Core;
using ASC.Projects.Engine;
using ASC.Projects.Core.Domain;

namespace ASC.Web.Projects.Classes
{
    public class RequestContext
    {
        public bool IsInConcreteProject { get; private set; }
        private readonly EngineFactory engineFactory;
        private Project currentProject;

        private IEnumerable<Project> currentUserProjects;
        public IEnumerable<Project> CurrentUserProjects
        {
            get
            {
                return currentUserProjects ??
                       (currentUserProjects =
                           engineFactory.ProjectEngine.GetByParticipant(SecurityContext.CurrentAccount.ID));
            }
        }

        #region Project

        public RequestContext(EngineFactory engineFactory)
        {
            IsInConcreteProject = UrlParameters.ProjectID >= 0;
            this.engineFactory = engineFactory;
        }

        public Project GetCurrentProject(bool isthrow = true)
        {
            if (currentProject != null) return currentProject;

            currentProject = engineFactory.ProjectEngine.GetByID(GetCurrentProjectId(isthrow));

            if (currentProject != null || !isthrow)
            {
                return currentProject;
            }

            throw new ApplicationException("ProjectFat not finded");
        }

        public int GetCurrentProjectId(bool isthrow = true)
        {
            var pid = UrlParameters.ProjectID;

            if (pid >= 0 || !isthrow)
                return pid;

            throw new ApplicationException("ProjectFat Id parameter invalid");
        }

        #endregion
    }
}
