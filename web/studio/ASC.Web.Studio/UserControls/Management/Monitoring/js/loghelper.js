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


LogHelper = new function() {
    this.Init = function () {
        var $cnt = jq(".monitoring-container"),
            startDate = new Date(),
            endDate = new Date();
            
        startDate.setDate(endDate.getDate() - 2);

        $cnt.find(".textEditCalendar").datepicker();
        $cnt.find("input.start-date").datepicker('setDate', startDate);
        $cnt.find("input.end-date").datepicker('setDate', endDate);

        jq("#downloadLogsBtn").on("click", function() {
            LogHelper.DownloadArchive();
        });
    };

    this.DownloadArchive = function() {
        var $cnt = jq(".monitoring-container"),
            startDate = Teamlab.serializeTimestamp($cnt.find("input.start-date").datepicker('getDate')),
            endDate = Teamlab.serializeTimestamp($cnt.find("input.end-date").datepicker('getDate'));

        window.location.href = window.location.href + "&start=" + startDate + "&end=" + endDate + "&download=true";
    };
};

jq(function() {
    LogHelper.Init();
});