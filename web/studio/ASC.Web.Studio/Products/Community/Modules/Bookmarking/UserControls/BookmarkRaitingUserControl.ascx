<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkRaitingUserControl.ascx.cs"
	Inherits="ASC.Web.UserControls.Bookmarking.BookmarkRaitingUserControl" %>	
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>


<div>
	<%--Add to favourite--%>	
	<%if (IsCurrentUserBookmark()) { %>
		
		<%--Added to favourites, click to remove--%>
		<%if (!SimpleMode && PermissionCheckRemoveFromFavourite()) {%>
		<div style="float: left;" class="bookmarkingRemoveFromFavourites  bookmarkingAreaWithHand"
				title="<%=BookmarkingUCResource.RemoveBookmarkFromFavouritesLinkTitle %>"
				onclick="removeBookmarkFromFavouriteConfirmDialog(	'<%=GetBookmarkID()%>',
																	'<%=GetBookmarkInfoUrl().ReplaceSingleQuote()%>',
																	'<%=SingleBookmarkDivID%>',
																	<%=FavouriteBookmarksMode%>,
																	'<%=GetUniqueIDFromSingleBookmark() %>');">&nbsp;</div>
		<%} %>
		<div style="float: left" class="bookmarkingAreaWithHand"
				title="<%=BookmarkingUCResource.BookmarkRaitingLinkTitle %>"
				onclick="navigateToBookmarkUrl('<%=GetBookmarkInfoUrlAddedByTab() %>');">
			<div style="float: left;" class="bookmarkingRaitingCenterGold">&nbsp;</div>
			
			<div style="float: left;" class="bookmarkingRaitingGoldLabel" >
				<div style="margin-top: 2px;"><%=SimpleModeRaiting%></div>
			</div>
			
			<div style="float: left;" class="bookmarkingRaitingRightGold">&nbsp;</div>
		</div>
				
		
	<%} else{ %>					
						
		<%--Add to favourites--%>
		<%if (!SimpleMode && PermissionCheckAddToFavourite()) {%>
			<div class="bookmarkingAddToFavouritesPlus bookmarkingAreaWithHand" style="float: left;"
				title="<%=BookmarkingUCResource.AddBookmarkToFavouriteLinkTitle %>"
				onclick="	jq(this).hide();
							addBookmarkToFavourite(	'<%=URL.ReplaceSingleQuote()%>',													
													'<%=DivId %>',
													'<%=SingleBookmarkDivID%>',
													'<%=GetUniqueIDFromSingleBookmark() %>');
							jq(this).show();">
				&nbsp;
			</div>
		<%} %>
		<div style="float: left" class="bookmarkingAreaWithHand"
				title="<%=BookmarkingUCResource.BookmarkRaitingLinkTitle %>"
				onclick="navigateToBookmarkUrl('<%=GetBookmarkInfoUrlAddedByTab() %>');">
			<div style="float: left;" class="bookmarkingRaitingCenterGrey">&nbsp;</div>			
			
			<div style="float: left;" class="bookmarkingRaitingLabel">
				<div style="margin-top: 2px;"><%=SimpleModeRaiting%></div>
			</div>
			
			<div style="float: left;" class="bookmarkingRaitingRight">&nbsp;</div>
		</div>
		
	<%} %>
			
</div>