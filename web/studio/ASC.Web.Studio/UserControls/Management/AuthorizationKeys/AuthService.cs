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
            Instruction = consumer.GetResourceString(consumer.Name + "Instruction");
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
