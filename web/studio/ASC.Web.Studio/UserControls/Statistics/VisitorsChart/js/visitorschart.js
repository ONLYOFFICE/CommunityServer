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


(function($) {
    var 
    colors = {
        hits: '#EDC240',
        hosts: '#AFD8F8',
        filterbyweek: '#EDC240',
        filterbymonth: '#CB4B4B',
        filterby3months: '#AFD8F8',
        filterbyperiod: '#4DA74D'
    },
    displayDates = {},
    chartData = {},
    lastFilter = '';

    function managePeriodFilter(toggle) {
        if (toggle === true) {
            $('#periodSelection').removeClass('disabled');
            $('#startDate input').removeAttr('disabled');
            $('#endDate input').removeAttr('disabled');
        } else {
            $('#periodSelection').addClass('disabled');
            $('#startDate input').attr('disabled', 'disabled');
            $('#endDate input').attr('disabled', 'disabled');
        }
    }

    function changePeriod(evt) {
        var from, to;

        try {
            from = jq("#studio_chart_FromDate").datepicker("getDate");
            to = jq("#studio_chart_ToDate").datepicker("getDate");
            from = new Date(Date.UTC(from.getFullYear(), from.getMonth(), from.getDate()));
            to = new Date(Date.UTC(to.getFullYear(), to.getMonth(), to.getDate()));
        }
        catch (e) { }

        if (from instanceof Date && isFinite(from.getTime()) && to instanceof Date && isFinite(to.getTime()) && from.getTime() < to.getTime()) {
            $('#visitorsChartCanvas').addClass('loader32').empty();
            $('#chartDownloadStatistics').addClass('disabled');
            $('#chartLegend').children('li.label:not(.default)').remove();
            VisitorsChart.GetVisitStatistics(from, to, showChart);
        }
        else {
            $('#visitorsChartCanvas').empty();
            $('#chartDownloadStatistics').addClass('disabled');
            $('#chartLegend').children('li.label:not(.default)').remove();
        }
    }

    function showChart(param) {
        if (!param) {
            return undefined;
        }
        $('#visitorsChartCanvas').removeClass('loader32');
        if (typeof param === 'string' && (param = param.toLowerCase()).length > 0) {
            var 
        from = new Date(),
        to = new Date();
            from = new Date(Date.UTC(from.getFullYear(), from.getMonth(), from.getDate()));
            to = new Date(Date.UTC(to.getFullYear(), to.getMonth(), to.getDate()));

            switch (param) {
                case 'filterbyweek':
                    $('#visitorsFilter').find('li.filter').removeClass('selected');
                    $('#filterByWeek').addClass('selected');
                    managePeriodFilter(false);
                    from.setDate(to.getDate() - 6);
                    break;
                case 'filterbymonth':
                    $('#visitorsFilter').find('li.filter').removeClass('selected');
                    $('#filterByMonth').addClass('selected');
                    managePeriodFilter(false);
                    from.setMonth(to.getMonth() - 1);
                    break;
                case 'filterby3months':
                    $('#visitorsFilter').find('li.filter').removeClass('selected');
                    $('#filterBy3Months').addClass('selected');
                    managePeriodFilter(false);
                    from.setMonth(to.getMonth() - 3);
                    break;
                case 'filterbyperiod':
                    $('#visitorsFilter').find('li.filter').removeClass('selected');
                    $('#filterByPeriod').addClass('selected');
                    managePeriodFilter(true);

                    from = jq("#studio_chart_FromDate").datepicker("getDate");
                    to = jq("#studio_chart_ToDate").datepicker("getDate");

                    if (from instanceof Date && to instanceof Date) {
                        from = new Date(Date.UTC(from.getFullYear(), from.getMonth(), from.getDate()));
                        to = new Date(Date.UTC(to.getFullYear(), to.getMonth(), to.getDate()));
                    }

                    break;
                default:
                    return undefined;
            }
            lastFilter = param;
            if (from instanceof Date && isFinite(from.getTime()) && to instanceof Date && isFinite(to.getTime()) && from.getTime() < to.getTime()) {
                $('#visitorsChartCanvas').addClass('loader32').empty();
                $('#chartDownloadStatistics').addClass('disabled');
                $('#chartLegend').children('li.label:not(.default)').remove();
                VisitorsChart.GetVisitStatistics(from, to, arguments.callee);
            }
        } else if (typeof param === 'object' && param.hasOwnProperty('value') && param.value) {
            var 
        date = null,
        hits = [],
        hosts = [];
            switch (lastFilter) {
                case 'filterbyweek':
                case 'filterbymonth':
                case 'filterby3months':
                case 'filterbyperiod':
                    break;
                default:
                    return undefined;
            }

            param = param.value;

            for (var i = 0, n = param.length; i < n; i++) {
                param[i].Date.setUTCHours(0, 0, 0);
                displayDates[param[i].Date.getTime()] = param[i].DisplayDate;
                hits.push([param[i].Date, param[i].Hits]);
                // hosts.push([param[i].Date, param[i].Hosts]);
            }

            $('#chartDownloadStatistics').removeClass('disabled');
            $('#chartLegend').children('li.label:not(.default)').remove();

            var $label = $('#chartLegend').children('li.label.default:first').clone().removeClass('default');
            $label.find('div.color:first').css('backgroundColor', colors.hits);
            $label.find('div.title:first').html(ASC.Resources.hitLabel);
            $('#chartLegend').append($label);

            $.plot(
        $('#visitorsChartCanvas'),
        [
          {
              label: ASC.Resources.hitLabel,
              color: colors.hits,
              data: hits
          }
            //          {
            //            label : ASC.Resources.hostLabel,
            //            color : colors.hosts,
            //            data  : hosts
            //          }
        ],
        {
            grid: { hoverable: true, clickable: true },
            legend: { show: false },
            series: { lines: { show: true }, points: { show: true, radius: 2} },
            xaxis: { mode: 'time', timeformat: ASC.Resources.chartDateFormat, monthNames: ASC.Resources.chartMonthNames.split(/\s*,\s*/) },
            yaxis: { min: 0 }
        }
      );
        }
    }

    $(document).ready(function() {
      $('#visitorsChartCanvas')
        .bind("plothover", function(evt, pos, item) {
          if (item) {
            if (!displayDates.hasOwnProperty(item.datapoint[0])) {
              return undefined;
            }
            var content =
              '<h6 class="label">' + item.series.label + ' : ' + displayDates[item.datapoint[0]] + '</h6>' +
              '<div class="info">' + item.datapoint[1] + ' visits' + '</div>';
            ASC.Common.toolTip.show(content, function() {
              var $this = $(this);
              $this.css({
                  left: item.pageX + 5,
                  top: item.pageY - $this.outerHeight(true) - 5
              });
            });
          } else {
            ASC.Common.toolTip.hide();
          }
        });

      $("#studio_chart_FromDate, #studio_chart_ToDate").mask(jq('input[id$=jQueryDateMask]').val());

      var
        defaultFromDate = new Date(),
        defaultToDate = new Date();
      defaultFromDate.setMonth(defaultFromDate.getMonth() - 6);

        var maxDate = new Date();
        maxDate.setDate(maxDate.getDate() - 1);
        var minDate = new Date();
        minDate.setMonth(minDate.getMonth() - 6);
        minDate.setDate(minDate.getDate() + 1);
      
      $("#studio_chart_FromDate")
        .datepicker({
          onSelect : function () {
            var date = jq(this).datepicker("getDate");
            date.setDate(date.getDate() + 1);
            $("#studio_chart_ToDate").datepicker("option", "minDate", date || null);
            changePeriod();
          }
        })
        .datepicker("setDate", defaultFromDate)
        .datepicker("option", "maxDate", maxDate);

      $("#studio_chart_ToDate")
        .datepicker({
          onSelect : function () {
            var date = jq(this).datepicker("getDate");
            date.setDate(date.getDate() - 1);
            $("#studio_chart_FromDate").datepicker("option", "maxDate", date || null);
            changePeriod();
          }
        })
        .datepicker("setDate", defaultToDate)
        .datepicker("option", "minDate", minDate)
        .datepicker("option", "maxDate", defaultToDate);

      $('#chartDownloadStatistics').click(function() {
        return false;
      });

      $('#visitorsFilter')
        .css('visibility', 'visible')
        .click(function(evt) {
          var $target = $(evt.target);
          if ($target.is('li.filter') && !$target.is('li.filter.selected')) {
            showChart($target.attr('id'));
          }
        });

      showChart('filterbyweek');
    });
})(jQuery);
