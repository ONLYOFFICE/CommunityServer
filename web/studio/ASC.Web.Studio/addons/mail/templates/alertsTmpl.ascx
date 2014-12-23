<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>

<script id="alertPopupBodyTmpl" type="text/x-jquery-tmpl">
    <div class="popup">
        {{tmpl({
            errorBodyHeader     : $item.data.errorBodyHeader,
            errorBody           : $item.data.errorBody,
            errorBodyFooter     : $item.data.errorBodyFooter
        }) "errorBodyTmpl"}}
        <div class="buttons">
            {{tmpl(buttons) "alertPopupButtonTmpl"}}
        </div>
    </div>
</script>

<script id="alertPopupButtonTmpl" type="text/x-jquery-tmpl">
    <a href="${href}" class="button middle ${css_class}">${text}</a>
</script>
