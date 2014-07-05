<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PrivatePanel.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.PrivatePanel" %>

<div>
    <span class="header-base"><%= Title.HtmlEncode()%></span>
    <p><%= Description.HtmlEncode()%></p>

    <div>
        <table class="border-panel" cellpadding="5" cellspacing="0">
            <tr>
                <td>
                    <input style="margin: 0" type="checkbox" id="isPrivate" <%=IsPrivateItem ? "checked='checked'" : "" %> onclick="ASC.CRM.PrivatePanel.changeIsPrivateCheckBox();" />
                </td>
                <td style="padding-left:0">
                    <label for="isPrivate">
                        <%= CheckBoxLabel.HtmlEncode()%>
                    </label>
                </td>
            </tr>
        </table>
    </div>

    <div id="privateSettingsBlock" <%=IsPrivateItem ? "" : "style='display:none;'" %>>
        <br />
        <b><%= AccessListLable.HtmlEncode()%>:</b>
    </div>
</div>