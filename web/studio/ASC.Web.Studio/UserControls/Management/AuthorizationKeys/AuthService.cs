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


using System.Collections.Generic;
using System.Diagnostics;
using ASC.Core.Common.Configuration;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    public class AuthService
    {
        public Consumer Consumer { get; set; }

        public string Name { get { return Consumer.Name; } }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Instruction { get; private set; }

        public bool CanSet { get { return Consumer.CanSet; } }

        public int? Order { get { return Consumer.Order; } }

        public List<AuthKey> Props { get; private set; }

        public AuthService(Consumer consumer)
        {
            Consumer = consumer;
            Title = consumer.GetResourceString(consumer.Name) ?? consumer.Name;
            Description = consumer.GetResourceString(consumer.Name + "Description");
            Instruction = consumer.GetResourceString(consumer.Name + "InstructionV11");
            Props = new List<AuthKey>();

            foreach (var item in consumer.ManagedKeys)
            {
                Props.Add(new AuthKey { Name = item, Value = Consumer[item], Title = consumer.GetResourceString(item) ?? item });
            }
        }
    }

    public static class ConsumerExtension
    {
        public static string GetResourceString(this Consumer consumer, string resourceKey)
        {
            try
            {
                Resource.ResourceManager.IgnoreCase = true;
                return Resource.ResourceManager.GetString("Consumers" + resourceKey);
            }
            catch
            {
                return null;
            }
        }
    }

    [DebuggerDisplay("({Name},{Value})")]
    public class AuthKey
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
