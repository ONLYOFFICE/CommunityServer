<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IpSecurity.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.IpSecurity.IpSecurity" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled)
   { %>
<div id="iprestrictions-view">
    <script id="restriction-tmpl" type="text/x-jquery-tmpl">
        <div class="restriction clearFix" data-restrictionid="${id}">
            <div>
                <input value="${ip}" class="ip textEdit" />
                <% if (!TenantAccessAnyone)
                   { %>
                    <span class="menu-item-icon trash delete-btn icon-link"></span>
                <% } %>
            </div>
        </div>
    </script>

    <div class="clearFix">
        <div class="settings-block">
            <div class="header-base clearFix">
                <%= Resource.IpSecurityNav %>
            </div>
            
            <div class="clearFix">
                <div class="clearFix">
                    <input id="ipsecurityOff" type="radio" name="ipsecuritySwitch" <%= !RestrictionsSettings.Enable || TenantAccessAnyone ? "checked=\"checked\"" : "" %> <%= TenantAccessAnyone ? "disabled=\"disabled\"" : "" %>/>
                    <label for="ipsecurityOff"><%= Resource.IPSecurityDisable %></label>
                </div>
                <div class="clearFix">
                    <input id="ipsecurityOn" type="radio" name="ipsecuritySwitch" <%= RestrictionsSettings.Enable && !TenantAccessAnyone ? "checked=\"checked\"" : "" %> <%= TenantAccessAnyone ? "disabled=\"disabled\"" : "" %>/>
                    <label for="ipsecurityOn"><%= Resource.IPSecurityEnable %></label>
                </div>
            </div>

            <% if (!TenantAccessAnyone)
               { %>
                <div id="restrictions-list" class="<%= !RestrictionsSettings.Enable ? "none" : "" %> ">
                    <span id="add-restriction-btn" class="link dotline plus"><%= Resource.AddIPRestrictionBtn %></span>

                    <div class="header-base red-text"><%= Resource.Warning %></div>
                    <div><%= Resource.IpSecurityWarning %></div>
                </div>
            <% } %>

            <div class="middle-button-container">
                <a id="save-restriction-btn" class="button blue <%= TenantAccessAnyone ? "disable" : "" %>"><%= Resource.SaveButton %></a>
            </div>

        </div>
        <div class="settings-help-block">
            <p><%= string.Format(Resource.IpSecurityHelp.HtmlEncode(), "<b>", "</b>") %></p>
        </div>
    </div>
</div>
<% } %>