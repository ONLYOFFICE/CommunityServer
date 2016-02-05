<%@ Page MasterPageFile="~/Masters/basetemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="ASC.Web.Studio.Terms" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Resources" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="server">
<div class="personal-page-header"><h1><%=string.Format(Resource.TermsHeader.HtmlEncode(), "<b>", "</b>") %></h1></div>
    <%
        var termsLinks = (new Dictionary<string, string>
            {
                {"de", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543238&doc=Z0UxWDZ4SHE3eDZEUGxCYzErU1NjeHJuOSswcm0xYTBZQVk4OUJvdEoyRT0_IjQ1NDMyMzgi0"},
                {"en", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543205&doc=cCs2di9Vd211WG1BVWMzZ3ZDSkFzSGRFbXB2UHNjSHB6djNDTWR1YThqST0_IjQ1NDMyMDUi0"},
                {"es", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543242&doc=bDZYYUx6WE44Yk4zNlp5djNiNDg4UW5Za2VGanZHUTFYbzdoTG1UTXR5az0_IjQ1NDMyNDIi0"},
                {"fr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543271&doc=QUQ3alVkR0hLS2Q0c1BSNUt1RjdqZXEwc3gvUVZVZlp1ay9veWFoMW4xOD0_IjQ1NDMyNzEi0"},
                {"pt", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543279&doc=N0NuYkFHdEk2c2xSU1pEZWxEK2xFSTlXM3NwL2RPSzlxQUlnTFBYeXQ5VT0_IjQ1NDMyNzki0"},
                {"ru", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543231&doc=eDFDYzZOa0F1NjhERGQ0eHQ5QThlRnBheFFxSSt2dkZoZkxzZ3htditobz0_IjQ1NDMyMzEi0"},
                {"sl", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543286&doc=eEpoK1dGSmJXcVViMU4xTXJmalBidWc1S0wzNTJ1TVJkdGJ5UHZZTmdSMD0_IjQ1NDMyODYi0"},
                {"tr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543292&doc=c09kSWdVeVN5eUdRNzArbUtsYmtoc2JmbndISS9tRzZ2MnJxcjNOUW9UOD0_IjQ1NDMyOTIi0"},
            });
        var termsLink = termsLinks.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                            ? termsLinks[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
                            : termsLinks["en"];
        Response.Redirect(termsLink, true);
    %>

<div class="personal-page-container __terms">
    <p><%: Resource.TermsWelcome %></p>
    <p><%: Resource.TermsRelations %></p>
    <p><%: Resource.TermsLogIn %></p>
    <ul class="terms-page-content">
        <li>
             <h2>1. <%: Resource.TermsInternetConnection %></h2>
            <ul>
                <li>1.1. <%: Resource.TermsInternetConnection1 %></li>
                <li>1.2. <%: Resource.TermsInternetConnection2 %></li>
                <li>1.3. <%: Resource.TermsInternetConnection3 %></li>
            </ul>
        </li>
        <li>
            <h2>2. <%: Resource.TermsRegistration %></h2>
            <ul>
                <li>2.1. <%: Resource.TermsRegistration1 %></li>
                <li>2.2. <%: Resource.TermsRegistration2 %></li>
            </ul>           
        </li>
        <li>
            <h2>3. <%: Resource.TermsRights %></h2>
            <ul>
                <li>3.1. <%: Resource.TermsRights1 %></li>
                <li>3.2. <%: Resource.TermsRights2 %></li>
                <li>3.3. <%: Resource.TermsRights3 %></li>
                <li>3.4. <%: Resource.TermsRights4 %></li>
            </ul>
        </li>
        <li>
            <h2>4. <%: Resource.TermsRestrictions %></h2>
            <span><%= string.Format(Resource.TermsRestrictionsText.HtmlEncode(), "<b>", "</b>") %></span>
            <ul>
                <li>a) <%: Resource.TermsRestrictions1 %></li>
                <li>b) <%: Resource.TermsRestrictions2 %></li>
                <li>c) <%: Resource.TermsRestrictions3 %></li>
                <li>d) <%: Resource.TermsRestrictions4 %></li>
                <li>e) <%: Resource.TermsRestrictions5 %></li>
            </ul>
        </li>
        <li>
             <h2>5. <%=Resource.TermsWarranties.HtmlEncode() %></h2>
            <ul>
                <li>5.1. <%: Resource.TermsWarranties1 %></li>
                <li>5.2. <%: Resource.TermsWarranties2 %></li>
                <li>5.3. <%: Resource.TermsWarranties3 %></li>
                <li>5.4. <%: Resource.TermsWarranties4 %></li>
                <li>5.5. <%: Resource.TermsWarranties5 %></li>
                <li>5.6. <%: Resource.TermsWarranties6 %></li>
                <li>5.7. <%: Resource.TermsWarranties7 %></li>
                <li>5.8. <%: Resource.TermsWarranties8 %></li>
            </ul>
        </li>
        <li>
            <h2>6. <%: Resource.TermsDisclaimer %></h2>
            <ul>
                <li>6.1. <%: Resource.TermsDisclaimer1 %></li>
                <li>6.2. <%: Resource.TermsDisclaimer2 %></li>
            </ul>
        </li>
        <li>
             <h2>7. <%: Resource.TermsLimitation %></h2>
            <div><%: Resource.TermsLimitationText %></div>
        </li>
        <li>
            <h2>8. <%: Resource.TermsAmendments %></h2>
            <div><%= string.Format(Resource.TermsAmendmentsText.HtmlEncode(), "<a href=\"http://www.onlyoffice.com/\" target=\"blank\">", "<a href=\"https://personal.onlyoffice.com/terms.aspx\" target=\"blank\">", "</a>" ) %></div>
        </li>
        <li>
            <h2>9. <%: Resource.TermsTermination %></h2>
            <div><%: Resource.TermsTermination1 %></div>
            <div><%: Resource.TermsTermination2 %></div>
        </li>
        <li>
            <h2>10. <%: Resource.TermsRelationship %></h2>
            <div><%: Resource.TermsRelationshipText %></div>
        </li>
        <li>
            <h2>11. <%: Resource.TermsSeverability %></h2>
            <div><%: Resource.TermsSeverabilityText %></div>
        </li>
        <li>
            <h2>12. <%: Resource.TermsAgreement %></h2>
            <div><%: Resource.TermsAgreement1 %></div>
            <div><%: Resource.TermsAgreement2 %></div>
            <div><%: Resource.TermsAgreement3 %></div>
        </li>
        <li>
            <h2>13. <%: Resource.TermsNonAssignment %></h2>
            <div><%: Resource.TermsNonAssignmentText %></div>
        </li>
        <li>
            <h2>14. <%: Resource.TermsApplicableLaw %></h2>
            <ul>
                <li>14.1. <%: Resource.TermsApplicableLaw1 %></li>
                <li>14.2. <%: Resource.TermsApplicableLaw2 %></li>
            </ul>
        </li>
    </ul>
    <p class="terms-page-questions"><%= string.Format(Resource.TermsQuestions.HtmlEncode(), "<a href=\"mailto:sales@onlyoffice.com\">", "</a>") %></p>
</div>
 <asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>
</asp:Content>