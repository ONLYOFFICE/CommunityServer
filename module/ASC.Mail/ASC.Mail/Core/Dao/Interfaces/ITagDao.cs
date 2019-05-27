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
