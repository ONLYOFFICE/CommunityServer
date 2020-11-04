<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NamingPeopleSettingsContent.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Management.NamingPeopleSettingsContent" %>
<table class="namingPeopleBox">
    <%--schema--%>
    <tr>
        <td colspan="2" class="caption header-base-small schemaCaption headertitle">
            <%=Resources.Resource.NamingPeopleSchema%>
        </td>
    </tr>
    <tr valign="top">
        <td colspan="2">
            <select id="namingPeopleSchema" class="comboBox">
                <asp:Repeater runat="server" ID="namingSchemaRepeater">
                    <ItemTemplate>
                        <option <%#((bool)Eval("Current"))?"selected":""%> value="<%#(String)Eval("Id")%>">
                            <%#HttpUtility.HtmlEncode((String)Eval("Name"))%>
                        </option>
                    </ItemTemplate>
                </asp:Repeater>
            </select>
        </td>
    </tr>
    <%--user--%>
    <tr>
        <td>
            <input class="textEdit" id="usrcaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.UserCaption%>
        </td>
    </tr>
    <tr>
        <td>
            <input class="textEdit" id="usrscaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.UsersCaption%>
        </td>
    </tr>
    <%--group--%>
    <tr>
        <td>
            <input class="textEdit" id="grpcaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.GroupCaption%>
        </td>
    </tr>
    <tr>
        <td>
            <input class="textEdit" id="grpscaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.GroupsCaption%>
        </td>
    </tr>
    <%--post--%>
    <tr>
        <td>
            <input class="textEdit" id="usrstatuscaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.UserStatusCaption%>
        </td>
    </tr>
    <%--red date--%>
    <tr>
        <td>
            <input class="textEdit" id="regdatecaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.RegDateCaption%>
        </td>
    </tr>
    <%--group head--%>
    <tr>
        <td>
            <input class="textEdit" id="grpheadcaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.GroupHeadCaption%>
        </td>
    </tr>
    <%--guest caption--%>
    <tr>
        <td>
            <input class="textEdit" id="guestcaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.GuestCaption%>
        </td>
    </tr>
    <%--guests captoin--%>
    <tr>
        <td>
            <input class="textEdit" id="guestscaption" type="text" maxlength="30" />
        </td>
        <td class="caption">
            <%=Resources.Resource.GuestsCaption%>
        </td>
    </tr>
</table>
