<%@ Page MasterPageFile="~/Masters/basetemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="ASC.Web.Studio.Terms" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="server">
<div class="personal-page-header"><h1><%=string.Format(Resource.TermsHeader, "<b>", "</b>") %></h1></div>
<div class="personal-page-container __terms">
    <p><%=Resource.TermsWelcome %></p>
    <p><%=Resource.TermsRelations %></p>
    <p><%=Resource.TermsLogIn %></p>
    <ul class="terms-page-content">
        <li>
             <h2>1. <%=Resource.TermsInternetConnection %></h2>
            <ul>
                <li>1.1. <%=Resource.TermsInternetConnection1 %></li>
                <li>1.2. <%=Resource.TermsInternetConnection2 %></li>
                <li>1.3. <%=Resource.TermsInternetConnection3 %></li>
            </ul>
        </li>
        <li>
            <h2>2. <%=Resource.TermsRegistration %></h2>
            <ul>
                <li>2.1. <%=Resource.TermsRegistration1 %></li>
                <li>2.2. <%=Resource.TermsRegistration2 %></li>
            </ul>           
        </li>
        <li>
            <h2>3. <%=Resource.TermsRights %></h2>
            <ul>
                <li>3.1. <%=Resource.TermsRights1 %></li>
                <li>3.2. <%=Resource.TermsRights2 %></li>
                <li>3.3. <%=Resource.TermsRights3 %></li>
                <li>3.4. <%=Resource.TermsRights4 %></li>
            </ul>
        </li>
        <li>
            <h2>4. <%=Resource.TermsRestrictions %></h2>
            <span><%= string.Format(Resource.TermsRestrictionsText, "<b>", "</b>") %></span>
            <ul>
                <li>a) <%=Resource.TermsRestrictions1 %></li>
                <li>b) <%=Resource.TermsRestrictions2 %></li>
                <li>c) <%=Resource.TermsRestrictions3 %></li>
                <li>d) <%=Resource.TermsRestrictions4 %></li>
                <li>e) <%=Resource.TermsRestrictions5 %></li>
            </ul>
        </li>
        <li>
             <h2>5. <%=Resource.TermsWarranties %></h2>
            <ul>
                <li>5.1. <%=Resource.TermsWarranties1 %></li>
                <li>5.2. <%=Resource.TermsWarranties2 %></li>
                <li>5.3. <%=Resource.TermsWarranties3 %></li>
                <li>5.4. <%=Resource.TermsWarranties4 %></li>
                <li>5.5. <%=Resource.TermsWarranties5 %></li>
                <li>5.6. <%=Resource.TermsWarranties6 %></li>
                <li>5.7. <%=Resource.TermsWarranties7 %></li>
                <li>5.8. <%=Resource.TermsWarranties8 %></li>
            </ul>
        </li>
        <li>
            <h2>6. <%=Resource.TermsDisclaimer %></h2>
            <ul>
                <li>6.1. <%=Resource.TermsDisclaimer1 %></li>
                <li>6.2. <%=Resource.TermsDisclaimer2 %></li>
            </ul>
        </li>
        <li>
             <h2>7. <%=Resource.TermsLimitation %></h2>
            <div><%=Resource.TermsLimitationText %></div>
        </li>
        <li>
            <h2>8. <%=Resource.TermsAmendments %></h2>
            <div><%= string.Format(Resource.TermsAmendmentsText, "<a href=\"http://www.onlyoffice.com/\" target=\"blank\">", "<a href=\"https://personal.onlyoffice.com/terms.aspx\" target=\"blank\">", "</a>" ) %></div>
        </li>
        <li>
            <h2>9. <%=Resource.TermsTermination %></h2>
            <div><%=Resource.TermsTermination1 %></div>
            <div><%=Resource.TermsTermination2 %></div>
        </li>
        <li>
            <h2>10. <%=Resource.TermsRelationship %></h2>
            <div><%=Resource.TermsRelationshipText %></div>
        </li>
        <li>
            <h2>11. <%=Resource.TermsSeverability %></h2>
            <div><%=Resource.TermsSeverabilityText %></div>
        </li>
        <li>
            <h2>12. <%=Resource.TermsAgreement %></h2>
            <div><%=Resource.TermsAgreement1 %></div>
            <div><%=Resource.TermsAgreement2 %></div>
            <div><%=Resource.TermsAgreement3 %></div>
        </li>
        <li>
            <h2>13. <%=Resource.TermsNonAssignment %></h2>
            <div><%=Resource.TermsNonAssignmentText %></div>
        </li>
        <li>
            <h2>14. <%=Resource.TermsApplicableLaw %></h2>
            <ul>
                <li>14.1. <%=Resource.TermsApplicableLaw1 %></li>
                <li>14.2. <%=Resource.TermsApplicableLaw2 %></li>
            </ul>
        </li>
    </ul>
    <p class="terms-page-questions"><%= string.Format(Resource.TermsQuestions, "<a href=\"mailto:sales@onlyoffice.com\">", "</a>") %></p>
</div>
 <asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>
</asp:Content>