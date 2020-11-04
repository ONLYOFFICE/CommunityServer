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


using System;

namespace ASC.Web.Core
{
    public abstract class SpaceUsageStatManager
    {
        public class UsageSpaceStatItem        
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string ImgUrl { get; set; }
            public bool Disabled { get; set; }
            public long SpaceUsage { get; set; }
        }

        public abstract System.Collections.Generic.List<UsageSpaceStatItem> GetStatData();
    }

    public interface IUserSpaceUsage
    {
        long GetUserSpaceUsage(Guid userId);
    }
}
