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

        internal AzManagerAcl GetAzManagerAcl(ISubject subject, IAction action, ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider)
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

        internal IEnumerable<ISubject> GetSubjects(ISubject subject, ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider)
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

        internal class AzManagerAcl
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