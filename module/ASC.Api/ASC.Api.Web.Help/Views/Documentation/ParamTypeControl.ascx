<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TypeDescription>" %>
<p>
    <strong>Parameter type</strong>: <tt>
    <%=Model.Description%>&nbsp;<%if (Model.IsCollection) {%> <span class="label notice">Collection</span><% } %> </tt></p>
<%if (!string.IsNullOrEmpty(Model.Example))
  { %>
<p>
    <strong>Example</strong>: <tt>
        <%=Model.Example%></tt></p>
<% } %>
<%if (!string.IsNullOrEmpty(Model.Note))
  { %>
<p>
    <span class="label notice"><%=Model.Note%></span></p>
<% } %>
