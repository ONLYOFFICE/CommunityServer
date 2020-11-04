<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListFiles.aspx.cs" Inherits="ASC.Web.Community.Wiki.ListFiles"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Data" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="Server">
    <link href="<%=CommonLinkUtility.ToAbsolute("~/Products/Community/Modules/Wiki/App_Themes/default/css/filetype_style.css")%>"
        rel="stylesheet" type="text/css" />

    <script language="JavaScript" type="text/javascript">

        var newwindow = ''
        function popitup(url) {
            if (newwindow.location && !newwindow.closed) {
                newwindow.location.href = url;
                newwindow.focus();
            }
            else {
                newwindow = window.open(url, 'htmlname', 'width=404,height=316,resizable=1');
            }

            if (!document.body.onUnload || document.body.onUnload == null) {
                document.body.onUnload = tidy;
            }
        }

        function tidy() {
            if (newwindow.location && !newwindow.closed) {
                newwindow.close();
            }
        }
   
    </script>

</asp:Content>
<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">

    <% if (HasFiles) %>
    <% { %>
    <% if(CanUpload) %>
    <% { %>
    <div>
        <a class="gray button" id="cmdUploadFile" onclick="ShowUploadFileBox();">
            <span class="plus">
                <%=WikiResource.menu_AddNewFile%>
            </span>
        </a>
    </div>
    <br/>
    <% } %>
    <div>
    <asp:Repeater ID="rptFilesList" runat="server">
        <HeaderTemplate>
            <table width="100%" class="tableBase" border="0" cellpadding="10" cellspacing="0">
                <colgroup>
                    <col/>
                    <col style="width: 150px;"/>
                    <col style="width: 140px;"/>
                    <asp:PlaceHolder ID="phDeleteArea" runat="Server" Visible='<%#hasFilesToDelete%>'>
                        <col style="width: 100px"/>
                    </asp:PlaceHolder>
                </colgroup>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr class="row">
                <td class="borderBase <%# GetFileTypeClass(Container.DataItem as File)%>" style="padding-left:35px;">
                    <asp:HyperLink ToolTip='<%#string.Format("{0} - {1}", GetFileName(Container.DataItem as File), GetFileLengthToString((Container.DataItem as File).FileSize)) %>'
                        runat="server" ID="hlFileLink" CssClass = "linkHeaderMedium" Text='<%#GetFileName(Container.DataItem as File)%>'
                        NavigateUrl='<%#GetFileViewLink(Container.DataItem as File)%>' OnClick='<%#GetFileViewLinkPopUp(Container.DataItem as File)%>' />
                </td>
                <td class="borderBase">
                    <%#GetAuthor(Container.DataItem as File)%>
                </td>
                <td class="borderBase gray-text">
                    <%#GetDate(Container.DataItem as File)%>
                </td>
                <asp:PlaceHolder ID="phDeleteArea" runat="Server" Visible='<%#hasFilesToDelete%>'>
                    <td class="borderBase" style="text-align:right;">
                        <asp:LinkButton runat="Server" ID="cmdDelete" CssClass = "linkMedium" OnClick="cmdDelete_Click" Text='<%#WikiResource.cmdDelete%>'
                            CommandName='<%#(Container.DataItem as File).FileName %>' OnClientClick='<%#string.Format("javascript:return confirm(\"{0}\");", WikiResource.cfmDeleteFile)%>'
                            Visible='<%#CanDeleteTheFile(Container.DataItem as File)%>' />
                    </td>
                </asp:PlaceHolder>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    </div>
    <% } else { %>
    <asp:PlaceHolder ID="EmptyContent" runat="server"/>
    <% } %>
    
    <div id="wiki_blockOverlay" class="wikiBlockOverlay" style="display: none;"></div>
    <div id="wiki_UploadFileBox" class="blockMsg blockPage" style="display: none;">
        <sc:Container ID="UploadFileContainer" runat="server">
            <header><%=WikiResource.wikiFileUploadSubject%></header>
            <body>
                <div id="wikiUpload_Normal">
                    <asp:FileUpload ID="fuFile" runat="Server" Width="100%" size="77" />
                    <div class="wikiMaxFileSizeBlock">
                        <%=GetMaxFileUploadString()%></div>
                    <div class="middle-button-container">
                        <asp:LinkButton ID="cmdFileUpload" runat="Server" CssClass="button blue middle" OnClientClick="javascript:ShowUploadintProcess();"
                            OnClick="cmdFileUpload_Click" />
                       <span class="splitter-buttons"></span>
                        <asp:HyperLink ID="cmdFileUploadCancel" runat="Server" CssClass="button gray middle"
                            NavigateUrl="javascript:HideUploadFileBox();"></asp:HyperLink>
                    </div>
                </div>
            </body>
        </sc:Container>
    </div>

    <script type="text/javascript">
        function ShowUploadintProcess() {
            LoadingBanner.showLoaderBtn("#wiki_UploadFileBox");
        }
        
    </script>

</asp:Content>
