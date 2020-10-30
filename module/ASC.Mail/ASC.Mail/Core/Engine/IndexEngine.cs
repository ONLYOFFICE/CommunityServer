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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ASC.Common.Logging;
using ASC.ElasticSearch;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;

namespace ASC.Mail.Core.Engine
{
    public class IndexEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        private ILog Log { get; set; }

        public IndexEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.IndexEngine");
        }

        public bool IsIndexAvailable()
        {
            if (!FactoryIndexer<MailWrapper>.Support)
            {
                Log.Info("[SKIP INDEX] IsIndexAvailable->FactoryIndexer<MailWrapper>.Support == false");
                return false;
            }
            
            if (!FactoryIndexer.CheckState(false))
            {
                Log.Info("[SKIP INDEX] IsIndexAvailable->FactoryIndexer.CheckState(false) == false");
                return false;
            }

            return true;
        }

        public void Add<T>(T data) where T : Wrapper, new()
        {
            var typeParameterType = typeof(T);

            try
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (!IsIndexAvailable())
                    return;

                FactoryIndexer<T>.Index(data);

                Log.InfoFormat("IndexEngine->Add<{0}>(mail Id = {1}) success", typeParameterType, data == null ? -1 : data.Id);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Add<{0}>(mail Id = {1}) error: {2}", typeParameterType, data == null ? -1 : data.Id, ex.ToString());
            }
        }

        public void Update(List<MailWrapper> mails, UpdateAction action, Expression<Func<MailWrapper, IList>> fields)
        {
            try
            {
                if (mails == null || !mails.Any())
                    throw new ArgumentNullException("mails");

                if (!IsIndexAvailable())
                    return;

                mails.ForEach(x => FactoryIndexer<MailWrapper>.Update(x, action, fields));
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Update(count = {0}) error: {1}", mails == null ? 0 : mails.Count,
                    ex.ToString());
            }
        }

        public void Update(MailWrapper data, Expression<Func<Selector<MailWrapper>, Selector<MailWrapper>>> expression,
            UpdateAction action, Expression<Func<MailWrapper, IList>> fields)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (expression == null)
                    throw new ArgumentNullException("expression");

                if (!IsIndexAvailable())
                    return;

                FactoryIndexer<MailWrapper>.Update(data, expression, action, fields);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Update() error: {0}", ex.ToString());
            }
        }

        public void Update(MailWrapper data, Expression<Func<Selector<MailWrapper>, Selector<MailWrapper>>> expression,
            params Expression<Func<MailWrapper, object>>[] fields)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (expression == null)
                    throw new ArgumentNullException("expression");

                if (!IsIndexAvailable())
                    return;

                FactoryIndexer<MailWrapper>.Update(data, expression, true, fields);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Update() error: {0}", ex.ToString());
            }
        }

        public void Update<T>(List<T> list, params Expression<Func<T, object>>[] fields) where T : Wrapper, new()
        {
            try
            {
                if (list == null || !list.Any())
                    throw new ArgumentNullException("list");

                if (!IsIndexAvailable())
                    return;

                list.ForEach(x => FactoryIndexer<T>.Update(x, true, fields));
            }
            catch (Exception ex)
            {
                var typeParameterType = typeof(T);

                Log.ErrorFormat("IndexEngine->Update<{0}>(mail Id = {1}) error: {2}", typeParameterType, list == null ? 0 : list.Count, ex.ToString());
            }
        }

        public void Remove(List<int> ids, int tenant, Guid user)
        {
            try
            {
                if (ids == null || !ids.Any())
                    throw new ArgumentNullException("ids");

                if (!IsIndexAvailable())
                    return;

                ids.ForEach(id =>
                    FactoryIndexer<MailWrapper>.Delete(
                        r => new Selector<MailWrapper>()
                            .Where(m => m.Id, id)
                            .Where(e => e.UserId, user)
                            .Where(e => e.TenantId, tenant)));
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Remove(count = {0}) error: {1}", ids == null ? 0 : ids.Count, ex.ToString());
            }
        }

        public void Remove(MailBoxData mailBox)
        {
            try
            {
                if (mailBox == null)
                    throw new ArgumentNullException("mailBox");

                if (!IsIndexAvailable())
                    return;

                var selector = new Selector<MailWrapper>()
                    .Where(m => m.MailboxId, mailBox.MailBoxId)
                    .Where(e => e.UserId, new Guid(mailBox.UserId))
                    .Where(e => e.TenantId, mailBox.TenantId);

                FactoryIndexer<MailWrapper>.Delete(r => selector);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->Remove(mailboxId = {0}) error: {1}", mailBox == null ? -1 : mailBox.MailBoxId, ex.ToString());
            }
        }

        public void RemoveContacts(List<int> ids, int tenant, Guid user)
        {
            try
            {
                if (ids == null || !ids.Any())
                    throw new ArgumentNullException("ids");

                if (!IsIndexAvailable())
                    return;

                FactoryIndexer<MailContactWrapper>.Delete(
                    r => new Selector<MailContactWrapper>()
                        .In(s => s.Id, ids.ToArray())
                        .Where(e => e.User, user)
                        .Where(e => e.TenantId, tenant));
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("IndexEngine->RemoveContacts(count = {0}) error: {1}", ids == null ? 0 : ids.Count, ex.ToString());
            }
        }
    }
}
