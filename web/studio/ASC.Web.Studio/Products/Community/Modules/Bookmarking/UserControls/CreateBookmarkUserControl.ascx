<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateBookmarkUserControl.ascx.cs" Inherits="ASC.Web.UserControls.Bookmarking.CreateBookmarkUserControl" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins"%>
<%@ Register Assembly="ASC.Web.Community" Namespace="ASC.Web.UserControls.Bookmarking.Common" TagPrefix="ascbc" %>
<%--Create bookmark panel--%>
<div id="AddBookmarkPanel" class="bookmarkingContentArea" style="display: none;">	
	<%if (!IsNewBookmark)
   { %>
	<div id="AddBookmarkPanelToMove" class="bookmarkingCreateNewBookmarkContainer bookmarkingAddToFavouritePanelWithMargin">
		<div class="clearFix headerPanel">
			<%if (IsEditMode)
     { %>
				<%=BookmarkingUCResource.EditFavouriteBookmarkTitle%>	
			<%}
     else
     { %>
				<%=BookmarkingUCResource.AddToFavourite%>	
			<% } %>
		</div>		
	<% }
   else
   { %>
	<div id="AddBookmarkPanelToMove">
	<% } %>
		<div class="headerPanel-splitter requiredField" id="BookmarkURLPanel">
			<div class="headerPanelSmall-splitter headerPanelSmall">
				<b><%=BookmarkingUCResource.BookmarkUrl%>:</b>
			</div>
			<div class="clearFix">
				<%if (!IsNewBookmark)
      { %>
					<div style="float: left; width: calc(100% - 70px);" id="BookmarkUrlContainer">
				<% }
      else
      { %>
					<div style="float: left; width: calc(100% - 70px);" id="BookmarkUrlContainer">
				<% } %>
					<input type="text" class="bookmarkingInputUrl" onkeydown="return bookmarkInputUrlOnKeyDown(event);"
						style="background-color:#FFFFFF;" id="BookmarkUrl" name="BookmarkUrl"
						autocomplete="off"/>
				</div>
				<div id="NewBookmarkRaiting" style="float: right; margin-top: 2px;">
				</div>				
			</div>
						
			<div class="big-button-container" id="CheckBookmarkUrlButtonsPanel">
						
				<ascbc:ActionButton ButtonID="CheckBookmarkUrlLinkButton" ID="CheckBookmarkUrlLinkButton"
						OnClickJavascript="getBookmarkByUrlButtonClick(); return false;" ButtonContainer="AddBookmarkPanelToMove"
						ButtonCssClass="button blue big" runat="server"></ascbc:ActionButton>
				<span class="splitter-buttons"></span>
				<%if (!IsNewBookmark) 
                { %>			
				<a href="javascript:void(0);" class="button gray big"
						onclick="cancelButtonClick(); return false;">
				<% }
                 else
                 { %>				
				<a href="Default.aspx" class="button gray big">
				<% } %>
					<%= BookmarkingUCResource.Cancel%>
				</a>
			</div>
		</div>
		<%--Bookmark name, description and tags--%>
		<div id="BookmarkDetailsPanel" style="display: none;" class="clearFix">
			<div class="headerPanel-splitter">
				<div class="headerPanelSmall-splitter">
					<b><%=BookmarkingUCResource.BookmarkName%>:</b>
				</div>
				<div>
					<input type="text" class="bookmarkingInputText" id="BookmarkName" name="BookmarkName" 
							onkeydown="return createBookmarkOnCtrlEnterKeyDown(event);"
							maxlength="255"/>
				</div>
			</div>
			<div class="headerPanel-splitter">
				<div class="headerPanelSmall-splitter">
					<b><%=BookmarkingUCResource.BookmarkDescription%>:</b>
				</div>
				<div>
					<textarea rows="3" class="bookmarkingInputTextArea"
						id="BookmarkDescription" name="BookmarkDescription"
						onkeydown="return createBookmarkOnCtrlEnterKeyDown(event, true);"></textarea>
				</div>
			</div>
			<div class="headerPanel-splitter">
				<div class="headerPanelSmall-splitter">
					<b><%=BookmarkingUCResource.Tags%>:</b>
				</div>
				<div>
					<input type="text" autocomplete="off" maxlength="255" class="bookmarkingInputText"
							id="BookmarkTagsInput" name="BookmarkTagsInput"
							onkeydown="return createBookmarkOnCtrlEnterKeyDown(event);"/>
					<div class="text-medium-describe"><%=BookmarkingUCResource.BookmarkTagDescription%></div>
				</div>
			</div>
			
			<div class="big-button-container" id="bookmarkingCreateNewBookmarkButtonsDiv">				
				<%if (CreateBookmarkMode)
      { %>
					<ascbc:ActionButton ButtonID="SaveBookmarkButton" ID="SaveBookmarkButton"
						OnClickJavascript="createNewBookmarkButtonClick(); return false;" ButtonContainer="BookmarkDetailsPanel"
						ButtonCssClass="button blue big" EnableRedirectAfterAjax="true" runat="server"></ascbc:ActionButton>
						
					<ascbc:ActionButton ButtonID="SaveBookmarkButtonCopy" id="SaveBookmarkButtonCopy"
						OnClickJavascript="createNewBookmarkButtonClick(); return false;" ButtonContainer="BookmarkDetailsPanel"
						ButtonCssClass="button blue big" EnableRedirectAfterAjax="true" runat="server"></ascbc:ActionButton>
				<%}
      else
      { %>
					<ascbc:ActionButton ButtonID="SaveBookmarkButtonCopy" id="AddToFavouritesBookmarkButton" ButtonContainer="BookmarkDetailsPanel"
						ButtonCssClass="button blue middle" runat="server"></ascbc:ActionButton>
				<%} %>				
				<span class="splitter-buttons"></span>
				<%if (!IsNewBookmark)
                { %>			                
				<a href="javascript:void(0);" class="button gray middle" onclick="hideAddBookmarkPanelWithAnimation();">
							
				<% }
              else
              { %>				
				<a href="Default.aspx" class="button gray big">
				<% } %>
					<%= BookmarkingUCResource.Cancel%>
				</a>				
				
			</div>
		</div>
		<div id="BookmarkingAjaxRequestImage" style="display: none;">
			<img src='<%=WebImageSupplier.GetAbsoluteWebPath("loader_16.gif")%>'
				alt="Ajax request is in progress" />
		</div>
	</div>
</div>

<input type="hidden" id="EmptyBookmarkUrlErrorMessageHidden" value="<%=BookmarkingUCResource.EmptyBookmarkUrlErrorMessage %>" />


