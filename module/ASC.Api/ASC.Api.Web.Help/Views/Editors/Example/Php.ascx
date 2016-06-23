<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <span class="hdr">How to integrate online editors into your own web site on PHP</span>
</h1>

<h2>Introduction</h2>
<p>To integrate <b>ONLYOFFICE&trade; online editors</b> into your own website on <b>PHP</b> you need to download and install ONLYOFFICE&trade; editors on your local server and use the <a class="underline" href="<%= Url.Action("demopreview") %>">PHP Example</a> for their integration.</p>
<p>This guide will show you the sequence of actions to integrate the editors successfully.</p>

<h2 id="wind-1"><span class="style_step">Step 1. </span>Download and Install Document Server</h2>
<p>First, download the <a class="underline" href="<%= Url.Action("demopreview") %>"><b>ONLYOFFICE&trade; Editors</b></a> (the ONLYOFFICE&trade; Document Server).</p>
<p>See the detailed guide to learn how to <a class="underline" href="http://helpcenter.onlyoffice.com/server/windows/document/install-office-apps.aspx">install Document Server</a> correctly.</p>

<h2 id="wind-2"><span class="style_step">Step 2. </span>Download the PHP code for the editors integration</h2>
<p>Download the <a class="underline" href="<%= Url.Action("demopreview") %>">PHP Example</a> from our site.</p>
<p>You need to connnect the editors to your web site. For that specify the path to the editors installation in the <em>config.php</em> file:</p>
<pre>
$GLOBALS['DOC_SERV_STORAGE_URL'] = "http://documentserver/FileUploader.ashx";
$GLOBALS['DOC_SERV_CONVERTER_URL'] = "http://documentserver/ConvertService.ashx";
$GLOBALS['DOC_SERV_API_URL'] = "http://documentserver/OfficeWeb/apps/api/documents/api.js";
$GLOBALS['DOC_SERV_PRELOADER_URL'] = "http://documentserver/OfficeWeb/apps/api/documents/cache-scripts.html";
</pre>

<p>where the <b>documentserver</b> is the name of the server with the ONLYOFFICE&trade; Document Server installed.</p>
<p>If you want to experiment with the editor configuration, modify the <a class="underline" href="<%= Url.Action("advanced") %>">parameters</a> it the <em>doceditor.php</em> file.</p>

<h2 id="wind-3"><span class="style_step">Step 3. </span>Install the prerequisites</h2>
<p>You can use any web server capable of runnig PHP code to run the sample. We will demonstrate how to run the PHP sample using <b>Internet Information Services (IIS)</b> web server. To set up and configure PHP on IIS <b>PHP Manager for IIS</b> will be used.</p>

<ul>
    <li><b>IIS: version 7</b> or later (refer to <a class="underline " href="http://www.iis.net/learn/application-frameworks/scenario-build-a-php-website-on-iis/configuring-step-1-install-iis-and-php">Microsoft official website</a> to learn how to install <b>IIS</b>);</li>
    <li><b>PHP</b> (download it from the <a class="underline" href="http://php.net/downloads.php">http://php.net</a> site);</li>
    <li><b>PHP Manager for IIS</b> (download it from the <a class="underline" href="https://phpmanager.codeplex.com/releases/view/69115">Microsoft open source site)</a>.</li>
</ul>

<h2 id="wind-4"><span class="style_step">Step 4. </span>IIS configuration</h2>
<ol>
    <li>
        <p><b>PHP Manager for IIS</b> configuration:</p>
        <p>After <b>PHP Manager for IIS</b> installation is complete launch the <b>IIS Manager:</b></p>
        <p>Start -> ControlPanel -> System and Security -> Administrative Tools -> Internet Information Services (IIS) Manager </p>
        <p>and find the <b>PHP Manager</b> feature in the <b>Features View</b> in <b>IIS</b>.</p>
        <img src="/content/img/php/manager.png" alt="">

        <p>You need to register the installed PHP version in <b>IIS</b> using <b>PHP Manager</b>.</p>

        <p>Double-click <b>PHP Manager</b> to open it, click the <b>Register new PHP version</b> task and specify the full path to the main PHP executable file location. For example: C:\Program Files\PHP\php-cgi.exe</p>
        <img src="/content/img/php/php-version-1.jpg" width="700" alt="" />

        <p>After clicking <b>OK</b> the new <b>PHP version</b> will be registered with IIS and will become active.</p>
        <img src="/content/img/php/php-version-2.jpg" width="650" alt="" />
    </li>
    <li>
        <p>Configure IIS to handle PHP requests</p>
        <p>For IIS to host PHP applications, you must add handler mapping that tells IIS to pass all PHP-specific requests to the PHP application framework by using the FastCGI protocol.</p>
        <p>Double-click the <b>Handler Mappings</b> feature:</p>
        <img src="/content/img/php/handlerclick.png" alt="" />

        <p>In the <b>Action</b> panel, click <b>Add Module Mapping</b>. In the <b>Add Module Mapping</b> dialog box, specify the configuration settings as follows:</p>
        <ul>
            <li>Request path: <b>*.php</b></li>
            <li>Module: <b>FastCgiModule</b></li>
            <li>Executable: <b>"C:\[Path to your PHP installation]\php-cgi.exe"</b></li>
            <li>
                <p>Name: <b>PHP via FastCGI</b></p>
                <p>Click <b>OK</b>.</p>
            </li>
        </ul>
        <img src="/content/img/php/handler-add.png" alt="" />
    </li>
</ol>
<p>After IIS manager configuration is complete everything is ready for running the <b>PHP</b> example.</p>

<h2 id="wind-5"><span class="style_step">Step 5. </span>Running your web site with the editors</h2>
<ol>
    <li>
        <p>Add your web site in the IIS Manager</p>
        <p>On the <b>Connections</b> panel right-click the <b>Sites</b> node in the tree, then click <b>Add Website</b></p>
        <img alt="" src="/content/img/csharp/add.png" />
    </li>
    <li>
        <p>In the <b>Add Website</b> dialog box specify the name of the folder with the PHP project in the <b>Site name</b> box.</p>
        <p>Specify the path to the folder with your project in the <b>Physical Path</b> box.</p>
        <p>Specify the unique value used only for this website in the <b>Port</b> box.</p>
        <img alt="" src="/content/img/php/add.png" />
    </li>
    <li>
        <p>Browse your web site with the IIS manager:</p>
        <p>Right-click the site -> <b>Manage Website</b> -> <b>Browse</b></p>
        <img alt="" src="/content/img/php/browse.png" />
    </li>
</ol>
<p>If you integrated the editors successfully the result should look like the <a class="underline" href="<%= Url.Action("demopreview") %>#DemoPreview">demo preview</a> on our site.</p>
