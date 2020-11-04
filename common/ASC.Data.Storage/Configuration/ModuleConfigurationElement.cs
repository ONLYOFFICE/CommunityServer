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

        [ConfigurationProperty(Schema.DISABLEDMIGRATE)]
        public bool DisabledMigrate
        {
            get { return (bool)this[Schema.DISABLEDMIGRATE]; }
            set { this[Schema.DISABLEDMIGRATE] = value; }
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

        [ConfigurationProperty(Schema.DISABLEDENCRYPTION)]
        public bool DisabledEncryption
        {
            get { return (bool)this[Schema.DISABLEDENCRYPTION]; }
            set { this[Schema.DISABLEDENCRYPTION] = value; }
        }
    }
}