/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Projects.Wrappers;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region tags

        ///<summary>
        ///Returns the list of all available project tags
        ///</summary>
        ///<short>
        ///Project tags
        ///</short>
        /// <category>Tags</category>
        ///<returns>List of tags</returns>
        [Read(@"tag")]
        public IEnumerable<ObjectWrapperBase> GetAllTags()
        {
            return EngineFactory.GetTagEngine().GetTags().Select(x => new ObjectWrapperBase {Id = x.Key, Title = x.Value}).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed list of all projects with the specified tag
        ///</summary>
        ///<short>
        ///Project by tag
        ///</short>
        /// <category>Tags</category>
        ///<param name="tag">Tag name</param>
        ///<returns>List of projects</returns>
        [Read(@"tag/{tag}")]
        public IEnumerable<ProjectWrapper> GetProjectsByTags(string tag)
        {
            var projectsTagged = EngineFactory.GetTagEngine().GetTagProjects(tag);
            return EngineFactory.GetProjectEngine().GetByID(projectsTagged).Select(x => new ProjectWrapper(x)).ToSmartList();
        }


        ///<summary>
        ///Returns the list of all tags like the specified tag name
        ///</summary>
        ///<short>
        ///Tags by tag name
        ///</short>
        /// <category>Tags</category>
        ///<param name="tagName">Tag name</param>
        ///<returns>List of tags</returns>
        [Read(@"tag/search")]
        public string[] GetTagsByName(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && tagName.Trim() != string.Empty
                       ? EngineFactory.GetTagEngine().GetTags(tagName.Trim()).Select(r => r.Value).ToArray()
                       : new string[0];
        }

        #endregion
    }
}