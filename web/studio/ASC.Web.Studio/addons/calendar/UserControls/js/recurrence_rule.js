/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

if (typeof ASC === 'undefined')
    ASC = {};
    
if (typeof ASC.Api === 'undefined')
    ASC.Api = {};

ASC.Api.iCal = {}
ASC.Api.iCal.Frequency = {
    Never: 0,
    Daily: 3,
    Weekly: 4,
    Monthly: 5,
    Yearly: 6
};

ASC.Api.iCal.DayOfWeek = function(id, order) {
    this.Id = id;
    this.Order = 0;
    if (order != undefined)
        this.Order = order;

    this.Clone = function() {
        return new ASC.Api.iCal.DayOfWeek(this.Id, this.Order);
    }
};

ASC.Api.iCal.DayOfWeek.Monday =  new ASC.Api.iCal.DayOfWeek('mo');
ASC.Api.iCal.DayOfWeek.Tuesday = new ASC.Api.iCal.DayOfWeek('tu');
ASC.Api.iCal.DayOfWeek.Wednesday = new ASC.Api.iCal.DayOfWeek('we');
ASC.Api.iCal.DayOfWeek.Thursday = new ASC.Api.iCal.DayOfWeek('th');
ASC.Api.iCal.DayOfWeek.Friday = new ASC.Api.iCal.DayOfWeek('fr');
ASC.Api.iCal.DayOfWeek.Saturday = new ASC.Api.iCal.DayOfWeek('sa');
ASC.Api.iCal.DayOfWeek.Sunday = new ASC.Api.iCal.DayOfWeek('su');


ASC.Api.iCal.RecurrenceRule = function() {

    this.Freq = ASC.Api.iCal.Frequency.Never;
    this.Count = -1;
    this.Until = null;
    this.Interval = 1;
    this.ByDay = new Array();

    this.AdditionalParams = '';

    this.DateToiCalFormat = function(date) {

        var m = (date.getMonth() + 1);
        var d = date.getDate();
        var h = date.getHours();
        var min = date.getMinutes();

        var str = '' + date.getFullYear() + (m > 9 ? m : ('0' + m)) + (d > 9 ? d : ('0' + d))
                        + 'T' + (h > 9 ? h : ('0' + h)) + (min > 9 ? min : ('0' + min)) + "00Z";

        return str;
    }

    this.Equals = function(rrule) {
        return (this.ToiCalString() == rrule.ToiCalString());
    }

    this.ToiCalString = function() {

        var sb = new String();
        sb += 'freq='
        switch (this.Freq) {
            case ASC.Api.iCal.Frequency.Never:
                return "";

            case ASC.Api.iCal.Frequency.Daily:
                sb += 'daily';
                break;

            case ASC.Api.iCal.Frequency.Weekly:
                sb += 'weekly';
                break;

            case ASC.Api.iCal.Frequency.Monthly:
                sb += 'monthly';
                break;

            case ASC.Api.iCal.Frequency.Yearly:
                sb += 'yearly';
                break;
        }

        if (this.Until != null && this.Until != undefined && this.Until != "")
            sb += ';until=' + this.DateToiCalFormat(this.Until);

        if (this.Count >= 0)
            sb += ';count=' + this.Count;

        if (this.Interval > 1)
            sb += ';interval=' + this.Interval;

        if (this.ByDay != null && this.ByDay != undefined && this.ByDay.length > 0) {

            sb += ';byday=';
            var isFirst = true;
            for (var i = 0; i < this.ByDay.length; i++) {
                var d = this.ByDay[i];
                if (!isFirst)
                    sb += ',';
                else
                    isFirst = false;

                if (d.Order != 0)
                    sb += d.Order + d.Id;
                else
                    sb += d.Id;
            }
        }

        if (this.AdditionalParams != null && this.AdditionalParams != undefined && this.AdditionalParams != '')
            sb += this.AdditionalParams;

        return sb.toUpperCase();
    }
}

ASC.Api.iCal.ParseRRuleFromString = function(str) {

    var ParseiCalDate = function(iCalDate) {

        var year = parseInt(iCalDate.substr(0, 4),10);
        var month = parseInt(iCalDate.substr(4, 2),10) - 1;
        var day = parseInt(iCalDate.substr(6, 2),10);
        var h = 0;
        var m = 0;
        if (iCalDate.toLocaleLowerCase().indexOf('t') != -1) {
            h = parseInt(iCalDate.substr(9, 2),10);
            m = parseInt(iCalDate.substr(11, 2),10);
        }
        return new Date(year, month, day, h, m);
    }

    var rrule = new ASC.Api.iCal.RecurrenceRule();
    str = str.toLowerCase();
    var pairs = str.split(';');
    for (var i = 0; i < pairs.length; i++) {

        var name = pairs[i].split('=')[0];
        var val = pairs[i].split('=')[1];

        switch (name) {
            case 'freq':
                switch (val) {
                    case 'daily':
                        rrule.Freq = ASC.Api.iCal.Frequency.Daily;
                        break;

                    case 'weekly':
                        rrule.Freq = ASC.Api.iCal.Frequency.Weekly;
                        break;

                    case 'monthly':
                        rrule.Freq = ASC.Api.iCal.Frequency.Monthly;
                        break;

                    case 'yearly':
                        rrule.Freq = ASC.Api.iCal.Frequency.Yearly;
                        break;

                }
                break;

            case 'interval':
                rrule.Interval = parseInt(val);
                break;

            case 'count':
                rrule.Count = parseInt(val);
                break;

            case 'until':
                rrule.Until = ParseiCalDate(val);
                break;

            case 'byday':
                var vals = val.split(',');
                for (var j = 0; j < vals.length; j++) {
                    var day = vals[j].substr(vals[j].length - 2);
                    var dayOfWeek = new ASC.Api.iCal.DayOfWeek(day);
                    if (vals[j].length > 2)
                        dayOfWeek.Order = parseInt(vals[j].substr(0, vals[j].length - 2));

                    rrule.ByDay.push(dayOfWeek);
                }
                break;

            default:

                rrule.AdditionalParams += ';' + pairs[i];
                break;
        }
    }
    return rrule;
}

ASC.Api.iCal.RecurrenceRule.Never = new ASC.Api.iCal.RecurrenceRule();
ASC.Api.iCal.RecurrenceRule.EveryDay = ASC.Api.iCal.ParseRRuleFromString("FREQ=DAILY");
ASC.Api.iCal.RecurrenceRule.EveryWeek = ASC.Api.iCal.ParseRRuleFromString("FREQ=WEEKLY");
ASC.Api.iCal.RecurrenceRule.EveryMonth = ASC.Api.iCal.ParseRRuleFromString("FREQ=MONTHLY");
ASC.Api.iCal.RecurrenceRule.EveryYear = ASC.Api.iCal.ParseRRuleFromString("FREQ=YEARLY");

ASC.Api.iCal.RecurrenceRule.isVisibleYearlyEvent =  function (event) {
    if (event.repeatRule && event.repeatRule.Freq == ASC.Api.iCal.Frequency.Yearly && event.repeatRule.AdditionalParams) {
        var params = event.repeatRule.AdditionalParams.split(";");
        for (var i = 0; i < params.length; i++) {
            if (params[i].startsWith("exdates=")) {
                var start = jq.datepicker.formatDate('yymmdd', event.start);
                var dates = params[i].substr("exdates=".length).split(",");
                for (var j = 0; j < dates.length; j++) {
                    if (start == dates[j])
                        return false;
                }
            }
        }
    }
    return true;
};