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
using System.Threading;
using ASC.Api.Attributes;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Web.Mail.Resources;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns list of the tags used in Mail
        /// </summary>
        /// <returns>Filters list. Filters represented as JSON.</returns>
        /// <short>Get filters list</short> 
        /// <category>Filters</category>
        [Read(@"filters")]
        public IEnumerable<MailSieveFilterData> GetFilters()
        {
            var filters = MailEngineFactory.FilterEngine.GetList();
            return filters;
        }

        /// <summary>
        ///    Creates a new filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>Filter</returns>
        /// <short>Create filter</short> 
        /// <category>Filters</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Create(@"filters")]
        public MailSieveFilterData CreateFilter(MailSieveFilterData filter)
        {
            var id = MailEngineFactory.FilterEngine.Create(filter);
            filter.Id = id;
            return filter;
        }

        /// <summary>
        ///    Updates the selected filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>Updated filter</returns>
        /// <short>Update filter</short> 
        /// <category>Filters</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"filters")]
        public MailSieveFilterData UpdateFilter(MailSieveFilterData filter)
        {
            MailEngineFactory.FilterEngine.Update(filter);

            return filter;
        }

        /// <summary>
        ///    Deletes the selected filter
        /// </summary>
        /// <param name="id">Filter id</param>
        /// <returns>Deleted Filter id</returns>
        /// <short>Delete filter</short> 
        /// <category>Filters</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Delete(@"filters/{id}")]
        public int DeleteFilter(int id)
        {
            MailEngineFactory.FilterEngine.Delete(id);
            return id;
        }

        /// <summary>
        ///    Check filter result
        /// </summary>
        /// <param name="filter"></param>
        /// <param optional="true" name="page">Page number</param>
        /// <param optional="true" name="pageSize">Number of messages on page</param>
        /// <returns>List messages</returns>
        /// <short>Check filter</short> 
        /// <category>Filters</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"filters/check")]
        public List<MailMessageData> CheckFilter(MailSieveFilterData filter, int? page, int? pageSize)
        {
            if (!page.HasValue)
                page = 0;

            if (!pageSize.HasValue)
                pageSize = 10;

            long total;
            var messages = MailEngineFactory.MessageEngine.GetFilteredMessages(filter, page.Value, pageSize.Value, out total);

            _context.SetTotalCount(total);

            return messages;
        }

        /// <summary>
        ///    Apply filter to existing messages
        /// </summary>
        /// <param name="id">Filter id</param>
        /// <returns>MailOperationResult object</returns>
        /// <short>Check filter</short> 
        /// <category>Filters</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"filters/{id}/apply")]
        public MailOperationStatus ApplyFilter(int id)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                return MailEngineFactory.OperationEngine.ApplyFilter(id, TranslateMailOperationStatus);
            }
            catch (Exception)
            {
                throw new Exception(MailApiErrorsResource.ErrorInternalServer);
            }
        }
    }
}
