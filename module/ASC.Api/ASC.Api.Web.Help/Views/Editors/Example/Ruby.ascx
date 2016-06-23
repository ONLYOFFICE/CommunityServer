<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <span class="hdr">How to integrate online editors into your own web site on Ruby</span>
</h1>

<h2>Introduction</h2>
<p>To integrate <b>ONLYOFFICE&trade; online editors</b> into your own website on <b>Ruby</b> you need to download and install ONLYOFFICE&trade; editors on your local server and use the <a class="underline" href="<%= Url.Action("demopreview") %>">Ruby Example</a> for their integration. We will show how to run the Ruby example on Linux OS.</p>
<p>This guide will show you the sequence of actions to integrate the editors successfully.</p>

<h2 id="linux-1"><span class="style_step">Step 1. </span>Download and Install Document Server</h2>
<p>First, download the <a class="underline" href="<%= Url.Action("demopreview") %>"><b>ONLYOFFICE&trade; Editors</b></a> (the ONLYOFFICE&trade; Document Server).</p>
<p>See the detailed guide to learn how to <a class="underline" href="http://helpcenter.onlyoffice.com/server/linux/document/index.aspx">install Document Server for Linux</a>.</p>

<h2 id="linux-2"><span class="style_step">Step 2. </span>Install the prerequisites and run the web site with the editors</h2>
<ol>
    <li>Install <b>Ruby Version Manager (RVM)</b> and the latest stable <b>Ruby</b> version:
        <div class="commandline">gpg --keyserver "hkp://keys.gnupg.net" --recv-keys 409B6B1796C275462A1703113804BB82D39DC0E3</div>
        <div class="commandline">\curl -sSL https://get.rvm.io | bash -s stable --ruby</div>
    </li>
    <li>Download the archive with the Ruby Example and unpack the archive:
        <div class="commandline">wget "http://api.onlyoffice.com/app_data/Ruby%20Example.zip"</div>
        <div class="commandline">unzip Ruby\ Example.zip</div>
    </li>
    <li>Change the current directory for the project directory:
        <div class="commandline">cd ~/OnlineEditorsExampleRuby</div>
    </li>
    <li>Install the dependencies:
        <div class="commandline">bundle install</div>
    </li>
    <li>Edit the <em>application.rb</em> configuration file. Specify the name of your local server with the ONLYOFFICE&trade; Document Server installed.
        <div class="commandline">nano config/application.rb</div>
        <p>Edit the following lines:</p>

        <pre>
Rails.configuration.urlStorage="http://documentserver/FileUploader.ashx"
Rails.configuration.urlConverter="http://documentserver/ConvertService.ashx"
Rails.configuration.urlApi="http://documentserver/OfficeWeb/apps/api/documents/api.js"
Rails.configuration.urlPreloader="http://documentserver/OfficeWeb/apps/api/documents/cache-scripts.html"
</pre>

        <p>Where the <b>documentserver</b> is the name of the server with the ONLYOFFICE&trade; Document Server installed.</p>
    </li>
    <li>Run the <b>Rails</b> application:
        <div class="commandline">rails s -b 0.0.0.0 -p 80</div>
    </li>
    <li>See the result in your browser using the address:
        <div class="commandline">http://localhost</div>
        <p>If you want to experiment with the editor configuration, modify the <a class="underline" href="<%= Url.Action("advanced") %>">parameters</a> it the <em>views\home\editor.html.erb</em> file.</p>
    </li>
</ol>
