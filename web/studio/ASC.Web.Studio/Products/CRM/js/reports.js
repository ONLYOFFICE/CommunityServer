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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.CRM === "undefined")
    ASC.CRM = function() { return {} };

ASC.CRM.Reports = (function() {

    var callbackMethods = {
        getContacts: function() {alert("1"); }
    };

    var bred = function() {
        var allData = [
          { label: "Данные 1", color: 0, data: [["2010/10/01", 0], ["2010/11/01", 1], ["2010/12/01", 7]]},
          { label: "Данные 2", color: 1, data: [["2010/10/01", 13], ["2010/11/01", 23], ["2010/12/01", 32]]}
        ];
        // преобразуем даты в UTC
        for(var j = 0; j < allData.length; ++j) {
            for (var i = 0; i < allData[j].data.length; ++i)
                allData[j].data[i][0] = Date.parse(allData[j].data[i][0]);
        }
        // свойства графика
        var plotConf = {
            series: {
                lines: {
                    show: true,
                    lineWidth: 2
                }
            },
            xaxis: {
                mode: "time",
                timeformat: "%y/%m/%d"
            }
        };

        // выводим график
        jq.plot(jq("#placeholder"), allData, plotConf);
    };

    function bred1() {
        bred2();
    }

    function bred2() {
        alert("3");
    }

    return {
            callbackMethods : callbackMethods,
            bred : bred,
            bred1 : bred1
    };
})();