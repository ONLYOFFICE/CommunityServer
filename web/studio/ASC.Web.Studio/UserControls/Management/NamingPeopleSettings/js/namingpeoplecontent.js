/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
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