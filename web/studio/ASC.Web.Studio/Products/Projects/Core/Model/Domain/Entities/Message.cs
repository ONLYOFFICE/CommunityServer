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


using ASC.Projects.Engine;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Projects.Core.Domain
{
    public class Message : ProjectEntity
    {
        public override EntityType EntityType { get { return EntityType.Message; } }

        public override string ItemPath { get { return "{0}Messages.aspx?prjID={1}&ID={2}"; } }

        public int CommentsCount { get; set; }

        public MessageStatus Status { get; set; }

        public override bool CanEdit()
        {
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<ProjectSecurity>().CanEdit(this);
            }
        }
    }

    public enum MessageStatus
    {
        Open,
        Archived
    }
}