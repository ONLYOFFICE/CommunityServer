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
    public interface IFilterDao
    {
        /// <summary>
        ///     Get a list of filters
        /// </summary>
        List<Filter> GetList();

        /// <summary>
        ///     Get filter by id.
        /// </summary>
        /// <param name="id">id</param>
        Filter Get(int id);

        /// <summary>
        ///     Save or update filter
        /// </summary>
        /// <param name="filter"></param>
        int Save(Filter filter);

        /// <summary>
        ///     Delete filter
        /// </summary>
        /// <param name="id">id</param>
        int Delete(int id);
    }
}
