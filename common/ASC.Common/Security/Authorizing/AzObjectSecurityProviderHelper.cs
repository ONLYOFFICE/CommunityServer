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

#region usings

using System;
using System.Collections.Generic;

#endregion

namespace ASC.Common.Security.Authorizing
{
    public class AzObjectSecurityProviderHelper
    {
        private readonly SecurityCallContext callContext;
        private readonly bool currObjIdAsProvider;
        private ISecurityObjectId currObjId;
        private ISecurityObjectProvider currSecObjProvider;

        public AzObjectSecurityProviderHelper(ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
        {
            if (objectId == null) throw new ArgumentNullException("objectId");
            currObjIdAsProvider = false;
            currObjId = objectId;
            currSecObjProvider = secObjProvider;
            if (currSecObjProvider == null && currObjId is ISecurityObjectProvider)
            {
                currObjIdAsProvider = true;
                currSecObjProvider = (ISecurityObjectProvider) currObjId;
            }
            callContext = new SecurityCallContext();
        }

        public ISecurityObjectId CurrentObjectId
        {
            get { return currObjId; }
        }

        public bool ObjectRolesSupported
        {
            get { return currSecObjProvider != null && currSecObjProvider.ObjectRolesSupported; }
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account)
        {
            IEnumerable<IRole> roles = currSecObjProvider.GetObjectRoles(account, currObjId, callContext);
            foreach (IRole role in roles)
            {
                if (!callContext.RolesList.Contains(role)) callContext.RolesList.Add(role);
            }
            return roles;
        }

        public bool NextInherit()
        {
            if (currSecObjProvider == null || !currSecObjProvider.InheritSupported) return false;
            currObjId = currSecObjProvider.InheritFrom(currObjId);
            if (currObjId == null) return false;
            if (currObjIdAsProvider)
            {
                currSecObjProvider = currObjId as ISecurityObjectProvider;
            }
            callContext.ObjectsStack.Insert(0, CurrentObjectId);
            return currSecObjProvider != null;
        }
    }
}