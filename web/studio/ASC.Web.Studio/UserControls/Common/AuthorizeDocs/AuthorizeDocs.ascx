<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizeDocs.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.AuthorizeDocs.AuthorizeDocs" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="Resources" %>

<div class="auth-form-with clearFix">

    <div class="auth-form-with_slogan">
        <div class="auth-form-with_slogan_docs"><%= Resource.AuthDocsGoing %></div>
        <div class="auth-form-with_slogan_less">Desk<span class="topless">topless</span></div>
        <h1 class="auth-form-with_slogan_action"><%= Resource.AuthActionsWithDocs %></h1>
        <div class="auth-form-with_slogan_carousel carousel">
            <div class="carousel-inner" data-lng="<%= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName%>">
                </div>
        </div>
    </div>

    <div class="auth-form-with_sevices">
        <div class="auth-form-with_sevices_text"><%= Resource.AuthWorksWith %></div>
        <ul class="auth-form-with_sevices_list">
            <li class="dropbox" title="Dropbox"></li>
            <li class="googledrive" title="Google Drive"></li>
            <li class="boxcom" title="Box"></li>
            <li class="onedrive" title="OneDrive"></li>
        </ul>
    </div>

    <div class="auth-form-with_buttons">
        <div class="auth-form-with_buttons_i">
            <span class="auth-form-with_buttons_text signup">
                 <%= Resource.AuthSignUpDocs %>
            </span>
            <span class="auth-form-with_buttons_social">
                <asp:PlaceHolder runat="server" ID="HolderLoginWithThirdParty"></asp:PlaceHolder>
            </span>
        </div>
        <div class="auth-form-with_buttons_i">
            <span class="auth-form-with_buttons_text actions">
                <%=Resource.AuthSeeActionDocs %>
            </span>
            <a href="<%= FilesLinkUtility.GetFileWebEditorTryUrl(FileType.Document) + "&lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>" target="_blank" class="try-button document" title="<%= Resource.TryEditorNow %>"><% =string.Format(Resource.AuthDocEditor, "</br>") %></a>
            <a href="<%= FilesLinkUtility.GetFileWebEditorTryUrl(FileType.Spreadsheet) + "&lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>" target="_blank" class="try-button spreadsheet" title="<%= Resource.TrySpreadsheetNow %>"><% =string.Format(Resource.AuthSpreadsheetEditor, "</br>") %></a>
            <a href="<%= FilesLinkUtility.GetFileWebEditorTryUrl(FileType.Presentation) + "&lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>" target="_blank" class="try-button presentation" title="<%= Resource.TryPresentationNow %>"><% =string.Format(Resource.AuthPresentationEditor , "</br>")%></a>
        </div>
    </div>

</div>

<asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>


