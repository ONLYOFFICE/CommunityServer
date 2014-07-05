<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master"
 AutoEventWireup="true" CodeBehind="ForumMaker2.aspx.cs" Inherits="ASC.Web.Community.Forum.ForumMaker2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ForumPageContent" runat="server">
<input type="hidden" id="forum_fmCallback" value="" />        
    <div class="containerHeaderBlock"><%=ASC.Web.Community.Forum.Resources.ForumResource.AddThreadCategoryTitle%></div>          
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
                        <textarea style="width: 100%; height: 100px;" id="forum_fmThreadDescription"></textarea>
                    </div>
                </div>
                <div id="forum_fm_panel_buttons">
                    <a class="baseLinkButton" href="javascript:ForumMakerProvider.SaveThreadCategory();">
                        <%=ASC.Web.Community.Forum.Resources.ForumResource.CreateButton%>
                    </a>
                    <span class="splitter"></span>
                    <a class="grayLinkButton" onclick="javascript:ForumManager.BlockButtons(); ForumManager.CancelForum('<%=""%>')" href="#">
                            <%=ASC.Web.Community.Forum.Resources.ForumResource.CancelButton%>
                    </a>
                </div>
                <div style="display: none;" id="forum_fm_action_loader" class="clearFix">
                    <div class="textMediumDescribe">
                        <%=ASC.Web.Community.Forum.Resources.ForumResource.PleaseWaitMessage%>
					</div>
					<img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
                </div>
            </div>
            <div id="forum_fmInfo" style="padding: 20px 0px; text-align: center; display: none;">
                <%=ASC.Web.Community.Forum.Resources.ForumResource.SuccessfullyCreateForumMessage%>
            </div>
</asp:Content>

