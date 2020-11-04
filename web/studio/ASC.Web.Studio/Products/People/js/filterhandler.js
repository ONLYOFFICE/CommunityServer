/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


(function() {
    function onRenderProfiles(evt, params, data) {
        jq('#peopleFilter').advansedFilter({
            nonetrigger: true,
            hintDefaultDisable: true,
            sorters: [
                        { id: params.sortby, dsc: params.sortorder }
                     ],
            filters: [
                    {
                        id: 'text',
                        params: { value: params.query }
                    },
                    {
                        id: 'selected-group',
                        params: params.groupId ? { id: params.groupId} : null
                    },
                    {
                        id: 'selected-employee-status-active',
                        params: params.employeestatus ? { value: params.employeestatus } : null
                    },
                    {
                        id: 'selected-employee-status-disabled',
                        params: params.employeestatus ? { value: params.employeestatus } : null
                    },
                    {
                        id: 'selected-activation-status-active',
                        params: params.activationstatus ? { value: params.activationstatus } : null
                    },
                    {
                        id: 'selected-activation-status-pending',
                        params: params.activationstatus ? { value: params.activationstatus } : null
                    },
                    {
                        id: 'selected-type-admin',
                        params: params.type ? { value: params.type } : null
                    },
                    {
                        id: 'selected-type-user',
                        params: params.type ? { value: params.type} : null
                    },
                    {
                        id: 'selected-type-visitor',
                        params: params.type ? { value: params.type} : null
                    }
                  ]
        });
        if (params.groupId || params.employeestatus || params.activationstatus || params.type) {
            jq('#peopleFilter').addClass("has-filters");
        }
    }

    jq(window).bind('people-render-profiles', onRenderProfiles);
})();
