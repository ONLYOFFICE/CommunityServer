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

namespace ASC.Core.Security.Authorizing
{
    class PermissionProvider : IPermissionProvider
    {
        public IEnumerable<Ace> GetAcl(ISubject subject, IAction action, ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (action == null) throw new ArgumentNullException("action");

            return CoreContext.AuthorizationManager
                .GetAcesWithInherits(subject.ID, action.ID, objectId, secObjProvider)
                .Select(r => new Ace(r.ActionId, r.Reaction));
        }
    }
}