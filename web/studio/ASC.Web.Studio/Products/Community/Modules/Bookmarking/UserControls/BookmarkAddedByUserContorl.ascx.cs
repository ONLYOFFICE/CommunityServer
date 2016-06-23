/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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