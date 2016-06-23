<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <span class="hdr">How to integrate online editors into your own web site on .Net (C#)</span>
</h1>

<h2>Introduction</h2>
<p>To integrate <b>ONLYOFFICE&trade; online editors</b> into your own website on <b>.Net (C#)</b> you need to download and install ONLYOFFICE&trade; editors on your local server and use the <a class="underline" href="<%= Url.Action("demopreview") %>">.Net (C#) Example</a> for their integration.</p>
<p>This guide will show you the sequence of actions to integrate the editors successfully.</p>

<h2 id="wind-1"><span class="style_step">Step 1. </span>Download and Install Document Server</h2>
<p>First, download the <a class="underline" href="<%= Url.Action("demopreview") %>"><b>ONLYOFFICE&trade; Editors</b></a> (the ONLYOFFICE&trade; Document Server).</p>
<p>See the detailed guide to learn how to <a class="underline" href="http://helpcenter.onlyoffice.com/server/windows/document/install-office-apps.aspx">install Document Server</a> correctly.</p>

<h2 id="wind-2"><span class="style_step">Step 2. </span>Download the .Net (C#) code for the editors integration</h2>
<p>Download the <a class="underline" href="<%= Url.Action("demopreview") %>">.Net (C#) Example</a> from our site.</p>
<p>You need to connnect the editors to your web site. For that specify the path to the editors installation in the <em>settings.config</em> file:</p>
<pre>
&lt;add key="files.docservice.url.storage" value="http://documentserver/FileUploader.ashx" /&gt;
&lt;add key="files.docservice.url.converter" value="http://documentserver/ConvertService.ashx" /&gt;
&lt;add key="files.docservice.url.api" value="http://documentserver/OfficeWeb/apps/api/documents/api.js" /&gt;
&lt;add key="files.docservice.url.preloader" value="http://documentserver/OfficeWeb/apps/api/documents/cache-scripts.html"/&gt;
</pre>

<p>where the <b>documentserver</b> is the name of the server with the ONLYOFFICE&trade; Document Server installed.</p>
<p>If you want to experiment with the editor configuration, modify the <a class="underline" href="<%= Url.Action("advanced") %>">parameters</a> it the <em>DocEditor.aspx</em> file.</p>

<h2 id="wind-3"><span class="style_step">Step 3. </span>Install the prerequisites</h2>
<p>To run your website with the editors successfully, check if your system meets the necessary system requirements:</p>
<ul>
    <li>Microsoft .NET Framework: version 4.5 (download it from the <a class="underline" href="https://www.microsoft.com/en-US/download/details.aspx?id=30653">official Microsoft website</a>);</li>
    <li>Internet Information Services: version 7 or later;</li>
</ul>

<h2 id="wind-4"><span class="style_step">Step 4. </span>Running your web site with the editors</h2>
<ol>
    <li>
        <p>Run the Internet Information Service (IIS) manager</p>
        <p>Start -> ControlPanel -> System and Security -> Administrative Tools -> Internet Information Services (IIS) Manager </p>
    </li>
    <li>
        <p>Add your web site in the IIS Manager</p>
        <p>On the <b>Connections</b> panel right-click the <b>Sites</b> node in the tree, then click <b>Add Website</b></p>
        <img alt="" src="/content/img/csharp/add.png" />
    </li>
    <li>
        <p>In the <b>Add Website</b> dialog box specify the name of the folder with the .Net (C#) project in the <b>Site name</b> box.</p>
        <p>Specify the path to the folder with your project in the <b>Physical Path</b> box.</p>
        <p>Specify the unique value used only for this website in the <b>Port</b> box.</p>
        <img alt="" src="/content/img/csharp/sitename.png" />
    </li>
    <li>
        <p>Check for the .NET platform version specified in IIS manager for you web site. Choose <b>v4.0.</b> version.</p>
        <p>Click the <b>Application Pool</b> -> right-click the platform name -> <b>Set application Pool defaults</b> -> <b>.NET CLR version</b></p>
        <img alt="" src="/content/img/csharp/platform.png" />
    </li>
    <li>
        <p>Browse your web site with the IIS manager:</p>
        <p>Right-click the site -> <b>Manage Website</b> -> <b>Browse</b></p>
        <img alt="" src="/content/img/csharp/browse.png" />
    </li>
</ol>
<p>If you integrated the editors successfully the result should look like the <a class="underline" href="<%= Url.Action("demopreview") %>#DemoPreview">demo preview</a> on our site.</p>
