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
