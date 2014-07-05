/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Collection;
using ASC.Mail.Aggregator.Filter;
using ASC.Mail.Service.DAO;
using Microsoft.ServiceModel.Web;
using System.Linq;
using ASC.Mail.Aggregator.WebRequestsDataWrappers;

#endregion

namespace ASC.Mail.Service
{
    public abstract class MailServiceBase : IMailService
    {
        protected abstract List<MailAccount> DB_GetMailboxesList();
        protected abstract string DB_GetTimeAll();
        protected abstract string DB_GetTimeFolder(int folder_id);
        protected abstract IList<MailMessageItem> DB_GetFilteredMessages(MailFilter filter, int page, int page_size);
        protected abstract long DB_GetFilteredMessagesCount(MailFilter filter);
        protected abstract List<int> DB_GetFilteredMessagesIds(MailFilter filter);
        protected abstract int DB_SaveMessage(int id, MailSendItem item);
        protected abstract int DB_SendMessage(int id, MailSendItem item);
        protected abstract void DB_UpdateUserActivity();
        protected abstract List<MailFolder> DB_GetFoldersList();
        protected abstract IEnumerable<MailTag> DB_GetTagsList();
        protected abstract MailFolder DB_GetInboxState();
        protected abstract List<string> DB_SearchContacts(string searchText);

        protected void CheckPermission()
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    if (!SecurityContext.AuthenticateMe(CookiesManager.GetAuthCookie()))
                    {
                        throw GenerateException(HttpStatusCode.Unauthorized, "Unauthorized", null);
                    }
                }
                catch (Exception exception)
                {
                    throw GenerateException(HttpStatusCode.Unauthorized, "Unauthorized", exception);
                }
            }
        }

        public CommonHash GetLeftPanelContent()
        {
            try
            {
                CheckPermission();
                string precised_time_all = DB_GetTimeAll();

                List<MailFolder> folders = DB_GetFoldersList();
                List<MailAccount> accounts = DB_GetMailboxesList();
                ItemList<MailTag> tags = new ItemList<MailTag>(DB_GetTagsList());
                return new CommonHash(null, folders, tags, accounts, 0, 0, null, precised_time_all, "");

            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        #region GetFolders

        public MailFolder GetInboxState()
        {
            try
            {
                CheckPermission();
                return DB_GetInboxState();
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        #endregion

        public int SendMessage(string id, MailSendItem item)
        {
            try
            {
                CheckPermission();
                item.Validate();
                return DB_SendMessage(int.Parse(id), item);
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        public int SaveMessage(string id, MailSendItem item)
        {
            try
            {
                CheckPermission();
                return DB_SaveMessage(int.Parse(id), item);
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        public ItemList<int> GetFilteredMessagesIds(MailFilter filter)
        {
            try
            {
                if (filter == null) throw new ArgumentNullException("filter");
                CheckPermission();

                return new ItemList<int>(DB_GetFilteredMessagesIds(filter));
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        private void CorrectPageValue(MailFilter filter, long total_messages)
        {
            int max_page = (int)Math.Ceiling((double)total_messages / (double)filter.PageSize);
            if (filter.Page > max_page) filter.Page = max_page;
            if (filter.Page < 1) filter.Page = 1;
        }

        protected WebProtocolException GenerateException(Exception inner)
        {
            return GenerateException(HttpStatusCode.BadRequest, inner);
        }

        protected WebProtocolException GenerateException(HttpStatusCode code, Exception inner)
        {
            return GenerateException(code, "Bad request", inner);
        }

        protected WebProtocolException GenerateException(HttpStatusCode code, string message, Exception inner)
        {
            var element = new XElement("error", new XElement("message", message));

            var current = element;
            while (inner!=null)
            {
                var el = new XElement("inner", 
                    new XElement("message",inner.Message),
                    new XElement("type", inner.GetType()),
                    new XElement("source", inner.Source), 
                    new XElement("stack", inner.StackTrace));
                current.Add(el);
                current = el;
                inner = inner.InnerException;
            }

            return new WebProtocolException(code, message, element, false, inner);
        }

        public ItemList<string> SearchContacts(string term)
        {
            CheckPermission();
            return new ItemList<string>(DB_SearchContacts(term));
        }

        private CommonHash CreateCommonHashWithFoldersList()
        {
            string precised_time_all = DB_GetTimeAll();

            List<MailFolder> folders = DB_GetFoldersList();
            return CommonHash.FoldersResponse(folders, precised_time_all);
        }
    }
}