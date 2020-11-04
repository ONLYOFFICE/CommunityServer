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