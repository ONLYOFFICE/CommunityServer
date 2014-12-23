<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Portal Registration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "portals"}, new {@class = "up"}) %>
        <span class="hdr">POST /api/partnerapi/registerportal</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Portal registration method is used to register a new portal with the parameters specified in the request.</p>

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
                    firstName
					<div class="infotext">sent in body</div>
				<td>Portal owner first name</td>
				<td>string</td>
				<td>John</td>
			</tr>
			<tr class="tablerow">
				<td>
                    lastName
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal owner last name</td>
				<td>string</td>
				<td>Smith</td>
			</tr>
			<tr class="tablerow">
				<td>
                    email
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal owner owner email address</td>
				<td>string</td>
				<td>email@gmail.com</td>
			</tr>
			<tr class="tablerow">
				<td>
                    phone
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal owner phone number</td>
				<td>string</td>
				<td>+1 (555) 555 2379</td>
			</tr>
			<tr class="tablerow">
				<td>
                    region
					<div class="infotext">sent in body</div>
				</td>
				<td>onlyoffice.com (Oregon area) or onlyoffice.eu.com (Ireland) region where the created portal will be created and located</td>
				<td>string</td>
				<td>onlyoffice.com</td>
			</tr>
			<tr class="tablerow">
				<td>
                    address
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal address, e.g. myportalname, the portal will be available at the https://myportalname.onlyoffice.com or https://myportalname.onlyoffice.eu.com address depending on the previous parameter selected</td>
				<td>string</td>
				<td>myportalname</td>
			</tr>
			<tr class="tablerow">
				<td>
                    name
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal title displayed at the portal main page</td>
				<td>string</td>
				<td>Created using ONLYOFFICE™ Partners API</td>
			</tr>
			<tr class="tablerow">
				<td>
                    culture
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal language in the four character code (e.g. ru-RU, en-US or en-GB)</td>
				<td>string</td>
				<td>en-US</td>
			</tr>
			<tr class="tablerow">
				<td>
                    password (optional)
					<div class="infotext">sent in body</div>
				</td>
				<td>User password set up during the registration (can be entered manually later at the portal access)</td>
				<td>string</td>
				<td>MyPassword123</td>
			</tr>
			<tr class="tablerow">
				<td>
                    activationStatus (optional)
					<div class="infotext">sent in body</div>
				</td>
				<td>Portal owner activation status: either the portal owner needs to be activated or not (can take the following values: <em>not activated</em> - <b>0</b>, <em>activated</em> - <b>1</b>, <em>pending</em> - <b>2</b>)</td>
				<td>employee activation status</td>
				<td>1</td>
			</tr>
		</tbody>
    </table>
    
    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p>Returns the URL of the created portal</p>
    
    <div class="header-gray">Example</div>
    <pre>
     [TestClass]
        public class ApiTests
        {
            private readonly string apiHost = "https://partners.onlyoffice.com";

            [TestMethod]
            public void RegisterPortalTest()
            {
                var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
                var url = string.Format("/api/partnerapi/RegisterPortal");
                var json = "{ " +
                    "\"firstName\": \"John\", " +
                    "\"lastName\": \"Smith\", " +
                    "\"email\": \"email@gmail.com\", " +
                    "\"phone\": \"+1 (555) 555 2379\", " +
                    "\"region\": \"onlyoffice.com\", " +
                    "\"address\": \"myportalname\", " +
                    "\"name\": \"Created using ONLYOFFICE™ Partners API\", " +
                    "\"culture\": \"en-US\" " +
                    "}";
                var buffer = Encoding.UTF8.GetBytes(json);
                var request = HttpWebRequest.CreateHttp(apiHost + url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
                request.ContentLength = buffer.Length;
                using (var body = request.GetRequestStream())
                {
                    body.Write(buffer, 0, buffer.Length);
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var result = reader.ReadToEnd();
                        Assert.IsNotNull(result);
                    }
                }
            }
    </pre>

</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>