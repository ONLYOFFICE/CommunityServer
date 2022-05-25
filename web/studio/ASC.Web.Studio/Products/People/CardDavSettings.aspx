<%@ Assembly Name="ASC.Web.People" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" AutoEventWireup="true"
    CodeBehind="CardDavSettings.aspx.cs" Inherits="ASC.Web.People.CardDavSettings" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>


<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server">

    <div id="cardDavBlock">
        <div class="clearFix">
            <div class="settings-block">

                <div class="header-base cardDav-settings-title uppercase"><%= PeopleResource.CardDavSettingsHeader %></div>

                <div class="clearFix">
                    <div class="clearFix">
                        <label class="cardDav-settings-label-checkbox cardDav-settings-never-disable">
                            <a id="cardDavSettingsCheckbox" class="on_off_button 
                        <% if (!IsEnabledLink)
                                { %> off <% }
                                else
                                { %> on <% }
                        %>"></a>
                            <span class="settings-checkbox-text"><%= PeopleResource.EnableCardDavLink %></span>
                        </label>
                    </div>


                    <div class="url-link cardDav">
                        <input id="cardDavUrl" class="textEdit display-none" type="text" value="<%= CardDavLink %>" readonly>
                        <div class="button copy display-none">
                            <span class="move-or-copy">
                                <span class="text"><%= PeopleResource.CardDavCopyBtn %></span>
                            </span>
                        </div>
                        <div class="button try-again display-none">
                            <span class="move-or-copy">
                                <span class="text"><%= PeopleResource.CardDavTryAgainBtn %></span>
                            </span>
                        </div>

                    </div>

                </div>

            </div>
        <div class="settings-help-block">
            <p><%: PeopleResource.CardDavSettingsDescription %></p>
        </div>
    </div>
    </div>


</asp:Content>
