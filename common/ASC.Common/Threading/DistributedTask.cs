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


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Common.Threading
{
    public class DistributedTask : ISerializable
    {
        private readonly Dictionary<string, string> props = new Dictionary<string, string>();


        internal Action<DistributedTask> Publication { get; set; }

        public string InstanseId { get; internal set; }

        public string Id { get; private set; }

        public DistributedTaskStatus Status { get; internal set; }

        public AggregateException Exception { get; internal set; }



        public DistributedTask()
        {
            Id = Guid.NewGuid().ToString();
        }

        protected DistributedTask(SerializationInfo info, StreamingContext context)
        {
            InstanseId = info.GetValue("InstanseId", typeof(object)).ToString();
            Id = info.GetValue("Id", typeof(object)).ToString();
            Status = (DistributedTaskStatus)info.GetValue("Status", typeof(DistributedTaskStatus));
            Exception = (AggregateException)info.GetValue("Exception", typeof(AggregateException));
            foreach (var p in info)
            {
                if (p.Name.StartsWith("_"))
                {
                    props[p.Name.TrimStart('_')] = p.Value.ToString();
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("InstanseId", InstanseId);
            info.AddValue("Id", Id);
            info.AddValue("Status", Status);
            info.AddValue("Exception", Exception);
            foreach (var p in props)
            {
                info.AddValue("_" + p.Key, p.Value);
            }
        }


        public T GetProperty<T>(string name)
        {
            return props.ContainsKey(name) ? JsonConvert.DeserializeObject<T>(props[name]) : default(T);
        }

        public void SetProperty(string name, object value)
        {
            if (value != null)
            {
                props[name] = JsonConvert.SerializeObject(value);
            }
            else
            {
                props.Remove(name);
            }
        }

        public void PublishChanges()
        {
            if (Publication == null)
            {
                throw new InvalidOperationException("Publication not found.");
            }
            Publication(this);
        }
    }
}
