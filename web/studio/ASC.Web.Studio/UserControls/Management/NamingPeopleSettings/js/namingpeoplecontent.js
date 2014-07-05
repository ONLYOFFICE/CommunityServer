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

NamingPeopleContentManager = function() {

    this.SaveSchema = function(parentCallback) {
        var schemaId = jq('#namingPeopleSchema').val();

        if (schemaId == 'custom') {
            NamingPeopleContentController.SaveCustomNamingSettings(jq('#usrcaption').val().substring(0, 30), jq('#usrscaption').val().substring(0, 30),
                                                       jq('#grpcaption').val().substring(0, 30), jq('#grpscaption').val().substring(0, 30),
                                                       jq('#usrstatuscaption').val().substring(0, 30), jq('#regdatecaption').val().substring(0, 30),
                                                       jq('#grpheadcaption').val().substring(0, 30),
                                                       jq('#guestcaption').val().substring(0, 30), jq('#guestscaption').val().substring(0, 30),
                                                       function(result) { if (parentCallback != null) parentCallback(result.value); });
        }
        else
            NamingPeopleContentController.SaveNamingSettings(schemaId, function(result) { if (parentCallback != null) parentCallback(result.value); });
    }

    this.SaveSchemaCallback = function(res) {
    }

    this.LoadSchemaNames = function(parentCallback) {

        var schemaId = jq('#namingPeopleSchema').val();
        NamingPeopleContentController.GetPeopleNames(schemaId, function(res) {
            var names = res.value;

            jq('#usrcaption').val(names.UserCaption);
            jq('#usrscaption').val(names.UsersCaption);
            jq('#grpcaption').val(names.GroupCaption);
            jq('#grpscaption').val(names.GroupsCaption);
            jq('#usrstatuscaption').val(names.UserPostCaption);
            jq('#regdatecaption').val(names.RegDateCaption);
            jq('#grpheadcaption').val(names.GroupHeadCaption);
            jq('#guestcaption').val(names.GuestCaption);
            jq('#guestscaption').val(names.GuestsCaption);

            if (parentCallback != null)
                parentCallback(res.value);
        });
    }
}

NamingPeopleContentViewer = new function() {
    this.ChangeValue = function(event) {
    jq('#namingPeopleSchema').val('custom');
    }
};

jq(document).ready(function() {
    jq('.namingPeopleBox input[type="text"]').each(function(i, el) {
        jq(el).keypress(function(event) { NamingPeopleContentViewer.ChangeValue(); });
    });
    var manager = new NamingPeopleContentManager();
	jq('#namingPeopleSchema').change(function () {
		manager.LoadSchemaNames(null);
	});
    manager.LoadSchemaNames(null);
});