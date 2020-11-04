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
using System.Linq;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core.Caching;

namespace ASC.Core
{
    public class AuthorizationManager
    {
        private readonly IAzService service;

        public AuthorizationManager(IAzService service)
        {
            this.service = service;
        }


        public IEnumerable<AzRecord> GetAces(Guid subjectId, Guid actionId)
        {
            var aces = service.GetAces(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
            return aces
                .Where(a => a.ActionId == actionId && (a.SubjectId == subjectId || subjectId == Guid.Empty))
                .ToList();
        }

        public IEnumerable<AzRecord> GetAces(Guid subjectId, Guid actionId, ISecurityObjectId objectId)
        {
            var aces = service.GetAces(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
            return FilterAces(aces, subjectId, actionId, objectId)
                .ToList();
        }

        public IEnumerable<AzRecord> GetAcesWithInherits(Guid subjectId, Guid actionId, ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
        {
            if (objectId == null)
            {
                return GetAces(subjectId, actionId, null);
            }

            var result = new List<AzRecord>();
            var aces = service.GetAces(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
            result.AddRange(FilterAces(aces, subjectId, actionId, objectId));

            var inherits = new List<AzRecord>();
            var secObjProviderHelper = new AzObjectSecurityProviderHelper(objectId, secObjProvider);
            while (secObjProviderHelper.NextInherit())
            {
                inherits.AddRange(FilterAces(aces, subjectId, actionId, secObjProviderHelper.CurrentObjectId));
            }

            inherits.AddRange(FilterAces(aces, subjectId, actionId, null));

            result.AddRange(DistinctAces(inherits));
            return result;
        }

        public void AddAce(AzRecord r)
        {
            service.SaveAce(CoreContext.TenantManager.GetCurrentTenant().TenantId, r);
        }

        public void RemoveAce(AzRecord r)
        {
            service.RemoveAce(CoreContext.TenantManager.GetCurrentTenant().TenantId, r);
        }

        public void RemoveAllAces(ISecurityObjectId id)
        {
            foreach (var r in GetAces(Guid.Empty, Guid.Empty, id).ToArray())
            {
                RemoveAce(r);
            }
        }

        private IEnumerable<AzRecord> GetAcesInternal()
        {
            return service.GetAces(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
        }

        private IEnumerable<AzRecord> DistinctAces(IEnumerable<AzRecord> inheritAces)
        {
            var aces = new Dictionary<string, AzRecord>();
            foreach (var a in inheritAces)
            {
                aces[string.Format("{0}{1}{2:D}", a.SubjectId, a.ActionId, a.Reaction)] = a;
            }
            return aces.Values;
        }

        private IEnumerable<AzRecord> FilterAces(IEnumerable<AzRecord> aces, Guid subjectId, Guid actionId, ISecurityObjectId objectId)
        {
            var objId = AzObjectIdHelper.GetFullObjectId(objectId);
            var store = aces as AzRecordStore;
            return store != null ?
                store.Get(objId).Where(a => (a.SubjectId == subjectId || subjectId == Guid.Empty) && (a.ActionId == actionId || actionId == Guid.Empty)) :
                aces.Where(a => (a.SubjectId == subjectId || subjectId == Guid.Empty) && (a.ActionId == actionId || actionId == Guid.Empty) && a.ObjectId == objId);
        }
    }
}