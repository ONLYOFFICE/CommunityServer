<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Partner Information
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "index", new {id = "partners"}, new {@class = "up"}) %>
        <span class="hdr">GET /api/partnerapi/partner</span>
        <span class="auth">This function requires authentication</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">Partner information method is used to get the general information (first name, last name, email, domain, etc.) about the partner.</p>

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
    public void GetPartnerTest()
    {
        var partnerId = "00000000-c2fb-4de6-a8ae-e59a834a3cc7";
            
        using (var client = new WebClient())
        {
            var url = string.Format("/api/partnerapi/partner/{0}", partnerId);
            client.Headers.Add("Authorization", "ASC2 59ce3f5b-3e33-4539-87d9-c273bc656135:20131217091618:nn00000anUEKUkZGG0OfeK9gwwE1");
            result = client.DownloadString(apiHost + url);
            Assert.IsNotNull(result);
        }
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
                <td>domain</td>
                <td>Portal domain name</td>
                <td>string</td>
                <td>"myportal"</td>
            </tr>
            <tr class="tablerow">
                <td>email</td>
                <td>Portal owner owner email address</td>
                <td>string</td>
                <td>"email@gmail.com"</td>
            </tr>
            <tr class="tablerow">
                <td>email</td>
                <td>Portal owner owner email address</td>
                <td>string</td>
                <td>"email@gmail.com"</td>
            </tr>
			<tr class="tablerow">
				<td>firstName</td>
				<td>Portal owner first name</td>
				<td>string</td>
				<td>"John"</td>
			</tr>
			<tr class="tablerow">
				<td>lastName</td>
				<td>Portal owner last name</td>
				<td>string</td>
				<td>"Smith"</td>
			</tr>
			<tr class="tablerow">
				<td>url</td>
				<td>Portal address URL</td>
				<td>string</td>
				<td>"onlyoffice.com"</td>
			</tr>
			<tr class="tablerow">
				<td>phone</td>
				<td>Partner phone number</td>
				<td>string</td>
				<td>"+1 (555) 555 2379"</td>
			</tr>
			<tr class="tablerow">
				<td>language</td>
				<td>Portal display language</td>
				<td>string</td>
				<td>"en-US"</td>
			</tr>
			<tr class="tablerow">
				<td>companyName</td>
				<td>Company display name</td>
				<td>string</td>
				<td>"My Company"</td>
			</tr>
			<tr class="tablerow">
				<td>country</td>
				<td>Partner selected country (full country name)</td>
				<td>string</td>
				<td>"United States (United States)"</td>
			</tr>
			<tr class="tablerow">
				<td>countryCode</td>
				<td>Partner selected country (country code)</td>
				<td>string</td>
				<td>"US"</td>
			</tr>
			<tr class="tablerow">
				<td>countryHasVat</td>
				<td>Whether or not the VAT is applicable to the partner</td>
				<td>boolean</td>
				<td>false</td>
			</tr>
			<tr class="tablerow">
				<td>address</td>
				<td>Partner billing address</td>
				<td>string</td>
				<td>"Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021"</td>
			</tr>
			<tr class="tablerow">
				<td>vatId</td>
				<td>Partner VAT ID (in case "countryHasVat":true)</td>
				<td>string</td>
				<td>"LV40125269408"</td>
			</tr>
			<tr class="tablerow">
				<td>creationDate</td>
				<td>Partner account creation date</td>
				<td>date and time</td>
				<td>"2013-07-08T14:03:21"</td>
			</tr>
			<tr class="tablerow">
				<td>status</td>
				<td>Partner status (can take the following values: <em>created but not yet either approved or rejected</em> - <b>0</b>, <em>approved</em> - <b>1</b>, <em>rejected</em> - <b>2</b>, <em>blocked from the partner program</em> - <b>3</b>, <em>marked disconnected as the account does not response to the API request</em> - <b>4</b>)</td>
				<td>partner status</td>
				<td>1</td>
			</tr>
			<tr class="tablerow">
				<td>comment</td>
				<td>Comment to the partner account</td>
				<td>string</td>
				<td>"Here is the comment"</td>
			</tr>
			<tr class="tablerow">
				<td>portal</td>
				<td>Partner ONLYOFFICE™ portal address</td>
				<td>string</td>
				<td>"myportal.onlyoffice.com"</td>
			</tr>
			<tr class="tablerow">
				<td>portalConfirmed</td>
				<td>Whether the partner portal is confirmed or not</td>
				<td>boolean</td>
				<td>false</td>
			</tr>
			<tr class="tablerow">
				<td>isAdmin</td>
				<td>Whether the partner is the portal administrator or not</td>
				<td>boolean</td>
				<td>false</td>
			</tr>
			<tr class="tablerow">
				<td>limit</td>
				<td>Partner credit limit</td>
				<td>decimal number</td>
				<td>1500.00</td>
			</tr>
			<tr class="tablerow">
				<td>discount</td>
				<td>Partner currently assigned discount (percent value)</td>
				<td>number</td>
				<td>50</td>
			</tr>
			<tr class="tablerow">
				<td>payPalAccount</td>
				<td>Partner PayPal account email address</td>
				<td>string</td>
				<td>"email@gmail.com"</td>
			</tr>
			<tr class="tablerow">
				<td>payPalAccess</td>
				<td>Whether the PayPal access is used or not</td>
				<td>boolean</td>
				<td>true</td>
			</tr>
			<tr class="tablerow">
				<td>deposit</td>
				<td>Parner deposit value</td>
				<td>decimal number</td>
				<td>1500.00</td>
			</tr>
			<tr class="tablerow">
				<td>removed</td>
				<td>Whether the partner is removed from the partner program or not</td>
				<td>boolean</td>
				<td>false</td>
			</tr>
			<tr class="tablerow">
				<td>currency</td>
				<td>Currency used by the partner (currency code)</td>
				<td>string</td>
				<td>"US"</td>
			</tr>
			<tr class="tablerow">
				<td>logoUrl</td>
				<td>Partner logo URL</td>
				<td>string</td>
				<td>"/partners/Data/partners/logo/00000000-c2fb-4de6-a8ae-e59a834a3cc7/logo.jpg"</td>
			</tr>
			<tr class="tablerow">
				<td>displayName</td>
				<td>Partner portal display name</td>
				<td>string</td>
				<td>"This is John Smith Company"</td>
			</tr>
			<tr class="tablerow">
				<td>displayType</td>
				<td>Partner logo and caption display type (can take the following values: <em>only logo is displayed</em> - <b>0</b>, <em>only name is displayed</em> - <b>1</b>, <em>both logo and name are displayed</em> - <b>2</b>)</td>
				<td>partner display type</td>
				<td>0</td>
			</tr>
			<tr class="tablerow">
				<td>supportPhone</td>
				<td>Support phone number</td>
				<td>string</td>
				<td>"+1 (555) 555 2380"</td>
			</tr>
			<tr class="tablerow">
				<td>supportEmail</td>
				<td>Support email address</td>
				<td>string</td>
				<td>"support@onlyoffice.com"</td>
			</tr>
			<tr class="tablerow">
				<td>salesEmail</td>
				<td>Sales department email address</td>
				<td>string</td>
				<td>"sales@onlyoffice.com"</td>
			</tr>
			<tr class="tablerow">
				<td>termsUrl</td>
				<td>URL for the Terms and Condidions page</td>
				<td>string<</td>
				<td>"https://partners.onlyoffice.com/terms"</td>
			</tr>
			<tr class="tablerow">
				<td>theme</td>
				<td>Portal selected display skin</td>
				<td>string</td>
				<td>"wild-pink"</td>
			</tr>
			<tr class="tablerow">
				<td>ruAccount</td>
				<td>Bank account number ("РС" - for Russian Federation banks only)</td>
				<td>string</td>
				<td>"64664643906846741582"</td>
			</tr>
			<tr class="tablerow">
				<td>ruBank</td>
				<td>Bank name (for Russian Federation banks only)</td>
				<td>string</td>
				<td>"Bank of Russia"</td>
			</tr>
			<tr class="tablerow">
				<td>ruKs</td>
				<td>Bank corresponding account number ("КС" - for Russian Federation banks only)</td>
				<td>string</td>
				<td>"30101810600000000957"</td>
			</tr>
			<tr class="tablerow">
				<td>ruKpp</td>
				<td>Registration reason code ("КПП" - for Russian Federation banks only)</td>
				<td>string</td>
				<td>"345452345"</td>
			</tr>
			<tr class="tablerow">
				<td>ruBik</td>
				<td>Bank identification number ("БИК" - for Russian Federation banks only)</td>
				<td>string</td>
				<td>"049923790"</td>
			</tr>
			<tr class="tablerow">
				<td>ruInn</td>
				<td>Taxpayer individual identification number ("ИНН" - for Russian Federation banks only)</td>
				<td>string</td>
				<td>"345452345675"</td>
			</tr>
			<tr class="tablerow">
				<td>partnerType</td>
				<td>Partner type related to the role on the portal (can take the following values: <em>default role</em> - <b>0</b>, <em>portal administrator</em> - <b>1</b>, <em>system (special value)</em> - <b>2</b>, <em>portal owner</em> - <b>3</b>)</td>
				<td>partner type</td>
				<td>0</td>
			</tr>
			<tr class="tablerow">
				<td>paymentMethod</td>
				<td>Payment method used by the partner (can take the following values: <em>keys</em> - <b>0</b>, <em>PayPal</em> - <b>1</b>, <em>external payment system</em> - <b>2</b>)</td>
				<td>partner payment method</td>
				<td>2</td>
			</tr>
			<tr class="tablerow">
				<td>paymentUrl</td>
				<td>Payment system URL used for payment for the portals</td>
				<td>string</td>
				<td>"paymenturl.com"</td>
			</tr>
			<tr class="tablerow">
				<td>adminId</td>
				<td>Portal administrator identifier</td>
				<td>GUID</td>
				<td>"11111111-c2fb-4de6-a8ae-e59a834a3cc7"</td>
			</tr>
			<tr class="tablerow">
				<td>activationDate</td>
				<td>Partner account activation date</td>
				<td>date and time</td>
				<td>"2014-01-01T14:45:34"</td>
			</tr>
			<tr class="tablerow">
				<td>availableCredit</td>
				<td>Partner currently available credit</td>
				<td>decimal number</td>
				<td>1325.00</td>
			</tr>
			<tr class="tablerow">
				<td>timeZone</td>
				<td>Portal time zone</td>
				<td>string</td>
				<td>"(UTC+01:00) Europe/Berlin"</td>
			</tr>
			<tr class="tablerow">
				<td>contractNumber</td>
				<td>Partner contract number</td>
				<td>string</td>
				<td>"#345643"</td>
			</tr>
			<tr class="tablerow">
				<td>taxNumber</td>
				<td>Taxation identification number used</td>
				<td>string</td>
				<td>"34545234567562"</td>
			</tr>
			<tr class="tablerow">
				<td>taxFileUrl</td>
				<td>Tax registration document used in some countries (except EU and Russian Federation)</td>
				<td>string</td>
				<td>"/partners/Data/partners/tax/00000000-c2fb-4de6-a8ae-e59a834a3cc7/taxfile.txt"</td>
			</tr>
			<tr class="tablerow">
				<td>helpTourStatus</td>
				<td>Portal hints status (can take the following values: <em>hints are disabled</em> - <b>-1</b>, <em>hint steps 1-6</em> - <b>0-5</b>, <em>all hints are viewed</em> - <b>6</b>)</td>
				<td>number</td>
				<td>-1</td>
			</tr>
			<tr class="tablerow">
				<td>customEmailSignature</td>
				<td>Whether or not the custom email signature is used</td>
				<td>boolean</td>
				<td>true</td>
			</tr>
		</tbody>
    </table>
    <pre>
    {"id":"00000000-c2fb-4de6-a8ae-e59a834a3cc7", "domain":"myportal", "email":"email@gmail.com", "firstName":"John", "lastName":"Smith", "url":"onlyoffice.com", "phone":"+1 (555) 555 2379", "language":"en-US", "companyName":"My Company", "country":"United States (United States)", "countryCode":"US", "countryHasVat":false, "address":"Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021", "vatId":"LV40125269408", "creationDate":"2013-07-08T14:03:21", "status":1, "comment":"Here is the comment", "portal":"myportal.onlyoffice.com", "portalConfirmed":false, "isAdmin":false, "limit":1500.00, "discount":50, "payPalAccount":"email@gmail.com", "payPalAccess":true, "deposit":1500.00, "removed":false, "currency":"US", "logoUrl":"/partners/Data/partners/logo/00000000-c2fb-4de6-a8ae-e59a834a3cc7/logo.jpg", "displayName":"This is John Smith Company", "displayType":0, "supportPhone":"+1 (555) 555 2380", "supportEmail":"support@onlyoffice.com", "salesEmail":"sales@onlyoffice.com", "termsUrl":"https://partners.onlyoffice.com/terms", "theme":"wild-pink", "ruAccount":"64664643906846741582", "ruBank":"Bank of Russia", "ruKs":"30101810600000000957", "ruKpp":"345452345", "ruBik":"049923790", "ruInn":"345452345675", "partnerType":0, "paymentMethod":2, "paymentUrl":"paymenturl.com", "adminId":"11111111-c2fb-4de6-a8ae-e59a834a3cc7", "activationDate":"2014-01-01T14:45:34", "availableCredit":1325.00, "timeZone":"(UTC+01:00) Europe/Berlin", "contractNumber":"#345643", "taxNumber":"34545234567562", "taxFileUrl":"/partners/Data/partners/tax/00000000-c2fb-4de6-a8ae-e59a834a3cc7/taxfile.txt", "helpTourStatus":-1, "customEmailSignature":true}
    </pre>
</asp:Content>

<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>