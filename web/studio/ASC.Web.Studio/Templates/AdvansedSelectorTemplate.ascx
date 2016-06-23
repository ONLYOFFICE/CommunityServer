<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>


<script id="template-selector-container" type="text/x-jquery-tmpl">
        <div class="advanced-selector-container {{if (opts.onechosen === true)}}onechosen{{/if}}">
            <div class="advanced-selector-list-block">
                <div class="advanced-selector-block clearFix">
                    <div class="advanced-selector-block-list">
                        {{if (opts.showSearch ==true)}}
                    <div class="advanced-selector-search">
                        <input class="advanced-selector-search-field" type="text" />
                        <div class="advanced-selector-search-btn"></div>
                        <div class="advanced-selector-reset-btn"></div>
                    </div>
                         {{/if}}
                        {{if (opts.canadd === true)}}
                        <div class="advanced-selector-add-new">
                            <span class="advanced-selector-add-new-link">
                                <span class="advanced-selector-add-new-text"> ${opts.addtext}</span>
                            </span>
                        </div>
                        {{/if}}
                        {{if (opts.onechosen === false) && (opts.showGroups === false) && !(opts.isTempLoad)}}
                        <div class="advanced-selector-all-select">
                            <input type="checkbox" />
                            <label><%= Resources.UserControlsCommonResource.SelectAll%></label>
                        </div>
                        {{/if}}
                        <div class="advanced-selector-list-items">
                            <div class="advanced-selector-no-results"> ${opts.noresults}</div>
                            <div class="advanced-selector-no-items">${opts.noitems}</div>
                            <div class="advanced-selector-empty-list">${opts.emptylist}</div>
                            <ul class="advanced-selector-list"></ul>
                        </div>

                    </div>
                    {{if (opts.showGroups === true)}}
                    <div class="advanced-selector-block-list">
                        <div class="advanced-selector-all-select chosen">
                            {{if (opts.onechosen == false)}}<input type="checkbox" />{{/if}}
                            <label><%= Resources.UserControlsCommonResource.SelectAll%></label>
                        </div>
                        <div class="advanced-selector-list-groups">
                            <div class="advanced-selector-no-groups">${opts.nogroups}</div>
                            <ul class="advanced-selector-list"></ul>
                        </div>
                    </div>
                    {{/if}}
                    {{if (opts.onechosen === false)}}
                    <div class="advanced-selector-selected-count"></div>
                    {{/if}}
                </div>
                {{if (opts.onechosen === false)}}
                <div class="advanced-selector-btn-cnt">
                    <button type="button" class="advanced-selector-btn-action button blue"><%= Resources.UserControlsCommonResource.SaveButton%></button>
                    <span class="splitter-buttons"></span>
                    <button type="button" class="advanced-selector-btn-cancel button gray"><%= Resources.UserControlsCommonResource.CancelButton%></button>
                </div>
                {{/if}}
            </div>
        </div>
   
</script>

<script id="template-selector-add-new-item" type="text/x-jquery-tmpl">
    <div class="advanced-selector-add-new-block">
        <div class="advanced-selector-add-new-fields">
        {{each(i, field) fields}}
            <div class="advanced-selector-field-wrapper ${field.tag}">
                <div class="advanced-selector-title">${field.title}</div>
                    {{if field.type == "input"}}
                <input class="advanced-selector-field" type="text" />
                    {{else field.type == "date"}}
                <input class="textEditCalendar" type="text" />
                    {{else field.type == "choice"}}
                <select class="advanced-selector-field">
                     {{each(j, item) field.items}}}
                       <option value="${item.type}">${item.title}</option>
                     {{/each}}
                </select>
                    {{else field.type == "select"}}
                <div class="advanced-selector-field-search">
                    <input class="advanced-selector-field" type="text" />
                    <div class="advanced-selector-search-btn"></div>
                    <div class="advanced-selector-reset-btn" ></div>
                    <ul class="advanced-selector-field-list">
                    </ul>
                </div>
                    {{/if}}
                <span class="advanced-selector-field-error"></span>
            </div>
        {{/each}}
        </div>
        <div class="advanced-selector-btn-cnt">
            <button type="button" class="advanced-selector-btn-add button blue">${btnTitle}</button>
            <span class="splitter-buttons"></span>
            <button type="button" class="advanced-selector-btn-cancel button gray"><%= Resources.UserControlsCommonResource.CancelButton%></button>
        </div>
    </div>
</script>

<script id="template-selector-input" type="text/x-jquery-tmpl">
    <input class="advanced-selector-search" type="text" />
</script>

<script id="template-selector-items-list" type="text/x-jquery-tmpl">
    {{each(i, item) Items}}
        <li title="${item.status}" class="${item.status} ${item.type} {{if (item.isChecked==true)}} selected {{/if}}" data-id="${item.id}" {{if (item.data==true)}} data-cnt="${item.data}" {{/if}}>
            {{if (isJustList == false)}} <input type="checkbox" {{if (item.isChecked==true)}} checked{{/if}}/><label>{{/if}}
                ${item.title}
             {{if (isJustList == false)}}</label>{{/if}}
       </li>
    {{/each}}
</script>

<script id="template-selector-groups-list" type="text/x-jquery-tmpl">
    {{each(i, item) Items}}
        <li class="" data-id="${item.id}">
            {{if (isJustList == false)}}<input type="checkbox" />{{/if}}
            <label>${item.title}</label>
        </li>
    {{/each}}
</script>

<script id="template-selector-add-new-list-items" type="text/x-jquery-tmpl">
   {{each(i, item) Items}}
       <li data-id="${item.id}">${item.title}</li>
   {{/each}}
</script>

<script id="template-selector-add-new-select-items" type="text/x-jquery-tmpl">
   {{each(i, item) Items}}
       <option value="${item.type}">${item.title}</option>
   {{/each}}
</script>

<script id="template-selector-selected-items" type="text/x-jquery-tmpl">
    {{each(i, item) Items}}
        <li data-id="${item.id}">
            <span class="result-name">${item.title}</span>
            <span class="reset-icon"></span>
        </li>
    {{/each}}
</script>