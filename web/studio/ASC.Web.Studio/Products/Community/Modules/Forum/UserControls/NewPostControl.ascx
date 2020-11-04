<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewPostControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.NewPostControl" %>

<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.UserControls.Common.PollForm" TagPrefix="sc" %>

<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum.Common" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum.Resources" %>

<input id="forum_postType" name="forum_postType" type="hidden" value="<%=(int)PostType%>" />
<input id="forum_attachments" name="forum_attachments" type="hidden" value="<%=_attachmentsString%>" />
<div id="forum_errorMessage">
    <%=_errorMessage%>
</div>
<div id="post_container">
    <%=RenderForumSelector()%>
    <%TopicHeader();%>
    <sc:PollFormMaster runat="server" ID="_pollMaster" />
    <div class="headerPanel-splitter">
        <div class="headerPanelSmall-splitter">
            <b><%=ForumUCResource.Message%>:</b>
        </div>
        <div>
            <textarea id="ckEditor" name="forum_text" style="width:100%; height:400px;visibility:hidden;" autocomplete="off"><%=_text%></textarea>
        </div>
    </div>
    <%if (PostAction == PostAction.Edit)%>
    <% { %>
        <div class="headerPanel-splitter">
          <% Response.Write(PostControl.AttachmentsList(EditedPost, SettingsID)); %>
        </div>
    <% } %>
    <%AddTags();%>
    <div <%=(!IsAllowCreateAttachment)?"style=\"display:none;\"":""%>>
        <div class="clearFix headerPanel-splitter">
            <div style="padding: 15px; border: 1px solid #d1d1d1" id="forum_uploadDialogContainer">
                <table id='forum_upload_select' cellpadding="0" cellspacing="0" border="0">
                    <tr valign="middle">
                        <td style="width: 50px; padding: 5px 0 0 10px;">
                            <div class="forum_uploadIcon">
                            </div>
                        </td>
                        <td height="20">
                            <div class="describeUpload">
                                <%=string.Format(ForumUCResource.MaximumAttachmentSize, ASC.Web.Studio.Core.SetupInfo.MaxUploadSize / 1024 / 1024)%></div>
                        </td>
                    </tr>
                </table>
                <div id="forum_overallprocessHolder" style="display:none;">
                </div>
                <div id="forum_uploadContainer" class="forum_uploadContainer">
                </div>
                <div id="forum_upload_pnl" class="clearFix">
                <a class="button gray forum_uploadButton" id="forum_uploadButton"><%=ForumUCResource.FileUploadAddButton%></a>
            </div>
            </div>

        </div>
    </div>

    <div class="clearFix">
        <div id="panel_buttons" class="middle-button-container">
            <a id="createPostBth" class="button blue middle" onclick="ForumManager.SendMessage()"><%=ForumUCResource.PublishButton%></a>
            <span class="splitter-buttons"></span>
            <% if (string.IsNullOrEmpty(_text))
            { %>
            <a id="btnPreview" class="button blue middle disable" onclick="ForumManager.PreviewEditor()"><%= ForumUCResource.PreviewButton %></a>
        <% } else { %>
            <a id="btnPreview" class="button blue middle" onclick="ForumManager.PreviewEditor()"><%= ForumUCResource.PreviewButton %></a>
        <% } %>
            <span class="splitter-buttons"></span>
            <a class="button gray middle cancelFckEditorChangesButtonMarker" onclick="javascript:ForumManager.BlockButtons(); ForumManager.CancelPost('<%=(EditedPost == null)?"":EditedPost.ID.ToString()%>');">
                <%=ForumUCResource.CancelButton%>
            </a>
            <div style="display:inline-block; vertical-align: middle; margin-left: 17px;">
                <%=RenderSubscription()%>
            </div>
        </div>
    </div>
</div>

<div id="forum_previewBoxFCK" style="display: none;">
    <div class="headerPanel" style="margin-top:20px;">
            <%=ForumUCResource.PagePreview%>
    </div>
    <div class="borderBase clearFix" style="padding: 10px 0px;
    border-left: none; border-right: none;">
    <table cellpadding="0" cellspacing="0" style="width: 100%;">
        <tr valign="top">
            <%--user info--%>
            <td align="center" style="width: 200px;">
                <div class="forum_postBoxUserSection" style="overflow: hidden; width: 150px;">
                    <%="<a class=\"link bold\"  href=\"" + CommonLinkUtility.GetUserProfile(_currentUser.ID) + "\"><span>" + ASC.Web.Core.Users.DisplayUserSettings.GetFullUserName(_currentUser) + "</span></a>"%>
                    <div style="margin: 5px 0px;" class="text-medium-describe">
                        <%=HttpUtility.HtmlEncode(_currentUser.Title ?? "")%>
                    </div>
                    <a href="<%=CommonLinkUtility.GetUserProfile(_currentUser.ID) %>">
                        <%=_settings.ForumManager.GetHTMLImgUserAvatar(_currentUser.ID)%>
                    </a>
                </div>
            </td>
            <td>
                <div style="margin-bottom: 5px; padding: 0px 5px;">
                    <%=DateTimeService.DateTime2StringPostStyle(DateTimeService.CurrentDate())%>
                </div>
                <div id="forum_message_previewfck" class="forum_mesBox">
                </div>
            </td>
        </tr>
    </table>    
    </div>
    <div style='margin-top:15px;'>
        <a class="button gray" href='javascript:void(0);' onclick='ForumManager.HidePreview(); return false;'>
            <%=ForumUCResource.HideButton%>
        </a>
    </div>
</div>
<asp:PlaceHolder ID="_recentPostsHolder" runat="server"/>