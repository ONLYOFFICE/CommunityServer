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

using ASC.ActiveDirectory.Expressions;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Principal;

namespace ASC.ActiveDirectory
{
    /// <summary>
    
    /// </summary>
    /// <remarks>
    
    /// </remarks>
    public abstract class LDAPAccount : LDAPObject
    {
        /// <summary>
        
        /// </summary>
        
        internal LDAPAccount(DirectoryEntry directoryEntry) : base(directoryEntry)
        { 
        }

        /// <summary>
        
        /// </summary>
        public string AccountName
        { 
            get
            {
                string aname = null;
                try
                {
                    aname = InvokeGet(Constants.ADSchemaAttributes.AccountName) as string;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPAccount AccountName property. {0}", e);
                }
                return aname;
            }
        }

        /// <summary>
        
        /// </summary>
        public Constants.AccountType AccountType
        {
            get 
            {
                Constants.AccountType at = Constants.AccountType.SAM_DOMAIN_OBJECT;
                try 
                {
                    if (PropertyContains(Constants.ADSchemaAttributes.AccountType))
                    {
                        at = (Constants.AccountType)InvokeGet(Constants.ADSchemaAttributes.AccountType);
                    }
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPAccount AccountType property. {0}", e);
                }
                return at;
            }
        }

        /// <summary>
        
        /// </summary>
        public SecurityIdentifier Sid
        {
            get
            {
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
                try
                {
                    byte[] binaryForm = InvokeGet(Constants.ADSchemaAttributes.ObjectSid) as byte[];
                    if (binaryForm != null)
                    {
                        sid = new SecurityIdentifier(binaryForm, 0);
                    }
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPAccount Sid property. {0}", e);
                }
                return sid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+String.Format(" [{0}]",AccountType);
        }
    }
}
