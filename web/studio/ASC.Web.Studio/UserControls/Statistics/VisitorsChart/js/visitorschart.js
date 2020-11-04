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


(function ($) {
    var colors = {
        hits: '#B6C9D9',
        border: '#D1D1D1'
    },
    displayDates = {};

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

    function changePeriod() {
        var from = jq("#studio_chart_FromDate").datepicker("getDate"),
            to = jq("#studio_chart_ToDate").datepicker("getDate");

        if (from instanceof Date && isFinite(from.getTime()) && to instanceof Date && isFinite(to.getTime()) && from.getTime() < to.getTime()) {
            $('#visitorsChartCanvas').addClass('loader32').empty();
            $('#chartLegend').children('li.label:not(.default)').remove();

            Teamlab.getVisitStatistics({}, from, to, { success: showChart, error: showError });
        } else {
            $('#visitorsChartCanvas').empty();
            $('#chartLegend').children('li.label:not(.default)').remove();
        }
    }

    function changeFilter(param) {
        var from = new Date(new Date().setHours(0, 0, 0, 0)),
            to = new Date(new Date().setHours(0, 0, 0, 0));

        switch (param) {
        case 'filterByWeek':
            $('#visitorsFilter').find('li.filter').removeClass('selected');
            $('#filterByWeek').addClass('selected');
            managePeriodFilter(false);
            from.setDate(to.getDate() - 6);
            break;
        case 'filterByMonth':
            $('#visitorsFilter').find('li.filter').removeClass('selected');
            $('#filterByMonth').addClass('selected');
            managePeriodFilter(false);
            from.setMonth(to.getMonth() - 1);
            break;
        case 'filterBy3Months':
            $('#visitorsFilter').find('li.filter').removeClass('selected');
            $('#filterBy3Months').addClass('selected');
            managePeriodFilter(false);
            from.setMonth(to.getMonth() - 3);
            break;
        case 'filterByPeriod':
            $('#visitorsFilter').find('li.filter').removeClass('selected');
            $('#filterByPeriod').addClass('selected');
            managePeriodFilter(true);

            from = jq("#studio_chart_FromDate").datepicker("getDate");
            to = jq("#studio_chart_ToDate").datepicker("getDate");

            break;
        default:
            return;
        }

        if (from instanceof Date && isFinite(from.getTime()) && to instanceof Date && isFinite(to.getTime()) && from.getTime() < to.getTime()) {
            $('#visitorsChartCanvas').addClass('loader32').empty();
            $('#chartLegend').children('li.label:not(.default)').remove();

            Teamlab.getVisitStatistics({}, from, to, { success: showChart, error: showError });
        }
    }

    function showChart(p, param) {
        if (!param) return;

        $('#visitorsChartCanvas').removeClass('loader32');

        var hits = [],
            hosts = [];

        for (var i = 0, n = param.length; i < n; i++) {
            var date = Teamlab.serializeDate(param[i].date);
            date = new Date(date.valueOf() - 60000 * date.getTimezoneOffset());
            displayDates[date.getTime()] = param[i].displayDate;
            hits.push([date, param[i].hits]);
            hosts.push([param[i].Date, param[i].Hosts]);
        }

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
            ],
            {
                grid: {
                    hoverable: true,
                    clickable: true,
                    aboveData: false,
                    borderColor: colors.border,
                    borderWidth: 1
                },
                legend: { show: false },
                series: { lines: { show: true }, points: { show: true, radius: 2 } },
                xaxis: {
                    mode: 'time',
                    timeformat: ASC.Resources.chartDateFormat,
                    monthNames: ASC.Resources.chartMonthNames.split(/\s*,\s*/)
                },
                yaxis: {
                    min: 0
                }
            }
        );
    }

    function showError(p, errors) {
        $('#visitorsChartCanvas').removeClass('loader32');

        var err = errors[0];
        if (err != null) {
            toastr.error(err);
        }
    }

    $(document).ready(function () {
        $('#visitorsChartCanvas')
          .bind("plothover", function (evt, pos, item) {
              if (item) {
                  if (!displayDates.hasOwnProperty(item.datapoint[0])) {
                      return;
                  }
                  var content =
                    '<h6 class="label">' + item.series.label + ' : ' + displayDates[item.datapoint[0]] + '</h6>' +
                    '<div class="info">' + item.datapoint[1] + ' ' + window.ASC.Resources.visitsLabel + '</div>';
                  ASC.Common.toolTip.show(content, function () {
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

        $("#studio_chart_FromDate, #studio_chart_ToDate").mask(ASC.Resources.Master.DatePatternJQ);

        var defaultFromDate = new Date(),
            defaultToDate = new Date();

        defaultFromDate.setMonth(defaultFromDate.getMonth() - 6);

        var maxDate = new Date();
        maxDate.setDate(maxDate.getDate() - 1);

        var minDate = new Date();
        minDate.setMonth(minDate.getMonth() - 6);
        minDate.setDate(minDate.getDate() + 1);

        $("#studio_chart_FromDate")
          .datepicker({
              onSelect: function (strDate) {
                  var date = jq(this).datepicker("getDate");

                  if (!date) {
                      jq(this).datepicker("setDate", strDate || "");
                      return;
                  }

                  date.setDate(date.getDate() + 1);
                  $("#studio_chart_ToDate").datepicker("option", "minDate", date);
                  changePeriod();
              }
          })
          .datepicker("setDate", defaultFromDate)
          .datepicker("option", "maxDate", maxDate);

        $("#studio_chart_ToDate")
          .datepicker({
              onSelect: function (strDate) {
                  var date = jq(this).datepicker("getDate");

                  if (!date) {
                      jq(this).datepicker("setDate", strDate || "");
                      return;
                  }

                  date.setDate(date.getDate() - 1);
                  $("#studio_chart_FromDate").datepicker("option", "maxDate", date);
                  changePeriod();
              }
          })
          .datepicker("setDate", defaultToDate)
          .datepicker("option", "minDate", minDate)
          .datepicker("option", "maxDate", defaultToDate);

        $('#visitorsFilter').click(function(evt) {
            var $target = $(evt.target);
            if ($target.is('li.filter') && !$target.is('li.filter.selected')) {
                changeFilter($target.attr('id'));
            }
        });

        changeFilter('filterBy3Months');

        $(window).bind("resize resizeWinTimerWithMaxDelay", function () {
            var plot = jq("#visitorsChartCanvas").data("plot");
            if (typeof (plot) !== "undefined") {
                try {
                    var placeholder = plot.getPlaceholder();

                    if (placeholder.width() == 0 || placeholder.height() == 0)
                        return;

                    plot.resize();
                    plot.setupGrid();
                    plot.draw();
                }
                catch (e) { console.log(e); }
            }
        });
    });
})(jQuery);
