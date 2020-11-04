<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Wiki._Default"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>
<%@ Import Namespace="ASC.Web.Community.Wiki" %>

<%@ Register Src="WikiUC/ViewPage.ascx" TagName="ucViewPage" TagPrefix="wiki" %>
<%@ Register Src="WikiUC/EditPage.ascx" TagName="ucEditPage" TagPrefix="wiki" %>
<%@ Register Src="WikiUC/ViewFile.ascx" TagName="ucViewFile" TagPrefix="wiki" %>
<%@ Register Src="WikiUC/EditFile.ascx" TagName="ucEditFile" TagPrefix="wiki" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="Server">
    <link href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/App_Themes/default/css/wikicssprint.css")%>"
        rel="stylesheet" type="text/css" media="print" />
</asp:Content>

<asp:Content ContentPlaceHolderID="WikiTitleContent" runat="Server">
    <div class="WikiHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon wiki"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(WikiPageName)%></span>
        <% if(!CommunitySecurity.IsOutsider()) { %>
        <asp:Literal ID="SubscribeLinkBlock" runat="server"></asp:Literal>
        <% } %>
        <span class="menu-small topic"></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <div id="actionWikiPage">

    <asp:Panel ID="pView" runat="Server" CssClass="wikiBody">
        <asp:Panel ID="PrintHeader" runat="Server" CssClass="PrintHeader">
            <%=PrintPageNameEncoded%>
        </asp:Panel>
        <wiki:ucViewPage runat="Server" ID="wikiViewPage" OnPageEmpty="OnPageEmpty" OnPublishVersionInfo="On_PublishVersionInfo"
            OnWikiPageLoaded="wikiViewPage_WikiPageLoaded" />

        <wiki:ucViewFile runat="Server" ID="wikiViewFile" OnPageEmpty="OnPageEmpty" OnPublishVersionInfo="On_PublishVersionInfo"
            OnWikiPageLoaded="wikiViewPage_WikiPageLoaded" />

        <asp:Panel runat="Server" ID="pPageIsNotExists">
            <asp:Label ID="txtPageEmptyLabel" CssClass="lblNotExists" runat="Server" />
        </asp:Panel>

    </asp:Panel>

    <asp:Literal ID="ActionPanel" runat="server"></asp:Literal>
    <div id="edit_container">
        <wiki:ucEditPage runat="Server" PreviewContainer="_PrevContainer" PreviewView="_PrevValue"
            OnPreviewReadyHandler="scrollPreview" ID="wikiEditPage" OnPublishVersionInfo="On_PublishVersionInfo"
            OnSaveNewCategoriesAdded="wikiEditPage_SaveNewCategoriesAdded" OnSetNewFCKMode="wikiEditPage_SetNewFCKMode"
            OnGetUserFriendlySizeFormat="wikiEditPage_GetUserFriendlySizeFormat" OnWikiPageLoaded="wikiEditPage_WikiPageLoaded" />
        <wiki:ucEditFile runat="Server" ID="wikiEditFile" OnPublishVersionInfo="On_PublishVersionInfo" />
    </div>
    <div class = "big-button-container">
        <asp:Panel ID="pEditButtons" runat="Server" CssClass="editCommandPanel">
            <asp:LinkButton ID="cmdSave" CssClass="button blue big" runat="Server" OnClientClick="javascript:WikiEditBtns();" OnClick="cmdSave_Click" />
            <span class="splitter-buttons"></span>
            <asp:HyperLink ID="hlPreview" CssClass="button blue big" runat="Server" />
            <span class="splitter-buttons"></span>
            <asp:LinkButton ID="cmdCancel" CssClass="button gray big cancelFckEditorChangesButtonMarker" runat="Server" OnClick="cmdCancel_Click" OnClientClick="javascript:WikiEditBtns();" />
        </asp:Panel>
    </div>
    <div id="_PrevContainer" style="display: none;">
        <div class="headerPanel">
            <%=WikiResource.PagePreview%>
        </div>
        <div class="subHeaderPanel">
            <div id="_PrevValue" class="wiki">
            </div>
        </div>
        <div class = "big-button-container">
            <a class="button blue big" onclick="HidePreview(); return false;" href="javascript:void(0);">
                <%=WikiResource.cmdHide%>
            </a>
        </div>
    </div>
    <asp:Panel ID="pCredits" CssClass="wikiCredits" runat="Server" Style="margin: 6px 0px 36px 0px;
        clear: both;">
        <span class="wikiVersionInfo gray-text" style="padding-left: 5px;">
            <asp:Literal runat="Server" ID="litAuthorInfo" />
        </span>
        <span class="actionsBar">
            <asp:Literal ID="litVersionSeparator" runat="Server" Visible="false">&nbsp; &nbsp;</asp:Literal>
            <asp:LinkButton runat="Server" ID="cmdDelete" CssClass="linkMedium display-none" OnClick="cmdDelete_Click" Text="" />
            <asp:Literal ID="litVersionSeparatorDel" runat="Server" Visible="false">&nbsp; &nbsp;</asp:Literal>
            <asp:HyperLink ID="hlVersionPage" CssClass="linkMedium" runat="Server" Visible="false" />
        </span>
        <div style="clear: both;">
            &nbsp;</div>
    </asp:Panel>
    <asp:PlaceHolder ID="phCategoryResult" runat="Server">
        <asp:Repeater ID="rptCategoryPageList" runat="Server">
            <HeaderTemplate>
                <table class="catDict" border="0" cellspacing="0" cellpadding="0" width="100%">
                    <tr>
            </HeaderTemplate>
            <ItemTemplate>
                <td class="catLetter">
                    <asp:Repeater ID="rptPageList" runat="server" DataSource='<%#Container.DataItem %>'>
                        <ItemTemplate>
                            <div class="block">
                                <div class="letter">
                                    <%#(Container.DataItem as PageDictionary).HeadName%>
                                </div>
                                <asp:Repeater runat="Server" ID="rptPages" DataSource='<%#(Container.DataItem as PageDictionary).Pages%>'>
                                    <HeaderTemplate>
                                        <div class="catList">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="category">
                                            <a class = "linkHeaderMedium" href="<%#GetPageViewLink(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>"
                                                title="<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName.HtmlEncode()%>">
                                                <%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName.HtmlEncode()%></a>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </td>
            </ItemTemplate>
            <FooterTemplate>
                </tr> </table>
            </FooterTemplate>
        </asp:Repeater>
    </asp:PlaceHolder>
    <div id="wikiCommentsDiv">
        <input type="hidden" id="hdnPageName" value="<%=HttpUtility.HtmlEncode(PageNameUtil.Decode(WikiPage))%>"/>
        <scl:Commentslist id="commentList" runat="server" style="width: 100%;" Visible="false"> </scl:Commentslist>
    </div>
</div>
</asp:Content>
