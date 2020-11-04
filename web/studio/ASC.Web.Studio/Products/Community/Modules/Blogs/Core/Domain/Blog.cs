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
using System.Collections.Generic;

namespace ASC.Blogs.Core.Domain
{
	public class Blog
    {
        private long blogID;
        private string name;
        private Guid userID;
        private Guid groupID;
        private IList<Guid> memberList = new List<Guid>();
        private IList<Post> posts = new List<Post>();

        public virtual long BlogID
        {
            get { return blogID; }
            set { blogID = value; }
        }
        public virtual BlogType BlogType
        {
            get { return this.GroupID.Equals(Guid.Empty) ? BlogType.Corporate : BlogType.Personal; }
        }
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
        public virtual Guid UserID
        {
            get { return userID; }
            set { userID = value; }
        }
        public virtual Guid GroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }
        public virtual IList<Post> Posts
        {
            get { return posts; }
            set { posts = value; }
        }

        public virtual IList<Guid> MemberList
        {
            get { return memberList; }
            set { memberList = value; }
        }
    }
}
