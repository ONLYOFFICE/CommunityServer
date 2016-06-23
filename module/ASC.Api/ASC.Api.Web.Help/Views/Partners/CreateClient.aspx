<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Client Creation
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "clients"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/partnerapi/client</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Client creation method is used to create a client using the parameters (pricing plan, portal address, user email address, etc.) specified in the request.</p>

    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col style="width: 20%"/>
            <col/>
            <col style="width: 110px"/>
            <col style="width: 20%"/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>
                    partnerId
                    <div class="infotext">sent in URL</div>
                </td>
                <td>Partner ID</td>
                <td>GUID</td>
                <td>00000000-c2fb-4de6-a8ae-e59a834a3cc7</td>
            </tr>
			<tr class="tablerow">
				<td>
					email
					<div class="infotext">sent in URL</div>
				</td>
				<td>Portal owner owner email address</td>
				<td>string</td>
				<td>email@gmail.com</td>
			</tr>
			<tr class="tablerow">
				<td>
					firstName
					<div class="infotext">sent in URL</div>
				</td>
				<td>Client first name</td>
				<td>string</td>
				<td>John</td>
			</tr>
			<tr class="tablerow">
				<td>
					lastName
					<div class="infotext">sent in URL</div>
				</td>
				<td>Client last name</td>
				<td>string</td>
				<td>Doe</td>
			</tr>
			<tr class="tablerow">
				<td>
					phone
					<div class="infotext">sent in URL</div>
				</td>
				<td>Client phone number</td>
				<td>string</td>
				<td>+1 (555) 555 2379</td>
			</tr>
			<tr class="tablerow">
				<td>
					portal
					<div class="infotext">sent in URL</div>
				</td>
				<td>Portal address for the portal available at the https://myportalname.onlyoffice.com or https://myportalname.onlyoffice.eu.com address depending on the next parameter selected</td>
				<td>string</td>
				<td>myportal</td>
			</tr>
			<tr class="tablerow">
				<td>
					portalDomain
					<div class="infotext">sent in URL</div>
				</td>
				<td>Portal domain address, onlyoffice.com (Oregon area) or onlyoffice.eu.com (Ireland), i.e. the region where the portal is located</td>
				<td>string</td>
				<td>onlyoffice.com</td>
			</tr>
		</tbody>
    </table>
    
    <div class="header-gray">Example</div>
    <pre>
      [TestClass]
        public class ApiTests
        {
            private readonly string apiHost = "https://partners.onlyoffice.com";

            [TestMethod]
            public void CreateClientTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var email = "email@gmail.com";
                var firstName = "John";
                var lastName = "Doe";
                var phone = "+1 (555) 555 2379";
                var portal = "myportal";
                var portalDomain = "onlyoffice.com";

                var result = string.Empty;
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/client?partnerId={0}&amp;email={1}&amp;firstName={2}&amp;lastName={3}&amp;phone={4}&amp;portal={5}&amp;portalDomain={6}", partnerId, tariff, portal, userEmail, requestType);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>