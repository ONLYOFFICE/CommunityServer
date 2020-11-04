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


namespace ASC.Bookmarking.Pojo
{
	public class UserBookmarkTag
	{
		public virtual long UserBookmarkTagID { get; set; }

		public virtual long UserBookmarkID { get; set; }

		public virtual long TagID { get; set; }

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			
			var tag = obj as UserBookmarkTag;
			if (this.TagID.Equals(tag.TagID))
			{
				return true;
			}
			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{			
			var hash = TagID.GetHashCode();
			return hash;
		}

	}
}
