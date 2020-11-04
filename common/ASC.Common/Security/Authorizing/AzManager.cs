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

namespace ASC.Common.Security.Authorizing
{
    public class AzManager
    {
        private readonly IPermissionProvider permissionProvider;
        private readonly IRoleProvider roleProvider;


        internal AzManager()
        {
        }

        public AzManager(IRoleProvider roleProvider, IPermissionProvider permissionProvider)
            : this()
        {
            if (roleProvider == null) throw new ArgumentNullException("roleProvider");
            if (permissionProvider == null) throw new ArgumentNullException("permissionProvider");

            this.roleProvider = roleProvider;
            this.permissionProvider = permissionProvider;
        }


        public bool CheckPermission(ISubject subject, IAction action, ISecurityObjectId objectId,
                                    ISecurityObjectProvider securityObjProvider, out ISubject denySubject,
                                    out IAction denyAction)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (action == null) throw new ArgumentNullException("action");

            var acl = GetAzManagerAcl(subject, action, objectId, securityObjProvider);
            denySubject = acl.DenySubject;
            denyAction = acl.DenyAction;
            return acl.IsAllow;
        }

        public AzManagerAcl GetAzManagerAcl(ISubject subject, IAction action, ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider)
        {
            if (action.AdministratorAlwaysAllow && (Constants.Admin.ID == subject.ID || roleProvider.IsSubjectInRole(subject, Constants.Admin)))
            {
                return AzManagerAcl.Allow;
            }

            var acl = AzManagerAcl.Default;
            var exit = false;

            foreach (var s in GetSubjects(subject, objectId, securityObjProvider))
            {
                var aceList = permissionProvider.GetAcl(s, action, objectId, securityObjProvider);
                foreach (var ace in aceList)
                {
                    if (ace.Reaction == AceType.Deny && !exit)
                    {
                        acl.IsAllow = false;
                        acl.DenySubject = s;
                        acl.DenyAction = action;
                        exit = true;
                    }
                    if (ace.Reaction == AceType.Allow && !exit)
                    {
                        acl.IsAllow = true;
                        if (!action.Conjunction)
                        {
                            // disjunction: first allow and exit
                            exit = true;
                        }
                    }
                    if (exit) break;
                }
                if (exit) break;
            }
            return acl;
        }

        public IEnumerable<ISubject> GetSubjects(ISubject subject, ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider)
        {
            var subjects = new List<ISubject>();
            subjects.Add(subject);
            subjects.AddRange(
                roleProvider.GetRoles(subject)
                    .ConvertAll(r => { return (ISubject)r; })
                );
            if (objectId != null)
            {
                var secObjProviderHelper = new AzObjectSecurityProviderHelper(objectId, securityObjProvider);
                do
                {
                    if (!secObjProviderHelper.ObjectRolesSupported) continue;
                    foreach (IRole role in secObjProviderHelper.GetObjectRoles(subject))
                    {
                        if (!subjects.Contains(role)) subjects.Add(role);
                    }
                } while (secObjProviderHelper.NextInherit());
            }
            return subjects;
        }

        #region Nested type: AzManagerAcl

        public class AzManagerAcl
        {
            public IAction DenyAction;
            public ISubject DenySubject;
            public bool IsAllow;

            public static AzManagerAcl Allow
            {
                get { return new AzManagerAcl { IsAllow = true }; }
            }

            public static AzManagerAcl Default
            {
                get { return new AzManagerAcl { IsAllow = false }; }
            }
        }

        #endregion
    }
}