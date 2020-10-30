<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomNavigation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.CustomNavigation" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled) { %>
<div class="clearFix">
    <div id="studio_customNavigation" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%= Resource.CustomNavigationTitle %>
        </div>
        <div class="clearFix">
            <table id="customNavigationItems" class="table-list height32">
                <tbody>
                    <tr class="display-none">
                        <td class="item-img">
                            <img/>
                        </td>
                        <td class="item-label">
                            <div></div>
                        </td>
                        <td class="item-action">
                            <a title="<%= Resource.CustomNavigationEditBtn %>"></a>
                        </td>
                    </tr>
                    <% foreach (var item in Items) { %>
                    <tr>
                        <td class="item-img">
                            <img src="<%= item.SmallImg %>"/>
                        </td>
                        <td class="item-label">
                            <div><%= item.Label.HtmlEncode() %></div>
                        </td>
                        <td class="item-action">
                            <a id="<%= item.Id %>" title="<%= Resource.CustomNavigationEditBtn %>"></a>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>

            <div class="<%= Items.Any() ? "middle-button-container" : "" %>">
                <a id="addBtn" class="link dotline plus"><%= Resource.CustomNavigationAddBtn %></a>
            </div>
        </div>
    </div>

    <div class="settings-help-block">
        <p><%= String.Format(Resource.CustomNavigationHelp.HtmlEncode(), "<b>", "</b>", "<br/>") %></p>
    </div>

    <div id="customNavigationItemDialog" class="popupContainerClass display-none">
        <div class="containerHeaderBlock">
            <table style="width: 100%;">
                <tbody>
                    <tr>
                        <td>
                            <span class="add-header"><%= Resource.CustomNavigationAddDialogHeader %></span>
                            <span class="settings-header"><%= Resource.CustomNavigationEditDialogHeader %></span>
                        </td>
                        <td class="popupCancel">
                            <div class="cancelButton cancel-btn">×</div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div class="containerBodyBlock clearFix">
            <div class="clearFix">
                <input id="itemId" type="hidden"/>
                <div class="elements-container">
                    <div class="header-base-small"><%= Resource.CustomNavigationLabelHeader %></div>
                    <div class="element-container">
                        <input id="labelText" type="text" class="textEdit" maxlength="25" placeholder="<%= Resource.CustomNavigationLabelPlaceholder %>" />
                    </div>
                    <div class="header-base-small"><%= Resource.CustomNavigationURLHeader %></div>
                    <div class="element-container">
                        <input id="urlText" type="text" class="textEdit" maxlength="255" placeholder="<%= Resource.CustomNavigationURLPlaceholder %>" />
                    </div>
                </div>
                <div class="elements-container">
                    <div class="element-container">
                        <label>
                            <input id="showInMenuCbx" type="checkbox"/>
                            <%= Resource.CustomNavigationShowInMenu %>
                        </label>
                    </div>
                    <div class="element-container">
                        <label>
                            <input id="showOnHomePageCbx" type="checkbox"/>
                            <%= Resource.CustomNavigationShowOnHomePage %>
                        </label>
                    </div>
                </div>
                <div class="clearFix">
                    <div class="image-container">
                        <div class="header-base-small"><%= Resource.CustomNavigationSmallImgHeader %></div>
                        <img id="smallImg" class="borderBase" />
                        <a id="smallUploader" class="link dotline small"><%= Resource.CustomNavigationChangeImage %></a>
                    </div>
                    <div class="image-container">
                        <div class="header-base-small"><%= Resource.CustomNavigationBigImgHeader %></div>
                        <img id="bigImg" class="borderBase" />
                        <a id="bigUploader" class="link dotline small"><%= Resource.CustomNavigationChangeImage %></a>
                    </div>
                </div>
                <div id="notImageError" class="display-none"><%= WhiteLabelResource.ErrorFileNotImage %></div>
                <div id="sizeError" class="display-none"><%= WhiteLabelResource.ErrorImageSize %></div>
            </div>
            <div class="middle-button-container">
                <a id="removeBtn" class="button gray"><%= Resource.CustomNavigationRemoveBtn %></a>
                <a id="saveBtn" class="button blue"><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray cancel-btn"><%= Resource.CancelButton %></a>
            </div>
        </div>
    </div>
</div>
<% } %>