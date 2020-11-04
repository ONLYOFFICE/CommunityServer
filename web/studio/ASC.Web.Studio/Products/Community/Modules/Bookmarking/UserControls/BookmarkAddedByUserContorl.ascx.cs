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
using System.IO;
using System.Text;
using System.Web.UI;

namespace ASC.Web.UserControls.Bookmarking
{
	public partial class BookmarkAddedByUserContorl : System.Web.UI.UserControl
	{

		public const string Location = "~/Products/Community/Modules/Bookmarking/UserControls/BookmarkAddedByUserContorl.ascx";

		public bool TintFlag { get; set; }

		public string UserImage { get; set; }

		public string UserPageLink { get; set; }

		public string UserBookmarkDescription { get; set; }

		public string DateAddedAsString { get; set; }

		public string DivID { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		public string GetAddedByTableItem(bool TintFlag, string UserImage, string UserPageLink,
										 string UserBookmarkDescription, string DateAddedAsString, object DivID) 
		{
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (HtmlTextWriter textWriter = new HtmlTextWriter(sw))
				{
					using (var c = LoadControl(Location) as BookmarkAddedByUserContorl)
					{
						c.DivID = DivID.ToString();
						c.TintFlag = TintFlag;
						c.UserImage = UserImage;
						c.UserPageLink = UserPageLink;
						c.UserBookmarkDescription = UserBookmarkDescription;
						c.DateAddedAsString = DateAddedAsString;
						c.RenderControl(textWriter);
					}
				}
			}
			return sb.ToString();
		}		
	}
}