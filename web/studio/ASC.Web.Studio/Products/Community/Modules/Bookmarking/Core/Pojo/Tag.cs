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
	public class Tag
	{
		public virtual long TagID { get; set; }

		public virtual string Name { get; set; }

		public virtual long Populatiry { get; set; }

		public long BookmarkID { get; set; }

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

			var t = obj as Tag;

			if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(t.Name) && Name.Equals(t.Name))
			{
				return true;
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return (GetType().FullName + "|" + TagID.ToString()).GetHashCode();
		}
	}
}
