<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <span class="hdr">How to integrate online editors into your own web site on Node.js</span>
</h1>

<h2>Introduction</h2>
<p>To integrate <b>ONLYOFFICE&trade; online editors</b> into your own website on <b>Node.js</b> you need to download and install ONLYOFFICE&trade; editors on your local server and use the <a class="underline" href="<%= Url.Action("demopreview") %>">Node.js Example</a> for their integration. We will show you how to run the  Node.js example on <a class="underline" href="<%= Url.Action("NodeExample") %>#Windows">Windows OS</a> and <a class="underline" href="<%= Url.Action("NodeExample") %>#Linux">Linux OS</a>.</p>
<p>This guide will show you the sequence of actions to integrate the editors successfully.</p>

<h2 id="windows">Running the example on Windows OS</h2>
<h2 id="wind-1"><span class="style_step">Step 1. </span>Download and Install Document Server</h2>
<p>First, download the <a class="underline" href="<%= Url.Action("demopreview") %>"><b>ONLYOFFICE&trade; Editors</b></a> (the ONLYOFFICE&trade; Document Server).</p>
<p>See the detailed guide to learn how to <a class="underline" href="http://helpcenter.onlyoffice.com/server/windows/document/index.aspx">install Document Server for Windows</a>.</p>

<h2 id="wind-2"><span class="style_step">Step 2. </span>Download the Node.js code for the editors integration</h2>
<p>Download the <a class="underline" href="<%= Url.Action("demopreview") %>">Node.js Example</a> from our site.</p>
<p>You need to connnect the editors to your web site. For that specify the path to the editors installation in the <em>config.js</em> file:</p>
<pre class="commandline">
config.converterUrl = "http://<b>documentserver</b>/ConvertService.ashx";
config.tempStorageUrl = "http://<b>documentserver</b>/ResourceService.ashx";
config.apiUrl = "http://<b>documentserver</b>/OfficeWeb/apps/api/documents/api.js";
config.preloaderUrl = "http://<b>documentserver</b>/OfficeWeb/apps/api/documents/cache-scripts.html";
</pre>

<p>where the <b>documentserver</b> is the name of the server with the ONLYOFFICE&trade; Document Server installed.</p>
<p>If you want to experiment with the editor configuration, modify the <a class="underline" href="<%= Url.Action("advanced") %>">parameters</a> it the <em>\views\editor.ejs</em> file.</p>

<h2 id="wind-3"><span class="style_step">Step 3. </span>System requirements</h2>
<p>Download and install the <b>node.js</b> enviroment which is going to be used to run the Node.js project. Please follow the link at the oficial website: <a class="underline" href="https://nodejs.org/en/download/">https://nodejs.org/en/download/</a>, choosing the correct version for your Windows OS (32-bit or 64-bit).</p>

<h2 id="wind-4"><span class="style_step">Step 4. </span>Running the Node.js code</h2>
<p>We will run the code in Node.js runtime environment and will interact with it using the command line interface (cmd).</p>

<ol>
    <li>Launch the <b>Command Prompt</b> and switch to the folder with the Node.js project code, for example:
        <div class="commandline">cd  /d C:\OnlineEditorsExampleNodeJS</div>
    </li>
    <li>Node.js comes with a package manager, <b>node package manager (npm)</b>, which is automatically installed along with Node.js. To run the Node.js code install the project modules using the following npm command:
        <div class="commandline">npm install</div>
        <p>A new <em>node_modules</em> folder will be created in the project folder.</p>
    </li>
    <li>Run the project using the <b>Command Prompt</b>:
        <div class="commandline">node bin/www</div>
    </li>
    <li>See the result in your browser using the address:
        <div class="commandline">http://localhost:3000</div>
    </li>
</ol>

<h2 id="Linux">Running the example on Linux OS</h2>
<h2 id="linux-1"><span class="style_step">Step 1. </span>Download and Install Document Server</h2>
<p>First, download the <a class="underline" href="<%= Url.Action("demopreview") %>"><b>ONLYOFFICE&trade; Editors</b></a> (the ONLYOFFICE&trade; Document Server).</p>
<p>See the detailed guide to learn how to <a class="underline" href="http://helpcenter.onlyoffice.com/server/linux/document/index.aspx">install Document Server for Linux</a>.</p>

<h2 id="linux-2"><span class="style_step">Step 2. </span>Install the prerequisites and run the web site with the editors</h2>
<ol>
    <li>Install <b>Node.js</b>:
        <div class="commandline">curl -sL https://deb.nodesource.com/setup_4.x | sudo bash -</div>
        <div class="commandline">apt-get install nodejs</div>
    </li>
    <li>Download the archive with the Node.js Example and unpack the archive:
        <div class="commandline">wget http://api.onlyoffice.com/app_data/Node.js%20Example.zip</div>
        <div class="commandline">unzip Node.js\ Example.zip</div>
    </li>
    <li>Change the current directory for the project directory:
        <div class="commandline">cd ~/OnlineEditorsExampleNodeJS/</div>
    </li>
    <li>Install the dependencies:
        <div class="commandline">npm install</div>
    </li>
    <li>Edit the <em>config.js</em> configuration file. Specify the name of your local server with the ONLYOFFICE&trade; Document Server installed.
        <div class="commandline">nano config.js</div>
        <p>Edit the following lines:</p>

        <pre>
config.storageUrl = "http://documentserver/FileUploader.ashx";
config.converterUrl = "http://documentserver/ConvertService.ashx";
config.tempStorageUrl = "http://documentserver/ResourceService.ashx";
config.apiUrl = "http://documentserver/OfficeWeb/apps/api/documents/api.js";
config.preloaderUrl = "http://documentserver/OfficeWeb/apps/api/documents/cache-scripts.html";
</pre>

        <p>Where the <b>documentserver</b> is the name of the server with the ONLYOFFICE&trade; Document Server installed.</p>
    </li>
    <li>Run the project with Node.js:
        <div class="commandline">nodejs bin/www</div>
    </li>
    <li>See the result in your browser using the address:
        <div class="commandline">http://localhost</div>
    </li>
</ol>
