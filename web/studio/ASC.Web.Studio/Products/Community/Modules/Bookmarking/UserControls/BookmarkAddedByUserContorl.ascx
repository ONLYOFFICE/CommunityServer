<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkAddedByUserContorl.ascx.cs"
	Inherits="ASC.Web.UserControls.Bookmarking.BookmarkAddedByUserContorl" %>

<div id="<%=DivID%>" class='borderBase bookmarks-row'>
	<table style="width: 100%;" cellpadding="8" cellspacing="0">
		<tr>
			<td style="width: 82px;">
				<%=UserImage%>
			</td>
			<td class="longWordsBreak" style="width: 200px;">
				<%=UserPageLink%>
			</td>
			<td class="bookmarkingGreyText">
				<div class="longWordsBreak">
					<%=UserBookmarkDescription%>
				</div>
			</td>
			<td class="bookmarkingGreyText" style="width: 120px; text-align: right;">
				<%=DateAddedAsString%>
			</td>
		</tr>
	</table>
</div>
