<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddContentControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.AddContentControl" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_AddContentDialog" class="display-none">
  <sc:Container runat="server" ID="AddContentContainer">
    <Header>
      <%=Resources.Resource.AddContentTitle%>
    </Header>
    <Body>
      <div id="studio_AddContentContent">
        <asp:Repeater runat="server" ID="ContentTypesRepeater" ItemType=" ASC.Web.Studio.UserControls.Users.AddContentControl.ContentTypes">
          <HeaderTemplate>
            <ul class="types">
          </HeaderTemplate>
          <ItemTemplate>
            <li class="type even">
              <a class="item" href="<%#Item.Link%>">
                <img class="icon" src="<%#Item.Icon%>" alt="" />
                <span class="label"><%#HttpUtility.HtmlEncode(Item.Label)%></span>   
              </a>
            </li>
          </ItemTemplate>
          <FooterTemplate>
            </ul>
          </FooterTemplate>
        </asp:Repeater>

        <div class="clearFix middle-button-container">        
          <a class="btn button gray float-left" href="./" onclick="return StudioManager.CloseAddContentDialog()"><%=Resources.Resource.CancelButton %></a>
        </div>
      </div>
    </Body>
  </sc:Container>
</div>
