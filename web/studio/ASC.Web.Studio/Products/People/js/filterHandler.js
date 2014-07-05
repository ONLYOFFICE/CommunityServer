/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
(function() {
    function onRenderProfiles(evt, params, data) {
        jq('#peopleFilter').advansedFilter({
            nonetrigger: true,
            sorters: [
                        { id: 'by-name', dsc: params.sortorder }
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
                        id: 'selected-status-active',
                        params: params.status ? { value: params.status} : null
                    },
                    {
                        id: 'selected-status-pending',
                        params: params.status ? { value: params.status} : null
                    },
                    {
                        id: 'selected-status-disabled',
                        params: params.status ? { value: params.status} : null
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
        if (params.groupId || params.status || params.type) {
            jq('#peopleFilter').addClass("has-filters");
        }
    }

    jq(window).bind('people-render-profiles', onRenderProfiles);
})();
