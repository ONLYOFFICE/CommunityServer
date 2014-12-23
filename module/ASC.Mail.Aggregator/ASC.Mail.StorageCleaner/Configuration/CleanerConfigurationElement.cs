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

namespace ASC.Mail.StorageCleaner.Configuration
{
    public class CleanerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.property_max_threads, IsRequired = false, IsKey = false, DefaultValue = 1)]
        public int MaxThreads
        {
            get { return (int)this[Schema.property_max_threads]; }
            set { this[Schema.property_max_threads] = value; }
        }

        [ConfigurationProperty(Schema.property_tasks_chunk_size, IsRequired = false, IsKey = false, DefaultValue = 10)]
        public int TasksChunkSize
        {
            get { return (int)this[Schema.property_tasks_chunk_size]; }
            set { this[Schema.property_tasks_chunk_size] = value; }
        }

        [ConfigurationProperty(Schema.property_tasks_gen_chunks_count, IsRequired = false, IsKey = false, DefaultValue = 10)]
        public int TasksGenChunkCount
        {
            get { return (int)this[Schema.property_tasks_gen_chunks_count]; }
            set { this[Schema.property_tasks_gen_chunks_count] = value; }
        }

        [ConfigurationProperty(Schema.property_db_lock_name, IsRequired = false, IsKey = false, DefaultValue = "storage_cleaner")]
        public string DbLockName
        {
            get { return (string)this[Schema.property_db_lock_name]; }
            set { this[Schema.property_db_lock_name] = value; }
        }

        [ConfigurationProperty(Schema.property_db_lock_timeout, IsRequired = false, IsKey = false, DefaultValue = 5)]
        public int DbLockTimeot
        {
            get { return (int)this[Schema.property_db_lock_timeout]; }
            set { this[Schema.property_db_lock_timeout] = value; }
        }

        [ConfigurationProperty(Schema.property_watchdog_timeout, IsRequired = false, IsKey = false, DefaultValue = 600)]
        public int WatchdogTimeout
        {
            get { return (int)this[Schema.property_watchdog_timeout]; }
            set { this[Schema.property_watchdog_timeout] = value; }
        }
    }
}