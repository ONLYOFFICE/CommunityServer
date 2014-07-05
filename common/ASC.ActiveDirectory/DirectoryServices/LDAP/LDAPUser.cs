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
using System.DirectoryServices;

namespace ASC.ActiveDirectory
{
    /// <summary>
    
    /// </summary>
    public class LDAPUser : LDAPAccount
    { 
        /// <summary>
        
        /// </summary>
        
        internal LDAPUser(DirectoryEntry directoryEntry) : base(directoryEntry)
        { 
        }

        /// <summary>
        
        /// </summary>
        public Constants.UserAccauntControl UserAccauntControl
        {
            get
            {
                Constants.UserAccauntControl uac = Constants.UserAccauntControl.Empty;
                try
                {
                    if (PropertyContains(Constants.ADSchemaAttributes.UserAccountControl))
                    {
                        uac = (Constants.UserAccauntControl)InvokeGet(Constants.ADSchemaAttributes.UserAccountControl);
                    }
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAP?User UserAccauntControl property. {0}", e);
                }
                return uac;
            }
        }

        /// <summary>
        
        /// </summary>
        public string FirstName
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.FirstName) as string;
            }
        }
        /// <summary>
        
        /// </summary>
        public string SecondName
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Surname) as string;
            }
        }


        /// <summary>
        
        /// </summary>
        public string Mobile
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Mobile) as string;
            }
        }


        /// <summary>
        
        /// </summary>
        public string Mail
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Mail) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string TelephoneNumber
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.TelephoneNumber) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string Title
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Title) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string Street
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Street) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string PostalCode
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.PostalCode) as string;
            }
        }


        /// <summary>
        
        /// </summary>
        public string HomePhone
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.HomePhone) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string Initials
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Initials) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string Division
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Division) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public string Company
        {
            get
            {
                return InvokeGet(Constants.ADSchemaAttributes.Company) as string;
            }
        }

        /// <summary>
        
        /// </summary>
        public bool IsDisabled
        {
            get {
                return (UserAccauntControl & Constants.UserAccauntControl.ADS_UF_ACCOUNTDISABLE) > 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + String.Format(" [{0}]", UserAccauntControl);
        }
    }
}
