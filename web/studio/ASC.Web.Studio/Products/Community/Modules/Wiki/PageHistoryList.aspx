<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PageHistoryList.aspx.cs"
    Inherits="ASC.Web.Community.Wiki.PageHistoryList" MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="Server">

    <script language="javascript" type="text/javascript">

        function VersionRevernConfirm() {
            return confirm('<%=WikiResource.wikiRevertConfirm%>');
        }
             
    </script>

</asp:Content>
<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <%=WikiPageIn%>
    <a id="WikiPage" class="link underline gray" href="<%=WikiPageURL%>"><%=WikiPageTitle.HtmlEncode()%></a>
    <div style="margin: 15px 0;">

    <asp:Repeater ID="rptPageHistory" runat="server">
        <HeaderTemplate>
            <table class="tableBase" cellpadding="10" cellspacing="0">
                <colgroup>
                    <col style="width: 15px"/>
                    <col style="width: 15px"/>
                    <col style="width: 15px"/>
                    <col style="width: 140px"/>
                    <col/>
                    <col style="width: 120px"/>
                </colgroup>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr class="row">                
                <td class="borderBase" style="text-align:center">
                    <asp:Literal ID="litDiff" Text='<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version > 9 ? (Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version.ToString() : "0" + (Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version.ToString()%>'
                        runat="Server" Visible="false" />
                    <asp:HyperLink runat="server" ID="HyperLink1" CssClass = "linkHeaderMedium" Text='<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version%>'
                        NavigateUrl='<%#GetPageViewLink(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>' />
                </td>
                <td class="borderBase">
                    <asp:RadioButton ID="rbOldDiff" CssClass="wikiDiffRadion" Style='<%#Container.ItemIndex == 0 ? "display:none": ""%>'
                        onclick="javascript:ASC.Community.Wiki.PageHistoryVersionSelected(this);" _Version='<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version%>'
                        runat="Server" Checked='<%#Container.ItemIndex == 1%>' />
                </td>
                <td class="borderBase">
                    <asp:RadioButton ID="rbNewDiff" CssClass="wikiDiffRadion" Style='<%#Container.ItemIndex != 0 ? "display:none": ""%>'
                        onclick="javascript:ASC.Community.Wiki.PageHistoryVersionSelected(this);" _Version='<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version%>'
                        runat="Server" Checked='<%#Container.ItemIndex == 0%>' />
                </td>
                <td  class="borderBase gray-text">
                    <%#GetDate(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>
                </td>
                <td class="borderBase">
                    <%#GetAuthor(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>
                </td>
                <td class="borderBase" style="text-align:right">
                    <% if(!CommunitySecurity.IsOutsider()) { %>
                    <asp:LinkButton ID="cmdRevert" Text='<%#WikiResource.cmdRevert%>' runat="Server" CssClass="linkMedium"
                        OnClick="cmdRevert_Click" CommandName='<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).Version%>'
                        Visible='<%#Container.ItemIndex != 0%>' OnClientClick="javascript:return VersionRevernConfirm();" />
                    <% } %>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    </div>
    <div class = "big-button-container">
        <asp:LinkButton ID="cmdDiff" CssClass="button blue big" runat="Server" OnClick="cmdDiff_Click" />
    </div>
</asp:Content>
