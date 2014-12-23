<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Invoice List
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "invoices"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/getpartnerinvoices</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Invoice list method returns the list of all invoices for the current partner.</p>
			
	<div class="header-gray">Parameters</div>
    <p>This method has not got any parameters.</p>
    
    <div class="header-gray">Example</div>
    <pre>
      [TestClass]
        public class ApiTests
        {
            private readonly string apiHost = "https://partners.onlyoffice.com";

            [TestMethod]
            public void GetPartnerInvoicesTest()
            {
            
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/GetPartnerInvoices");
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
				<td>Invoice ID</td>
				<td>number</td>
				<td>128</td>
			</tr>
			<tr class="tablerow">
				<td>payPalID</td>
				<td>Partner PayPal account ID</td>
				<td>string</td>
				<td>"mypaypal"</td>
			</tr>
			<tr class="tablerow">
				<td>status</td>
				<td>Invoice status (can take the following values: <em>SENT</em> - <b>1</b>, <em>PAID</em> - <b>2</b>)</td>
				<td>status type</td>
				<td>2</td>
			</tr>
			<tr class="tablerow">
				<td>amount</td>
				<td>Invoice total sum or the sum for all keys in invoice (in US dollars)</td>
				<td>decimal number</td>
				<td>800.00</td>
			</tr>
			<tr class="tablerow">
				<td>exchangeRate</td>
				<td>Currecy exchange rate to convert into Russian Roubles (for Russian partners only)</td>
				<td>decimal number</td>
				<td>30.0000</td>
			</tr>
			<tr class="tablerow">
				<td>discount</td>
				<td>Partner currently assigned discount (percent value)</td>
				<td>number</td>
				<td>50</td>
			</tr>
			<tr class="tablerow">
				<td>creationDate</td>
				<td>Invoice creation date</td>
				<td>date and time</td>
				<td>"2014-02-19T17:52:45"</td>
			</tr>
			<tr class="tablerow">
				<td>statusDate</td>
				<td>Invoice status change date</td>
				<td>date and time</td>
				<td>"2014-05-29T07:09:20"</td>
			</tr>
			<tr class="tablerow">
				<td>tax</td>
				<td>Taxation identification number used</td>
				<td>number</td>
				<td>21</td>
			</tr>
			<tr class="tablerow">
				<td>partnerId</td>
				<td>Partner ID</td>
				<td>GUID</td>
				<td>"00000000-c2fb-4de6-a8ae-e59a834a3cc7"</td>
			</tr>
			<tr class="tablerow">
				<td>url</td>
				<td>PayPal URL that uses the above <em>id</em></td>
				<td>string</td>
				<td>"https://www.paypal.com/us/cgi-bin/?cmd=_inv-details&id=mypaypal"</td>
			</tr>
	    </tbody>
    </table>
    <pre class="egresponse">
    {"id":128, "payPalID":"mypaypal", "status":2, "amount":800.00, "exchangeRate":30.0000, "discount":50, "creationDate":"2014-02-19T17:52:45", "statusDate":"2014-05-29T07:09:20", "tax":21, "partnerId":"00000000-c2fb-4de6-a8ae-e59a834a3cc7", "url":"https://www.paypal.com/us/cgi-bin/?cmd=_inv-details&id=mypaypal"}
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>