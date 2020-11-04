<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditPage.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.EditPage" %>
<%@ Register Src="MiniToolbar.ascx" TagPrefix="uc" TagName="ToolBar" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>

<script language="javascript" type="text/javascript">
    var WikiPlugName = 'fck_wysiwyg';
    function WikiConfigAdditionalInfo(config) {
        config.Plugins.Add(WikiPlugName, 'en,ru,fr,es,de,lv,it');
        config.WikiSignature = '--~~~~';
        //config.ForcePasteAsPlainText = true ;
        config.FontFormats	= 'p;h1;h2;h3;h4;pre' ;
        /*config.ToolbarSets["WikiPanel"] = [
	        ['Source', 'WikiBold', 'WikiItalic', 'WikiUnderline', 'WikiStriked'], ['WikiHeader1', 'WikiHeader2', 'WikiHeader3', 'WikiHeader4'],
	        ['WikiSup', 'WikiSub'], ['WikiNoBr', 'WikiComment']
        ];*/
        config.ToolbarSets['WikiPanel'] = [
	['Source'],
	['FontFormat', 'Bold', 'Italic', 'Underline', 'StrikeThrough', 'Subscript', 'Superscript', 'RemoveFormat'],
	['Link', 'Unlink'],
	['WikiTOC', 'Table', 'Image', 'File', 'Code', 'Rule'],
	['OrderedList', 'UnorderedList'],
	['Undo', 'Redo']
	
	//, '-', 'SelectAll', 'RemoveFormat']
	
	
    ];
    
    config.ContextMenu = ['Generic','Image'] ;
    
    };
    <%=InitVariables() %>
    
    
    function getUserId()
    {
        return '<%=CurrentUserId %>';
    }
    
    function getAjaxUploaderJsPath()
    {
        return '<%=AlaxUploaderPath%>';
    }
    
    function getJQPath()
    {
        return '<%=JQPath%>';
    }
    
   
    function FCK_sajax(name, value, callback_func) {
        <%=GetPageClassName()%>.ConvertWikiToHtmlWysiwyg(pageName, value, appRelativeCurrentExecutionFilePath, imageHandlerUrl, callback_func);
    }
    
    function FCK_dlg_ajax(name, value, callback_func) {
        switch(name)
        {
            case "wfAjaxSearchPagesFCKeditor":
                if (<%=GetPageClassName()%>.SearchPagesByStartName) {
                    <%=GetPageClassName()%>.SearchPagesByStartName(value, callback_func);
                }
                break;
            case "wfAjaxSearchFilesFCKeditor":
                if (<%=GetPageClassName()%>.SearchFilesByStartName) {
                    <%=GetPageClassName()%>.SearchFilesByStartName(value, callback_func);
                }
                break;
           case "wfAjaxFCKeditorGetImageUrl":
                var result = new Object();
                result.value = imageHandlerUrl.replace('{0}', value);
                callback_func(result);
                break;
           case "wfAjaxFCKeditorGetImageHtml" :
                <%=GetPageClassName()%>.CreateImageFromWiki(pageName, value, appRelativeCurrentExecutionFilePath, imageHandlerUrl, callback_func);
                break;
           case "wfAjaxFCKeditorUpdateTempImage":
                <%=GetPageClassName()%>.UpdateTempImage(value.FileName, value.UserId, value.TempFileName, callback_func);
                break;
           case "wfAjaxFCKeditorCancelUpdateImage":
                <%=GetPageClassName()%>.CancelUpdateImage(value.UserId, value.TempFileName);
                break;
        }
        
    }
    
    function AppendFCKHeader(){
        return <%=GetAllStyles() %>
    }
    
    function AppendBodyClassName()
    {
        return '<%=MainWikiClassName %>';
    }
    
    var <%=this.ClientID%>FCKInstance = null;
    var wikiSourceCommand = null;
    

    function Page_FCKeditor_OnComplete(instance) //IE7 hack
    {
        <%=this.ClientID%>FCKInstance = instance;
        if(FCKeditorAPI == null)
        {
            FCKeditorAPI = new Object();
            FCKeditorAPI.GetInstance = function(instanceName)
            {
                return <%=this.ClientID%>FCKInstance;
            }
        }
        
        instance.OnSwitchedEditMode = function(isSource)
        {
            document.getElementById('<%=hfFCKLastState.ClientID %>').value = isSource ? 'true' : 'false';
        }
        
         <%if(!IsWysiwygDefault) {%>
            if(wikiSourceCommand == null)
            {
                wikiSourceCommand = instance.Commands.GetCommand('Source');
                wikiSourceCommand.UpdateToolBar(true);
                
                var item = null;
                
                for(var i = 0; i < instance.ToolbarSet.Items.length; i++)
                {
                    item = instance.ToolbarSet.Items[i];
                    if(item.CommandName.toUpperCase() == 'SOURCE')
                    {
                        break;
                    }
                    else
                    {
                        item = null;
                    }
                }
                
                if(item != null)
                {
                    item._UIButton.ChangeState(1, true);
                }
                
            }
        <%} %>

    }
    
    function GetFCKStartMode()
    {
        var result = 0; //FCK_EDITMODE_WYSIWYG;
        <%if(!IsWysiwygDefault) {%>
            result = 1; //FCK_EDITMODE_SOURCE
        <%} %>
        return result;
    }
   
    
    function <%=this.ClientID%>_ShowPreview()
    {
        var value='';

        var oEditor = <%=this.ClientID%>FCKInstance != null ? <%=this.ClientID%>FCKInstance : FCKeditorAPI.GetInstance('<%=Wiki_FCKEditor.ClientID%>');

        AjaxPro.onLoading = function(b)
        {
            if(b){
                LoadingBanner.showLoaderBtn("#actionWikiPage");
            }
            else{
                LoadingBanner.hideLoaderBtn("#actionWikiPage");
            }
        };

        if (oEditor) {
            value = oEditor.GetHTML();
        }

        <%=GetPageClassName()%>.ConvertWikiToHtml(pageName, value, appRelativeCurrentExecutionFilePath, imageHandlerUrl, <%=this.ClientID%>_ShowPreviewReady);
    }
    
    function <%=this.ClientID%>_ShowPreviewReady(result)
    {
        document.getElementById('<%=PreviewView%>').innerHTML = result.value;
        document.getElementById('<%=PreviewContainer%>').style.display = '';
        if(typeof(<%=OnPreviewReadyHandler%>) == 'function')
        {
            <%=OnPreviewReadyHandler%>();
        }
    }
    
