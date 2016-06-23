<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Faq
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Frequently Asked Questions</span>
    </h1>
    
    <div class="header-gray">What is the date/time format used in the response to the requests?</div>
    <p>
        The response to the requests use the Roundtrip format: 2008-04-10T06:30:00.0000000-07:00.
        <br />Where '-07:00' is UTC offset which is set on the portal.
        In case the portal uses UTC time without any offset the date/time format in the response will be the following:
        2008-04-10T06:30:00.0000000Z.
        <br />As for the request, only date can be send in it: 2008-04-10
        <br />
        <br />
        <b>If you use the date/time in URL request, colons must be avoided and replaced by hyphens: 2008-04-10T06-30-00.000Z.</b>
        <br />Please note that the UTC date and time without the offset are used in this case.
    </p>

    <div class="header-gray">How to get json or xml format?</div>
    <p>
        You can get json or xml format adding '.json' or '.xml' to the request or pointing the request content-type in application/json or text/xml.
        <br />E.g.:
        <a class="underline" href="<%= Url.DocUrl("people", null, "get", "api/2.0/people", "portals") %>">api/2.0/people.json</a> 
    </p>

    <div class="header-gray">Is the response data pagination supported?</div>
    <p>
        Yes. see the <%= Html.ActionLink("Request Filtering", "filters", null, new {@class = "underline"}) %> section.
    </p>

</asp:Content>
