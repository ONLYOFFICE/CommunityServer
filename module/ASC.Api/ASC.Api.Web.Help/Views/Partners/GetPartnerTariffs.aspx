<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Partner Pricing Plans
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "partners"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/tariffs</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Partner information method is used to get the partner pricing plans.</p>
		
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
        </tbody>
    </table>
    
    <div class="header-gray">Example</div>
    <pre>
      [TestClass]
        public class ApiTests
        {
            private readonly string apiHost = "https://partners.onlyoffice.com";

            [TestMethod]
            public void GetPartnerTariffsTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
            
                using (var client = new WebClient())
                {
                    var url = string.Format("/api/partnerapi/tariffs?partnerId={0}", partnerId);
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
    <p>The response will contain the following information about the partner</p>

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
                <td>Pricing plan identification number in ONLYOFFICE™ database</td>
                <td>number</td>
                <td>-53</td>
            </tr>
			<tr class="tablerow">
				<td>name</td>
				<td>Pricing plan conditional name which includes the pricing plan number and length</td>
				<td>string</td>
				<td>"#0 year"</td>
			</tr>
			<tr class="tablerow">
				<td>maxFileSize</td>
				<td>Maximal size of the files which can be uploaded to the portal allowed by the pricing plan (in Bytes)</td>
				<td>long number</td>
				<td>1073741824</td>
			</tr>
			<tr class="tablerow">
				<td>maxTotalSize</td>
				<td>Maximal size of available disk space on the portal allowed by the pricing plan (in Kilobytes)</td>
				<td>long number</td>
				<td>5368709120</td>
			</tr>
			<tr class="tablerow">
				<td>activeUsers</td>
				<td>Maximal number of active users allowed by the pricing plan</td>
				<td>number</td>
				<td>5</td>
			</tr>
			<tr class="tablerow">
				<td>features</td>
				<td>Features included into the pricing plan</td>
				<td>string</td>
				<td>"backup,domain,docs,year,sms"</td>
			</tr>
			<tr class="tablerow">
				<td>price</td>
				<td>Pricing plan price</td>
				<td>decimal number</td>
				<td>75.00</td>
			</tr>
			<tr class="tablerow">
				<td>price2</td>
				<td>Pricing plan price without discount (in case the partner uses the advertising for the pricing plan and displays both prices with and without discounts)</td>
				<td>decimal number</td>
				<td>0.0</td>
			</tr>
			<tr class="tablerow">
				<td>avangateId</td>
				<td>Identification number for the correlation of ONLYOFFICE™ pricing plans and Avangate prices</td>
				<td>string</td>
				<td>"60"</td>
			</tr>
			<tr class="tablerow">
				<td>visible</td>
				<td>Whether the pricing plan visible or not (some outdated pricing plans can be hidden in the database)</td>
				<td>boolean</td>
				<td>true</td>
			</tr>
			<tr class="tablerow">
				<td>year</td>
				<td>Whether the pricing plan is for month or for year (if <b>true</b> then the pricing plan is for year, if <b>false</b> then for month)</td>
				<td>boolean</td>
				<td>true</td>
			</tr>
		</tbody>
    </table>
    <pre>
    [{"id":-53, "name":"#0 year", "maxFileSize":1073741824, "maxTotalSize":5368709120, "activeUsers":5, "features":"backup,domain,docs,year,sms", "price":75.00, "price2":0.0, "avangateId":"60", "visible":true, "year":true},{"id":-51, "name":"#5 + docs", "maxFileSize":1073741824, "maxTotalSize":75161927680, "activeUsers":70, "features":"backup,domain,docs,sms", "price":200.00, "price2":0.0, "avangateId":"43", "visible":true, "year":false},{"id":-48, "name":"#9 year + docs", "maxFileSize":1073741824, "maxTotalSize":429496729600, "activeUsers":400, "features":"backup,domain,docs,year,sms", "price":8000.00, "price2":0.0, "avangateId":"40", "visible":true, "year":true}]
    </pre>
</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>