<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductsAndInstruments.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ProductsAndInstruments" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>

<div id="studio_productSettings">
    <div class="clearFix">
        <div class="float-left" style="margin: 0 5px 45px 0;">
        <div class="clearFix">
            <div class="web-item-list">
                <div class="header-base">
                    <%=Resource.ProductsAndInstruments_Products%>
                </div>
                <% foreach (var product in Products) %>
                <% { %>
                <div class="web-item">
                    <div class="web-item-header header-base-small">
                        <input id="cbx_<%= product.ItemName %>" type="checkbox" autocomplete="off" data-id="<%= product.ID %>" <%= !product.Disabled ? "checked=\"checked\"" : "" %> <%= TenantAccessAnyone ? "disabled=\"disabled\"" : "" %>/>
                        <img src="<%= product.IconUrl %>" align="absmiddle"/>
                        <label for="cbx_<%= product.ItemName %>"><%= product.Name%></label>
                    </div>
                    <% if (product.SubItems.Count > 0) %>
                    <% { %>
                    <div class="web-item-subitem-list" style="<%= product.Disabled ? "display:none" : "" %>">
                        <% foreach (var subItem in product.SubItems) %>
                        <% { %>
                        <div class="web-item-subitem">
                            <input id="cbx_<%= subItem.ItemName %>" autocomplete="off" type="checkbox" data-id="<%= subItem.ID %>" <%= !subItem.Disabled ? "checked=\"checked\"" : "" %> <%= TenantAccessAnyone ? "disabled=\"disabled\"" : "" %>/>
                            <label for="cbx_<%= subItem.ItemName %>"><%= subItem.Name%></label>
                        </div>
                        <% } %>
                    </div>
                    <% } %>
                </div>
                <% } %>
            </div>

            <div class="web-item-list">
                <div class="header-base">
                    <%=Resource.ProductsAndInstruments_Instruments%>
                </div>
                <% foreach (var module in Modules) %>
                <% { %>
                <div class="web-item">
                    <div class="web-item-header header-base-small">
                        <input id="cbx_<%= module.ItemName %>" autocomplete="off" type="checkbox" data-id="<%= module.ID %>" <%= !module.Disabled ? "checked=\"checked\"" : "" %> <%= TenantAccessAnyone ? "disabled=\"disabled\"" : "" %>/>
                        <img src="<%= module.IconUrl %>" align="absmiddle"/>
                        <label for="cbx_<%= module.ItemName %>"><%= module.Name%></label>
                    </div>
                </div>
                <% } %>
            </div>
        </div>
            <div class="middle-button-container">
                <a id="btnSaveSettings" class="button blue <%= TenantAccessAnyone ? "disable" : "" %>">
                    <%=Resource.SaveButton%>
                </a>
            </div>
        </div>
        <div class="settings-help-block">
            <p>
                <%= CustomNamingPeople.Substitute<Resource>("ProductsAndInstruments_Info").HtmlEncode() %>
            </p>
        </div>
    </div>

</div>