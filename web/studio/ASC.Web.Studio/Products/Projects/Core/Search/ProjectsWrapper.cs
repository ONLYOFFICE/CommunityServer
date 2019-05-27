/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using AutoMapper;

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
            return Mapper.Map<ProjectsWrapper>(project);
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