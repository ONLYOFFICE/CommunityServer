<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Request Client Payment
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "clients"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/requestclientpayment</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Notification on an attempt to get a key from the partner or to buy a portal pricing plan via PayPal payment system.</p>

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
				    partnerId
					<div class="infotext">sent in URL</div>
			    </td>
			    <td>Partner ID</td>
				<td>GUID</td>
				<td>00000000-c2fb-4de6-a8ae-e59a834a3cc7</td>
		    </tr>
		    <tr class="tablerow">
			    <td>
				    tariff
					<div class="infotext">sent in URL</div>
			    </td>
			    <td>Pricing plan identification number in ONLYOFFICE™ database</td>
				<td>number</td>
				<td>-53</td>
		    </tr>
		    <tr class="tablerow">
			    <td>
				    portal
					<div class="infotext">sent in URL</div>
			    </td>
			    <td>Portal address</td>
				<td>string</td>
				<td>myportal.onlyoffice.com</td>
		    </tr>
		    <tr class="tablerow">
			    <td>
				    userEmail
					<div class="infotext">sent in URL</div>
			    </td>
			    <td>Portal owner owner email address</td>
				<td>string</td>
				<td>email@gmail.com</td>
		    </tr>
		    <tr class="tablerow">
			    <td>
				    requestType
					<div class="infotext">sent in URL</div>
			    </td>
			    <td>The payment method used for the portal (can be either <b>Key</b> or <b>Payment</b>)</td>
				<td>string</td>
				<td>Key</td>
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
            public void RequestClientPaymentTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var tariff = -53;
                var portal = "myportal.onlyoffice.com";
                var userEmail = "email@gmail.com";
                var requestType = "Key";
                var result = string.Empty;
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/tariffs?partnerId={0}&amp;tariff={1}&amp;portal={2}&amp;userEmail={3}&amp;requestType={4}", partnerId, tariff, portal, userEmail, requestType);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>