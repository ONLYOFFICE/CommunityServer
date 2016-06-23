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


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.Storage;

namespace ASC.Api.Impl
{
    internal class ApiKeyValueInMemoryStorage : IApiKeyValueStorage
    {
        private static readonly Hashtable Cache = Hashtable.Synchronized(new Hashtable());

        #region IApiKeyValueStorage Members

        public object Get(IApiEntryPoint entrypoint, string key)
        {
            return Cache[GetKey(entrypoint, key)];
        }

        public void Set(IApiEntryPoint entrypoint, string key, object @object)
        {
            Cache[GetKey(entrypoint, key)] = @object;
        }

        public bool Exists(IApiEntryPoint entrypoint, string key)
        {
            return Cache.ContainsKey(GetKey(entrypoint, key));
        }

        public void Remove(IApiEntryPoint entrypoint, string key)
        {
            Cache.Remove(GetKey(entrypoint, key));
        }

        public void Clear(IApiEntryPoint entrypoint)
        {
            IEnumerable<string> toRemove = Cache.Keys.Cast<string>().Where(x => x.StartsWith(entrypoint.GetType() + ":"));
            foreach (string remove in toRemove)
            {
                Cache.Remove(remove);
            }
        }

        #endregion

        private static string GetKey(IApiEntryPoint point, string key)
        {
            return point.GetType() + ":" + key;
        }
    }
}