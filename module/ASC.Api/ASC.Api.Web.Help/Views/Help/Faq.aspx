<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Faq
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Frequently Asked Questions</h2>
    <h3>What is the date/time format used in the response to the requests?</h3>
    <p>
        The response to the requests use the Roundtrip format: 2008-04-10T06:30:00.0000000-07:00.
        <br />Where '-07:00' is UTC offset which is set on the portal.
        In case the portal uses UTC time without any offset the date/time format in the response will be the following:
        2008-04-10T06:30:00.0000000Z.
		  <br />As for the request, only date can be send in it: 2008-04-10
    </p>
    <h3>How to get json or xml format?</h3>
    <p>
        You can get json or xml format adding '.json' or '.xml' to the request or pointing the request content-type in application/json or text/xml.
        <br />E.g.:
        <a href="<%=Url.DocUrl("people", "get", "api/2.0/people")%>">api/2.0/people.json</a> 
    </p>
    <h3>Is the response data pagination supported?</h3>
    <p>
        Yes. see the <%=Html.ActionLink("Request Filtering", "Filters", "Help")%> section.
    </p>
</asp:Content>
