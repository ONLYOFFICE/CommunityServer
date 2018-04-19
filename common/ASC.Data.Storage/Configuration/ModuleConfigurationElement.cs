/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class ModuleConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.TYPE, IsRequired = true)]
        public string Type
        {
            get { return (string) this[Schema.TYPE]; }
            set { this[Schema.TYPE] = value; }
        }

        [ConfigurationProperty(Schema.DOMAINS)]
        public DomainConfigurationCollection Domains
        {
            get { return (DomainConfigurationCollection) base[Schema.DOMAINS]; }
        }

        [ConfigurationProperty(Schema.ACL, DefaultValue = ACL.Read)]
        public ACL Acl
        {
            get { return (ACL) this[Schema.ACL]; }
            set { this[Schema.ACL] = value; }
        }

        [ConfigurationProperty(Schema.EXPIRES)]
        public TimeSpan Expires
        {
            get
            {
                var val = base[Schema.EXPIRES];
                return val == null ? TimeSpan.Zero : (TimeSpan)val;
            }
            set { this[Schema.EXPIRES] = value; }
        }

        [ConfigurationProperty(Schema.PATH)]
        public string Path
        {
            get { return (string) this[Schema.PATH]; }
            set { this[Schema.PATH] = value; }
        }

        [ConfigurationProperty(Schema.DATA)]
        public string Data
        {
            get { return (string) this[Schema.DATA]; }
            set { this[Schema.DATA] = value; }
        }

        [ConfigurationProperty(Schema.APPEND_TENANT_ID, DefaultValue = true)]
        public bool AppendTenant
        {
            get { return (bool)this[Schema.APPEND_TENANT_ID]; }
            set { this[Schema.APPEND_TENANT_ID] = value; }
        }
        
        [ConfigurationProperty(Schema.VISIBLE, DefaultValue = true)]
        public bool Visible
        {
            get { return (bool) this[Schema.VISIBLE]; }
            set { this[Schema.VISIBLE] = value; }
        }

        [ConfigurationProperty(Schema.COUNT_QUOTA, DefaultValue = true)]
        public bool Count
        {
            get { return (bool)this[Schema.COUNT_QUOTA]; }
            set { this[Schema.COUNT_QUOTA] = value; }
        }

        [ConfigurationProperty(Schema.VIRTUALPATH)]
        public string VirtualPath
        {
            get { return (string) this[Schema.VIRTUALPATH]; }
            set { this[Schema.VIRTUALPATH] = value; }
        }

        [ConfigurationProperty(Schema.PUBLIC)]
        public bool Public
        {
            get { return (bool)this[Schema.PUBLIC]; }
            set { this[Schema.PUBLIC] = value; }
        }
    }
}