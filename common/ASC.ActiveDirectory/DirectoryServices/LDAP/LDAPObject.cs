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

using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.DirectoryServices;
using System.Web.Hosting;


namespace ASC.ActiveDirectory
{
    /// <summary>
    
    /// </summary>
    
    [System.Diagnostics.DebuggerDisplay("{SchemaClassName}:{Name}")]
    public abstract class LDAPObject
    {
        #region приватные данные
        private readonly DirectoryEntry _directoryEntry = null;
        protected static ILog _log = LogManager.GetLogger(typeof(LDAPObject));
        #endregion

        #region конструктор
        /// <summary>
        
        /// </summary>
        
        internal LDAPObject(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException("directoryEntry");

            _directoryEntry = directoryEntry;
        }
        #endregion

        #region публичные свойства
        /// <summary>
        
        /// </summary>
        public Guid Guid
        {
            get
            {
                Guid id = Guid.Empty;
                try
                {
                    id = _directoryEntry.Guid;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPObject Guid. {0}", e);
                }
                return id;
            }
        }

        /// <summary>
        
        /// </summary>
        public virtual string Name
        {
            get
            {
                string name = _directoryEntry.Path;
                try
                {
                    if (PropertyContains(Constants.ADSchemaAttributes.CommonName))
                        name = InvokeGet(Constants.ADSchemaAttributes.CommonName) as string;
                    else
                        name = _directoryEntry.Name;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPObject Name. {0}", e);
                }
                return name;
            }
        }

        /// <summary>
        
        /// </summary>
        public string DistinguishedName
        {
            get
            {
                var settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);
                string dname = null;
                try
                {
                    dname = InvokeGet(settings.GroupMembership ? settings.UserAttribute : Constants.ADSchemaAttributes.DistinguishedName) as string;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPObject DN. {0}", e);
                }
                return dname;
            }
        }


        /// <summary>
        
        /// </summary>
        public string Path
        {          
            get
            {
                string path = string.Empty;
                try
                {
                    path = _directoryEntry.Path;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPObject path. {0}", e);
                }
                return path;
            }
        }

        /// <summary>
        
        /// </summary>
        public string SchemaClassName
        {
            get
            {
                string className = string.Empty;
                try
                {
                    className = _directoryEntry.SchemaClassName;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPObject className. {0}", e);
                }
                return className;
            }
        }

        /// <summary>
        
        /// </summary>
        public bool IsCriticalSystemObject
        {
            get
            {
                using (HostingEnvironment.Impersonate())
                {
                    bool isCritical = false;
                    try
                    {
                        object o = _directoryEntry.InvokeGet(Constants.ADSchemaAttributes.IsCriticalSystemObject);
                        if (o != null)
                        {
                            isCritical = (bool)o;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.ErrorFormat("Can't get IsCriticalSystemObject property. {0}", e);
                    }
                    return isCritical;
                }
            }
        }

        /// <summary>
        
        /// </summary>
        public bool HasChildren {
            get 
            {
                try
                {
                    return _directoryEntry.Children.GetEnumerator().MoveNext();
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get IsCriticalSystemObject property. {0}", e);
                }
                return false;
            }
        }

        #endregion

        /// <summary>
        
        /// </summary>
        
        
        public object InvokeGet(string properyName)
        {
            using (HostingEnvironment.Impersonate())
            {
                return _directoryEntry.InvokeGet(properyName);
            }
        }

        /// <summary>
        
        /// </summary>
        
        
        public PropertyValueCollection GetValues(string properyName)
        {
            return _directoryEntry.Properties[properyName];
        }

        /// <summary>
        
        /// </summary>
        
        /// <returns></returns>
        protected bool PropertyContains(string propertyName)
        {
            return _directoryEntry.Properties.Contains(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}:{1}", SchemaClassName, Name);
        }

    }
}
