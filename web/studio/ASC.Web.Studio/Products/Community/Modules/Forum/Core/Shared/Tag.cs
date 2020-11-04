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


using System.Reflection;

namespace ASC.Forum
{
    public class Tag
    {
        public virtual int ID { get; set; }

        public virtual string Name { get; set; }

        public virtual bool IsApproved { get; set; }

        public virtual int TenantID { get; set; }
    }


	public class RankTag : Tag
    {
        

        public virtual int Rank { get; internal set; }

        public RankTag()
        {
            this.Rank = 0;
        }

        public RankTag(int id, string name)
        {
            this.ID = id;
            this.Name = name;
            this.IsApproved = false;
            this.Rank = 0;
        }
    }
}
