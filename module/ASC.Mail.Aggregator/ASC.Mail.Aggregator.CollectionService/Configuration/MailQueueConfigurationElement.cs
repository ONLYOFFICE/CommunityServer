/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

        [ConfigurationProperty(Schema.configuration_property_inactive_ratio, IsRequired = false, IsKey = false, DefaultValue = 25)]
        public int InactiveRatio
        {
            get { return (int)this[Schema.configuration_property_inactive_ratio]; }
            set { this[Schema.configuration_property_inactive_ratio] = value; }
        }
    }
}