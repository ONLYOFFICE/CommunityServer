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

using System;

#endregion

namespace ASC.Bookmarking.Pojo
{
	public class Comment
	{
	    public virtual Guid ID { get; set; }		

		public virtual Guid UserID { get; set; }

		public virtual string Content { get; set; }

		public virtual DateTime Datetime { get; set; }

		public virtual string Parent { get; set; }

		public virtual long BookmarkID { get; set; }

	    public virtual Bookmark Bookmark { get; set; }

        public Comment()
        {
        }

	    public Comment(Bookmark bookmark)
        {
            Bookmark = bookmark;
        }

		public virtual bool Inactive { get; set; }

		// override object.Equals
		public override bool Equals(object obj)
		{
			//       
			// See the full list of guidelines at
			//   http://go.microsoft.com/fwlink/?LinkID=85237  
			// and also the guidance for operator== at
			//   http://go.microsoft.com/fwlink/?LinkId=85238
			//

			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var c = obj as Comment;
			return c != null && ID.Equals(c.ID);
		}

		public override int GetHashCode()
		{
			return (GetType().FullName + "|" + ID).GetHashCode();
		}						
	}
}
