<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MiniToolbar.ascx.cs"
    Inherits="ASC.Web.UserControls.Wiki.UC.MiniToolbar" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>

<div class="MarkupToolbarDiv">
    <div class="TB_Start toolbarbuttonAdditional" ></div>
    <a href="javascript:;" class="toolbarbutton H1" title="<%= WikiUCResource.miniToolBar_H1 %>"
        onclick="javascript:return WrapSelectedMarkup('\n=', '=\n');">
    </a>
    <a href="javascript:;" class="toolbarbutton H2" title="<%= WikiUCResource.miniToolBar_H2 %>"
        onclick="javascript:return WrapSelectedMarkup('\n==', '==\n');">
    </a>
    <a href="javascript:;" class="toolbarbutton H3" title="<%= WikiUCResource.miniToolBar_H3 %>"
        onclick="javascript:return WrapSelectedMarkup('\n===', '===\n');">
    </a>
    <a href="javascript:;" class="toolbarbutton H4" title="<%= WikiUCResource.miniToolBar_H4 %>"
        onclick="javascript:return WrapSelectedMarkup('\n====', '====\n');">
    </a>
    <div class="TB_Start toolbarbuttonAdditional"></div>
    <a href="javascript:;" class="toolbarbutton Bold" title="<%=WikiUCResource.miniToolBar_Bold %>"
        onclick="javascript:return WrapSelectedMarkup('\'\'\'', '\'\'\'');">
    </a>
    <a href="javascript:;" class="toolbarbutton Italic" title="<%= WikiUCResource.miniToolBar_Italic %>"
        onclick="javascript:return WrapSelectedMarkup('\'\'', '\'\'');">
    </a>
    <a href="javascript:;" class="toolbarbutton Underlined" title="<%= WikiUCResource.miniToolBar_Underlined %>"
        onclick="javascript:return WrapSelectedMarkup('__', '__');">
    </a>
    <a href="javascript:;" class="toolbarbutton Striked" title="<%= WikiUCResource.miniToolBar_Striked %>"
        onclick="javascript:return WrapSelectedMarkup('--', '--');">
    </a>
    <div class="TB_Separator toolbarbuttonAdditional"></div>
    <a href="javascript:;" class="toolbarbutton Subscript" title="<%= WikiUCResource.miniToolBar_Subscript %>"
        onclick="javascript:return WrapSelectedMarkup('<sub>', '</sub>');">
    </a>
    <a href="javascript:;" class="toolbarbutton Superscript" title="<%= WikiUCResource.miniToolBar_Superscript %>"
        onclick="javascript:return WrapSelectedMarkup('<sup>', '</sup>');">
    </a>
    <div class="TB_Start toolbarbuttonAdditional"></div>
    <a href="javascript:;" class="toolbarbutton PageLink" title="<%= WikiUCResource.miniToolBar_PageLink %>"
        onclick="javascript:return InsertMarkup('[[PageName | Page Description]]');">
    </a>
    <a href="javascript:;" class="toolbarbutton ExternalLink" title="<%= WikiUCResource.miniToolBar_ExternalLink %>"
        onclick="javascript:return InsertMarkup('[[http://URL | Link Description]]');">
    </a>
    <div class="TB_Start toolbarbuttonAdditional"></div>
    <a href="javascript:;" class="toolbarbutton Image" title="<%= WikiUCResource.miniToolBar_Image %>"
        onclick="javascript:return InsertMarkup('[[Image:ImageName]]');">
    </a>
    <a href="javascript:;" class="toolbarbutton FileLink" title="<%= WikiUCResource.miniToolBar_FileLink %>"
        onclick="javascript:return InsertMarkup('[[File:FileName | File Description]]');">
    </a>
    <a href="javascript:;" class="toolbarbutton HR" title="<%= WikiUCResource.miniToolBar_HR %>"
        onclick="javascript:return InsertMarkup('\n----\n');">
    </a>
    <%--
    <a href="javascript:;" class="toolbarbutton Anchor" title="<%= WikiUCResource.miniToolBar_Anchor %>"
            onclick="javascript:return InsertMarkup('[[#AncorName | Ancor Description]]');">
        </a> 
    
    <a href="javascript:;" class="toolbarbutton LineBreak" title="<%= WikiUCResource.miniToolBar_LineBreak %>"
        onclick="javascript:return InsertMarkup('{br}');">
    </a>
    <a href="javascript:;" class="toolbarbutton SpetialTags" title="<%= WikiUCResource.miniToolBar_SpetialTags %>"
        onclick="javascript:return ShowSpecialTagsMenuMarkup(event);">
    </a>--%>
    
    <%--<a href="javascript:;" class="toolbarbutton CodeInline" title="<%= WikiUCResource.miniToolBar_CodeInline %>"
        onclick="javascript:return WrapSelectedMarkup('{{', '}}');">
    </a>
    <a href="javascript:;" class="toolbarbutton CodeEscaped" title="<%= WikiUCResource.miniToolBar_CodeEscaped %>"
        onclick="javascript:return WrapSelectedMarkup('@@', '@@');">
    </a>
    <a href="javascript:;" class="toolbarbutton Escape" title="<%= WikiUCResource.miniToolBar_Escape %>"
        onclick="javascript:return WrapSelectedMarkup('<esc>', '</esc>');">
    </a>
    <a href="javascript:;" class="toolbarbutton NoWiki" title="<%= WikiUCResource.miniToolBar_NoWiki %>"
        onclick="javascript:return WrapSelectedMarkup('<nowiki>', '</nowiki>');">
    </a>
    <a href="javascript:;" class="toolbarbutton NoBr" title="<%= WikiUCResource.miniToolBar_NoBr %>"
        onclick="avascript:return WrapSelectedMarkup('<nobr>', '</nobr>');">
    </a>
    <a href="javascript:;" class="toolbarbutton WrapperBox" title="<%= WikiUCResource.miniToolBar_WrapperBox %>"
        onclick="javascript:return WrapSelectedMarkup('(((', ')))');">
    </a>
    <a href="javascript:;" class="toolbarbutton Comment" title="<%= WikiUCResource.miniToolBar_Comment %>"
        onclick="javascript:return WrapSelectedMarkup('<!--', '-->');">
    </a>
    <a href="javascript:;" class="toolbarbutton Symbols" title="<%= WikiUCResource.miniToolBar_Symbols %>"
        onclick="javascript:return ShowSymbolsMenuMarkup(event);">
    </a>--%>
</div>
