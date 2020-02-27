/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Web.Sample.Classes;

namespace ASC.Api.Sample
{
    /// <summary>
    /// Sample CRUD Api
    /// </summary>
    public class SampleApi : IApiEntryPoint
    {
        /// <summary>
        /// ASC.Api.Interfaces.IApiEntryPoint.Name
        /// </summary>
        public string Name
        {
            get { return "sample"; }
        }

        /// <summary>
        /// Create item
        /// </summary>
        /// <param name="value">item value</param>
        /// <returns>SampleClass item</returns>
        [Create("create", false)]
        public SampleClass Create(string value)
        {
            return SampleDao.Create(value);
        }

        /// <summary>
        /// Read item by id
        /// </summary>
        /// <param name="id">item id</param>
        /// <returns>SampleClass item</returns>
        [Read(@"read/{id:[0-9]+}", false)]
        public SampleClass Read(int id)
        {
            return SampleDao.Read(id);
        }

        /// <summary>
        /// Read all items
        /// </summary>
        /// <returns>SampleClass items list</returns>
        [Read("read", false)]
        public List<SampleClass> Read()
        {
            return SampleDao.Read();
        }

        /// <summary>
        /// Update item
        /// </summary>
        /// <param name="id">item id</param>
        /// <param name="value">new item value</param>
        [Update("update", false)]
        public void Update(int id, string value)
        {
            SampleDao.Update(id, value);
        }

        /// <summary>
        /// Update item by id
        /// </summary>
        /// <param name="id">item id</param>
        [Delete("delete/{id:[0-9]+}", false)]
        public void Delete(int id)
        {
            SampleDao.Delete(id);
        }
    }
}
