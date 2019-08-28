<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Styles.aspx.cs" MasterPageFile="Masters/BasicTemplate.Master" Inherits="ASC.Web.Sample.Styles" %>

<%@ MasterType TypeName="ASC.Web.Sample.Masters.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div>
        <h1>Text styles</h1>
        <div class="header-base big">header-base big</div>
        <div class="header-base large">header-base large</div>
        <div class="header-base">header-base</div>
        <div class="header-base middle">header-base middle</div>
        <div class="header-base medium">header-base medium</div>
        <div class="header-base small">header-base small</div>
        <div class="bold">bold</div>
        <div class="red-text">red-text</div>
        <div class="gray-text">gray-text</div>
        <div class="blue-text">blue-text</div>

<pre class="top"><code>&lt;div class="header-base big"&gt;header-base big&lt;/div&gt;
&lt;div class="header-base large"&gt;header-base large&lt;/div&gt;
&lt;div class="header-base"&gt;header-base&lt;/div&gt;
&lt;div class="header-base middle"&gt;header-base middle&lt;/div&gt;
&lt;div class="header-base medium"&gt;header-base medium&lt;/div&gt;
&lt;div class="header-base small"&gt;header-base small&lt;/div&gt;
&lt;div class="bold"&gt;bold&lt;/div&gt;
&lt;div class="red-text"&gt;red-text&lt;/div&gt;
&lt;div class="gray-text"&gt;gray-text&lt;/div&gt;
&lt;div class="blue-text"&gt;blue-text&lt;/div&gt;</code></pre>
        
        <h1>Link styles</h1>
        <a class="link">link</a><br />
        <a class="link dotted">link dotted</a><br />
        <a class="link dotline">link dotline</a><br />
        <a class="link underline">link underline</a><br />
        <a class="link header blue">link header blue</a><br />
        <a class="link middle blue">link middle blue</a><br />
        <a class="link medium gray">link medium gray</a><br />
        <a class="link small gray">link small gray</a><br />
        <a class="link bold">link bold</a>

<pre class="top"><code>&lt;a class="link"&gt;link&lt;/a&gt;
&lt;a class="link dotted"&gt;link dotted&lt;/a&gt;
&lt;a class="link dotline"&gt;link dotline&lt;/a&gt;
&lt;a class="link underline"&gt;link underline&lt;/a&gt;
&lt;a class="link header blue"&gt;link header blue&lt;/a&gt;
&lt;a class="link middle blue"&gt;link middle blue&lt;/a&gt;
&lt;a class="link medium gray"&gt;link medium gray&lt;/a&gt;
&lt;a class="link small gray"&gt;link small gray&lt;/a&gt;
&lt;a class="link bold"&gt;link bold&lt;/a&gt;</code></pre>

        <h1>Button styles</h1>
        <div class="big-button-container">
            <a class="button blue big">button blue big</a>
            <span class="splitter-buttons"></span>
            <a class="button blue big disable">button blue big disable</a>
            <span class="splitter-buttons"></span>
            <a class="button gray big">button gray big</a>
        </div>

<pre class="top"><code>&lt;div class="big-button-container"&gt;
    &lt;a class="button blue big"&gt;button blue big&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button blue big disable"&gt;button blue big disable&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button gray big"&gt;button gray big&lt;/a&gt;
&lt;/div&gt;</code></pre>        

        <div class="middle-button-container">
            <a class="button blue middle">button blue middle</a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle">button gray middle</a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle disable">button gray middle disable</a>
        </div>
        
<pre class="top"><code>&lt;div class="middle-button-container"&gt;
    &lt;a class="button blue middle"&gt;button blue middle&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button gray middle"&gt;button gray middle&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button gray middle disable"&gt;button gray middle disable&lt;/a&gt;
&lt;/div&gt;</code></pre> 

        <div class="small-button-container">
            <a class="button blue medium">button blue medium</a>
            <span class="splitter-buttons"></span>
            <a class="button gray medium">button gray medium</a>
            <span class="splitter-buttons"></span>
            <a class="button gray group"><span>button gray group</span></a>
        </div>
        
<pre class="top"><code>&lt;div class="small-button-container"&gt;
    &lt;a class="button blue medium"&gt;button blue medium&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button gray medium"&gt;button gray medium&lt;/a&gt;
    &lt;span class="splitter-buttons"&gt;&lt;/span&gt;
    &lt;a class="button gray group"&gt;button gray group&lt;/a&gt;
&lt;/div&gt;</code></pre>

    </div>
</asp:Content>