</script>

<asp:HiddenField ID="hfFCKLastState" runat="Server" />
<div class="<%=MainWikiClassName %>">
    <div class="wikiEdit">
        <asp:PlaceHolder ID="phPageName" runat="Server">
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=WikiUCResource.editWiki_PageName%>:</b>
                </div>
                <div>
                    <input type="text" ID="txtPageName" maxlength="240" class="textEdit" runat="Server" style="width:100%;" />
                </div>
            </div>
        </asp:PlaceHolder>
        <div id="<%=this.ClientID%>_PrevContainer" style="display: none;">
            <div class="headerPanelSmall-splitter headerPanelSmall">
                <b><%=WikiUCResource.editWiki_PagePreview%></b>
            </div>
            <div class="subHeaderPanel">
                <div id="<%=this.ClientID%>_PrevValue" class="wiki">
                </div>
            </div>
        </div>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter">
                <b><%=WikiUCResource.editWiki_PageBody%>:</b>
            </div>
            <div id="taWikiTools" style="display: none;">
                <uc:ToolBar runat="Server" ID="ucToolBar" />
            </div>
            <%--<div>
                <asp:TextBox ID="txtPageBody" runat="Server" CssClass="textarea" Width="99%" Height="350px"
                    Rows="2" Columns="20" TextMode="MultiLine" />--%>
            <div>
                <FCKeditorV2:FCKeditor runat="server" ID="Wiki_FCKEditor" Width="100%" Height="400px" />
            </div>
        </div>
    </div>
</div>
<!--%=txtPageBody.ClientID %-->
