<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Customization
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "editor", null, new {@class = "up"}) %>
        <span class="hdr">Customization</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">The customization section allows to customize the editor interface so that it looked like your other products (if there are any) and change the presence or absence of the additional buttons, links, change logos and editor owner details</p>

    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col class="table-name" />
            <col />
            <col class="table-type" />
            <col class="table-example" />
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
                <td>about</td>
                <td>defines if the <b>About</b> menu button is displayed or hidden (the default value is set to <em>false</em>).</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>chat</td>
                <td>defines if the <b>Chat</b> menu button is displayed or hidden; please note that in case you hide the <b>Chat</b> button, the corresponding chat functionality will also be disabled. This functionality is defined by the license obtained and can be unavailable (and the parameter ignored) if you do not have the appropriate license.</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>comments</td>
                <td>defines if the <b>Comments</b> menu button is displayed or hidden; please note that in case you hide the <b>Comments</b> button, the corresponding commenting functionality will be available for viewing only, the adding and editing of comments will be unavailable. This functionality is defined by the license obtained and can be unavailable (and the parameter ignored) if you do not have the appropriate license.</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>customer</td>
                <td>Contains the information for the editor <b>About</b> section. The object has the following parameters:
                    <ul>
                        <li><b>address</b> - postal address of the above company or person
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "My City, 123a-45"
                        </li>
                        <li><b>info</b> - some information about the above company or person which will be displayed at the <b>About</b> page and visible to all editor users
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "Some additional information"
                        </li>
                        <li><b>logo</b> - the path to the image logo which will be displayed at the <b>About</b> page (there are no special recommendations for this file, but it would be better if it were in .png format with transparent background). The image must have the following size: 432x70.
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com/logo-big.png"
                        </li>
                        <li><b>mail</b> - email address of the above company or person
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "john@example.com"
                        </li>
                        <li><b>name</b> - the name of the company or person who gives access to the editors or the editor authors.
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "John Smith and Co."
                        </li>
                        <li><b>www</b> - home website address of the above company or person
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "example.com"
                        </li>
                    </ul>
                </td>
                <td>object</td>
                <td></td>
            </tr>
            <tr class="tablerow">
                <td>feedback</td>
                <td>defines settings for the <b>Feedback &amp; Support</b> menu button. Can be either boolean (simply displays or hides the <b>Feedback &amp; Support</b> menu button) or object. In case of object type the following parameters are available:
                    <ul>
                        <li><b>url</b> - the website address which will be opened when clicking the <b>Feedback &amp; Support</b> menu button
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com"
                        </li>
                        <li><b>visible</b> - show or hide the <b>Feedback &amp; Support</b> menu button
                            <br />
                            <b>type</b>: boolean
                            <br />
                            <b>example</b>: true
                        </li>
                    </ul>
                </td>
                <td>boolean or object</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>goback</td>
                <td>defines settings for the <b>Go to Documents</b> menu button and upper right corner link. The object has the following parameters:
                    <ul>
                        <li><b>text</b> - the text which will be displayed for the <b>Go to Documents</b> menu button and upper right corner link (i.e. instead of <em>Go to Documents</em>)
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "Go to Documents"
                        </li>
                        <li><b>url</b> - the website address which will be opened when clicking the <b>Go to Documents</b> menu button
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com"
                        </li>
                    </ul>
                </td>
                <td>boolean or object</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>logo</td>
                <td>Changes the image file at the top left corner of the Editor header. The recommended image height is 20 pixels. The object has the following parameters:
                    <ul>
                        <li><b>image</b> - path to the image file used to show in common work mode (i.e. in view and edit modes for all editors). The image must have the following size: 172x40.
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com/logo.png"
                        </li>
                        <li><b>imageEmbedded</b> - path to the image file used to show in the embedded mode (see the <a href="<%= Url.Action("config") %>" class="underline">config</a> section to find out how to define the <b>embedded</b> document type). The image must have the following size: 248x40.
                        <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com/logo_em.png"
                        </li>
                        <li><b>url</b> - the link which will be used when someone clicks the logo image (can be used to go to your web site, etc.).
                        <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com"
                        </li>
                    </ul>
                </td>
                <td>object</td>
                <td></td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/customization.png" alt="" />
                </td>
            </tr>
        </tbody>
    </table>

    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
        ...
        editorConfig: {
            ...
            customization: {
                about: true,
                chat: true,
                comments: true,
                customer: {
                    address: 'My City, 123a-45',
                    info: 'Some additional information',
                    logo: 'http://example.com/logo-big.png',
                    mail: 'john@example.com',
                    name: 'John Smith and Co.',
                    www: 'example.com',
                },
                feedback: {
                    url: 'http://example.com',
                    visible: true,
                },
                goback: {
                    text: 'Go to Documents',
                    url: 'http://example.com',
                },
                logo: {
                    image: 'http://example.com/logo.png',
                    imageEmbedded: 'http://example.com/logo_em.png',
                    url: 'http://www.onlyoffice.com',
                },
            },
        },
    });
</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
