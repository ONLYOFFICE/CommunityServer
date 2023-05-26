/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
        /// Returns a list of filters used in all the mails.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail">List of filters which is represented as JSON</returns>
        /// <short>Get filters</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"filters")]
        public IEnumerable<MailSieveFilterData> GetFilters()
        {
            var filters = MailEngineFactory.FilterEngine.GetList();
            return filters;
        }

        /// <summary>
        /// Creates a new filter with the parameters specified in the request.
        /// </summary>
        /// <param type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail.Data.Contracts" name="filter">Filter parameters: ID, name, position, enabled, conditions, actions, options</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail">Filter</returns>
        /// <short>Create a filter</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Create(@"filters")]
        public MailSieveFilterData CreateFilter(MailSieveFilterData filter)
        {
            var id = MailEngineFactory.FilterEngine.Create(filter);
            filter.Id = id;
            return filter;
        }

        /// <summary>
        /// Updates the selected filter with the parameters specified in the request.
        /// </summary>
        /// <param type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail.Data.Contracts" name="filter">New filter parameters: ID, name, position, enabled, conditions, actions, options</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail">Updated filter</returns>
        /// <short>Update a filter</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"filters")]
        public MailSieveFilterData UpdateFilter(MailSieveFilterData filter)
        {
            MailEngineFactory.FilterEngine.Update(filter);

            return filter;
        }

        /// <summary>
        /// Deletes a filter with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Filter ID</param>
        /// <returns>Filter ID</returns>
        /// <short>Delete a filter</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Delete(@"filters/{id}")]
        public int DeleteFilter(int id)
        {
            MailEngineFactory.FilterEngine.Delete(id);
            return id;
        }

        /// <summary>
        /// Checks the results of a filter specified in the request.
        /// </summary>
        /// <param type="ASC.Mail.Data.Contracts.MailSieveFilterData, ASC.Mail.Data.Contracts" method="url" name="filter">Filter parameters: ID, name, position, enabled, conditions, actions, options</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page">Page number</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="pageSize">Number of messages on the page</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">List of messages</returns>
        /// <short>Check filter results</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters/check</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Create(@"filters/check")]
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
        /// Applies a filter to the existing messages.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Filter ID</param>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">Mail operation status</returns>
        /// <short>Apply a filter</short> 
        /// <category>Filters</category>
        /// <path>api/2.0/mail/filters/{id}/apply</path>
        /// <httpMethod>GET</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
