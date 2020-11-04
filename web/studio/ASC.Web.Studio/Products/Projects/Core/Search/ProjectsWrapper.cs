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


using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Configuration;

namespace ASC.Web.Projects.Core.Search
{
    public sealed class ProjectsWrapper : Wrapper
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [Column("description", 2)]
        public string Description { get; set; }

        //[Join(JoinTypeEnum.Sub, "id:project_id")]
        //public List<TagsWrapper> TagsWrapper { get; set; }

        protected override string Table { get { return "projects_projects"; } }

        public static implicit operator ProjectsWrapper(Project project)
        {
            return ProductEntryPoint.Mapper.Map<ProjectsWrapper>(project);
            //var result = Mapper.Map<ProjectsWrapper>(project);

            //result.TagsWrapper = new List<TagsWrapper>();

            //return result;
        }
    }

    //public sealed class TagsWrapper : Wrapper
    //{
    //    [ColumnId("")]
    //    public override int Id { get; set; }

    //    [ColumnTenantId("")]
    //    public override int TenantId { get; set; }

    //    [ColumnLastModified("")]
    //    public override DateTime LastModifiedOn { get; set; }

    //    [ColumnMeta("tag_id", 1)]
    //    public int TagId { get; set; }

    //    protected override string Table { get { return "projects_project_tag"; } }
    //}
}