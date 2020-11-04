<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" AutoEventWireup="true" CodeBehind="NewForum.aspx.cs" Inherits="ASC.Web.Community.Forum.NewForum" %>


<asp:Content ContentPlaceHolderID="ForumTitleContent" runat="server">
    <div class="forumsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon forums"></span>
        <span class="header"><%=ASC.Web.Community.Forum.Resources.ForumResource.AddThreadCategoryTitle%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <input type="hidden" id="forum_fmCallback" value="" />            
    <div id="forum_fmContent">    
        <div id="forum_fmMessage" class='infoPanel alert' style='margin: 10px 0;'>
        </div>
        <div class="headerPanel-splitter" id="forum_fmCategoryList">
            <%=GetCategoryList()%>
        </div>
        <div class="headerPanel-splitter requiredField" id="forum_fmCaregoryNameBox">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall-splitter headerPanelSmall">
                <b><%=ASC.Web.Community.Forum.Resources.ForumResource.ThreadCategoryName%>:</b>
            </div>
            <div>
                <input class="textEdit" style="width: 100%;" type="text" id="forum_fmCategoryName" value="" />
            </div>
        </div>
        <div class="headerPanel-splitter" id="forum_fmCaregoryDescriptionBox">
            <div class="headerPanelSmall-splitter">
                <b><%=ASC.Web.Community.Forum.Resources.ForumResource.ThreadCategoryDescription%>:</b>
            </div>
            <div>
                <textarea style="width: 100%; height: 100px; resize: none;" id="forum_fmCategoryDescription"></textarea>
            </div>
        </div>
        <div class="headerPanel-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall-splitter headerPanelSmall">
                <b><%=ASC.Web.Community.Forum.Resources.ForumResource.ThreadName%>:</b>
            </div>
            <div>
                <input class="textEdit" style="width: 100%;" type="text" id="forum_fmThreadName" value="" />
            </div>
        </div>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter">
                <b><%=ASC.Web.Community.Forum.Resources.ForumResource.ThreadDescription%>:</b>
            </div>
            <div>
                <textarea style="width: 100%; height: 100px; resize: none;" id="forum_fmThreadDescription"></textarea>
            </div>
        </div>
        <div class="big-button-container">
            <a id="createThreadCategoryBth" class="button blue big" onclick="ForumMakerProvider.SaveThreadCategory();">
                <%=ASC.Web.Community.Forum.Resources.ForumResource.CreateButton%>
            </a>
            <span class="splitter-buttons"></span>
            <a class="button gray big" onclick="ForumManager.BlockButtons(); ForumManager.CancelForum('<%=""%>')">
                <%=ASC.Web.Community.Forum.Resources.ForumResource.CancelButton%>
            </a>
        </div>
    </div>
    <div id="forum_fmInfo" style="padding: 20px 0px; text-align: center; display: none;">
        <%=ASC.Web.Community.Forum.Resources.ForumResource.SuccessfullyCreateForumMessage%>
    </div>
</asp:Content>

