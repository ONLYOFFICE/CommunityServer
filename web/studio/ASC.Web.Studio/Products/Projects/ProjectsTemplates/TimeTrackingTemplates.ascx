<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_timeTrackingTmpl" type="text/x-jquery-tmpl">
            <tr id="timeSpendRecord${id}" class="timeSpendRecord" timeid="${id}" taskid="${relatedTask}" date="${date}">
                  {{if showCheckbox}}  
                    <td class="pm-ts-statusCheckboxColumn">
                        {{if canEditPaymentStatus}}
                            <input data-timeid="${id}" type="checkbox"/>
                        {{/if}}
                    </td>
                  {{/if}}  
                    <td class="pm-ts-statusColumn">
                        
                            <div class="check">
                            <div data-timeid="${id}" class="changeStatusCombobox {{if canEditPaymentStatus}} canEdit {{/if}}">
                              {{if paymentStatus == 0}}
                                  <span title="<%= TimeTrackingResource.PaymentStatusNotChargeable%>" class="not-chargeable"></span>
                              {{/if}}
                              {{if paymentStatus == 1}}
                                  <span title="<%= TimeTrackingResource.PaymentStatusNotBilled%>" class="not-billed"></span>
                              {{/if}}
                              {{if paymentStatus == 2}}
                                  <span title="<%= TimeTrackingResource.PaymentStatusBilled%>" class="billed"></span>
                              {{/if}}
                              {{if canEditPaymentStatus}}   
                                <span class="arrow"></span>
                              {{/if}}             
                            </div>
                            </div>
                         
                    </td>
                    <td class="pm-ts-dateColumn" id="date_ts${id}">
                            ${displayDateCreation}
                    </td>
                    <td class="pm-ts-noteColumn stretch" id="note_ts${id}">
                            <a href="tasks.aspx?prjID=${relatedProject}&id=${relatedTask}" created="${displayDateCreation}" createdby="${createdBy.displayName}">${relatedTaskTitle}</a>
                            <span>${note}</span>
                    </td>
                    <td class="pm-ts-hoursColumn" id="hours_ts${id}">
                            ${jq.timeFormat(hours).split(":")[0]}<%=TimeTrackingResource.ShortHours %> ${jq.timeFormat(hours).split(":")[1]}<%=TimeTrackingResource.ShortMinutes %>
                    </td>
                    <td class="pm-ts-personColumn" id="person_ts${id}">
                     {{if person != null}}
                            <span userid="${person.id}" {{if person.id=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}} class="not-action"{{/if}} title="${person.displayName}">${person.displayName}</span>                        
                      {{else}}
                            <span userid="${createdBy.id}" {{if createdBy.id=='4a515a15-d4d6-4b8e-828e-e0586f18f3a3'}} class="not-action"{{/if}} title="${createdBy.displayName}"> ${createdBy.displayName}</span>
                      {{/if}}
                    </td>

                    <td class="pm-ts-actionsColumn with-entity-menu">
                    {{if canEdit == true}}
                        <div class="entity-menu" timeid="${id}" prjId="${relatedProject}" userid="${person ? person.id : createdBy.id}"></div>
                    {{/if}}
                    </td>
            </tr>
</script>