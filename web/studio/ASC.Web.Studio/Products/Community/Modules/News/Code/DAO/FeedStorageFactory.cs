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


#region Usings

using ASC.Common.Web;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.Community.News.Code.DAO
{
    public static class FeedStorageFactory
	{
        public static string Id = "community";

        private const string Key = "FeedStorageKey";

        public static IFeedStorage Create()
		{
			var ctx = DisposableHttpContext.Current;
			
			var storage = ctx[Key] as IFeedStorage;
			if (storage == null)
			{
				storage = new DbFeedStorage(TenantProvider.CurrentTenantID);
				ctx[Key] = storage;
			}
			return storage;
		}

        public static IFeedStorage Create(bool useCache)
        {
            return !useCache ? new DbFeedStorage(TenantProvider.CurrentTenantID) : Create();
        }
	}
}
