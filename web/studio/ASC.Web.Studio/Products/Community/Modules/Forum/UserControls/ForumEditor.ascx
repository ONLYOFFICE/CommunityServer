<%@ Assembly Name="ASC.Web.Community"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumEditor.ascx.cs" Inherits="ASC.Web.Community.Forum.ForumEditor" %>
<%@ Import Namespace="ASC.Web.Community.Forum.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
  
  <input type="hidden" id="forum_editCategoryID" value="" />
  <input type="hidden" id="forum_securityObjID" value"" />
   
<%--edit category dlg--%>
<div id="forum_edit_categoryDialog" style="display: none;">
    <sc:Container ID="EditCategoryContainer" runat="server">
        <Header>
            <%=ForumResource.ThreadCategoryEditor%>
        </Header>
        <Body>            
            <div id="forum_editCategoryMessage" class='infoPanel alert' style='margin:10px 0;'></div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadCategoryName%>:</b>
                </div>
                <div>
                    <input class="textEdit" style="width:100%;" type="text" id="forum_editCategoryName" value="" />
                </div>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadCategoryDescription%>:</b>
                </div>
                <div>
                    <textarea style="width: 100%; height: 100px;" id="forum_editCategoryDescription"></textarea>
                </div>
            </div>
            <div class="middle-button-container">
                <a class="button blue middle" href="javascript:ForumMakerProvider.SaveCategory('edit');">
                    <%=ForumResource.SaveButton%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:ForumMakerProvider.CloseDialogByID('forum_edit_categoryDialog');">
                    <%=ForumResource.CancelButton%>
                </a>
            </div>  
        </Body>
    </sc:Container>
</div>
 
 <%--new cat--%>
<div id='forum_new_categoryDialog' style="display:none;">
<sc:Container ID="NewCategoryContainer" runat="server">
        <Header>
            <%=ForumResource.ThreadCategoryEditor%>
          </Header>
        <Body>
            <div id="forum_newCategoryMessage" class='infoPanel alert' style='margin:10px 0;'></div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadCategoryName%>:</b>
                </div>
                <div>
                    <input class="textEdit" style="width:100%;" type="text" id="forum_newCategoryName" value="" />
                </div>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadCategoryDescription%>:</b>
                </div>
                <div>
                    <textarea style="width:100%; height:100px;" id="forum_newCategoryDescription"></textarea>
                </div>
            </div>
            <div class="middle-button-container">
                <a class="button blue middle" href="javascript:ForumMakerProvider.SaveCategory('new');">
                    <%=ForumResource.SaveButton%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:ForumMakerProvider.CloseDialogByID('forum_new_categoryDialog');">
                    <%=ForumResource.CancelButton%>
                </a>
            </div>
        </Body>
    </sc:Container>
 </div> 
 
 <input type="hidden" id="forum_editThreadID" value="" />
 
 <%--new forum--%>
 <div id='forum_new_threadDialog' style="display:none;">
 <sc:Container ID="NewThreadContainer" runat="server">
        <Header>
            <%=ForumResource.ThreadEditor%>
        </Header>
        <Body>
            <div id="forum_newThreadMessage" class='infoPanel alert' style='margin:10px 0;'></div>    
            <input type="hidden" id="forum_newThreadCategoryID" value=""/>    
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadName%>:</b>
                </div>
                <div>
                    <input class="textEdit" style="width:100%;" type="text" id="forum_newThreadName" value="" />
                </div>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadDescription%>:</b>
                </div>
                <div>
                    <textarea style="width:100%; height:100px;" id="forum_newThreadDescription"></textarea>
                </div>
            </div>
            <div id="forum_new_thread_panel_buttons" class="middle-button-container">
                <a class="button blue middle" href="javascript:ForumMakerProvider.SaveThread('new');">
                    <%=ForumResource.SaveButton%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:ForumMakerProvider.CloseDialogByID('forum_new_threadDialog');">
                    <%=ForumResource.CancelButton%>
                </a>
            </div>  

        </Body>
    </sc:Container>
 </div> 
 
 <%--forum editor--%>
<div id='forum_edit_threadDialog' style="display:none;">
  <sc:Container ID="EditThreadContainer" runat="server">
        <Header>
            <%=ForumResource.ThreadEditor%>
          </Header>
        <Body>
            <div id="forum_editThreadMessage" class='infoPanel alert' style='margin:10px 0;'></div>
            <input type="hidden" id="forum_editThreadCategoryID" value=""/>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadName%>:</b>
                </div>
                <div>
                    <input class="textEdit" style="width:100%;" type="text" id="forum_editThreadName" value="" />
                </div>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=ForumResource.ThreadDescription%>:</b>
                </div>
                <div>
                    <textarea style="width:100%; height:100px;" id="forum_editThreadDescription"></textarea>
                </div>
            </div>
            <div class="middle-button-container">
                <a class="button blue middle" href="javascript:ForumMakerProvider.SaveThread('edit');">
                    <%=ForumResource.SaveButton%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:ForumMakerProvider.CloseDialogByID('forum_edit_threadDialog');">
                    <%=ForumResource.CancelButton%>
                </a>
            </div>            
        </Body>
    </sc:Container>
 </div> 

<div id="forum_threadCategories">
    <%= RenderForumCategories() %>
    <asp:PlaceHolder ID="EmptyContent" runat="server"/>
</div>

<% if (HasCategories)%>
<% { %>
    <div class="big-button-container">
        <a class="button blue big" href="newforum.aspx">
            <%= ForumResource.AddThreadButton %>
        </a>
    </div>
<% } %>