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
	public class ReviewPost
    {
        private Guid _ReviewID;
        private Post _Post;
        private Guid _UserID;
        private DateTime _Timestamp;
        private int _Count;

        public virtual Guid ReviewID
        {
            get { return _ReviewID; }
            set { _ReviewID = value; }
        }
        public virtual Post Post
        {
            get { return _Post; }
            set { _Post = value; }
        }
        public virtual Guid UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }
        public virtual DateTime Timestamp
        {
            get { return _Timestamp; }
            set { _Timestamp = value; }
        }
        public virtual int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
    }
}
