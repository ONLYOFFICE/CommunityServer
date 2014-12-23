<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Minimal Pricing Plan Key Generation
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "keys"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/generatekeys</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Minimal pricing plan key generation method is used to create the number of keys specified in the request for the minimal pricing plan.</p>

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
					count
					<div class="infotext">sent in URL</div>
				</td>
				<td>Number of keys to be generated</td>
				<td>number</td>
				<td>1</td>
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
            public void GenerateKeysTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var count = 1;
            
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/GenerateKeys?partnerId={0}&amp;count={1}", partnerId, count);
                    client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                    result = client.DownloadString(apiHost + url);
                    Assert.IsNotNull(result);
                }
            }
    </pre>

    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>The response will contain the following information</p>

    <div class="header-gray">Example Response</div>
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
				<td>id</td>
				<td>Key ID</td>
				<td>GUID</td>
				<td>"00000000-9ea5-43a5-90d8-ba34dcc7c85d"</td>
			</tr>
			<tr class="tablerow">
				<td>partnerId</td>
				<td>Partner ID</td>
				<td>GUID</td>
				<td>"00000000-c2fb-4de6-a8ae-e59a834a3cc7"</td>
			</tr>
			<tr class="tablerow">
				<td>code</td>
				<td>Key code</td>
				<td>string</td>
				<td>"9KE4F000IBRYUE6L3000"</td>
			</tr>
			<tr class="tablerow">
				<td>tariff</td>
				<td>Pricing plan identification number in ONLYOFFICE™ database</td>
				<td>number</td>
				<td>-53</td>
			</tr>
			<tr class="tablerow">
				<td>price</td>
				<td>Pricing plan price</td>
				<td>decimal number</td>
				<td>75.00</td>
			</tr>
			<tr class="tablerow">
				<td>status</td>
				<td>Key status (can take the following values: <em>Generated</em> (for a newly generated key) - <b>0</b>, <em>Assigned</em> (assigned or sent to client - set by the partner) - <b>1</b>, <em>Activated</em> - <b>2</b>, <em>Paid</em> - <b>3</b>)</td>
				<td>reg key status</td>
				<td>0</td>
			</tr>
			<tr class="tablerow">
				<td>paymentMethod</td>
				<td>Payment method used by the partner (can take the following values: <em>keys</em> - <b>0</b>, <em>PayPal</em> - <b>1</b>, <em>external payment system</em> - <b>2</b>)</td>
				<td>partner payment method</td>
				<td>0</td>
			</tr>
			<tr class="tablerow">
				<td>discount</td>
				<td>Partner currently assigned discount (percent value)</td>
				<td>number</td>
				<td>50</td>
			</tr>
			<tr class="tablerow">
				<td>creationDate</td>
				<td>Key generation date</td>
				<td>date and time</td>
				<td>"2014-06-05T09:58:57.9420877Z"</td>
			</tr>
			<tr class="tablerow">
				<td>activationDate</td>
				<td>Key activation date</td>
				<td>ate and time</td>
				<td>"2014-06-10T14:03:21"</td>
			</tr>
			<tr class="tablerow">
				<td>paymentDate</td>
				<td>Key payment date</td>
				<td>date and time</td>
				<td>"2014-06-12T14:03:21"</td>
			</tr>
			<tr class="tablerow">
				<td>dueDate</td>
				<td>Next key payment due date (the date till which the key is valid)</td>
				<td>date and time</td>
				<td>"2015-06-12T14:03:21"</td>
			</tr>
			<tr class="tablerow">
				<td>comment</td>
				<td>Comment to the key</td>
				<td>string</td>
				<td>"Here is the comment"</td>
			</tr>
			<tr class="tablerow">
				<td>clientId</td>
				<td>Client ID who the key has been generated for</td>
				<td>GUID</td>
				<td>"00000000-0e8c-4ce9-8081-4e31b668964d"</td>
			</tr>
			<tr class="tablerow">
				<td>invoiceId</td>
				<td>ID of the invoice that was billed for the key</td>
				<td>number</td>
				<td>128</td>
			</tr>
			<tr class="tablerow">
				<td>transactionId</td>
				<td>ID of the transaction used to pay for the key</td>
				<td>string</td>
				<td>"111"</td>
			</tr>
			<tr class="tablerow">
				<td>portalDomain</td>
				<td>Portal domain address, onlyoffice.com (Oregon area) or onlyoffice.eu.com (Ireland), i.e. the region where the portal is located</td>
				<td>string</td>
				<td>"myportal.onlyoffice.com"</td>
			</tr>
			<tr class="tablerow">
				<td>portalAlias</td>
				<td>Portal alias</td>
				<td>string</td>
				<td>"myportal"</td>
			</tr>
		</tbody>
    </table>
    <pre>
    {"id":"00000000-9ea5-43a5-90d8-ba34dcc7c85d", "partnerId":"00000000-c2fb-4de6-a8ae-e59a834a3cc7", "code":"9KE4F000IBRYUE6L3000", "tariff":-53, "price":75.00, "status":0, "paymentMethod":0, "discount":50, "creationDate":"2014-06-05T09:58:57.9420877Z", "activationDate":"2014-06-10T14:03:21", "paymentDate":"2014-06-12T14:03:21", "dueDate":"2015-06-12T14:03:21", "comment":"Here is the comment", "clientId":"00000000-0e8c-4ce9-8081-4e31b668964d", "invoiceId":128, "transactionId":null, "portalDomain":"myportal.onlyoffice.com", "portalAlias":"myportal"}
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>