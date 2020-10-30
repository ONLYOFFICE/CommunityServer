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


using System.Collections.Generic;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface ITagDao
    {
        /// <summary>
        ///     Get tag by id.
        /// </summary>
        /// <param name="id">id</param>
        Tag GetTag(int id);

        /// <summary>
        ///     Get CRM tag by id.
        /// </summary>
        /// <param name="id">id</param>
        Tag GetCrmTag(int id);

        /// <summary>
        ///     Get tag by name.
        /// </summary>
        /// <param name="name">name</param>
        Tag GetTag(string name);

        /// <summary>
        ///     Get a list of tags
        /// </summary>
        List<Tag> GetTags();

        /// <summary>
        ///     Get a list of CRM tags
        /// </summary>
        List<Tag> GetCrmTags();

        /// <summary>
        ///     Get a list of CRM tags
        /// </summary>
        /// <param name="contactIds">id</param>
        List<Tag> GetCrmTags(List<int> contactIds);

        /// <summary>
        ///     Save or update tag
        /// </summary>
        /// <param name="tag"></param>
        int SaveTag(Tag tag);

        /// <summary>
        ///     Delete tag
        /// </summary>
        /// <param name="id">id</param>
        int DeleteTag(int id);

        /// <summary>
        ///     Delete tags
        /// </summary>
        /// <param name="tagIds">id</param>
        int DeleteTags(List<int> tagIds);
    }
}
