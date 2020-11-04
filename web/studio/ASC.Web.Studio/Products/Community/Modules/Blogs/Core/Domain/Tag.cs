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

namespace ASC.Blogs.Core.Domain
{
    public class TagStat {
        public string Name;
        public int Count;
    }
	public class Tag
    {
        private Guid _ID;
        //private Guid _UserID;
        private string _Content;
        //private DateTime _Datetime;
        private Post _Post;

        public Guid PostId;

        public Tag() { }

        public Tag(Post blog)
        {
            _Post = blog;
            //_Blog.AddTag(this);
        }

        public virtual Guid ID
        {
            get { return _ID; }
            set { _ID = value; }
        }        
        public virtual Post Post
        {
            get { return _Post; }
            protected set { _Post = value; }
        }
        //public virtual Guid UserID
        //{
        //    get { return _UserID; }
        //    set { _UserID = value; }
        //}
        public virtual string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        //public virtual DateTime Datetime
        //{
        //    get { return _Datetime; }
        //    set { _Datetime = value; }
        //}

        /// <summary>
        /// Hash code should ONLY contain the "business value signature" of the object and not the ID
        /// </summary>
        public override int GetHashCode()
        {
            return (GetType().FullName + "|" +
                    _ID.ToString()).GetHashCode();
        }

    }
}
