/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System.Collections.Generic;
using System.Linq;

using ASC.Core.Common.Configuration;
using ASC.Core.Common.Settings;
using ASC.Data.Storage.Configuration;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Api.Settings
{
    public class StorageWrapper
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public List<AuthKey> Properties { get; set; }

        public bool Current { get; set; }

        public bool IsSet { get; set; }

        public StorageWrapper(DataStoreConsumer consumer, StorageSettings current)
        {
            StorageWrapperInit(consumer, current);
        }

        public StorageWrapper(DataStoreConsumer consumer, CdnStorageSettings current)
        {
            StorageWrapperInit(consumer, current);
        }

        private void StorageWrapperInit<T>(DataStoreConsumer consumer, BaseStorageSettings<T> current) where T : class, ISettings, new()
        {
            Id = consumer.Name;
            Title = consumer.GetResourceString(consumer.Name) ?? consumer.Name;
            Current = consumer.Name == current.Module;
            IsSet = consumer.IsSet;

            var props = Current
                ? current.Props
                : current.Switch(consumer).AdditionalKeys.ToDictionary(r => r, a => consumer[a]);

            Properties = props.Select(
                r => new AuthKey
                {
                    Name = r.Key,
                    Value = r.Value,
                    Title = consumer.GetResourceString(consumer.Name + r.Key) ?? r.Key,
                    Description = consumer.GetResourceString(consumer.Name + r.Key + "Description"),
                }).ToList();
        }
    }
}
