<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoadPhotoControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.LoadPhotoControl" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace=" ASC.Web.Core.Users" %>
<%@ Import Namespace=" ASC.Web.Studio.Core" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="divLoadPhotoWindow">
        <sc:Container ID="_ctrlLoadPhotoContainer" runat="server">
            <header>
            <%= Resource.ChooseProfilePhoto%>
        </header>
            <body>
                <div id="divLoadPhotoFromPC">
                    <h4>
                        <%= Resource.LoadPhotoFromPC%></h4>
                    <div class="describe-text">
                        <%= FileSizeComment.GetFileImageSizeNote(Resource.ContactPhotoInfo, true)%>
                    </div>
                    <div>
                        <a id="changeLogo" class="button middle gray"><%= Resource.Browse %>...</a>
                        <span id="userProfilePhotoDscr"><%= Resource.NoFileSelected %></span>
                        <span id="userProfilePhotoError" class="red-text"></span>
                    </div>
                    <h4><%=Resource.ReturnDefaultImage %></h4>
                    <img class="default-image" data-src="<%= UserPhotoManager.GetDefaultPhotoAbsoluteWebPath() %>" 
                        src="<%= UserPhotoManager.GetDefaultMediumPhotoURL() %>" />
                </div>
            </body>
        </sc:Container>
 </div>