/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System.Configuration;

namespace ASC.Mail.Aggregator.CollectionService.Configuration
{
    public class MailQueueConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.configuration_property_check_interval, IsRequired = false, IsKey = false, DefaultValue = 1)]
        public int CheckInterval
        {
            get { return (int)this[Schema.configuration_property_check_interval]; }
            set { this[Schema.configuration_property_check_interval] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_concurentthreads, IsRequired = false, IsKey = false, DefaultValue = 5)]
        public int Threads
        {
            get { return (int)this[Schema.configuration_property_concurentthreads]; }
            set { this[Schema.configuration_property_concurentthreads] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_newmessagespersession, IsRequired = false, IsKey = false, DefaultValue = 200)]
        public int MaxNewMessages
        {
            get { return (int)this[Schema.configuration_property_newmessagespersession]; }
            set { this[Schema.configuration_property_newmessagespersession] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_activity_timeout, IsRequired = false, IsKey = false, DefaultValue = 90)]
        public int ActivityTimeout
        {
            get { return (int)this[Schema.configuration_property_activity_timeout]; }
            set { this[Schema.configuration_property_activity_timeout] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_overdueaccountdelay, IsRequired = false, IsKey = false, DefaultValue = 600)]
        public int OverdueAccountDelay
        {
            get { return (int)this[Schema.configuration_property_overdueaccountdelay]; }
            set { this[Schema.configuration_property_overdueaccountdelay] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_overdue_days, IsRequired = false, IsKey = false, DefaultValue = 10)]
        public int OverdueDays
        {
            get { return (int)this[Schema.configuration_property_overdue_days]; }
            set { this[Schema.configuration_property_overdue_days] = value; }
        }

        [ConfigurationProperty(Schema.configuration_property_tenant_caching_period, IsRequired = false, IsKey = false, DefaultValue = 86400)]
        public int TenantCachingPeriod
        {
            get { return (int)this[Schema.configuration_property_tenant_caching_period]; }
            set { this[Schema.configuration_property_tenant_caching_period] = value; }
        }
    }
}