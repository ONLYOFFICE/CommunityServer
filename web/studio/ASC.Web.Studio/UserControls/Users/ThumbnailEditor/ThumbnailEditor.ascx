<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThumbnailEditor.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.ThumbnailEditor" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="usrdialog_<%= SelectorID %>" class="display-none">
<sc:Container runat="server" ID="_container">
<Header>
    <%= (Title ?? "").HtmlEncode() %>
</Header>
<Body>
    <div class="clearFix">
		<table cellpadding="0" cellspacing="0" width="100%">
			<tr>
				<td valign="top">
					<div class="thumbnailMainImg">
						<img id="mainimg_<%=SelectorID%>" src="<%=MainImgUrl%>" alt="" />
					</div>
				</td>
				<td class="thumbnailCell"></td>
				<td valign="top">
					<span class="thumbnailCaption describe-text"><%=Description%></span>
					<div>
						<asp:PlaceHolder runat="server" ID="placeThumbnails"></asp:PlaceHolder>
					</div>
				</td>
			</tr>
        </table>
    </div>
    <div class="clearFix middle-button-container">
        <a class="button blue middle" href="javascript:void(0);" onclick="<%=BehaviorID%>.ApplyAndCloseDialog(); return false;"><%= Resources.Resource.SaveButton %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle" href="javascript:void(0);" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;"><%= Resources.Resource.CancelButton %></a>
    </div>
    <input type="hidden" id="UserIDHiddenInput" value="<%= UserID %>" />
</Body>
</sc:Container>
</div>