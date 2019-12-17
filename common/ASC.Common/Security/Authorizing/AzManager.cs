/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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