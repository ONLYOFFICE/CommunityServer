<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleBookmarkUserControl.ascx.cs"
	Inherits="ASC.Web.UserControls.Bookmarking.SingleBookmarkUserControl" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>

<div class="clearFix <% if(!IsBookmarkInfoMode){ %> borderBase bookmarks-row <% } %>" id="<%=GetSingleBookmarkID() %>">
    <table class="tableBase" width="100%" cellpadding="10" cellspacing="0">
        <tr>
            <td style="width: 96px; vertical-align: top">
			<%--Thumbnail--%>
				<a href="<%=URL %>" title="<%=UserBookmarkName %>" target="_blank" style="color: White; border: solid 0px White;">
					<img src="App_Themes/default/images/noimageavailable.jpg" data-url="<%= GetThumbnailUrl() %>" alt="<%=URL %>" class="bookmarkingThumbnail" title="<%=UserBookmarkName %>"
						width="96" height="72" id='<%=System.Guid.NewGuid().ToString()%>'/>
				</a>
			</td>
			<td class="bookmarks-content">
				<%--Bookmark header--%>
				<div class="clearFix">
					<%--Raiting panel--%>
					<div style="float: right;" id="<%=GetUniqueId() %>">
						<%=GetUserBookmarkRaiting() %>
					</div>			
					<a href="<%=URL %>" title="<%=HttpUtility.HtmlEncode(UserBookmarkDescription)%>" class="link bold" target="_blank"
						id="<%=GetSingleBookmarkID() %>Name"><%=UserBookmarkName %></a>			
				</div>
				
				<%--Bookmark description--%>
				<%if(HasDescription()) { %>
				<div class="clearFix describe-text" id="<%=GetSingleBookmarkID() %>Description"><%=UserBookmarkDescription %></div>
				<% } %>
				
				<%--Tags, use for add bookmark to favourites only--%>
				<div id="<%=GetSingleBookmarkID() %>Tags" style="display: none;"><%=HttpUtility.HtmlEncode(UserTagsString)%></div>

				<%--Creator, comments and tags--%>	
				<div class="clearFix bookmarkingSingleBookmarkDescriptionArea">
				
					<%--Details link--%>
					<%if(!IsBookmarkInfoMode) { %>
						<div style="float: left;" class="bookmarkingDetailsDivWithImage bookmarkingAreaWithRightMargin10">				
							<a href="<%=HttpUtility.HtmlEncode(GetBookmarkInfoUrl())%>&selectedtab=bookmarkaddedbytab" class="link"
								title="<%=BookmarkingUCResource.BookmarkDetailsLinkTitle %>">
								<%=BookmarkingUCResource.Details%>
							</a>
						</div>
					<% } else if(IsCurrentUserBookmark()) { %>
						<div style="float: left;" class="bookmarkingEditDivWithImage bookmarkingAreaWithRightMargin10">				
							<a href="javascript:void(0);" class="link"
								title="<%=BookmarkingUCResource.EditFavouriteBookmarkLinkTitle %>"
								onclick="addBookmarkToFavourite('<%=URL.ReplaceSingleQuote()%>',
																'<%=GetUniqueIDFromSingleBookmark(GetSingleBookmarkID()) %>',
																'<%=GetSingleBookmarkID() %>',
																'<%=GetUniqueIDFromSingleBookmark(GetSingleBookmarkID()) %>');">
								<%=BookmarkingUCResource.EditFavouriteBookmarkLink%>
							</a>
						</div>
					<% } %>
					
					<%--Bookmark creator--%>
					<div style="float: left;" class="bookmarkingAreaWithRightMargin10">
						<%=RenderProfileLink() %>
					</div>
					
					<%--Comments--%>			
					<%if(!IsBookmarkInfoMode && (CommentsCount != 0)){ %>
						<div style="float: left;" class="bookmarkingAreaWithRightMargin10">
							<div style="float:left" class="bookmarkingCommentsDiv">&nbsp;</div>
							<a href="<%=HttpUtility.HtmlEncode(GetBookmarkInfoUrl())%>&selectedtab=bookmarkcommnetstab" class="link" 
								title="<%=BookmarkingUCResource.CommentsLinkTitle %>"><%=CommentString%></a>
						</div>
					<%} %>
				</div>
				<%--Tags--%>
				<div class="clearFix bookmarkingSingleBookmarkDescriptionArea">
					<%if (IsTagsIncluded()) {%>
					<%--User tags if exists--%>			
					<span class="text-medium-describe" style="margin-top: 2px;">
						<asp:Repeater runat="server" ID="TagsRepeater">
							<HeaderTemplate>
								<div style="float: left;" class="bookmarkingTag16">&nbsp;</div>
							</HeaderTemplate>
							<ItemTemplate>
								<a href="<%#GetSearchByTagUrl(DataBinder.Eval(Container.DataItem, "Name"))%>"
                                    class="link" style="margin-left: 3px;">
									<%#HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "Name").ToString())%></a></ItemTemplate>
							<%--Separator--%>
							<SeparatorTemplate>,&nbsp;</SeparatorTemplate>
						</asp:Repeater>
					</span>	
					<%}%>
				</div>
	        </td>
        </tr>
    </table>
		<%--Panel to attach add bookmark to favourites--%>
		<div class="clearFix" id="<%=GetUniqueId()%>ToAppend"></div>


</div>