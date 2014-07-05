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
using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class DomainConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.TYPE, IsRequired = false)]
        public string Type
        {
            get { return (string) this[Schema.TYPE]; }
            set { this[Schema.TYPE] = value; }
        }

        [ConfigurationProperty(Schema.PATH, IsRequired = false)]
        public string Path
        {
            get { return (string) this[Schema.PATH]; }
            set { this[Schema.PATH] = value; }
        }

        [ConfigurationProperty(Schema.DATA, IsRequired = false)]
        public string Data
        {
            get { return (string) this[Schema.DATA]; }
            set { this[Schema.DATA] = value; }
        }

        [ConfigurationProperty(Schema.QUOTA, IsRequired = false, DefaultValue = long.MaxValue)]
        public long Quota
        {
            get { return (long) this[Schema.QUOTA]; }
            set { this[Schema.QUOTA] = value; }
        }

        [ConfigurationProperty(Schema.VIRTUALPATH, IsRequired = false)]
        public string VirtualPath
        {
            get { return (string) this[Schema.VIRTUALPATH]; }
            set { this[Schema.VIRTUALPATH] = value; }
        }

        [ConfigurationProperty(Schema.OVERWRITE, IsRequired = false, DefaultValue = true)]
        public bool Overwrite
        {
            get { return (bool) this[Schema.OVERWRITE]; }
            set { this[Schema.OVERWRITE] = value; }
        }

        [ConfigurationProperty(Schema.ACL, IsRequired = false, DefaultValue = ACL.Read)]
        public ACL Acl
        {
            get { return (ACL) this[Schema.ACL]; }
            set { this[Schema.ACL] = value; }
        }

        [ConfigurationProperty(Schema.EXPIRES, IsRequired = false)]
        public TimeSpan Expires
        {
            get { return (TimeSpan) this[Schema.EXPIRES]; }
            set { this[Schema.EXPIRES] = value; }
        }

        [ConfigurationProperty(Schema.VISIBLE, IsRequired = false, DefaultValue = true)]
        public bool Visible
        {
            get { return (bool) this[Schema.VISIBLE]; }
            set { this[Schema.VISIBLE] = value; }
        }
    }
}