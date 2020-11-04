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


ASC.Projects.GantChart = (function (window) {

    'use strict';

    var kInternalVersion = '1.0.1.205',
        kEps = 0.0001,
        kTimeDoubleClicks = 350,    //  ms

        kElementActive = 1,
        kElementCompleted = 2,
        kElementStopped = 3,

        kOpenProject = 0,
        kReadModeProject = 1000,

        kLinkBeginBegin = 0,
        kLinkEndEnd = 1,
        kLinkBeginEnd = 2,

        kTaskSideNone = 0,
        kTaskSideLeft = -10,
        kTaskSideRight = 10,

        kLineCaptureEps = 5,      //  px
        kTaskCaptureEps = 5,      //  px
        kTaskPinCaptureEps = 15,     //  px

        kTaskEndFailSetup = { bigClampPct: 0.75, smallClampPx: 45 },

        kTaskNormalColor = '#A4C7E5',
        kTaskOverdueColor = '#CC3300',
        kTaskCompleteColor = '#BDDA7F',

        kTaskNormalBorderColor = '#82A0B9',
        kTaskOverdueBorderColor = '#872200',
        kTaskCompleteBorderColor = '#9EB56C',

        kTaskWithLinkBorderSettings = { from: '#4980C3', to: '#FFFFFF', dash: [4, 2] },
        kLinkAddModeSettings = { color: '#24547E', dash: [4, 2] },

        kTaskNormalBorderHitColor = '#567da1',
        kTaskOverdueBorderHitColor = '#840701',
        kTaskCompleteBorderHitColor = '#729335',

        kTaskNormalHitColor = '#78A8D6',
        kTaskCompleteHitColor = '#9AC54A',
        kTaskOverdueHitColor = '#B00600',
        kTaskNoEndTimeColor = '#A4C7E5',

        kTaskNormalPriorityArrowColor = '#1d5381',
        kTaskOverduePriorityArrowColor = '#621C05',
        kTaskCompletePriorityArrowColor = '#496410',

        kTaskSelectedBackColor = 'rgba(100,100,100,0.15)',

        kTaskTextColor = '#333333',
        kTaskArrowPriorityBaseColor = '#FFFFFF',

        kMilestoneColor = '#7e7e7e',
        kMilestoneSelectedColor = '#666666',
        kMilestoneCompleteColor = '#bdda7f',
        kMilestoneOverdueColor = '#cc3300',
        kMilestoneTextColor = '#7e7e7e',
        kMilestoneBeforeDragColor = '#4fa7d1',

        kMilestoneColorBackLight = 'rgba(100,100,100,0.1)',

        kTaskDragDropFillColor = '#ffffff',
        kTaskDragDropBorderColor = '#6699ff',
        kTaskDragDropInvalidBorderColor = '#cc3300',
        kTaskTextUnderEditColor = '#999999',

        kLinkNormalColor = '#4980C3',
        kLinkInvalidColor = '#CC3300',
        kLinkSelectedColor = '#6699FF',
        kLinkAddEditColor = '#24547E',

        kDayLineColor = '#97b8fa',
        kDayBandColor = '#EFF5FF',
        kWeekendColor = '#FFF5EB',

        kLinesHorizontalColor = '#CCCCCC',
        kLinesVerticalColor = '#CCCCCC',
        kLinesDateScaleTopColor = '#D9D9D9',
        kDateScaleCurrentDayColor = '#666666',

        kLinkPinColor = '#628BAE',
        kLinkPinSize = 8,
        kLinkSmallPinSize = 2,

        kDefaultFontName = 'arial',

        kUIScrollBarWidth = 8,
        kUIScrollBarIndentWidth = 2,
        kUIScrollBarThumbWidth = 50,
        kUIScrollBarStepMoveY = 40,
        kUIScrollStepInWidthPercent = 0.1,  //  % move with keys left&right

        CollapseMilestones = 10,
        ExpandMilestones = 20,

        kCirclePlus = 0,
        kCircleMinus = 1,
        kCircleComplete = 2,
        kCircleWarning = 3,

        kEditBoxClipboardDebugMode = false,

        kEditBoxSettings = { backgroundColor: '#FAFAFA', dash: [1, 2], dashColor: '#4FA7D1', fontColor: '#333333', fontSelColor: '#FFFFFF', backgroundSelTextColor: '#3399FF', fontHeight: 9, caretColor: '#000000', placeHolderColor: '#999999', marginWidth: 10, minBoxW: 100 },

        kMaxTitleSymbolsCount = 250,    // максимальная длина названия для вех и задав

        kUIScrollBarThumbColor = 'rgba(0, 0, 0, 0.25)',
        kUIScrollBarThumbPressedColor = 'rgba(0, 0, 0, 0.5)',
        kUIScrollBarBackgroundColor = 'rgba(0, 0, 0, 0.1)',
        kUIScrollBarThumbMinWidth = 50,          // px
        kUIScrollBarMinWidthShowControl = 50 + 1,      // px

        kLPCompleteColor = '#83888d',
        kLPOverdueColor = '#cc3300',
        kLPNormalColor = '#333333',
        kLockProjectColor = '#83888d',

        kHandlerAddTask = '101',
        kHandlerAddMilestone = '102',
        kHandlerDeleteTask = '111',
        kHandlerDeleteMilestone = '112',
        kHandlerChangeTitleTask = '121',
        kHandlerChangeTitleMilestone = '122',
        kHandlerChangeTime = '123',
        kHandlerChangeTaskStatus = '124',
        kHandlerChangeMilestoneStatus = '125',
        kHandlerMoveTask = '130',
        kHandlerAddTaskLink = '140',
        kHandlerDeleteTaskLink = '145',
        kHandlerMoveGroupTasks = '150',
        kHandlerChangeResponsible = '160',

        kHandlerUndoRedoUpdateUI = 'UndoRedoUI',

        kHandlerBeforeDeleteTask = '200',
        kHandlerBeforeDeleteMilestone = '220',
        kHandlerBeforeChangeTaskStatus = '240',
        kHandlerBeforeChangeMilestoneStatus = '260',
        kHandlerBeforeAddTaskLink = '270',
        kHandlerBeforeDeleteTaskLink = '280',
        kHandlerBeforeMoveTaskWithLinks = '300',
        kHandlerBeforeMenuAddTaskLink = '350',
        kHandlerBeforeChangeResponsibles = '360',
        kHanderShowTaskPopUpWindow = '500',
        kHanderShowTaskPopUpCustomWindow = '501',
        kHanderShowEditPopUpMenuWindow = '502',
        kHanderShowRespPopUpMenuWindow = '503',
        kHanderShowEditElemPopUpMenuWindow = '504',
        kHanderChangeTaskProperties = '970',
        kHanderChangeMilestoneProperties = '980',

        kOperationChangeTimeTask = 100,
        kOperationChangeTimeMilestone = 200,
        kOperationAddTask = 300,
        kOperationAddMilestone = 400,
        kOperationDeleteTask = 500,
        kOperationDeleteMilestone = 600,
        kOperationChangeTitleTask = 700,
        kOperationChangeTitleMilestone = 800,
        kOperationMoveTask = 900,
        kOperationMoveGroupTasks = 905,
        kOperationChangeTaskStatus = 910,
        kOperationChangeMilestoneStatus = 920,
        kOperationAddTaskLink = 950,
        kOperationDeleteTaskLink = 955,
        kOperationChangeResponsible = 960,
        kOperationChangeTaskProperties = 970,
        kOperationChangeMilestoneProperties = 980,
        kOperationDummy = 999,

        kZoomBarCurDayLineColor = '#6699FF',
        kZoomBarThumbColor = '#E2E2E2',
        kZoomBarThumbHandlesColor = '#CCCCCC',
        kZoomBarThumbMinWidth = 20,  // px

        kScaleUnitMinSize = 28,  // kScaleUnitNormalizeDays

        kScaleUnitOneHour = 1,
        kScaleUnitTwoHours = 1.5,
        kScaleUnitFourHours = 3.0,
        kScaleUnitEightHours = 4,
        kScaleUnitTwentyHours = 8,
        kScaleUnitDay = 12,
        kScaleUnitNormalizeDays = 28,
        kScaleUnitTwoDays = 40,
        kScaleUnitThreeDays = 70,
        kScaleUnitFourDays = 100,
        kScaleUnitFiveDays = 130,
        kScaleUnitSevenDays = 170,
        kScaleUnitNineDays = 210,
        kScaleUnitTwelveDays = 250,
        kScaleUnitFifteenDays = 270,
        kScaleUnitOneMonth = 500,
        kScaleUnitMinMonth = 340,

        kTypeScaleHours = 0,  //  default
        kTypeScaleDays = 1,
        kTypeScaleWeek = 2,
        kTypeScaleMonth = 3,
        kTypeScaleYears = 4,
        kTypeScaleMinutes = 5,
        kTypeScaleSeconds = 6,

        kDateScaleTextColor = '#999999',
        kDateScaleDayOffTextColor = '#FFC78F',
        kDateScaleBackgroundColor = '#ffffff',

        kUIDateScaleUpMargin = 8,          //   px

        kMilestoneMinDurationInHours = 168,        //  в часах 7 дней

        kEnableQueryTaskWithLinksMove = true,       //  двигаем связные задачи со связью End-Begin

        kAnimationScaleFactor = 0.15,
        kAnimationScrollFactor = 0.2,        //  0.2;    // 0.0002;// test value

        kTaskDateCaption = { width: 63, height: 15, offX: 13, bkcolor: '#FFFFFF', color: '#999999' },
        kHitLightLink = true,       //  подсветка связи для виджета

        kMenuSettings = { backgroundColor: '#888d92', borderColor: '#fafafa', icoSize: 16, elementsWidth2X: 36, elementsWidth3X: 56, elementsWidth4X: 76, elementsWidth5X: 96, elementsWidth6X: 116, borderSz: 7 },
        kLinksWidgetSettings = { w: 36, h: 24, trx: 11, titleFx: 18, titleFy: 10 },
        kLinksWidgetIcoSettings = { s: 16, xw: 14, yh: 4 },

        kMaxShowTextStrings = 10,

        kHitSidePixels = 5,

        kEditModeNoUse = 100,        //  обычный режим в котором overlay используется только для popup элементов
        kEditModeElementDrag = 200,        //  режим переноса задачи
        kEditModeAddLink = 300,        //  режим добавления связей
        kEditElementTitle = 400,        //  режим редактирование названия элемента (задача, веха)

        kEndBeginLinksEditMode = true,       //  можем только работать со связами End-Begin

        kTimeLineItemHeight = 18,
        kTimeLineItemMargin = 30,
        kTimeLineItemFontHeight = 8,

        kLinkCircleZone = 12,         //  px

        kDetailsWidgetSettings = { backgroundColor: '#ffffff', titleColor: '#999999', descriptionColor: '#666666', width: 170, maxWidth: 300 },
        lockImg = new Image();

    function findCustomStatus(fn) {
        return ASC.Projects.Master.customStatuses.find(fn);
    }

    function deepCopy(obj) {
        if (!obj) { return obj; } // null, undefined values check

        var types = [Number, String, Boolean],
            result;

        // normalizing primitives if someone did new String('aaa'), or new Number('444');
        types.forEach(function (type) {
            if (obj instanceof type) {
                result = type(obj);
            }
        });

        if (typeof result == 'undefined') {
            if (Object.prototype.toString.call(obj) === '[object Array]') {
                result = [];
                obj.forEach(function (child, index, array) {
                    result[index] = deepCopy(child);
                });
            } else if (typeof obj == 'object') {
                // testing that this is DOM
                if (obj.nodeType && typeof obj.cloneNode == 'function') {
                    result = obj.cloneNode(true);
                } else if (!obj.prototype) { // check that this is a literal
                    if (obj instanceof Date) {
                        result = new Date(obj);
                    } else {
                        // it is an object literal
                        result = {};
                        for (var i in obj) {
                            result[i] = deepCopy(obj[i]);
                        }
                    }
                } else {

                    // depending what you would like here,
                    // just keep the reference, or create new object

                    // NOTE: закоментил, потому что ругается минимизатор

                    //                    if (false && obj.constructor) {
                    //                        // would not advice to do that, reason? Read below
                    //                        result = new obj.constructor();
                    //                    } else {
                    //                        result = obj;
                    //                    }

                    result = obj;
                }
            } else {
                result = obj;
            }
        }

        return result;
    }
    function stopSystemEvent(e) {
        if (e.preventDefault) {
            e.preventDefault()          // Chrome
        } else if (e.returnValue) {
            e.returnValue = false;      // IE9+
        }

        if (e.stopPropagation) {
            e.stopPropagation();        // Firefox
        }
    }
    function targetSystemEvent(e) {
        var targ = null;

        if (!e) {
            e = window.event;
        }

        if (e.target) {
            targ = e.target;
        } else if (e.srcElement) {
            targ = e.srcElement;
        }

        // defeat Safari bug

        if (targ.nodeType == 3) {
            targ = targ.parentNode;
        }

        return targ;
    }
    function floor2(N) { return N >> 0; }
    function abs2(N) { return N < 0 ? ~N++ : N; }
    function pointInRectangle(x, y, x1, y1, x2, y2, eps) { return (x1 - eps <= x && y1 - eps <= y && x2 + eps >= x && y2 + eps >= y); }
    function ellipsis(value, len, word) {
        if (value && value.length > len) {
            if (word) {
                var vs = value.substr(0, len - 2),
                    index = Math.max(vs.lastIndexOf(' '), vs.lastIndexOf('.'), vs.lastIndexOf('!'), vs.lastIndexOf('?'));
                if (index !== -1 && index >= (len - 15)) {
                    return vs.substr(0, index) + "...";
                }
            }
            return value.substr(0, len - 3) + "...";
        }
        return value;
    }

    if (!Array.prototype.last) { Array.prototype.last = function () { return this[this.length - 1]; } }
    if (!Date.prototype.dateFormat) {
        Date.prototype.dateFormat = function () {
            var day = this.getUTCDate();
            var month = this.getMonth() + 1;
            var year = this.getFullYear().toString();

            if (month < 10) month = '0' + month.toString();
            if (day < 10) day = '0' + day.toString();

            year = year.slice(year.length - 2, year.length);

            return day + '.' + month + '.' + year;
        }
    }
    if (!Date.prototype.dateFormatFullYear) {
        Date.prototype.dateFormatFullYear = function () {

            var day = this.getUTCDate();
            var month = this.getMonth() + 1;
            var year = this.getFullYear().toString();

            if (month < 10) month = '0' + month.toString();
            if (day < 10) day = '0' + day.toString();

            return day + '.' + month + '.' + year;
        }
    }

    function TreeNode() {
        this.root = null;
        this.nodes = [];
        this.data = null;
    }
    TreeNode.prototype = {
        createNode: function () {
            var node = new TreeNode();

            this.nodes.push(node);

            return node;
        },
        walkElements: function (parent, func/*, data*/) {

            func(parent, this.root, this.data);

            parent = this.root;

            for (var i = 0; i < this.nodes.length; ++i) {
                this.nodes[i].walkElements(parent, func, this.nodes[i].data);
            }
        }
    };

    function UserDefaults(delegate) {
        this.delegate = delegate;
    }
    UserDefaults.prototype = {
        setFontFamily: function (name) {
            if (name) {
                kDefaultFontName = name;

                this.delegate.updateDefaults();
            }
        }
    };

    function Painter(ctx, overlay) {
        this.ctx = ctx;
        this.overlay = overlay;
        this.zones = [];
    }
    Painter.prototype = {
        drawDashedLineH: function (ctx, fromX, fromY, toX, toY, dashPattern) {
            ctx.beginPath();

            var dx = toX - fromX;
            var dy = 0;//toY - fromY;
            var angle = 0;//Math.atan2(dy, dx);
            var x = fromX;
            var y = fromY;
            ctx.moveTo(fromX, fromY);
            var idx = 0;
            var draw = true;
            while (!((dx < 0 ? x <= toX : x >= toX) && (dy < 0 ? y <= toY : y >= toY))) {
                var dashLength = dashPattern[idx++ % dashPattern.length];
                //var nx = x + (Math.cos(angle) * dashLength);
                var nx = x + dashLength;
                x = dx < 0 ? Math.max(toX, nx) : Math.min(toX, nx);
                var ny = y;
                //var ny = y + (Math.sin(angle) * dashLength);
                y = dy < 0 ? Math.max(toY, ny) : Math.min(toY, ny);
                if (draw) {
                    ctx.lineTo(x, y);
                } else {
                    ctx.moveTo(x, y);
                }
                draw = !draw;
            }

            ctx.closePath();
            ctx.stroke();
        },
        drawDashedLineV: function (ctx, fromX, fromY, toX, toY, dashPattern) {
            ctx.beginPath();

            var dx = 0;
            var dy = toY - fromY;
            var x = fromX;
            var y = fromY;
            ctx.moveTo(fromX, fromY);
            var idx = 0;
            var draw = true;
            while (!((dx < 0 ? x <= toX : x >= toX) && (dy < 0 ? y <= toY : y >= toY))) {
                var dashLength = dashPattern[idx++ % dashPattern.length];
                var nx = x;
                x = dx < 0 ? Math.max(toX, nx) : Math.min(toX, nx);
                var ny = y + dashLength;
                y = dy < 0 ? Math.max(toY, ny) : Math.min(toY, ny);
                if (draw) {
                    ctx.lineTo(x, y);
                } else {
                    ctx.moveTo(x, y);
                }
                draw = !draw;
            }

            ctx.closePath();
            ctx.stroke();
        },
        drawLineH: function (x, y, dx, dy, color, width) {
            this.ctx.strokeStyle = color;
            this.ctx.lineWidth = width;

            this.ctx.beginPath();
            this.ctx.moveTo(x, y);
            this.ctx.lineTo(dx, dy);
            this.ctx.closePath();
            this.ctx.stroke();
        },

        lineTo: function (ctx, x, y, toX, toY, dashPattern) {
            if (x === toX && y !== toY) {

                this.drawDashedLineV(ctx, x, Math.min(y, toY), toX, Math.max(y, toY), dashPattern); // NOTE! none optimize

                //this.drawDashedLineV(ctx, x, y, toX, toY, dashPattern);
            }

            if (x !== toX && y === toY) {

                this.drawDashedLineH(ctx, Math.min(x, toX), y, Math.max(x, toX), toY, dashPattern); // NOTE! none optimize

                //this.drawDashedLineH(ctx, x, y, toX, toY, dashPattern);
            }
        },

        addZone: function (x, y, width, height, margin) {
            this.zones.push({ x: x, y: y, width: width, height: height, margin: margin })
        },
        addBoundZone: function (bound, margin) {
            this.zones.push({ x: bound.x, y: bound.y, width: bound.w, height: bound.h, margin: margin })
        },
        clearZones: function (clear) {
            if (clear) {
                for (var i = this.zones.length - 1; i >= 0; --i) {
                    this.overlay.
                        clearRect(this.zones[i].x - this.zones[i].margin,
                            this.zones[i].y - this.zones[i].margin,
                            this.zones[i].width + this.zones[i].margin * 2,
                            this.zones[i].height + this.zones[i].margin * 2);
                }
            }

            this.zones = [];
        },

        drawStrokeRectangle: function (ctx, x, y, w, h, color) {
            ctx.strokeStyle = color;
            ctx.lineWidth = 1;

            ctx.beginPath();

            ctx.moveTo(x, y);
            ctx.lineTo(x + w, y);

            ctx.moveTo(x + w, y - 0.5);
            ctx.lineTo(x + w, y + h);
            ctx.lineTo(x, y + h);
            ctx.lineTo(x, y);

            ctx.stroke();
        },

        drawPencil: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 20 / 120 * sw, y + 100 / 120 * sh);
            this.ctx.lineTo(x + 20 / 120 * sw, y + 80 / 120 * sh);
            this.ctx.lineTo(x + 79 / 120 * sw, y + 20 / 120 * sh);
            this.ctx.lineTo(x + 99 / 120 * sw, y + 40 / 120 * sh);
            this.ctx.lineTo(x + 40 / 120 * sw, y + 100 / 120 * sh);
        },
        drawPlay: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 0.30 * sw, y + 0.17 * sh);
            this.ctx.lineTo(x + 0.71 * sw, y + 0.49 * sh);
            this.ctx.lineTo(x + 0.30 * sw, y + 0.82 * sh);
        },
        drawDelete: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 0.16 * sw, y + 0.30 * sh);
            this.ctx.lineTo(x + 0.30 * sw, y + 0.15 * sh);
            this.ctx.lineTo(x + 0.50 * sw, y + 0.35 * sh);

            this.ctx.lineTo(x + 0.70 * sw, y + 0.15 * sh);
            this.ctx.lineTo(x + 0.85 * sw, y + 0.30 * sh);
            this.ctx.lineTo(x + 0.65 * sw, y + 0.49 * sh);

            this.ctx.lineTo(x + 0.85 * sw, y + 0.68 * sh);
            this.ctx.lineTo(x + 0.70 * sw, y + 0.82 * sh);
            this.ctx.lineTo(x + 0.50 * sw, y + 0.63 * sh);

            this.ctx.lineTo(x + 0.30 * sw, y + 0.82 * sh);
            this.ctx.lineTo(x + 0.16 * sw, y + 0.68 * sh);
            this.ctx.lineTo(x + 0.36 * sw, y + 0.49 * sh);
        },
        drawFitSw: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 20 / 120 * sw, y + 20 / 120 * sh);
            this.ctx.lineTo(x + 59 / 120 * sw, y + 59 / 120 * sh);
            this.ctx.lineTo(x + 20 / 120 * sw, y + 99 / 120 * sh);
            this.ctx.lineTo(x + 20 / 120 * sw, y + 20 / 120 * sh);

            this.ctx.lineTo(x + 61 / 120 * sw, y + 59 / 120 * sh);
            this.ctx.lineTo(x + 99 / 120 * sw, y + 20 / 120 * sh);
            this.ctx.lineTo(x + 99 / 120 * sw, y + 99 / 120 * sh);
            this.ctx.lineTo(x + 61 / 120 * sw, y + 59 / 120 * sh);
        },
        drawAdd: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 20 / 120 * sw, y + 51 / 120 * sh);
            this.ctx.lineTo(x + 50 / 120 * sw, y + 51 / 120 * sh);
            this.ctx.lineTo(x + 50 / 120 * sw, y + 21 / 120 * sh);

            this.ctx.lineTo(x + 68 / 120 * sw, y + 21 / 120 * sh);
            this.ctx.lineTo(x + 68 / 120 * sw, y + 51 / 120 * sh);
            this.ctx.lineTo(x + 98 / 120 * sw, y + 51 / 120 * sh);

            this.ctx.lineTo(x + 98 / 120 * sw, y + 69 / 120 * sh);
            this.ctx.lineTo(x + 68 / 120 * sw, y + 69 / 120 * sh);
            this.ctx.lineTo(x + 68 / 120 * sw, y + 99 / 120 * sh);
            this.ctx.lineTo(x + 50 / 120 * sw, y + 99 / 120 * sh);

            this.ctx.lineTo(x + 50 / 120 * sw, y + 69 / 120 * sh);
            this.ctx.lineTo(x + 20 / 120 * sw, y + 69 / 120 * sh);
        },
        drawComplete: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 16 / 120 * sw, y + 54 / 120 * sh);
            this.ctx.lineTo(x + 29 / 120 * sw, y + 40 / 120 * sh);
            this.ctx.lineTo(x + 54 / 120 * sw, y + 65 / 120 * sh);

            this.ctx.lineTo(x + 91 / 120 * sw, y + 28 / 120 * sh);
            this.ctx.lineTo(x + 105 / 120 * sw, y + 42 / 120 * sh);

            this.ctx.lineTo(x + 54 / 120 * sw, y + 93 / 120 * sh);
        },
        drawIconLink: function (x, y, w, h) {
            this.ctx.fillRect(floor2(x + w * 2 / 16), floor2(y + h * 2 / 16), floor2(w * 6 / 16), floor2(h * 4 / 16));
            this.ctx.fillRect(floor2(x + w * 8 / 16), floor2(y + h * 10 / 16), floor2(w * 6 / 16), floor2(h * 4 / 16));

            this.ctx.lineWidth = 1;
            this.ctx.beginPath();

            this.ctx.moveTo(floor2(x + w * 4 / 16) + 0.5, floor2(y + h * 3 / 16) + 0.5);
            this.ctx.lineTo(floor2(x + w * 11 / 16) + 0.5, floor2(y + h * 3 / 16) + 0.5);       // fix +1px right

            this.ctx.moveTo(floor2(x + w * 11 / 16) + 0.5, floor2(y + h * 3 / 16));
            this.ctx.lineTo(floor2(x + w * 11 / 16) + 0.5, floor2(y + h * 7 / 16) + 0.5);
            this.ctx.lineTo(floor2(x + w * 11 / 16) + 0.5, floor2(y + h * 7 / 16) + 0.5);
            this.ctx.lineTo(floor2(x + w * 4 / 16) + 0.5, floor2(y + h * 7 / 16) + 0.5);
            this.ctx.lineTo(floor2(x + w * 4 / 16) + 0.5, floor2(y + h * 11 / 16) + 0.5);
            this.ctx.lineTo(floor2(x + w * 9 / 16) + 0.5, floor2(y + h * 11 / 16) + 0.5);

            this.ctx.stroke();
        },
        drawIcoResponsible: function (x, y, sw, sh) {
            this.ctx.moveTo(x + 0.19 * sw, y + 0.82 * sh);
            this.ctx.lineTo(x + 0.19 * sw, y + 0.70 * sh);

            this.ctx.lineTo(x + 0.40 * sw, y + 0.60 * sh);
            this.ctx.lineTo(x + 0.40 * sw, y + 0.53 * sh);

            this.ctx.lineTo(x + 0.59 * sw, y + 0.53 * sh);
            this.ctx.lineTo(x + 0.59 * sw, y + 0.60 * sh);

            this.ctx.lineTo(x + 0.80 * sw, y + 0.70 * sh);
            this.ctx.lineTo(x + 0.80 * sw, y + 0.82 * sh);

            this.ctx.fill();
            this.ctx.closePath();

            var centerX = x + 0.5 * sw;
            var centerY = y + 0.36 * sh;
            var width = 0.42 * sw;
            var height = 0.37 * sh;

            this.ctx.beginPath();
            this.ctx.moveTo(centerX, centerY - height / 2); // A1

            this.ctx.bezierCurveTo(
                    centerX + width / 2, centerY - height / 2, // C1
                    centerX + width / 2, centerY + height / 2, // C2
                centerX, centerY + height / 2); // A2

            this.ctx.bezierCurveTo(
                    centerX - width / 2, centerY + height / 2, // C3
                    centerX - width / 2, centerY - height / 2, // C4
                centerX, centerY - height / 2); // A1


            this.ctx.fill();
            this.ctx.closePath();
        },

        // arrow

        drawArrowRight: function (x, y, sw, sh) {

            this.ctx.beginPath();

            this.ctx.moveTo(x + 0.40 * sw, y + 0.47 * sh);
            this.ctx.lineTo(x + 0.85 * sw, y + 0.18 * sh);
            this.ctx.lineTo(x + 0.85 * sw, y + 0.77 * sh);

            this.ctx.closePath();
            this.ctx.fill();
        },
        drawArrowLeft: function (x, y, sw, sh) {

            this.ctx.beginPath();

            this.ctx.moveTo(x + 0.85 * sw, y + 0.47 * sh);
            this.ctx.lineTo(x + 0.40 * sw, y + 0.18 * sh);
            this.ctx.lineTo(x + 0.40 * sw, y + 0.77 * sh);

            this.ctx.closePath();
            this.ctx.fill();
        },
        drawArrowBottom: function (x, y, sw, sh) {

            this.ctx.beginPath();

            this.ctx.moveTo(x + 0.77 * sw, y + 0.18 * sh);
            this.ctx.lineTo(x + 0.47 * sw, y + 0.69 * sh);
            this.ctx.lineTo(x + 0.18 * sw, y + 0.18 * sh);

            this.ctx.closePath();
            this.ctx.fill();
        },
        // icons

        drawIcoKey: function (ctx, x, y, zoom) {
            ctx.fillRect(x - 8 * zoom, y - 14 * zoom, 2 * zoom, 8 * zoom);
            ctx.fillRect(x - 8 * zoom, y - 14 * zoom, 7 * zoom, 2 * zoom);

            ctx.fillRect(x - 3 * zoom, y - 14 * zoom, 2 * zoom, 8 * zoom);
            ctx.fillRect(x - 8 * zoom, y - 8 * zoom, 7 * zoom, 2 * zoom);

            ctx.fillRect(x - 17 * zoom, y - 14 * zoom, 3 * zoom, 5 * zoom);
            ctx.fillRect(x - 17 * zoom, y - 11 * zoom, 10 * zoom, 2 * zoom);

            ctx.fillRect(x - 13 * zoom, y - 14 * zoom, 2 * zoom, 5 * zoom);
        },
        drawIcoArrowUp: function (ctx, x, y, w, color) {
            ctx.fillStyle = color;
            ctx.beginPath();

            ctx.moveTo(x + w * 6 / 18, y + w * 10 / 18);
            ctx.lineTo(x + w * 6 / 18, y + w * 14 / 18);
            ctx.lineTo(x + w * 10 / 18, y + w * 14 / 18);
            ctx.lineTo(x + w * 10 / 18, y + w * 10 / 18);

            ctx.lineTo(x + w * 13 / 18, y + w * 10 / 18);
            ctx.lineTo(x + w * 8 / 18, y + w * 3.5 / 18);
            ctx.lineTo(x + w * 3 / 18, y + w * 10 / 18);

            ctx.closePath();
            ctx.fill();
        },
        drawIcoCross: function (ctx, x, y, sw, sh) {
            ctx.moveTo(x + 0.16 * sw, y + 0.30 * sh);

            ctx.lineTo(x + 0.30 * sw, y + 0.15 * sh);
            ctx.lineTo(x + 0.50 * sw, y + 0.35 * sh);

            ctx.lineTo(x + 0.70 * sw, y + 0.15 * sh);
            ctx.lineTo(x + 0.85 * sw, y + 0.30 * sh);
            ctx.lineTo(x + 0.65 * sw, y + 0.49 * sh);

            ctx.lineTo(x + 0.85 * sw, y + 0.68 * sh);
            ctx.lineTo(x + 0.70 * sw, y + 0.82 * sh);
            ctx.lineTo(x + 0.50 * sw, y + 0.63 * sh);

            ctx.lineTo(x + 0.30 * sw, y + 0.82 * sh);
            ctx.lineTo(x + 0.16 * sw, y + 0.68 * sh);
            ctx.lineTo(x + 0.36 * sw, y + 0.49 * sh);
        },

        roundRect: function (ctx, x, y, w, h, r) {

            //              в IE10+ не работает

            //            if (w < 2 * r) r = w / 2;
            //            if (h < 2 * r) r = h / 2;
            //
            //            ctx.beginPath();
            //            ctx.moveTo(x+r, y);
            //            ctx.arcTo(x+w, y,   x+w, y+h, r);
            //            ctx.arcTo(x+w, y+h, x,   y+h, r);
            //            ctx.arcTo(x,   y+h, x,   y,   r);
            //            ctx.arcTo(x,   y,   x+w, y,   r);
            //            ctx.closePath();

            ctx.beginPath();
            ctx.moveTo(x + r, y);
            ctx.lineTo(x + w - r, y);
            ctx.quadraticCurveTo(x + w, y, x + w, y + r);
            ctx.lineTo(x + w, y + h - r);
            ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
            ctx.lineTo(x + r, y + h);
            ctx.quadraticCurveTo(x, y + h, x, y + h - r);
            ctx.lineTo(x, y + r);
            ctx.quadraticCurveTo(x, y, x + r, y);
            ctx.closePath();
        }
    };

    function TextMeasurer() {
        var canvas = null;

        var readStyleValue = function (css, name) {
            var dummy = document.createElement('div');
            dummy.className = css;
            document.body.appendChild(dummy);

            var prop = window.getComputedStyle(dummy, null).getPropertyValue(name);
            document.body.removeChild(dummy);

            return prop;
        };
        var width = function (str, css) {
            if (null == canvas) {
                canvas = document.createElement('canvas');

                canvas.width = 450;
                canvas.height = 100;

                document.body.appendChild(canvas);
            }

            var name = readStyleValue(css, 'font-family');
            var weight = readStyleValue(css, 'font-weight');
            var size = readStyleValue(css, 'font-size');

            canvas.getContext('2d').font = weight + ' ' + size + ' ' + name;

            return canvas.getContext('2d').measureText(str + 'W').width;
        };
        var clear = function () {
            if (canvas) {
                document.body.removeChild(canvas);
                canvas = null;
            }
        };

        return {
            width: width,
            clear: clear,

            // help

            readStyleValue: readStyleValue
        };
    }

    function Clipboard() {
        this.textToCopy = '';
        this.execElement = this.clipboardElement('');
    }
    Clipboard.prototype = {
        copyText: function (s) {
            this.textToCopy = s;

            if (this.textToCopy.length) {
                this.copyToClipboard();
            }
        },
        oncopy: function (e) {
            if (e.clipboardData) {
                e.preventDefault();
                e.clipboardData.setData('text/plain', this.textToCopy);
            }
        },
        clear: function () {
            this.textToCopy = '';

            if (this.execElement) {
                document.body.removeChild(this.execElement);
            }
        },

        clipboardElement: function () {

            var element = document.createElement('div');
            if (element) {
                element.setAttribute('contentEditable', true);

                element.style.position = 'absolute';

                if (kEditBoxClipboardDebugMode) {
                    element.style.backgroundColor = 'white';
                    element.style.left = '100px';
                    element.style.top = '100px';
                    element.style.width = '1000px';
                    element.style.height = '100px';
                    element.style.overflow = 'hidden';
                    element.style.zIndex = 1000;
                } else {

                    element.style.left = '-10000px';
                    element.style.top = '-10000px';
                    element.style.width = '1000px';
                    element.style.height = '100px';
                    element.style.overflow = 'hidden';
                    element.style.zIndex = -1000;
                }

                element.setAttribute('contentEditable', true);
                document.body.appendChild(element);
            }

            return element;
        },
        selectContent: function (element) {
            // first create a range
            var rangeToSelect = document.createRange();
            rangeToSelect.selectNodeContents(element);

            // select the contents
            var selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(rangeToSelect);
        },
        copyToClipboard: function () {

            if (window['clipboardData']) { // Internet Explorer
                window['clipboardData'].setData('Text', this.textToCopy);
            } else {

                // create a temporary element for the execCommand method

                if (!this.execElement) {
                    this.execElement = this.clipboardElement(this.textToCopy);
                }

                this.execElement.textContent = this.textToCopy;

                this.selectContent(this.execElement);

                document.execCommand('copy', false, null);

                // remove the temporary element
                if (window.netscape) {
                    document.body.removeChild(this.execElement);
                }
            }
        }
    };

    function EditBox(parent) {
        this.init(parent);
    }
    EditBox.prototype = {

        init: function (parent) {
            this.parent = parent;

            this.ctx = this.parent.overlayctx;
            this.font = kEditBoxSettings.fontHeight + 'pt ' + kDefaultFontName;

            //

            this.text = '';
            this.select = { from: 0, to: 0 };
            this._clipboard = new Clipboard();
            this.placeHolder = '';

            this.bound = { x: 0, y: 0, w: 0, h: 0 };
            this.screenBound = { x: 0, y: 0, w: 0, h: 0 };
            this.screenAddX = 0;
            this.clampBound = { left: 0, right: 0 };

            // позиция каретки, автоматически устанавливается на конец текста

            this.carriage = 0;
            this.carriageFrame = 0;
            this.carriageWidth = 0;
            this.clickCarriage = 0;

            // смещение при отрисовке текста, что бы была видна корректка

            this.carriageLeft = 0;
            this.carriageLeftBuild = true;

            this.doubleMouseClickTime = null;

            this.mouseSelectDirection = 0;
            this.mouseSelectStartPos = 0;

            // need refactoring

            this.p = -1;
            this.m = -1;
            this.t = -1;

            this.enable = false;
            this.status = false;

            this.boundUpdate = false;

            this.saveText = '';
            this.taskRef = null;
            this.milestoneRef = null;
            this.milestoneRefForUpdate = null;
            this.checkUp = false;
            this.createMode = undefined;

            this.selectMode = false;

            // TODO

            this.undoStrings = [];

        },

        updateDefaults: function () {
            this.font = kEditBoxSettings.fontHeight + 'pt ' + kDefaultFontName;
        },

        setEnable: function () {
            this.enable = true;

            this.carriageFrame = 0;
            this.carriageWidth = 0;
            this.carriage = this.text.length;
            this.carriageLeft = 0;
            this.carriageLeftBuild = true;
            this.mouseSelectDirection = 0;
            this.mouseSelectStartPos = 0;

            this.screenAddX = 0;
        },
        setFont: function (work) {
            if (work) {
                this.parent.overlayctx.font = this.font;
            } else {
                this.parent.overlayctx.font = this.parent.titlesFont;
            }
        },
        setCarriage: function (ind) {

            this.carriageFrame = 41;

            this.carriage = ind;

            this.carriage = Math.max(0, this.carriage);
            this.carriage = Math.min(this.text.length, this.carriage);

            this.select = { from: this.carriage, to: this.carriage };

            this.carriageLeftBuild = true;
        },

        isEnable: function () {
            return this.enable;
        },

        draw: function () {
            if (this.enable) {

                if (this.screenBound.w <= 0 || this.screenBound.h <= 0) { return; }
                if (this.clampBound.left > this.clampBound.right) { return; }

                this.drawBackground();

                this.setFont(true);

                var space = this.calculateSelectSpace(this.ctx, this.text, this.select.from, this.select.to);

                this.ctx.save();
                this.ctx.beginPath();
                this.ctx.rect(Math.floor(this.clampBound.left), Math.floor(this.bound.y - 2), (this.clampBound.right - this.clampBound.left) - this.screenAddX + 2, this.bound.h + 4);
                this.ctx.clip();

                if (0 === this.text.length) {
                    this.ctx.fillStyle = kEditBoxSettings.placeHolderColor;
                    this.ctx.fillText(this.placeHolder, this.bound.x - this.carriageLeft, this.bound.y + this.bound.h * 0.75);
                } else {
                    this.ctx.fillStyle = kEditBoxSettings.fontColor;
                    this.ctx.fillText(this.text, this.bound.x - this.carriageLeft, this.bound.y + this.bound.h * 0.75);
                }

                if (!space) { this.drawCarriage(); }

                this.ctx.restore();

                this.drawSelectedText(space);

                this.setFont(false);
            }
        },

        drawBackground: function () {

            var ctx = this.ctx;

            // белый фон заднего фона

            ctx.fillStyle = kEditBoxSettings.backgroundColor;
            ctx.fillRect(this.clampBound.left, this.bound.y, this.clampBound.right - this.clampBound.left + 2, this.bound.h);

            // пунктирная обводка вокруг текстового поля

            ctx.fillStyle = kEditBoxSettings.dashColor;
            ctx.lineWidth = 1;

            this.parent.painter.lineTo(ctx,
                    Math.floor(this.clampBound.left) + 0.5,
                    Math.floor(this.bound.y) + 0.5,
                    Math.floor(this.clampBound.right + 2) + 0.5,
                    Math.floor(this.bound.y) + 0.5, kEditBoxSettings.dash);

            this.parent.painter.lineTo(ctx,
                    Math.floor(this.clampBound.left) + 0.5,
                    Math.floor(this.bound.y + this.bound.h) + 0.5,
                    Math.floor(this.clampBound.right + 2) + 0.5,
                    Math.floor(this.bound.y + this.bound.h) + 0.5, kEditBoxSettings.dash);

            this.parent.painter.lineTo(ctx,
                    Math.floor(this.clampBound.left) + 0.5,
                    Math.floor(this.bound.y) + 0.5,
                    Math.floor(this.clampBound.left) + 0.5,
                    Math.floor(this.bound.y + this.bound.h) + 0.5, kEditBoxSettings.dash);

            this.parent.painter.lineTo(ctx,
                    Math.floor(this.clampBound.right + 2) + 0.5,
                    Math.floor(this.bound.y) + 0.5,
                    Math.floor(this.clampBound.right + 2) + 0.5,
                    Math.floor(this.bound.y + this.bound.h) + 0.5, kEditBoxSettings.dash);
        },
        drawCarriage: function () {
            this.carriageFrame++;

            if (this.carriageFrame > 80) this.carriageFrame = 0;
            if (this.carriageFrame < 40) return;

            var posX = Math.floor(this.bound.x + this.carriageWidth - this.carriageLeft);
            if (posX <= this.parent.visibleLeft) { return; }

            this.ctx.fillStyle = kEditBoxSettings.caretColor;

            this.ctx.fillRect(posX, Math.floor(this.bound.y + 2), 2, Math.floor(this.bound.h - 3));
        },
        drawSelectedText: function (space) {
            if (space) {

                var spaceLeft = Math.floor(this.bound.x - this.carriageLeft + space.from) - this.screenAddX;
                var spaceRight = spaceLeft + Math.floor(space.to - space.from) + this.screenAddX;

                if (spaceLeft < this.screenBound.x)
                    spaceLeft = this.screenBound.x;

                if (spaceRight > this.screenBound.x + this.screenBound.w)
                    spaceRight = this.screenBound.x + this.screenBound.w;

                var leftX = this.parent.visibleLeft;
                if (spaceLeft < leftX)
                    spaceLeft = leftX;

                var spaceW = spaceRight - spaceLeft;

                if (this.parent.visibleLeft >= spaceRight ||
                    spaceLeft >= this.parent.ctxWidth - kUIScrollBarWidth * 3 || spaceW < 0)
                    return;

                // выделенный текст

                this.ctx.fillStyle = kEditBoxSettings.backgroundSelTextColor;
                this.ctx.fillRect(spaceLeft, Math.floor(this.bound.y + 2), spaceW, this.bound.h - 3);

                this.ctx.save();
                this.ctx.beginPath();
                this.ctx.rect(spaceLeft, Math.floor(this.bound.y - 2), spaceW, this.bound.h + 4);
                this.ctx.clip();


                this.ctx.fillStyle = kEditBoxSettings.fontSelColor;
                this.ctx.fillText(this.text, this.bound.x - this.carriageLeft, this.bound.y + this.bound.h * 0.75);

                this.ctx.restore();
            }
        },

        calcMetrics: function () {
            if (this.enable) {
                this.setFont(true);

                this.textWidth = this.ctx.measureText(this.text).width;
                this.carriageWidth = this.ctx.measureText(this.text.substring(0, this.carriage)).width;

                this.bound.w = Math.max(this.textWidth, this.bound.w);

                this.screenBound.w = this.bound.w;

                if (this.milestoneRef) {
                    this.bound.x -= this.bound.w + 2;
                    this.screenBound.x = this.bound.x;
                }

                var leftX = this.parent.visibleLeft;

                // this.screenBound.x    =   Math.max(this.screenBound.x, leftX);
                this.screenBound.w = Math.min(this.parent.ctxWidth - this.screenBound.x - kUIScrollBarWidth * 3, this.screenBound.w);

                //if (this.bound.x < this.parent.visibleLeft) {this.screenBound.w = this.bound.x + this.bound.w - leftX;}
                //if (this.textWidth + this.bound.x < leftX) {this.screenBound.w = 0;}

                this.screenAddX = 0;
                //if (this.screenBound.x <= leftX) {
                //    //this.screenAddX     =   this.screenBound.x - leftX;
                //}

                if (this.carriageLeftBuild) {
                    if (this.screenBound.x + this.screenBound.w > this.bound.x + this.textWidth - this.carriageLeft) {
                        this.carriageLeft -= this.screenBound.x + this.screenBound.w - (this.bound.x + this.textWidth - this.carriageLeft);
                    }

                    this.calcCarriageLeft();

                    this.carriageLeftBuild = false;
                }

                if (this.textWidth <= this.screenBound.w) {
                    this.carriageLeft = 0;
                }

                // обрезка

                this.clampBound.left = this.bound.x - this.screenAddX;
                this.clampBound.right = this.clampBound.left + this.screenBound.w + this.screenAddX;

                if (this.clampBound.left < this.screenBound.x)
                    this.clampBound.left = this.screenBound.x;

                if (this.clampBound.right > this.screenBound.x + this.screenBound.w)
                    this.clampBound.right = this.screenBound.x + this.screenBound.w;

                if (this.clampBound.left < leftX)
                    this.clampBound.left = leftX;

                this.setFont(false);
            }
        },
        calcCarriageLeft: function () {
            if (this.enable) {
                this.setFont(true);

                this.carriageWidth = this.ctx.measureText(this.text.substring(0, this.carriage)).width;

                if (this.screenBound.w + this.carriageLeft < this.carriageWidth) {
                    this.carriageLeft = this.carriageWidth - this.screenBound.w;
                } else if (this.carriageLeft > this.carriageWidth) {
                    this.carriageLeft = this.carriageWidth;
                }

                this.setFont(false);
            }
        },
        centeringViewPosition: function () {
            var t = this.parent;
            if (this.taskRef) {

                t.animator.moveCenterToX(this.taskRef.beginTime);

                var scrollY = t.getElementPosVertical(this.p, this.m, this.t) - t.ctxHeight * 0.5;

                var maxVal = t.rightScroll.maxValue();
                if (!t.fullscreen) {
                    maxVal = floor2((t.rightScroll.maxValue()) / t.itemMargin) * t.itemMargin;
                    scrollY = floor2(scrollY / t.itemMargin) * t.itemMargin;
                }

                scrollY = Math.min(Math.max(0, scrollY), maxVal) + (t.itemMargin * 2 - kEps) * t.fullscreen;
                scrollY = Math.max(scrollY, kEps);

                t.animator.moveToY(scrollY);
            }
        },

        reset: function (doNotClearFocus) {
            this.bound = { x: 0, y: 0, w: 0, h: 0 };
            this.text = '';
            this.saveText = '';
            this.placeHolder = '';
            this.enable = false;
            this.createMode = undefined;

            if (this.taskRef) {
                this.taskRef.isInEditMode = false;
                if (!doNotClearFocus) this.parent.leftPanelController().clearFocus(true);
            }

            if (this.milestoneRef) {
                this.milestoneRef.isInEditMode = false;
                if (!doNotClearFocus) this.parent.leftPanelController().clearFocus(true);
            }

            this.taskRef = null;
            this.milestoneRef = null;
            this.milestoneRefForUpdate = null;

            this.parent.overlay.style.cursor = '';
            this.parent.menuMileStone.reset();
            this.parent.menuTask.reset();
            this.parent.rightScroll.save();
            this.parent.update();
            this.parent.hitProject = -1;
            this.parent.hitTask = -1;
            this.parent.hitMilestone = -1;
            this.parent.needUpdateContent = true;
            this.parent.needUpdate = true;

            this.parent.hitLink = undefined;
            this.parent.linkLineEdit = undefined;
            this.parent.editMode = kEditModeNoUse;

            this.parent.capTask = -1;

            this.select = { from: 0, to: 0 };
            this.selectMode = false;
        },
        check: function (x, y) {
            return this.bound.x > x || x > this.bound.x + this.bound.w ||
                this.bound.y > y || y > this.bound.y + this.bound.h;
        },
        setBound: function (x, y, w, h, centering) {
            if (this.enable) {

                var wb = this.bound.w;
                var xb = this.bound.x;

                this.bound = { x: x, y: y, w: Math.max(kEditBoxSettings.minBoxW, w), h: h };
                this.screenBound = { x: x, y: y, w: Math.max(kEditBoxSettings.minBoxW, w), h: h };

                this.boundUpdate = true;

                this.calcMetrics();

                if (this.bound.w !== wb || this.bound.x !== xb) { this.carriageLeftBuild = true; }
            }
        },
        inBound: function (x, y) {
            if (this.milestoneRef && this.createMode) {
                if (Math.abs(x - (this.bound.x + this.bound.w)) < kHitSidePixels && y - (this.bound.y + this.bound.h) < this.parent.itemMargin)
                    return true;
            }

            return this.bound.x - kHitSidePixels <= x && x <= this.bound.x + this.bound.w + kHitSidePixels &&
                this.bound.y <= y && y <= this.bound.y + this.bound.h;
        },

        cancel: function (forse, doNotClearFocus) {
            if (this.taskRef || this.milestoneRef) {
                if (!this.saveText.length) {
                    this._removeElement();

                    // undo

                    this.parent._undoManager.flushPop(0);
                } else if (forse) {
                    // undo

                    this.parent._undoManager.flushPop(0);
                }

                this.reset(doNotClearFocus);
            }
        },

        _removeElement: function () {
            if (-1 === this.m) {
                this.parent.storage.p[this.p].t.splice(this.t, 1);
            } else {
                if (this.milestoneRef) {
                    this.parent.storage.p[this.p].m.splice(this.m, 1);
                } else {
                    this.parent.storage.p[this.p].m[this.m].t.splice(this.t, 1);
                }

                if (this.milestoneRefForUpdate) {
                    this.milestoneRefForUpdate.updateTimes();
                }
            }
        },
        complete: function () {

            // text spaces

            this.text = this.text.replace(/^\s+/, '').replace(/\s+$/, '');
            if (this.text.length > kMaxTitleSymbolsCount) {
                this.text = this.text.substring(0, kMaxTitleSymbolsCount);
            }

            if (0 === this.text.length && this.placeHolder.length > 0) {
                this.text = this.placeHolder;
            }

            if (this.addMode && !this.text.length) {
                this._removeElement();
                this.reset();

                // undo
                this.parent._undoManager.flushPop(0);
                return true;
            }

            if (this.saveText.length && !this.text.length) {
                this.text = this.saveText;
            }

            var ts = null;


            if (this.taskRef) {
                ts = this.taskRef;

                this.taskRef._title = this.text;
                this.taskRef.isInEditMode = false;
                this.taskRef.update();
                this.taskRef = null;
            }

            if (this.milestoneRef) {
                ts = this.milestoneRef;

                this.milestoneRef.isInEditMode = false;
                this.milestoneRef._title = this.text;
                this.milestoneRef.update();
            }

            if (!this.text.length) {
                this._removeElement();

                // undo
                this.parent._undoManager.flushPop(0);
            } else {
                if (this.saveText === this.text && !this.addMode) {
                    this.parent._undoManager.flushPop(0);
                } else {

                    // notifity

                    this.parent._undoManager.updateOperation(0, ts);
                }
            }

            this.reset();

            return false;
        },

        onkeypress: function (e) {
            e = window.event || e;

            var titleNorm,
                keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;

            this.counter = 0;

            switch (keyCode) {
                case 13:    // enter
                    {
                        return this.complete();
                    }
                    // mozilla fix

                case 46:  // delete
                    {
                        if (0 === e.charCode)
                            return true;

                        break;
                    }
                    // case 45: // insert
                case 35:    // end
                case 36:    // home
                case 37:    // left
                    //case 39:  // right
                case 8:     // backspace
                case 27:    // esc
                case 38:    // up
                case 40:    // down
                case 33:    // page up
                    { if (e.shiftKey) break; else return false; }
                case 34:    // page down
                    return false;
            }

            if (e.ctrlKey || e.metaKey) { return true; }

            if (this.select.from !== this.select.to) {
                this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                this.setCarriage(this.select.from);
            }

            var symbol = String.fromCharCode(keyCode);

            if (this.carriage !== this.text.length) {
                this.text = this.text.substring(0, this.carriage) + symbol + this.text.substring(this.carriage, this.text.length);
                this.setCarriage(this.carriage + 1);

                //this.centeringViewPosition();
            } else {
                this.text += symbol;
                this.setCarriage(this.text.length);

                //this.centeringViewPosition();
            }

            return true;
        },
        onkeydown: function (e) {
            e = window.event || e;

            var workKey = false, symbol, select, carriage,
                keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;

            this.counter = 0;

            switch (keyCode) {
                case 35:    // end
                    {
                        if (e.shiftKey) {
                            carriage = this.carriage;
                            select = { from: this.select.from, to: this.select.to };

                            this.setCarriage(this.text.length);

                            if (select.from !== select.to) {
                                if (1 === this.mouseSelectDirection) {
                                    this.select = { from: select.from, to: this.text.length };
                                } else {
                                    this.select = { from: select.to, to: this.text.length };
                                }
                            } else {
                                this.select = { from: carriage, to: this.text.length };
                            }
                        } else {
                            this.setCarriage(this.text.length);

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }

                        workKey = true;
                    }
                    break;

                case 36:    // home
                    {
                        if (e.shiftKey) {
                            carriage = this.carriage;
                            select = { from: this.select.from, to: this.select.to };

                            this.setCarriage(0);

                            if (select.from !== select.to) {
                                if (1 === this.mouseSelectDirection) {
                                    this.select = { from: 0, to: select.from };
                                } else {
                                    this.select = { from: 0, to: select.to };
                                }
                            } else {
                                this.select = { from: 0, to: carriage };
                            }
                        } else {
                            this.setCarriage(0);

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }

                        workKey = true;
                    }
                    break;

                case 37:    // left
                    {
                        if (e.ctrlKey || e.metaKey) {
                            this.setCarriage(this.findWord(this.text, this.carriage));
                        } else if (e.shiftKey) {

                            // выделение текста

                            if (-1 === this.mouseSelectDirection) {

                                if (this.select.to > this.mouseSelectStartPos) {
                                    this.select.to--;
                                    this.select.to = Math.min(this.select.to, this.text.length);

                                    carriage = this.select.to;
                                } else {
                                    this.select.from--;
                                    this.select.from = Math.max(this.select.from, 0);

                                    carriage = this.select.from;
                                }
                            } else {
                                if (this.select.to <= this.mouseSelectStartPos) {
                                    this.select.from--;
                                    this.select.from = Math.max(this.select.from, 0);

                                    carriage = this.select.from;
                                } else {
                                    this.select.to--;
                                    this.select.to = Math.min(this.select.to, this.text.length);

                                    carriage = this.select.to;
                                }
                            }

                            select = { from: this.select.from, to: this.select.to };

                            this.setCarriage(carriage);
                            this.select = { from: select.from, to: select.to };
                        } else {
                            if (this.select.from !== this.select.to) {
                                this.setCarriage(this.select.from);
                            } else {
                                this.setCarriage(this.carriage - 1)
                            }

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }
                        workKey = true;
                    }
                    break;

                case 39:    // right
                    {
                        if (e.ctrlKey || e.metaKey) {
                            if (this.select.from !== this.select.to)
                                this.setCarriage(this.findWord(this.text, this.select.to, true));
                            else
                                this.setCarriage(this.findWord(this.text, this.carriage, true));
                        } else if (e.shiftKey) {

                            // выделение текста

                            if (-1 === this.mouseSelectDirection) {

                                if (this.select.from < this.mouseSelectStartPos) {
                                    this.select.from++;
                                    this.select.from = Math.min(this.select.from, this.mouseSelectStartPos);

                                    carriage = this.select.from;
                                } else {
                                    this.select.to++;
                                    this.select.to = Math.min(this.select.to, this.text.length);

                                    carriage = this.select.to;
                                }
                            } else {
                                if (this.select.from >= this.mouseSelectStartPos) {
                                    this.select.to++;
                                    this.select.to = Math.min(this.select.to, this.text.length);

                                    carriage = this.select.to;
                                } else {
                                    this.select.from++;
                                    this.select.from = Math.max(this.select.from, 0);

                                    carriage = this.select.from;
                                }
                            }

                            select = { from: this.select.from, to: this.select.to };

                            this.setCarriage(carriage);
                            this.select = { from: select.from, to: select.to };
                        } else {
                            if (this.select.from !== this.select.to) {
                                this.setCarriage(this.select.to);
                            } else {
                                this.setCarriage(this.carriage + 1);
                            }

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }

                        workKey = true;
                    }
                    break;

                case 8:     // backspace
                    {
                        if (this.text.length) {
                            if (this.select.from !== this.select.to) {

                                this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                this.setCarriage(this.select.from);

                            } else {

                                this.text = this.text.substring(0, this.carriage - 1) + this.text.substring(this.carriage, this.text.length);
                                this.setCarriage(this.carriage - 1);
                            }

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }

                        workKey = true;
                    }
                    break;

                case 46:    // 'delete'
                    {
                        if (this.text.length) {
                            if (this.select.from !== this.select.to) {

                                this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                this.setCarriage(this.select.from);

                            } else {

                                this.text = this.text.substring(0, this.carriage) + this.text.substring(this.carriage + 1, this.text.length);
                                this.setCarriage(this.carriage);
                            }

                            this.mouseSelectDirection = 0;
                            this.mouseSelectStartPos = this.carriage;
                            this.select = { from: this.carriage, to: this.carriage };
                        }

                        workKey = true;
                    }
                    break;

                case 27:    // esc
                    {
                        //

                        if (!this.saveText.length) {
                            this._removeElement();
                        }

                        // undo
                        this.parent._undoManager.flushPop(0);

                        this.reset();

                        workKey = true;
                    }
                    break;

                case 65:    // ctr + a
                    {
                        if (e.ctrlKey || e.metaKey) {
                            this.setCarriage(this.text.length);
                            this.select = { from: 0, to: this.text.length };

                            this.mouseSelectDirection = 1;
                            this.mouseSelectStartPos = 0;

                            workKey = true;
                        }
                    }
                    break;

                case 86:    // ctr + v
                    {
                        if (e.ctrlKey || e.metaKey) {

                            if (window['clipboardData']) {
                                var length, textPaste;

                                textPaste = window['clipboardData'].getData('Text');
                                if (textPaste) {
                                    textPaste = this.formatBuffer(textPaste);
                                    if (textPaste.length) {
                                        if (this.select.from !== this.select.to) {
                                            this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                            this.carriage = this.select.from;
                                        }

                                        length = this.text.length;
                                        this.text = this.text.substring(0, this.carriage) + textPaste + this.text.substring(this.carriage, length);

                                        this.setCarriage(this.carriage + textPaste.length);

                                        this.parent.needUpdate = true;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 67:    // 'ctr + c'
                    {
                        if (e.ctrlKey || e.metaKey) {
                            if (this.select.from !== this.select.to) {
                                if (this._clipboard) {
                                    var textToCopy = this.text.substring(this.select.from, this.select.to);
                                    if (textToCopy.length) {
                                        this._clipboard.copyText(textToCopy);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 88:    // 'ctr + x'
                    {
                        if (e.ctrlKey || e.metaKey) {

                            if (this.text.length) {
                                if (this.select.from !== this.select.to) {

                                    this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                    this.setCarriage(this.select.from);

                                    this.parent.needUpdate = true;
                                }
                            }

                            stopSystemEvent(e);

                            return false;

                        }
                    }
                    break;

                default:
                    break;
            }

            if (workKey) {
                stopSystemEvent(e);
            }

            this.parent.needUpdate = true;

            return false;
        },

        // mouse events

        onmousedown: function (e) {

            if (null === this.taskRef && null === this.milestoneRef) { return; }

            if (this.inBound(e.x, e.y)) {
                if (!this.parent.pressedButtonRight) {

                    var curTime = new Date().getTime();

                    if (this.doubleMouseClickTime) {

                        curTime -= this.doubleMouseClickTime.getTime();

                        this.doubleMouseClickTime = null;

                        if (curTime < kTimeDoubleClicks) {

                            this.setCarriage(this.text.length);
                            this.select = { from: 0, to: this.text.length };

                            this.mouseSelectDirection = 1;
                            this.mouseSelectStartPos = 0;

                            this.clickTime = new Date();
                            return;
                        }
                    } else {

                        if (this.clickTime) {

                            if (curTime - this.clickTime.getTime() < kTimeDoubleClicks) {
                                this.ondblclick(e);
                                this.clickTime = new Date();
                                return;
                            }
                        }
                    }

                    this.updateSelection(e.x + this.carriageLeft);

                    this.mouseSelectStartPos = this.carriage;

                    this.checkUp = true;
                    this.selectMode = true;
                    this.clickCarriage = this.carriage;

                    this.parent.needUpdate = true;
                }
            } else {
                if (!this.saveText.length) {

                    if (0 !== this.text.length)
                        return this.complete();

                    this._removeElement();

                    // undo
                    this.parent._undoManager.flushPop(0);
                } else {
                    if (this.saveText === this.text && undefined == this.addMode) {
                        this.parent._undoManager.flushPop(0);
                    } else {
                        if (this.addMode && !this.text.length) {
                            this.parent._undoManager.flushPop(0);
                            this._removeElement();
                            this.reset();
                        } else {
                            if (this.taskRef) {
                                // notifity
                                this.parent._undoManager.updateOperation(0, this.taskRef);
                            }
                        }
                    }
                }

                this.reset();

                this.checkUp = true;
            }
        },
        onmouseup: function (e) {

            if (this.selectMode) {
                this.setFont(true);

                var posX = this.bound.x;
                if (this.textWidth >= this.bound.w)
                    posX = this.bound.x + this.bound.w - this.textWidth;

                var pos = this.symbolIndexAt(this.ctx, this.text, e.x - posX + this.carriageLeft);
                if (undefined !== pos) {
                    var oldPos = this.clickCarriage;
                    // this.setCarriage (Math.max(oldPos, pos));
                    this.setCarriage(pos);
                    this.select = { from: Math.min(oldPos, pos), to: Math.max(oldPos, pos) };

                    this.mouseSelectDirection = 0;

                    if (this.mouseSelectStartPos > this.carriage)
                        this.mouseSelectDirection = -1;
                    if (this.mouseSelectStartPos < this.carriage)
                        this.mouseSelectDirection = 1;
                }
            }

            this.checkUp = false;
            this.selectMode = false;
        },
        onmousemove: function (e) {
            if (null === this.taskRef && null === this.milestoneRef) { return false; }

            if (this.selectMode) {
                this.setFont(true);

                var posX = this.bound.x;
                if (this.textWidth >= this.bound.w)
                    posX = this.bound.x + this.bound.w - this.textWidth;

                var select = { from: this.select.from, to: this.select.to };

                var pos = this.symbolIndexAt(this.ctx, this.text, e.x - posX + this.carriageLeft);
                if (undefined !== pos) {
                    var oldPos = this.clickCarriage;
                    this.setCarriage(Math.max(oldPos, pos));
                    this.select = { from: Math.min(oldPos, pos), to: Math.max(oldPos, pos) };

                    if (this.select.from < select.from) {
                        this.setCarriage(this.select.from);
                        this.select = { from: Math.min(oldPos, pos), to: Math.max(oldPos, pos) };
                    } else if (this.select.to > select.to) {
                        this.setCarriage(this.select.to);
                        this.select = { from: Math.min(oldPos, pos), to: Math.max(oldPos, pos) };
                    } else if (this.select.to === this.select.from) {
                        this.select = { from: this.carriage, to: this.carriage };
                    }

                    this.parent.needUpdate = true;
                }
            }

            return true;

        },
        ondblclick: function (e) {
            if (this.inBound(e.x, e.y)) {
                this.doubleMouseClickTime = new Date();

                var posX = this.bound.x;
                if (this.textWidth >= this.bound.w)
                    posX = this.bound.x + this.bound.w - this.textWidth;

                this.setFont(true);

                var pos = this.symbolIndexAt(this.ctx, this.text, e.x - posX + this.carriageLeft);
                if (undefined !== pos) {
                    var sep = this.separateWord(this.text, pos);
                    if (sep) {
                        if (this.select.from === sep.from && this.select.to === sep.to) {
                            this.setCarriage(this.text.length);

                            this.select = { from: 0, to: this.text.length };

                            this.mouseSelectDirection = 1;
                            this.mouseSelectStartPos = 0;
                        } else {
                            this.setCarriage(sep.to);

                            this.select = { from: sep.from, to: sep.to };

                            this.mouseSelectDirection = 1;
                            this.mouseSelectStartPos = sep.from;
                        }
                    }
                }
            }
        },

        updateSelection: function (x) {
            this.setFont(true);

            var posX = this.bound.x;
            if (this.textWidth >= this.bound.w)
                posX = this.bound.x + this.bound.w - this.textWidth;

            var pos = this.symbolIndexAt(this.parent.overlayctx, this.text, x - posX);
            if (undefined !== pos) {
                this.setCarriage(pos);
                //this.carriage   =   pos;
                this.select = { from: pos, to: pos };
            }
        },
        calculateSelectSpace: function (ctx, s, from, to) {
            if (from === to) { return undefined; }

            return {
                from: ctx.measureText(s.substring(0, from)).width,
                to: ctx.measureText(s.substring(0, to)).width
            };
        },
        symbolIndexAt: function (ctx, s, x) {
            var length = s.length, i = 0, width = 0, curW = 0, str = '', half = 0;

            if (0 === length) { return undefined; }
            if (x <= 0) { return 0; }
            if (ctx.measureText(s).width <= x) { return length; }

            for (i = 0; i < length; ++i) {

                str += s[i];
                half = curW;
                curW = ctx.measureText(str).width;

                half = (curW - half) * 0.5 + half;

                if (width <= x && x <= curW) {
                    if (x >= half) return i + 1;

                    return i;
                }

                width = curW;
            }

            return undefined;
        },
        separateWord: function (s, ind) {
            var separators = ' .,<>?;:{}[]~!@#$%^&&*()_+*=-';

            var i, length = s.length, word = '', symbol, from = ind, to = ind + 1;

            if (length) {

                word += s[ind];

                if (-1 === separators.indexOf(word)) {

                    for (i = ind + 1; i < length; ++i) {
                        symbol = s[i];

                        if (-1 !== separators.indexOf(symbol)) { break; }

                        word += symbol;
                        to++;
                    }

                    for (i = ind - 1; i >= 0; --i) {
                        symbol = s[i];

                        if (-1 !== separators.indexOf(symbol)) { break; }
                        word = symbol + word;

                        --from;
                    }
                }
            }

            return { word: word, from: from, to: to };
        },

        findWord: function (s, ind, direction) {
            var separators = ' .,<>?;:{}[]~!@#$%^&&*()_+*=-';

            var i, length = s.length, next = ind;
            if (length) {
                if (direction) {

                    // нормальный символ

                    if (-1 === separators.indexOf(s[ind])) {
                        for (i = ind; i < length; ++i) {
                            next = i;
                            if (-1 !== separators.indexOf(s[i])) { break; }
                        }

                        for (i = next; i < length; ++i) {
                            if (-1 === separators.indexOf(s[i])) { if (i === length - 1) return length; return i; }
                        }
                    } else {
                        for (i = ind; i < length; ++i) {
                            next = i;
                            if (-1 === separators.indexOf(s[i])) { break; }
                        }

                        for (i = next; i < length; ++i) {
                            if (-1 === separators.indexOf(s[i])) { if (i === length - 1) return length; return i; }
                        }
                    }

                } else {
                    if (ind > 0) { if (-1 !== separators.indexOf(s[ind - 1]))--ind; }

                    if (-1 === separators.indexOf(s[ind])) {
                        for (i = ind; i >= 0; --i) {
                            next = i;
                            if (-1 !== separators.indexOf(s[i])) { return i + 1; }
                        }

                        for (i = next; i >= 0; --i) {
                            if (-1 === separators.indexOf(s[i])) { return i; }
                        }
                    } else {
                        for (i = ind; i >= 0; --i) {
                            next = i;
                            if (-1 === separators.indexOf(s[i])) { break; }
                        }

                        for (i = next; i >= 0; --i) {
                            if (-1 !== separators.indexOf(s[i])) { return i + 1; }
                        }
                    }

                    return 0;
                }
            }

            return length;
        },

        // clipboard

        onpaste: function (e) {

            var length, textPaste;

            if (e.clipboardData) {

                if (e.clipboardData.types) {

                    var i = 0, key;

                    while (i < e.clipboardData.types.length) {

                        key = e.clipboardData.types[i];

                        if ('text/plain' === key) {
                            textPaste = e.clipboardData.getData(key);

                            if (textPaste) {
                                textPaste = this.formatBuffer(textPaste);

                                if (textPaste.length) {

                                    if (this.select.from !== this.select.to) {
                                        this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                        this.carriage = this.select.from;
                                    }

                                    length = this.text.length;
                                    this.text = this.text.substring(0, this.carriage) + textPaste + this.text.substring(this.carriage, length);

                                    this.setCarriage(this.carriage + textPaste.length);
                                }
                            }
                        }

                        ++i;
                    }
                } else {

                    textPaste = e.clipboardData.getData('text/plain');
                    if (textPaste) {
                        textPaste = this.formatBuffer(textPaste);
                        if (textPaste.length) {

                            if (this.select.from !== this.select.to) {
                                this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                                this.carriage = this.select.from;
                            }

                            length = this.text.length;
                            this.text = this.text.substring(0, this.carriage) + textPaste + this.text.substring(this.carriage, length);

                            this.setCarriage(this.carriage + textPaste.length);
                        }
                    }
                }
            }

            if (window['clipboardData']) {

                textPaste = window['clipboardData']['getData']('Text');
                if (textPaste) {
                    textPaste = this.formatBuffer(textPaste);
                    if (textPaste.length) {
                        if (this.select.from !== this.select.to) {
                            this.text = this.text.substring(0, this.select.from) + this.text.substring(this.select.to, this.text.length);
                            this.carriage = this.select.from;
                        }

                        length = this.text.length;
                        this.text = this.text.substring(0, this.carriage) + textPaste + this.text.substring(this.carriage, length);

                        this.setCarriage(this.carriage + textPaste.length);
                    }
                }
            }
        },
        oncopy: function (e) {
            if (this._clipboard) {
                this._clipboard.oncopy(e);
            }
        },

        formatBuffer: function (s) {
            return s.replace(/(\r\n|\n|\r)/gm, ' ');
        }
    };

    function ScrollBar(ctx, delegate) {
        this.delegate = delegate;
        this.ctx = ctx;

        this.position = 0;
        this.thumb = 0;
        this.step = 0;
        this.mouse = { x: 0, y: 0 };
        this.offmouse = { x: 0, y: 0 };
        this.isLBMDown = false;
        this.store = -1;
        this.offDown = 0;

        this._value = 0;
        this._focus = true;
        this.afterNormalize = false;
    }
    ScrollBar.prototype = {

        init: function (viewY, viewWidth, maxValue, ctxWidth) {
            this.viewWidth = viewWidth;
            this.viewY = viewY;
            this._maxValue = maxValue + this.offDown;

            this.ctxWidth = ctxWidth;
            this.ctxHeight = this.ctx.canvas.height;

            this.needRebuild();
        },

        refresh: function () {
            if (0 === this._maxValue || (this._maxValue - this.viewWidth) < 1)
                return;

            if ((this._maxValue - this.viewWidth) < this.viewWidth - kUIScrollBarThumbWidth) {
                this.thumb = this.viewWidth - (this._maxValue - this.viewWidth);
                this.step = 1;
            } else {
                this.thumb = kUIScrollBarThumbWidth;
                this.step = (this._maxValue - this.viewWidth) / (this.viewWidth - kUIScrollBarThumbWidth);
            }

            if (-1 != this.store) {
                this.position = this.store / this.step;
                this.store = -1;
            }

            this.ctx.fillStyle = kUIScrollBarBackgroundColor;

            this.ctx.fillRect(this.ctxWidth - kUIScrollBarWidth - kUIScrollBarIndentWidth,
                this.viewY,
                kUIScrollBarWidth,
                    this.ctxHeight - kUIScrollBarWidth * 1.5 - this.viewY);

            if (this.isLBMDown && this._focus)
                this.ctx.fillStyle = kUIScrollBarThumbPressedColor;
            else
                this.ctx.fillStyle = kUIScrollBarThumbColor;

            this.delegate.painter.roundRect(this.ctx, this.ctxWidth - kUIScrollBarWidth - kUIScrollBarIndentWidth,
                    this.viewY + this.position,
                kUIScrollBarWidth,
                this.thumb, 2);

            this.ctx.fill();
        },

        value: function () {
            return this._value;
        },
        maxValue: function () {
            return this._maxValue - this.viewWidth;
        },
        save: function (afterNormalize) {
            this.store = this.position * this.step;
            this.afterNormalize = afterNormalize;
        },

        clamp: function () {
            if (this.step > 0) {
                if (this.position > (this._maxValue - this.viewWidth) / this.step) {
                    this._normalize();
                }

                if (this.delegate)
                    this.delegate.update();
            }
        },

        onmousemove: function (e) {
            if (0 != this._maxValue) {
                if (this.isLBMDown) {
                    this.mouse = e;

                    this.mouseChange = true;

                    if (this._focus) {
                        this.position += ((this.mouse.y - this.offmouse.y) / this.delegate.itemMargin) * this.delegate.itemMargin;
                        this.offmouse.y = this.mouse.y;
                    }

                    this._focus = true;

                    this._normalize();

                    var t = this.delegate;
                    this._value = floor2(this.position * this.step / t.itemMargin) * t.itemMargin - kEps;

                    if (this.delegate)
                        this.delegate.update();

                    return true;
                }
            }

            return false;
        },
        onmousedown: function (e) {
            if ((0 != this._maxValue) && this.isHit(e)) {
                if (this.mouse.y >= this.viewY + this.position && this.mouse.y < this.viewY + this.position + this.thumb) {
                    this.offmouse.y = this.mouse.y;
                    this.isLBMDown = true;
                    this._focus = true;
                    this.mouseChange = false;

                    var t = this.delegate;
                    this._value = floor2(this.position * this.step / t.itemMargin) * t.itemMargin - kEps;

                    t.update();
                }

                return true;
            }

            return false;
        },
        onmouseup: function (e) {
            this.isLBMDown = false;
            this.mouseChange = false;

            if ((0 != this._maxValue) && this.isHit(e)) {

                var t = this.delegate;
                this._value = floor2(this.position * this.step / t.itemMargin) * t.itemMargin - kEps;

                t.update();

                return true;
            }

            return false;
        },
        onmousewheel: function (e) {
            if ((this._maxValue - this.viewWidth) < 1)
                return false;

            e = e || window['event'];

            var delta = e['deltaY'] || e['detail'] || e['wheelDelta'];
            if (e['deltaY']) delta *= -1;
            if (e['detail']) delta *= -1;

            var t = this.delegate;

            if (0 != this._maxValue && 0 != delta) {

                var sign = -delta / Math.abs(delta);

                if (t.fullscreen) {
                    this.position += sign * kUIScrollBarStepMoveY / this.step;
                } else {
                    var y = floor2((this.position * this.step + sign * t.itemMargin) / t.itemMargin + 0.5) * t.itemMargin;

                    y = Math.max(y, 0);
                    y = Math.min(y, this._maxValue - this.viewWidth);
                    y = floor2(y / t.itemMargin) * t.itemMargin - kEps;

                    this.setWorldValue(y);
                }

                this._normalize();
                t.update();

                t.offMenus();
                t.clearPopUp = true;

                return true;
            }

            return false;
        },

        pageMove: function (direction, position) {
            if (direction !== 0) {
                if ('home' === position) {
                    this.position = (this._maxValue - this.viewWidth) / this.step;
                    this._value = this.position * this.step;
                } else if ('end' === position) {
                    this.position = 0;
                    this._value = 0;
                } else {
                    this.position = this.calculatePageMove(direction);
                    this._value = this.position * this.step;
                    this._normalize();
                }

                if (this.delegate)
                    this.delegate.update();
            }
        },
        calculatePageMove: function (direction) {
            var pageSy = this.viewWidth / (this._maxValue - this.viewWidth);
            return this.position - direction / Math.abs(direction) * pageSy * (this._maxValue - this.viewWidth) / this.step;
        },

        forwardMoveY: function (Y) {
            if (this.step > 0) {
                this.position -= Y / this.step;

                this._normalize();

                var t = this.delegate;
                this._value = this.position * this.step;

                t.update();
            } else {
                this.position = 0;
                this._value = 0;
            }
        },
        setWorldValue: function (X, maxnormalize) {
            if (this.step > 0) {
                this.position = X / this.step;

                this._normalize(maxnormalize);
                var t = this.delegate;
                this._value = this.position * this.step;
            } else {
                this.position = 0;
                this._value = 0;
            }
        },
        setTopPos: function () {
            this.position = 0;
            this._value = 0;
        },

        isHit: function (e) {
            this.mouse = e;
            return (this.mouse.x >= this.ctxWidth - kUIScrollBarWidth - kUIScrollBarIndentWidth * 2 && this.mouse.x < this.ctxWidth);
        },
        needRebuild: function () {
            if (-1 != this.store) {

                if (0 == this._maxValue || (this._maxValue - this.viewWidth) < 1) {
                    this.position = 0;
                    this._value = 0;
                    this.store = -1;
                    return;
                }

                if ((this._maxValue - this.viewWidth) < this.viewWidth - kUIScrollBarThumbWidth) {
                    this.thumb = this.viewWidth - (this._maxValue - this.viewWidth);
                    this.step = 1;
                } else {
                    this.thumb = kUIScrollBarThumbWidth;
                    this.step = (this._maxValue - this.viewWidth) / (this.viewWidth - kUIScrollBarThumbWidth);
                }

                this.position = this.store / this.step;
                this._value = this.position * this.step;

                this._normalize();

                this.store = -1;
            }
        },

        setOffDown: function (x) {
            this.offDown = x;
        },

        focus: function (fc) {
            this._focus = fc;
        },

        //
        _normalize: function (maxnormalize) {
            this.position = Math.max(this.position, 0);

            if (maxnormalize && this.position > (this._maxValue - this.viewWidth) / this.step) {
                var maxVal = floor2((this.maxValue()) / this.delegate.itemMargin) * this.delegate.itemMargin;
                this.forwardMoveY(maxVal - kEps);
            } else {
                this.position = Math.min(this.position, (this._maxValue - this.viewWidth) / this.step);
                this._value = this.position * this.step;

                if (this.afterNormalize) {

                    this._value = floor2((this._value + kEps) / this.delegate.itemMargin) * this.delegate.itemMargin - kEps;
                    this.position = this._value / this.step;
                    this.afterNormalize = false;
                }
            }

            if (this._maxValue < this.viewWidth) {
                this.position = 0;
                this._value = 0;
            }
        }
    };

    function BottomScrollBar(ctx, delegate) {
        this.delegate = delegate;
        this.ctx = ctx;
        this.ctxWidth = this.ctx.canvas.width;
        this.ctxHeight = this.ctx.canvas.height;

        this.begin = 0;
        this.end = 0;
        this.anchor = 0;

        this.mouse = { x: 0, y: 0 };
        this.offmouse = { x: 0, y: 0 };
        this.isLBMDown = false;

    }
    BottomScrollBar.prototype = {

        updateThumb: function (obj, width, height) {
            this.begin = obj.begin;          //  [0.0 ~ 1.0]
            this.end = obj.end;            //  [0.0 ~ 1.0]

            this.ctxWidth = width - kUIScrollBarWidth * 2;
            this.ctxHeight = height;
            this.leftX = this.delegate.visibleLeft + 2; // +2 px что бы ползунок не плотно прилигал если его сдвинуть влево до упора
        },

        refresh: function () {
            if (this.ctxWidth - this.leftX < kUIScrollBarMinWidthShowControl) { return; }

            if (this.begin < this.end) {

                // фон

                this.ctx.fillStyle = kUIScrollBarBackgroundColor;
                this.ctx.fillRect(0, this.ctxHeight - (kUIScrollBarWidth + kUIScrollBarIndentWidth), this.ctxWidth, kUIScrollBarWidth);

                if (this.isLBMDown) { this.ctx.fillStyle = kUIScrollBarThumbPressedColor; } else { this.ctx.fillStyle = kUIScrollBarThumbColor; }

                var cWidth = this.regionSize();
                var thumbW = cWidth * (this.end - this.begin);
                var thumbX = this.leftX + cWidth * this.begin;

                if (thumbW < kUIScrollBarThumbMinWidth) {
                    thumbW = kUIScrollBarThumbMinWidth;
                    var cX = (this.end + this.begin) * 0.5;
                    cX = 0.5 + (cX - 0.5) * (1.0 + (this.end - this.begin));
                    thumbX = this.leftX + (cWidth - thumbW) * cX;

                    // if (this.isLBMDown) {this.ctx.fillStyle = '#FF0000';} else {this.ctx.fillStyle = '#0000FF';}   // debug
                }

                // ползунок с закругленными углами

                this.delegate.painter.roundRect(this.ctx, thumbX, this.ctxHeight - (kUIScrollBarWidth + kUIScrollBarIndentWidth), thumbW, kUIScrollBarWidth, 2);
                this.ctx.fill();
            }
        },

        onmousemove: function (e) {
            if (0 != this._maxValue) {
                if (this.isLBMDown) {
                    this.mouse = e;

                    var t = this.delegate,
                        oneHourInPixels, pos, fx, dx, leftHours, duration, timeCur, fraction, cWidth, thumbW, thumbX, cX, maxPos, minPos;

                    if (t) {
                        t.animator.stop();

                        duration = t.zoomBar.duration;
                        timeCur = t.zoomBar.timeCur;
                        fraction = t.zoomBar.fraction;

                        oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
                        leftHours = this.leftX / oneHourInPixels;

                        cWidth = this.regionSize();
                        thumbW = cWidth * (this.end - this.begin);
                        thumbX = this.leftX + cWidth * this.begin;

                        if (thumbW < kUIScrollBarThumbMinWidth) {

                            thumbW = kUIScrollBarThumbMinWidth;
                            cX = (this.end + this.begin) * 0.5;
                            cX = 0.5 + (cX - 0.5) * (1.0 + (this.end - this.begin));
                            thumbX = this.leftX + (cWidth - thumbW) * cX;

                            pos = (this.mouse.x - this.anchor - this.leftX) / (cWidth - thumbW);

                            maxPos = t.zoomBar.maxPos();
                            minPos = t.zoomBar.minPos();

                            t.offsetX = pos * (maxPos - minPos) + minPos;

                            t.offsetX = Math.max(t.offsetX, maxPos);
                            t.offsetX = Math.min(t.offsetX, minPos);

                        } else {

                            pos = (this.mouse.x - this.anchor - this.leftX) / cWidth;

                            fx = fraction - pos;
                            dx = Math.max((Math.min(fx, fraction) * duration), (t.zoomBar.bandwidth / t.zoomBar.regionSize() - 1.0 + fraction) * duration) + leftHours;

                            t.offsetX = dx * oneHourInPixels / t.timeScale.hourInPixels;
                        }

                        t.zoomBar.needRepaint();
                        t.updateWithStrafe();

                        return true;
                    }
                }
            }

            return false;
        },
        onmousedown: function (e) {
            if (this.ctxWidth - this.leftX < kUIScrollBarMinWidthShowControl) { return false; }

            if (this.begin < this.end && this.isHit(e)) {

                var cWidth = this.regionSize();
                var thumbW = cWidth * (this.end - this.begin);
                var thumbX = this.leftX + cWidth * this.begin;

                if (thumbW < kUIScrollBarThumbMinWidth) {
                    thumbW = kUIScrollBarThumbMinWidth;
                    var cX = (this.end + this.begin) * 0.5;
                    cX = 0.5 + (cX - 0.5) * (1.0 + (this.end - this.begin));
                    thumbX = this.leftX + (cWidth - thumbW) * cX;
                }

                this.isLBMDown = true;
                this.anchor = this.mouse.x - thumbX;

                this.delegate.offMenus();
                this.delegate.updateData();

                return true;
            }

            return false;
        },
        onmouseup: function (e) {
            this.isLBMDown = false;
        },
        onmousewheel: function (e) {
            return this.delegate.onmousewheel(e);
        },

        isHit: function (e) {
            if (this.ctxWidth - this.leftX < kUIScrollBarMinWidthShowControl) { return false; }

            this.mouse = e;

            if (this.mouse.y >= this.ctxHeight - kUIScrollBarWidth - kUIScrollBarIndentWidth * 2 && this.mouse.y < this.ctxHeight && this.begin < this.end) {

                var cWidth = this.regionSize();
                var thumbW = cWidth * (this.end - this.begin);
                var thumbX = this.leftX + cWidth * this.begin;

                if (thumbW < kUIScrollBarThumbMinWidth) {
                    thumbW = kUIScrollBarThumbMinWidth;
                    var cX = (this.end + this.begin) * 0.5;
                    cX = 0.5 + (cX - 0.5) * (1.0 + (this.end - this.begin));
                    thumbX = this.leftX + (cWidth - thumbW) * cX;
                }

                return (this.mouse.x > thumbX && this.mouse.x < thumbX + thumbW);
            }

            return false;
        },

        regionSize: function () {
            return this.ctxWidth - this.leftX;
        }
    };

    function BaseEditBox(delegate) {
        this.delegate = delegate;
        this.element = null;
        this.anchor = { p: undefined, m: undefined, t: undefined };
        this.viewUpdate = true;
        this.ref = null;
        this.baseText = '';
        this.enable = false;
    }
    BaseEditBox.prototype = {
        create: function (parent) {
            this.element = document.createElement('input');
            if (this.element) {

                this.element.id = 'editControl';
                this.element.type = 'text';
                this.element.value = '';

                this.element.style.position = 'absolute';
                this.element.style.backgroundColor = 'white';
                //this.element.style.left            =   '1000px';
                //this.element.style.top             =   '10px';
                this.element.style.width = '100px';
                this.element.style.height = '20px';
                //this.element.style.overflow        =   'hidden';

                //this.element.style.font            =   '13px ' + kDefaultFontName;
                this.element.style.border = 'solid';
                this.element.style.borderColor = '#CCCCCC';
                this.element.style.borderWidth = '1px';
                this.element.style.outline = 'none';

                this.show(false);

                //this.element.setAttribute('contentEditable', true);

                var t = this;

                this.element.onkeypress = function (e) {
                    e = window.event || e;
                    var keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
                    this.counter = 0;

                    var titleNorm, text;

                    switch (keyCode) {
                        case 13:    // 'enter'

                            // text spaces

                            text = this.value.replace(/^\s+/, '').replace(/\s+$/, '');
                            if (text.length > kMaxTitleSymbolsCount) {
                                text = text.substring(0, kMaxTitleSymbolsCount);
                            }

                            if (t.baseText.length && !text.length) {
                                text = t.baseText;
                            }

                            var ref = null;

                            if (t.ref) {
                                ref = t.ref;

                                t.ref._title = text;
                                t.ref.isInEditMode = false;
                                t.ref.update();
                                t.ref = null;
                            }

                            if (!text.length) {

                                // undo

                                t.delegate._undoManager.flushPop(0);

                            } else {
                                if (t.baseText === text) {

                                    // текст не изменился, поэтому выкидываем операцию из стека undo

                                    t.delegate._undoManager.flushPop(0);

                                } else {

                                    // notifity

                                    t.delegate._undoManager.updateOperation(0, ref);
                                }
                            }

                            t.close();
                            t.delegate.updateData();

                            return false;
                    }

                    return true;
                };
                this.element.onkeydown = function (e) {
                    e = window.event || e;
                    var keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;

                    var titleNorm, text;

                    switch (keyCode) {
                        case 27:  // 'esc'
                            {
                                // undo

                                t.delegate._undoManager.flushPop(0);

                                t.close();
                                t.delegate.updateData();

                                return false;
                            }
                            break;

                        case 13:    // 'enter'
                            {
                                // text spaces

                                text = this.value.replace(/^\s+/, '').replace(/\s+$/, '');
                                if (text.length > kMaxTitleSymbolsCount) {
                                    text = text.substring(0, kMaxTitleSymbolsCount);
                                }

                                if (t.baseText.length && !text.length) {
                                    text = t.baseText;
                                }

                                var ref = null;

                                if (t.ref) {
                                    ref = t.ref;

                                    t.ref._title = text;
                                    t.ref.isInEditMode = false;
                                    t.ref.update();
                                    t.ref = null;
                                }

                                if (!text.length) {

                                    // undo

                                    t.delegate._undoManager.flushPop(0);

                                } else {
                                    if (t.baseText === text) {

                                        // текст не изменился, поэтому выкидываем операцию из стека undo

                                        t.delegate._undoManager.flushPop(0);

                                    } else {

                                        // notifity

                                        t.delegate._undoManager.updateOperation(0, ref);
                                    }
                                }

                                t.close();
                                t.delegate.updateData();

                                return false;
                            }
                    }

                    return true;
                };

                if (parent)
                    parent.appendChild(this.element);
                else
                    document.body.appendChild(this.element);
            }
        },

        updateDefaults: function () {
            //if (this.element)
            //    this.element.style.font            =   '13px ' + kDefaultFontName;
        },

        setValue: function (s) {
            if (this.element) {
                this.baseText = s;
            }
        },
        setReference: function (reference, p, m, t) {
            this.ref = reference;
            this.anchor = { p: p, m: m, t: t };
            this.enable = (this.anchor.p !== undefined || this.anchor.m !== undefined || this.anchor.t !== undefined);
        },
        setBoldText: function (flag) {
            //if (flag)
            //    this.element.style.fontWeight  = 'bold';
            //else
            //    this.element.style.fontWeight  = '';
        },

        updateInnerValue: function () {

            if (this.element) {

                this.element.value = '';
                this.element.value = this.baseText;

                this.element.focus();

                this.element.selectionStart = this.element.selectionEnd = this.element.value.length;
            }
        },
        show: function (flag) {
            if (flag) {
                if ('' !== this.element.style.display) {
                    this.element.style.overflow = '';
                    this.element.style.zIndex = 1000;
                    this.element.style.display = '';

                    // устанавливаем текст после того как уже все метрики выставлены. (что бы не было моргания)

                    this.updateInnerValue();
                }
            } else {
                if ('none' !== this.element.style.display) {
                    this.element.style.display = 'none';
                    this.element.style.overflow = 'hidden';
                    this.element.style.zIndex = -1000;
                }
            }
        },
        close: function () {
            this.delegate.updateData();

            if (this.ref) {
                this.delegate._undoManager.flushPop(0);
                this.ref = null;
            }

            this.anchor = { p: undefined, m: undefined, t: undefined };
            this.enable = false;

            this.show(false);
        },

        isEnable: function () {
            return this.enable;
        },
        inBound: function (e) {
            return (this.enable && this.element.style.left <= e.clientX && (this.element.style.left + this.element.style.width) >= e.clientX);
        },
        isValidAnchor: function (p, m, t) {
            return (p === this.anchor.p && m === this.anchor.m && t === this.anchor.t);
        }
    };

    //

    function numberOfValidTasks(tasks) {
        if (0 == tasks.length) {
            return ' (0/0)';
        }

        var complete = 0, filter = 0;
        for (var i = tasks.length - 1; i >= 0; --i) {
            if (tasks[i].filter) {
                continue;
            }

            if (kElementCompleted === tasks[i]._status) {
                ++complete;
            }

            ++filter;
        }

        return ' (' + complete + '/' + filter + ')';
    }
    function taskResponsibles(element, noresponsible) {
        if (null === element._responsibles) {
            return '';
        }

        var count = element._responsibles.length;
        if (count) {
            if (count > 1) {
                return window['Gantt']['Localize_strings']['responsibles']['format'](count);
            }

            return window['Encoder']['htmlDecode'](element._responsibles[0]['displayName']);
        }

        return noresponsible;
    }
    function milestoneResponsibles(element, noresponsible) {
        var src = element._responsibles;
        if (src) {
            return window['Encoder']['htmlDecode'](src['displayName']);
        }

        return noresponsible;
    }
    function taskFill(task, milestone) {
        if (kElementCompleted === task._status) {
            return kLPCompleteColor;
        } else {
            if (task.endFail && milestone === -1) {
                return kLPNormalColor;
            }

            if ((task.endTime <= 0) && (!task.endFail || milestone !== -1)) {
                return kLPOverdueColor;
            }
        }

        return kLPNormalColor;
    }
    function milestoneFill(milestone) {
        if (1 === milestone._status) {
            return kLPCompleteColor;
        }

        if (milestone.endTime <= 0) {
            return kLPOverdueColor;
        }

        return kLPNormalColor;
    }
    function formatTime(date, isTeamlabTime) {
        if (isTeamlabTime) {
            return Teamlab['getDisplayDate'](date);
        }

        return date.toLocaleDateString();
    }

    // LeftPanel MVC

    // leftpanel    -   view
    // model        -   storage
    // controller   -   leftpanelcontroller

    function LeftPanel(timeline, base, model) {
        this.base = base;
        this.el = undefined;
        this.elInner = undefined;
        this.model = model;
        this.timeline = timeline;
        this.controller = undefined;
        this.header = undefined;
        this.btnPrj = undefined;
        this.btnFlds = undefined;
        this.splitter = undefined;
        this.splitterLine = undefined;
        this.fontH1 = 'normal 16px Semibold, sans-serif';

        // cell width measurer

        this.textMeasurer = new TextMeasurer();
    }
    LeftPanel.prototype = {

        // internal

        collection: {
            elements: [],
            cache: [],
            metrics: [],
            indexer: []
        },

        select: {
            p: undefined,
            t: undefined,
            m: undefined
        },
        hit: {
            p: undefined,
            t: undefined,
            m: undefined
        },

        width: 250,
        maxWidth: 250,
        elementsCount: 50,
        distanceBetweenCells: 10, // px
        paddingLeftCellContent: -7, // px
        hiddenCellsCtr: 5 + 21 + 5 + 21, // px

        rows: [
            { name: 'Name', enable: true, minWidth: 265, el: null, posX: 0, isHidden: false, translate: 'Name' },
            { name: 'Responsibility', enable: true, minWidth: 140, el: document.createElement('div'), posX: 0, isHidden: false, translate: 'Responsible(-s)' },
            { name: 'BeginDate', enable: false, minWidth: 95, el: document.createElement('div'), posX: 0, isHidden: false, translate: 'Begin Date' },
            { name: 'EndDate', enable: false, minWidth: 95, el: document.createElement('div'), posX: 0, isHidden: false, translate: 'End Date' },
            { name: 'Status', enable: false, minWidth: 60, el: document.createElement('div'), posX: 0, isHidden: false, translate: 'Status' },
            { name: 'Priority', enable: false, minWidth: 65, el: document.createElement('div'), posX: 0, isHidden: false, translate: 'Priotity' }
        ],
        visibleRows: [],

        //
        _cacheHeightPanel: -1,

        // UI

        disableMode: false,

        // public

        init: function (controller) {
            var inner = this;
            var t = this.timeline;

            this.controller = controller;

            function handlerWheel(e) {
                inner.hit = undefined;

                if (inner.timeline.editBox.isEnable()) {
                    inner.highlightElement(inner.timeline.editBox.p,
                            -1 === inner.timeline.editBox.m ? undefined : inner.timeline.editBox.m,
                            -1 === inner.timeline.editBox.t ? undefined : inner.timeline.editBox.t);
                }

                t.onmousewheel(e);
                return true;
            }

            if (this.base) {

                this.el = document.createElement('div');

                if (this.el) {

                    this.fontH1 = this.textMeasurer.readStyleValue('line-project', 'font');

                    this.el.className = 'left-panel';
                    this.base.appendChild(this.el);

                    this.elInner = document.createElement('div');
                    this.elInner.style.width = '100%';
                    this.elInner.style.height = '100%';
                    this.elInner.style.position = 'absolute';
                    this.el.appendChild(this.elInner);

                    // handlers

                    this.el.onmouseout = function () {
                        t.leftPanelController().getPanel().clearHighlightElement();
                    };
                    //this.el.onmousemove = function () {
                    //
                    //    return true;
                    //};
                    this.el.onmousewheel = function (e) {
                        inner.hit = undefined;

                        if (inner.timeline.editBox.isEnable()) {
                            inner.highlightElement(inner.timeline.editBox.p,
                                    -1 === inner.timeline.editBox.m ? undefined : inner.timeline.editBox.m,
                                    -1 === inner.timeline.editBox.t ? undefined : inner.timeline.editBox.t);
                        }

                        t.onmousewheel(e);
                        return true;
                    };

                    this.el.addEventListener('MozMousePixelScroll', function (e) { handlerWheel(e); }, false);

                    // this.el.onmousedown = function () { inner.closeEditing(); return true; };
                    this.elInner.oncontextmenu = function () { return false; };

                    this.buildHeader(this.base);
                    this.buildSplitter(this.el);
                    this.buildCollection();

                    this.calcRowsWidth();
                    this.addRowAvailable(undefined);

                    // edit box control

                    this.editBox = new BaseEditBox(this.timeline);
                    this.editBox.create(this.elInner);
                }
            }
        },
        calcRowsWidth: function () {

            this.noResponsible = window['Gantt']['Localize_strings']['noResponsible'];

            this.rows[1].minWidth = Math.max(140, this.distanceBetweenCells + this.textMeasurer.width(window['Gantt']['Localize_strings']['responsibles2'] || 'Responsible(-s)', 'row-tab'));
            this.rows[2].minWidth = Math.max(95, this.distanceBetweenCells + this.textMeasurer.width(window['Gantt']['Localize_strings']['beginDate'] || 'Begin Date', 'row-tab'));
            this.rows[3].minWidth = Math.max(95, this.distanceBetweenCells + this.textMeasurer.width(window['Gantt']['Localize_strings']['endDate'] || 'End Date', 'row-tab'));
            this.rows[4].minWidth = Math.max(60, this.distanceBetweenCells + this.textMeasurer.width(window['Gantt']['Localize_strings']['status'] || 'Status', 'row-tab'));
            this.rows[5].minWidth = Math.max(65, this.distanceBetweenCells + this.textMeasurer.width(window['Gantt']['Localize_strings']['priotity'] || 'Priotity', 'row-tab'));

            this.textMeasurer.clear();
        },
        setHeight: function (height) {
            if (this.el) {
                if (this._cacheHeightPanel != height) {
                    this._cacheHeightPanel = height;
                    this.el.style.height = (height - kTimeLineItemMargin * 2 - 1) + 'px';
                }
            }
        },

        buildHeader: function (parent) {
            var inner = this;

            var header = document.createElement('div');
            if (header) {
                header.oncontextmenu = function () { return false; };
                header.className = 'header-element';
                parent.appendChild(header);

                var fakeAbsolute = document.createElement('div');
                fakeAbsolute.style.cssText = 'height:100%; width:100%;position:absolute;';
                header.appendChild(fakeAbsolute);

                header = fakeAbsolute;

                var fakeRelative = document.createElement('div');
                fakeRelative.style.cssText = 'height:100%; width:100%;overflow:hidden;position:relative;background:#D3D3D3;border-right: 1px solid #CCCCCC;';
                header.appendChild(fakeRelative);

                header = fakeRelative;

                var downPlane = document.createElement('div');
                downPlane.className = 'header-element-down';
                header.appendChild(downPlane);

                var inControl = false,
                    t = this.timeline,
                    captureRow = {};

                // init rows elements

                for (var i = 1; i < this.rows.length; ++i) {
                    this.rows[i].el.className = 'row-tab';
                    this.rows[i].el.style.top = (kTimeLineItemMargin - 4) + 'px';

                    header.appendChild(this.rows[i].el);

                    var txtBtn = document.createElement('div');
                    txtBtn.className = 'cell-text-button';
                    this.rows[i].el.appendChild(txtBtn);

                    var clsBtn = document.createElement('div');
                    clsBtn.className = 'cell-close-button';
                    this.rows[i].el.appendChild(clsBtn);

                    // перемещение колонок

                    txtBtn.onmousedown = (function (e) {
                        var index = i;
                        return function (e) {

                            inControl = true;
                            captureRow = undefined;

                            inner.closeEditing();

                            var mouseX = e.clientX - t.ctx.canvas.offsetLeft;
                            var left = inner.rows[0].minWidth;

                            for (var i = 0; i < inner.visibleRows.length; ++i) {
                                if (mouseX >= left && mouseX <= left + inner.visibleRows[i].width) {
                                    captureRow = {};
                                    captureRow.index = i;
                                    captureRow.name = inner.visibleRows[i].name;
                                    break;
                                }

                                left += inner.visibleRows[i].width;
                            }

                            if (!captureRow) return false;

                            document.onmousemove = function (e) {
                                var i = 0, rowW = 1000, cx = 0;
                                var mouseX = e.clientX - t.ctx.canvas.offsetLeft;
                                var left = inner.rows[0].minWidth;

                                for (i = 0; i < inner.visibleRows.length; ++i) {
                                    if (inner.getRowWithName(inner.visibleRows[i].name).isHidden)
                                        break;

                                    rowW = Math.min(rowW, inner.visibleRows[i].width);
                                }

                                for (i = 0; i < inner.visibleRows.length; ++i) {

                                    if (inner.getRowWithName(inner.visibleRows[i].name).isHidden)
                                        break;

                                    //  if (mouseX >= left && mouseX <= left + inner.visibleRows[i].width) {
                                    cx = left + inner.visibleRows[i].width * 0.5;
                                    if (mouseX >= cx - rowW * 0.5 && mouseX <= cx + rowW * 0.5) {

                                        if (captureRow.name !== inner.visibleRows[i].name) {

                                            var elements = [];
                                            var element = inner.visibleRows[i].name;
                                            for (var k = 0; k < inner.visibleRows.length; ++k) {
                                                elements.push(inner.visibleRows[k].name);
                                            }

                                            elements[captureRow.index] = element;
                                            elements[i] = captureRow.name;

                                            captureRow.index = i;

                                            var visibleRows = inner.getVisibleRows();

                                            inner.visibleRows = [];

                                            for (var m = 0; m < elements.length; ++m) {
                                                inner.addRowAvailable(elements[m]);
                                            }
                                            inner.showHiddenRows(visibleRows);

                                            inner.rebuildContent(undefined);

                                            if (inner.controller['onfieldsfilter'])
                                                inner.controller['onfieldsfilter'](e, true, 0, 0, inner.getAvailableRows());

                                            break;
                                        }
                                    }

                                    left += inner.visibleRows[i].width;
                                }
                            };

                            document.onmouseup = function (e) {
                                if (inControl) {

                                    inner.rebuildContent(undefined);

                                    if (inner.controller['onfieldsfilter']) {
                                        inner.controller['onfieldsfilter'](e, true, 0, 0, inner.getHiddenRows(), inner.getVisibleRows());
                                    }

                                    document.onmousemove = function (e) {
                                        t.onmousemove(e);
                                    };
                                    document.onmouseup = function (e) {
                                        t.onmouseup(e);
                                    };
                                }
                            };
                            return true;
                        }
                    })(i);
                    txtBtn.onmouseup = (function (e) {
                        var index = i;
                        return function (e) {

                            inControl = true;

                            if (inner.controller['onfieldsfilter'])
                                inner.controller['onfieldsfilter'](e, true, 0, 0, inner.getAvailableRows());

                            document.onmousemove = function (e) {
                                t.onmousemove(e);
                            };
                            document.onmouseup = function (e) {
                                t.onmouseup(e);
                            };
                            return true;
                        }
                    })(i);

                    // удаление

                    clsBtn.onclick = (function (e) {
                        var index = i;
                        return function () {

                            var cells = inner.getVisibleRows(inner.rows[index].name);
                            inner.closeEditing();
                            inner.deleteAvailableRow(inner.rows[index].name);
                            inner.showHiddenRows(cells);
                            inner.rebuildContent(undefined);

                            if (inner.controller['onfieldsfilter'])
                                inner.controller['onfieldsfilter'](e, true, 0, 0, inner.getAvailableRows());

                            return true;
                        }
                    })(i);
                }

                this.pX = 0; this.pY = 0; this.pfX = 0; this.pfY = 0;

                var btnPrj = document.createElement('div');
                if (btnPrj) {
                    btnPrj.id = 'gantt-filter-projects';
                    btnPrj.className = 'filter-projects';
                    fakeAbsolute.appendChild(btnPrj);

                    var btnFlds = document.createElement('div');
                    if (btnFlds) {
                        btnFlds.id = 'gantt-filter-fields';
                        btnFlds.className = 'filter-fields';
                        btnFlds.style.top = (kTimeLineItemMargin - 4) + 'px';
                        fakeAbsolute.appendChild(btnFlds);

                        btnFlds.onclick = function (e) {
                            inner.closeEditing();

                            if (!inner.disableMode) {

                                var bounds = inner.btnFlds.getBoundingClientRect();

                                if (inner.controller['onfieldsfilter']) {
                                    inner.controller['onfieldsfilter'](e, false, bounds.left + bounds.width, bounds.top + bounds.height, inner.getAvailableRows());
                                }
                            }

                            return true;
                        };

                        var btnFldsFake = document.createElement('div');
                        if (btnFldsFake) {
                            btnFldsFake.className = 'filter-fields';
                            //btnFldsFake.style.background = 'blue';
                            btnFldsFake.style.top = (kTimeLineItemMargin - 4) + 'px';
                            header.appendChild(btnFldsFake);
                        }

                        this.pX = btnPrj.offsetLeft + btnPrj.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
                        this.pY = btnPrj.offsetTop + btnPrj.offsetHeight + this.timeline.ctx.canvas.offsetTop;
                        this.pfX = btnFlds.offsetLeft + btnFlds.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
                        this.pfY = btnFlds.offsetTop + btnFlds.offsetHeight + this.timeline.ctx.canvas.offsetTop;
                    }

                    var btnHiddenFlds = document.createElement('div');
                    if (btnHiddenFlds) {
                        btnHiddenFlds.id = 'gantt-filter-hidden-fields';
                        btnHiddenFlds.className = 'filter-hidden-fields';
                        btnHiddenFlds.style.top = (kTimeLineItemMargin - 4) + 'px';
                        btnHiddenFlds.style.display = 'none';
                        fakeAbsolute.appendChild(btnHiddenFlds);

                        btnHiddenFlds.onclick = function (e) {
                            inner.closeEditing();
                            if (inner.controller['onhiddenfieldsfilter']) {
                                inner.controller['onhiddenfieldsfilter'](e, false, inner.pfHX, inner.pfHY, inner.getHiddenRows(), inner.getVisibleRows());
                            }
                            return true;
                        };

                        this.pHX = btnPrj.offsetLeft + btnPrj.offsetWidth * 2 + this.timeline.ctx.canvas.offsetLeft;
                        this.pHY = btnPrj.offsetTop + btnPrj.offsetHeight + this.timeline.ctx.canvas.offsetTop;
                        this.pfHX = btnFlds.offsetLeft + btnFlds.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
                        this.pfHY = btnFlds.offsetTop + btnFlds.offsetHeight + this.timeline.ctx.canvas.offsetTop;
                    }
                }

                this.header = header;
                this.btnPrj = btnPrj;
                this.btnFlds = btnFlds;
                this.btnFldsFake = btnFldsFake;
                this.btnHiddenFlds = btnHiddenFlds;
            }
        },
        buildCollection: function () {
            var inner = this, clicks = 0, line = null;

            for (var i = 0; i < this.elementsCount; ++i) {
                line = document.createElement('div');
                if (line) {
                    line.style.top = ((kTimeLineItemMargin * i) | 0) + 'px';
                    line.style.height = (kTimeLineItemMargin - 1) + 'px';
                    line.className = 'line-element';

                    this.appendNameField(i, line);
                    this.appendCounterField(i, line);
                    this.appendResponsiblesField(i, line);
                    this.appendBeginDateField(i, line);
                    this.appendEndDateField(i, line);
                    this.appendStatusField(i, line);
                    this.appendPriorityField(i, line);
                    this.appendCollapseButtons(i, line);
                    this.appendDragElements(i, line);
                    this.elInner.appendChild(line);
                    this.collection.elements.push(line);

                    // handlers

                    line.onmouseover = (function (e) {
                        var index = i;

                        return function (e) {
                            return inner.hitLine(e, inner, index);
                        }
                    })(i);
                    line.onmouseout = (function (e) {
                        var index = i;

                        return function (e) {

                            // в режиме редактирование не отрабатываем эвент
                            if (inner.editBox.isEnable()) {
                                return true;
                            }

                            var targ = targetSystemEvent(e);
                            if (targ.className !== 'line-element') {
                                targ = targ.parentNode;
                            }

                            targ.style.background = '#F3F3F3';
                            inner.hit = undefined;

                            return true;
                        }
                    })(i);
                    line.onclick = function (e) {
                        // уже находимся в режиме редактирование
                        if (inner.editBox.isEnable()) {
                            inner.closeEditing();
                            return true;
                        }

                        return false;
                    };

                    line.onmouseup = (function (e) {
                        var index = i;
                        var self = line;

                        return function (e) {
                            if (2 === e.button) {
                                stopSystemEvent(e);

                                if (inner.timeline.editBox.isEnable()) {
                                    inner.timeline.editBox.cancel();
                                }

                                inner.closeEditing();

                                inner.select = {
                                    p: inner.collection.indexer[index].p,
                                    m: inner.collection.indexer[index].m,
                                    t: inner.collection.indexer[index].t,
                                    line: index
                                };

                                inner.controller.showElementMenu(e, inner.select, self);
                            }
                        }
                    }(i));
                }
            }
        },
        buildSplitter: function (parent) {
            var splitterFake = document.createElement('div');
            if (splitterFake) {
                splitterFake.className = 'gantt-splitter-fake';
                parent.appendChild(splitterFake);

                var splitter = document.createElement('div');
                if (splitter) {
                    splitter.className = 'gantt-splitter';
                    parent.appendChild(splitter);

                    var t = this.timeline,
                        inner = this,
                        inControl = false,
                        showCells = false,
                        updateCells = false,
                        anchor = 0,
                        mouseX = 0,
                        leftSide = 0,

                        spliterWideWidth = '584px',
                        spliterWidth = 16;  // px

                    splitter.onmousemove = function (e) {
                        inner.clearHighlightElement();
                    };
                    splitter.onmousedown = function (e) {
                        if (0 === inner.visibleRows.length) return;

                        mouseX = e.clientX;

                        inControl = true;
                        anchor = (mouseX - splitter.getBoundingClientRect().left);

                        stopSystemEvent(e);

                        if (0 === inner.visibleRows.length) { return; }

                        document.onmousemove = function (e) {
                            if (mouseX == e.clientX) return;
                            mouseX = e.clientX;

                            if (!showCells) {
                                inner.clearHighlightElement();

                                var left = inner.rows[0].minWidth;
                                for (var m = 0; m < inner.visibleRows.length; ++m) {
                                    inner.hideVisibleRow(inner.visibleRows[m].name, true);
                                    left += inner.visibleRows[m].width + inner.distanceBetweenCells;
                                }

                                inner.btnFldsFake.style.left = (left - 4) + 'px';
                                inner.btnHiddenFlds.style.display = 'none';
                                inner.rebuildContent(undefined);

                                showCells = true;
                            }

                            updateCells = true;

                            var value = Math.min(Math.max(mouseX - anchor - 8, inner.rows[0].minWidth), inner.maxWidth);

                            splitter.style.left = (value - 250 - 50) + 'px';
                            splitter.style.width = spliterWideWidth;
                            inner.el.style.width = (value) + 'px';
                            inner.header.style.width = (value) + 'px';
                            inner.width = value;

                            inner.btnFlds.style.display = 'none';
                            inner.clearHighlightElement();
                            t.needUpdate = true;
                        };

                        document.onmouseup = function (e) {
                            if (inControl) {
                                if (updateCells) {
                                    var left = inner.rows[0].minWidth;
                                    var value = Math.min(Math.max(e.clientX, left), inner.maxWidth);
                                    var width = left, old = left;
                                    var i = 0, j = 0, showAll = true;
                                    for (; i < inner.visibleRows.length; ++i) {
                                        old = width;
                                        width += inner.visibleRows[i].width + inner.distanceBetweenCells;

                                        if (value >= old && value < width) {
                                            value = old;
                                            showAll = false;
                                            break;
                                        }
                                        if (i + 1 === inner.visibleRows.length) {
                                            if (value > width - inner.distanceBetweenCells) {
                                                value = inner.maxWidth;
                                            }
                                        }
                                    }

                                    for (; j < inner.visibleRows.length; ++j) {
                                        inner.hideVisibleRow(inner.visibleRows[j].name, j < i);
                                    }

                                    if (!showAll) {
                                        value += inner.hiddenCellsCtr;
                                        inner.btnHiddenFlds.style.display = '';
                                        inner.btnHiddenFlds.style.left = (value - inner.btnFlds.offsetWidth - 59) + 'px';
                                    }

                                    splitter.style.left = (value - 250) + 'px';
                                    splitter.style.width = spliterWideWidth;
                                    inner.el.style.width = (value) + 'px';
                                    inner.header.style.width = (value) + 'px';

                                    inner.width = value;
                                    //inner.splitterLine.style.display = 'none';

                                    inner.btnFlds.style.left = (inner.width - inner.btnFlds.offsetWidth - 34) + 'px';
                                    inner.btnFldsFake.style.left = inner.btnFlds.style.left;

                                    inner.pfX = inner.btnFlds.offsetLeft + inner.btnFlds.offsetWidth + inner.timeline.ctx.canvas.offsetLeft;
                                    inner.pfY = inner.btnFlds.offsetTop + inner.btnFlds.offsetHeight + inner.timeline.ctx.canvas.offsetTop;

                                    inner.pfHX = inner.btnHiddenFlds.offsetLeft + inner.btnHiddenFlds.offsetWidth + inner.timeline.ctx.canvas.offsetLeft;
                                    inner.pfHY = inner.btnHiddenFlds.offsetTop + inner.btnHiddenFlds.offsetHeight + inner.timeline.ctx.canvas.offsetTop;

                                    t.needUpdate = true;

                                    var c = parseInt(splitter.style.left.replace('px', '')) + parseInt((splitter.style.width.replace('px', ''))) * 0.5;
                                    splitter.style.left = (value - spliterWidth) + 'px';
                                    splitter.style.width = spliterWidth + 'px';

                                    inner.rebuildContent(undefined);

                                    if (inner.controller['onhiddenfieldsfilter']) {
                                        inner.controller['onhiddenfieldsfilter'](e, true, 0, 0, inner.getHiddenRows(), inner.getVisibleRows());
                                    }

                                    inner.btnFlds.style.display = '';
                                    showCells = false;
                                    inControl = false;
                                }
                                updateCells = false;

                                document.onmousemove = function (e) {
                                    t.onmousemove(e);
                                };
                                document.onmouseup = function (e) {
                                    t.onmouseup(e);
                                };
                            }
                        };
                    };
                    splitter.onmouseup = function (e) {
                        if (inControl) {
                            if (updateCells) {
                                var left = inner.rows[0].minWidth;
                                var value = Math.min(Math.max(e.clientX - leftSide, left), inner.maxWidth);
                                var width = left, old = left;
                                var i = 0, j = 0, showAll = true;
                                for (; i < inner.visibleRows.length; ++i) {
                                    old = width;
                                    width += inner.visibleRows[i].width + inner.distanceBetweenCells;

                                    if (value >= old && value < width) {
                                        value = old;
                                        showAll = false;
                                        break;
                                    }

                                    if (i + 1 === inner.visibleRows.length) {
                                        if (value > width - inner.distanceBetweenCells) {
                                            value = inner.maxWidth;
                                        }
                                    }
                                }
                                for (; j < inner.visibleRows.length; ++j) {
                                    inner.hideVisibleRow(inner.visibleRows[j].name, j < i);
                                }

                                if (!showAll) {
                                    value += inner.hiddenCellsCtr;
                                    inner.btnHiddenFlds.style.display = '';
                                    inner.btnHiddenFlds.style.left = (value - inner.btnFlds.offsetWidth - 59) + 'px';
                                }

                                splitter.style.left = (value - 250) + 'px';
                                splitter.style.width = spliterWideWidth;
                                inner.el.style.width = (value) + 'px';
                                inner.header.style.width = (value) + 'px';

                                inner.width = value;

                                inner.btnFlds.style.left = (inner.width - inner.btnFlds.offsetWidth - 34) + 'px';
                                inner.btnFldsFake.style.left = (inner.width - inner.btnFlds.offsetWidth - 34) + 'px';

                                inner.pfX = inner.btnFlds.offsetLeft + inner.btnFlds.offsetWidth + inner.timeline.ctx.canvas.offsetLeft;
                                inner.pfY = inner.btnFlds.offsetTop + inner.btnFlds.offsetHeight + inner.timeline.ctx.canvas.offsetTop;

                                inner.pfHX = inner.btnHiddenFlds.offsetLeft + inner.btnHiddenFlds.offsetWidth + inner.timeline.ctx.canvas.offsetLeft;
                                inner.pfHY = inner.btnHiddenFlds.offsetTop + inner.btnHiddenFlds.offsetHeight + inner.timeline.ctx.canvas.offsetTop;

                                t.needUpdate = true;

                                var c = parseInt(splitter.style.left.replace('px', '')) + parseInt((splitter.style.width.replace('px', ''))) * 0.5;
                                splitter.style.left = (value - spliterWidth) + 'px';
                                splitter.style.width = spliterWidth + 'px';

                                inner.rebuildContent(undefined);

                                if (inner.controller['onhiddenfieldsfilter']) {
                                    inner.controller['onhiddenfieldsfilter'](e, true, 0, 0, inner.getHiddenRows(), inner.getVisibleRows());
                                }

                                inner.btnFlds.style.display = '';
                                showCells = false;
                                inControl = false;
                            }
                            updateCells = false;

                            document.onmousemove = function (e) {
                                t.onmousemove(e);
                            };
                            document.onmouseup = function (e) {
                                t.onmouseup(e);
                            };
                        }
                    };

                    this.splitter = splitter;
                    this.splitterFake = splitterFake;
                }
            }
        },

        appendNameField: function (i, parent) {
            var inner = this, clicks = 0;

            function DoEditTitle(index) {
                inner.closeEditing();

                inner.select = {
                    p: inner.collection.indexer[index].p,
                    m: inner.collection.indexer[index].m,
                    t: inner.collection.indexer[index].t,
                    line: index
                };

                inner.showEditBox(inner);
            }
            function DoCenteringElement(index) {
                inner.closeEditing();

                if (inner.collection.indexer[index].freeTasks)
                    return;

                inner.select = {
                    p: inner.collection.indexer[index].p,
                    m: inner.collection.indexer[index].m,
                    t: inner.collection.indexer[index].t,
                    line: index
                };

                inner.controller.elementCentering(inner.select);
            }

            function SingleClick(e, index) {
                return DoCenteringElement(index);
            }
            function DoubleClick(e, index) {
                return DoEditTitle(index);
            }

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column';
                parent.appendChild(element);

                element.ondblclick = (function (e) {
                    var index = i;
                    return function (e) {

                        if (undefined !== inner.collection.indexer[index].p &&
                            undefined === inner.collection.indexer[index].m &&
                            undefined === inner.collection.indexer[index].t) {

                            var project_id = inner.timeline.storage.p[inner.collection.indexer[index].p].id();
                            stopSystemEvent(e);

                            window.open('Tasks.aspx?prjID=' + project_id);

                            return true;
                        }

                        clicks = 2;
                        return DoubleClick.call(this, e, index);
                    }
                })(i);
                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {
                        var that = this;
                        setTimeout(function () {
                            var dblclick = clicks;
                            if (dblclick > 0) {
                                clicks = dblclick - 1;
                            } else {
                                SingleClick.call(that, e, index);
                            }
                        }, 300);
                    }
                }(i));
                element.onmousemove = (function (e) {
                    var index = i;
                    return function (e) {
                        return inner.hitLine(e, inner, index);
                    }
                })(i);
            }
        },
        appendCounterField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-counter';
                parent.appendChild(element);
            }
        },
        appendResponsiblesField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-resp';
                parent.appendChild(element);

                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {

                        stopSystemEvent(e);

                        if (inner.timeline.editBox.isEnable()) {
                            return;
                        }

                        inner.closeEditing();

                        inner.select = {
                            p: inner.collection.indexer[index].p,
                            m: inner.collection.indexer[index].m,
                            t: inner.collection.indexer[index].t,
                            line: index
                        };

                        inner.controller.changeResponsibles(e, inner.select, targetSystemEvent(e));
                    }
                }(i));
            }
        },
        appendBeginDateField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-begin-date';
                parent.appendChild(element);

                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {

                        stopSystemEvent(e);

                        if (inner.timeline.editBox.isEnable()) {
                            return;
                        }

                        inner.closeEditing();

                        inner.select = {
                            p: inner.collection.indexer[index].p,
                            m: inner.collection.indexer[index].m,
                            t: inner.collection.indexer[index].t,
                            line: index
                        };

                        inner.controller.changeTimes(e, inner.select, targetSystemEvent(e), false);
                    }
                }(i));
            }
        },
        appendEndDateField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-end-date';
                parent.appendChild(element);

                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {

                        stopSystemEvent(e);

                        if (inner.timeline.editBox.isEnable()) {
                            return;
                        }

                        inner.closeEditing();

                        inner.select = {
                            p: inner.collection.indexer[index].p,
                            m: inner.collection.indexer[index].m,
                            t: inner.collection.indexer[index].t,
                            line: index
                        };

                        inner.controller.changeTimes(e, inner.select, targetSystemEvent(e), true);
                    }
                }(i));
            }
        },
        appendStatusField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-status';
                parent.appendChild(element);

                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {

                        stopSystemEvent(e);

                        if (inner.timeline.editBox.isEnable()) {
                            return;
                        }

                        inner.closeEditing();

                        inner.select = {
                            p: inner.collection.indexer[index].p,
                            m: inner.collection.indexer[index].m,
                            t: inner.collection.indexer[index].t,
                            line: index
                        };

                        inner.controller.changeStatus(e, inner.select, targetSystemEvent(e));
                    }
                }(i));
            }
        },
        appendPriorityField: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('span');
            if (element) {
                element.className = 'name-column-priority';
                parent.appendChild(element);
            }
        },
        appendCollapseButtons: function (i, parent) {
            var inner = this;
            var clicks = 0;

            var element = document.createElement('div');
            if (element) {
                element.className = 'button-colapse-open';
                parent.appendChild(element);

                element.onclick = (function (e) {
                    var index = i;
                    return function (e) {

                        stopSystemEvent(e);

                        inner.closeEditing();

                        inner.select = {
                            p: inner.collection.indexer[index].p,
                            m: inner.collection.indexer[index].m,
                            t: inner.collection.indexer[index].t,
                            line: index,
                            freeTasks: inner.collection.indexer[index].freeTasks
                        };

                        inner.controller.collapseElement(e, inner.select);
                    }
                }(i));
            }
        },
        appendDragElements: function (i, parent) {
            var t = this.timeline,
             inner = this,
             clicks = 0,
             parentDom = parent,
             anchor = {},
             dragInd = {},
             dragTo = {},
             dragNode = undefined,
             fakeBkg = null;

            var element = document.createElement('div');
            if (element) {
                element.className = 'button-drag-drop';

                parent.appendChild(element);

                element.onmousedown = (function (e) {
                    var index = i;
                    var parentNode = parent;

                    return function (e) {

                        stopSystemEvent(e);

                        var offsets = parentNode.getBoundingClientRect();
                        anchor.X = e.clientX - offsets.left;
                        anchor.Y = e.clientY - offsets.top;

                        dragNode = parentNode.cloneNode(true);
                        if (dragNode) {

                            dragNode.className = 'element-drag-drop';
                            dragNode.style.background = '';
                            dragNode.style.zIndex = '1000';
                            dragNode.style.width = inner.rows[0].minWidth + 'px';
                            dragNode.style.left = -anchor.X + e.clientX + 'px';
                            dragNode.style.top = -anchor.Y + e.clientY + 'px';

                            for (var i = 1; i < dragNode.childNodes.length - 1; ++i) {
                                dragNode.childNodes[i].style.display = 'none';
                            }

                            if (document.body.appendChild(dragNode)) {

                                document.body.style.overflow = 'hidden';

                                fakeBkg = document.createElement('div');
                                if (fakeBkg) {
                                    fakeBkg.style.zIndex = '990';
                                    fakeBkg.style.position = 'absolute';
                                    fakeBkg.style.background = 'transparent';
                                    fakeBkg.style.left = '0';
                                    fakeBkg.style.top = '0';
                                    fakeBkg.style.width = '100%';
                                    fakeBkg.style.height = '100%';
                                    fakeBkg.style.cursor = 'pointer';

                                    document.body.appendChild(fakeBkg);
                                }

                                if ('onmousewheel' in document) {
                                    fakeBkg.onmousewheel = function (e) {
                                        t.onmousewheel(e);
                                        return true;
                                    };
                                    dragNode.onmousewheel = function (e) {
                                        t.onmousewheel(e);
                                        return true;
                                    };
                                } else {
                                    dragNode.addEventListener('MozMousePixelScroll', function (e) {
                                        t.onmousewheel(e);
                                        return true;
                                    }, false);

                                    fakeBkg.addEventListener('MozMousePixelScroll', function (e) {
                                        t.onmousewheel(e);
                                        return true;
                                    }, false);
                                }

                                if (undefined !== inner.dragIndex) {
                                    inner.collection.elements[inner.dragIndex].childNodes[8].style.display = 'none';
                                    inner.collection.elements[inner.hit.line].style.background = '#F3F3F3';
                                    inner.dragIndex = undefined;
                                    inner.clearHighlightElement();
                                }

                                // save
                                t.itemToDrop = {};
                                t.itemToDrop.p = undefined !== inner.collection.indexer[index].p ? inner.collection.indexer[index].p : -1;
                                t.itemToDrop.m = undefined !== inner.collection.indexer[index].m ? inner.collection.indexer[index].m : -1;
                                t.itemToDrop.t = undefined !== inner.collection.indexer[index].t ? inner.collection.indexer[index].t : -1;

                                inner.dragMode = true;
                                t.editMode = kEditModeElementDrag;

                                if (undefined === inner.collection.indexer[index].t) {
                                    t.viewController().collapseProjects(false);
                                    t.viewController().collapse(true);

                                    // Прямое обновление

                                    t.updateData();
                                    t.needUpdateContent = true;
                                    t.drawScene();
                                }

                                document.onmousemove = function (e) {
                                    t.mouse = t.windowToCanvas(e.clientX, e.clientY);
                                    t.mouse.baseEvent = e;

                                    dragNode.style.left = -anchor.X + e.clientX + 'px';
                                    dragNode.style.top = -anchor.Y + e.clientY + 'px';

                                    t.calculateLineHit(e);
                                    inner.highlightElement(t.hitLine.p, t.hitLine.m === -1 ? undefined : t.hitLine.m, t.hitLine.t + 1);
                                    inner.updateTimeLineHitLine();

                                    if (t.itemToDrop.p === t.hitLine.p) {
                                        dragNode.style.border = '2px solid #bdda7f';
                                    } else {
                                        dragNode.style.border = '2px solid #ff0000';
                                    }

                                    var project = t.storage.getProject(t.hitLine.p);
                                    if (project) {
                                        if (-1 !== t.hitLine.t) {
                                            if (t.hitLine.m === -1) {
                                                if (project.collapse) {
                                                    project.collapse = false;

                                                    t.updateData();
                                                    t.needUpdateContent = true;
                                                    t.drawScene();
                                                }

                                            } else {
                                                var ms = project.getMilestone(t.hitLine.m);
                                                if (ms.collapse) {
                                                    ms.setCollapse(false);

                                                    t.updateData();
                                                    t.needUpdateContent = true;
                                                    t.drawScene();
                                                }
                                            }
                                        }
                                    }
                                };
                                document.onmouseup = function (e) {
                                    inner.dragMode = undefined;
                                    t.editMode = kEditModeNoUse;

                                    var rect = parentNode.getBoundingClientRect();

                                    t.dragBound = {
                                        x: e.clientX, y: e.clientY - t.itemMargin * 2,
                                        width: 0, height: t.itemHeight, dy: 0, dx: 0
                                    };
                                    t.calculateLineHit(e);

                                    if (t.itemToDrop.p === t.hitLine.p) {
                                        t.moveElement(e, t.hitLine);
                                        t.updateData();
                                    }

                                    document.body.removeChild(fakeBkg);
                                    document.body.removeChild(dragNode);
                                    fakeBkg = null;

                                    document.body.style.overflow = '';

                                    // set default mouse handlers

                                    document.onmousemove = function (e) {
                                        t.onmousemove(e);
                                    };
                                    document.onmouseup = function (e) {
                                        t.onmouseup(e);
                                    };
                                };
                            }
                        }
                    }
                }(i));
            }
        },

        setDisableMode: function (flag) {
            this.disableMode = flag;

            if (this.splitter) {
                this.splitter.style.display = flag || this.visibleRows.length === 0 ? 'none' : '';
            }

            if (this.splitterFake) {
                this.splitterFake.style.display = flag || this.visibleRows.length === 0 ? 'none' : '';
            }

            if (this.btnFlds) {
                this.btnFlds.className = flag ? 'filter-fields-disable' : 'filter-fields';
            }

            if (flag) {
                this.addRowAvailable(undefined);
                this.showHiddenRows([]);
            }
        },

        // handlers helpers

        hitLine: function (e, t, index) {

            if (undefined !== t.dragIndex) {
                t.collection.elements[t.dragIndex].childNodes[8].style.display = 'none';
                t.dragIndex = undefined;
            }

            // в режиме редактирование не отрабатываем эвент

            if (t.editBox.isEnable()) {
                return true;
            }

            if (t.timeline.editBox.isEnable()) {
                t.highlightElement(t.timeline.editBox.p,
                        -1 === t.timeline.editBox.m ? undefined : t.timeline.editBox.m,
                        -1 === t.timeline.editBox.t ? undefined : t.timeline.editBox.t);

                return true;
            }

            var targ = targetSystemEvent(e);
            if (targ.className !== 'line-element') {
                targ = targ.parentNode;
            }

            if (t.collection.indexer[index].disabled) {
                //t.splitterLine.style.display = 'none';
                t.updateTimeLineHitLine();
                return true;
            }

            targ.style.background = '#E1E1E1';

            //t.splitterLine.style.top = targ.style.top;
            //t.splitterLine.style.display = '';

            t.hit = {
                p: t.collection.indexer[index].p,
                m: t.collection.indexer[index].m,
                t: t.collection.indexer[index].t,
                line: index
            };

            var project = t.timeline.storage.getProject(t.hit.p);
            if (project) {
                if (kOpenProject === project.status()) {

                    // task

                    if (undefined !== t.collection.indexer[index].t && undefined === t.dragMode) {
                        var task = t.timeline.storage.getTask(t.hit.p, t.hit.m, t.hit.t);
                        if (undefined !== task && kElementCompleted !== task._status) {
                            t.collection.elements[index].childNodes[8].style.display = '';
                            t.dragIndex = index;
                        }
                    }

                    //            if (undefined === t.collection.indexer[index].t && undefined === t.dragMode) {
                    //                var milestone = t.timeline.storage.getMilestone(t.hit.p, t.hit.m);
                    //                if (kElementCompleted - 1 !== milestone._status) {
                    //                    t.collection.elements[index].childNodes[8].style.display = '';
                    //                    t.dragIndex = index;
                    //                }
                    //            }
                }
            }

            t.updateTimeLineHitLine();

            return true;
        },

        rebuildContent: function (value) {

            // var start = performance.now();

            // TODO: двигать контент, без изменения содержимого
            if (this.OK) {
                this.elInner.style.top = -this.timeline.rightScroll.value() + 'px';
                return;
            }

            var inner = this,
                t = this.timeline;

            // localize

            var noneMilestones = window['Gantt']['Localize_strings']['taskWithoutMilestones'] || 'Task Without Milestones',
                highPriority = window['Gantt']['Localize_strings']['highPriority'] || 'high',
                openStatus = window['Gantt']['Localize_strings']['openStatus'] || 'open',
                closeStatus = window['Gantt']['Localize_strings']['closeStatus'] || 'close',
                addDate = window['Gantt']['Localize_strings']['addDate'] || 'add',

                i, j, k, mi, text, respCount, resp, selContext, curLineInd = 0, it, element = 0, color, task, domLine;

            //this.elInner.style.display = 'none';
            // this.editBox.element.style.display = 'none';

            for (i = this.elementsCount - 1; i >= 0; --i) {
                this.collection.elements[i].className = 'line-element';
                this.collection.elements[i].style.display = 'none';

                this.collection.elements[i].style.background = '#F3F3F3';

                this.collection.elements[i].childNodes[0].style.font = '';
                this.collection.elements[i].childNodes[0].style.color = '#333333';
                this.collection.elements[i].childNodes[1].style.color = '#333333';
                this.collection.elements[i].childNodes[2].style.color = '#333333';  // resp
                this.collection.elements[i].childNodes[3].style.color = '#333333';
                this.collection.elements[i].childNodes[4].style.color = '#333333';
                this.collection.elements[i].childNodes[5].style.color = '#333333';

                this.collection.elements[i].childNodes[0].className = 'name-column';
                this.collection.elements[i].childNodes[2].className = 'name-column-resp';
                this.collection.elements[i].childNodes[3].className = 'name-column-begin-date';
                this.collection.elements[i].childNodes[4].className = 'name-column-end-date';
                this.collection.elements[i].childNodes[5].className = 'name-column-status';
                this.collection.elements[i].childNodes[6].className = 'name-column-priority';

                this.collection.elements[i].childNodes[1].textContent = '';
                this.collection.elements[i].childNodes[2].textContent = '';
                this.collection.elements[i].childNodes[3].textContent = '';
                this.collection.elements[i].childNodes[4].textContent = '';
                this.collection.elements[i].childNodes[5].textContent = '';
                this.collection.elements[i].childNodes[6].textContent = '';

                this.collection.elements[i].childNodes[7].style.display = 'none';
                this.collection.elements[i].childNodes[8].style.display = 'none';
            }

            function afterElementComplete() {
                if (inner.timeline.editBox.isEnable()) {
                    inner.highlightElement();
                }
            }

            var editPosM = 15 + 10, editPosT = 24 + 10;
            // fix for mozilla browser
            if (undefined !== window['netscape'] || undefined !== window['clipboardData']) { --editPosT; --editPosM; }
            editPosM += 'px'; editPosT += 'px';

            var isTeamlabTime = ((undefined !== Teamlab) && (undefined !== Teamlab['getDisplayDate']));

            value = t.rightScroll.value();

            this.collection.indexer = [];

            this.setDisableMode(0 === t.storage.projects().length);

            var divLine22 = (t.rightScroll.value() + 0.5) % t.itemMargin,
                height22 = -divLine22 + 1,

                marginHalf = t.itemMargin * 0.5,
                margin = t.itemMargin,

                firstVisibleLine = floor2((value + 0.5) / margin),
                elements = Math.min((t.ctxHeight / margin) + 1, this.elementsCount - 1),

                divLine = floor2((t.rightScroll.value()) % margin + 0.5),
                height = t.timeScale.height() - divLine + margin * 0.7,
                height2 = t.timeScale.height() - divLine,

                respSrc = [],

                leftPosX = 0,
                posY = 0,

                lockProject = false,

                p = t.storage.projects(),
                pl = p.length,

                respPosX = this.rows[1].posX + 'px',
                beginDateX = this.rows[2].posX + 'px',
                endDateX = this.rows[3].posX + 'px',
                statusX = this.rows[4].posX + 'px',
                priorityX = this.rows[5].posX + 'px',

                showRowResp = !this.rows[1].isHidden,
                showRowBeg = !this.rows[2].isHidden,
                showRowEnd = !this.rows[3].isHidden,
                showRowStat = !this.rows[4].isHidden,
                showRowPrior = !this.rows[5].isHidden;

            for (j = 0; j < pl; ++j) {

                lockProject = p[j].isLocked() || p[j].isInReadMode();

                if (curLineInd >= firstVisibleLine) {

                    //

                    domLine = this.collection.elements[element];

                    // PROJECT

                    text = p[j]._title;
                    text = text.substring(0, 30);

                    domLine.style.top = ((element * kTimeLineItemMargin + height22) | 0) + 'px';

                    domLine.childNodes[0].textContent = text;
                    domLine.childNodes[0].title = p[j]._title;
                    domLine.childNodes[0].style.left = '16px';
                    domLine.childNodes[0].style.font = this.fontH1;

                    //domLine.childNodes[1].innerText = '';

                    if (showRowResp) {
                        domLine.childNodes[2].style.left = respPosX;
                        domLine.childNodes[2].textContent = milestoneResponsibles(p[j], this.noResponsible);
                        //domLine.childNodes[2].style.color = color;

                        domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                    }

                    //domLine.childNodes[3].innerText = '';
                    //domLine.childNodes[4].innerText = '';

                    // collapse button

                    if (p[j].fullCollapse)
                        domLine.childNodes[7].className = 'button-colapse-close';
                    else
                        domLine.childNodes[7].className = 'button-colapse-open';

                    domLine.childNodes[7].style.display = '';

                    if (lockProject) {
                        domLine.childNodes[0].style.color = kLockProjectColor;
                        domLine.childNodes[1].style.color = kLockProjectColor;
                        domLine.childNodes[2].style.color = kLockProjectColor;

                        domLine.childNodes[0].className = 'name-column gantt-cell-pointer';
                        domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                    }

                    if (p[j].isLocked()) {
                        domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                        domLine.childNodes[7].style.display = 'none';
                    }

                    if (p[j]._isprivate) {
                        domLine.className = 'line-element icon-private';
                    }

                    domLine.style.display = '';

                    // index

                    this.collection.indexer.push({ p: j, m: undefined, t: undefined, text: text, disabled: true });

                    ++element;

                    if (elements < element) {
                        afterElementComplete();
                        this.elInner.style.display = '';
                        return;
                    }
                }

                ++curLineInd;

                if (p[j].fullCollapse) { continue; }

                for (i = 0; i < p[j].m.length; ++i) {

                    if (p[j].m[i].filter) { continue; }

                    if (curLineInd >= firstVisibleLine) {

                        //

                        domLine = this.collection.elements[element];

                        // MILESTONE

                        text = p[j].m[i]._title;
                        text = text.substring(0, 30);

                        color = milestoneFill(p[j].m[i]);

                        domLine.style.top = ((element * kTimeLineItemMargin + height22) | 0) + 'px';

                        domLine.childNodes[0].textContent = text;
                        domLine.childNodes[0].title = p[j].m[i]._title;
                        domLine.childNodes[0].style.fontWeight = 'bold';
                        domLine.childNodes[0].style.left = '26px';
                        if (!lockProject) domLine.childNodes[0].style.color = color;

                        domLine.childNodes[1].textContent = numberOfValidTasks(p[j].m[i].t);
                        if (!lockProject) domLine.childNodes[1].style.color = color;

                        if (showRowResp) {
                            domLine.childNodes[2].style.left = respPosX;
                            domLine.childNodes[2].textContent = milestoneResponsibles(p[j].m[i], this.noResponsible);
                            if (!lockProject) domLine.childNodes[2].style.color = color;

                            if (kElementCompleted - 1 === p[j].m[i]._status) {
                                domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                            }
                        }

                        //domLine.childNodes[3].textContent = '';

                        if (showRowEnd) {
                            domLine.childNodes[4].textContent = formatTime(p[j].m[i]._endDate, isTeamlabTime);
                            domLine.childNodes[4].style.left = endDateX;
                            if (!lockProject) domLine.childNodes[4].style.color = color;

                            if (kElementCompleted - 1 === p[j].m[i]._status) {
                                domLine.childNodes[4].className = 'name-column-end-date gantt-cell-disable';
                            }
                        }

                        if (showRowStat) {
                            domLine.childNodes[5].style.left = statusX;
                            //domLine.childNodes[5].style.color = color;

                            // status label

                            if (kElementCompleted - 1 === p[j].m[i]._status) {
                                domLine.childNodes[5].textContent = closeStatus;
                            } else {
                                domLine.childNodes[5].textContent = openStatus;
                            }
                        }

                        // collapse button

                        if (p[j].m[i].t.length) {

                            if (p[j].m[i].collapse)
                                domLine.childNodes[7].className = 'button-colapse-close';
                            else
                                domLine.childNodes[7].className = 'button-colapse-open';

                            domLine.childNodes[7].style.display = '';
                        }

                        if (lockProject) {
                            domLine.childNodes[0].style.color = kLockProjectColor;
                            domLine.childNodes[1].style.color = kLockProjectColor;
                            domLine.childNodes[2].style.color = kLockProjectColor;
                            domLine.childNodes[3].style.color = kLockProjectColor;
                            domLine.childNodes[4].style.color = kLockProjectColor;
                            domLine.childNodes[5].style.color = kLockProjectColor;
                            domLine.childNodes[6].style.color = kLockProjectColor;
                            domLine.childNodes[7].style.color = kLockProjectColor;

                            domLine.childNodes[0].className = 'name-column gantt-cell-pointer';
                            domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                            domLine.childNodes[3].className = 'name-column-begin-date gantt-cell-disable';
                            domLine.childNodes[5].className = 'name-column-status gantt-cell-disable';
                            domLine.childNodes[6].className = 'name-column-priority gantt-cell-disable';
                        }

                        domLine.style.display = '';

                        this.collection.indexer.push({ p: j, m: i, t: undefined, text: text });

                        if (this.editBox.isEnable() && this.editBox.isValidAnchor(j, i, undefined)) {
                            this.editBox.element.style.left = editPosM;
                            this.editBox.element.style.top = (((element * kTimeLineItemMargin + height22) | 0) + 3) + 'px';
                            this.editBox.element.className = 'milestone-edit';
                            this.editBox.element.style.display = '';

                            domLine.style.background = '#E1E1E1';
                        }
                        else if (this.hit && this.hit.p === j && this.hit.m === i && this.hit.t === undefined) {
                            domLine.style.background = '#E1E1E1';
                        }

                        ++element;
                        if (elements < element) {
                            afterElementComplete();
                            this.elInner.style.display = '';
                            //console.log('Execution time: ' + (performance.now() - start));
                            return;
                        }
                    }

                    ++curLineInd;

                    if (!t.fullscreen && p[j].m[i].collapse) { continue; }

                    for (it = 0; it < p[j].m[i].t.length; ++it) {

                        if (p[j].m[i].t[it].filter) { continue; }

                        if (curLineInd >= firstVisibleLine) {

                            //

                            domLine = this.collection.elements[element];

                            // TASK IN MILESTONE

                            task = p[j].m[i].t[it];
                            text = task._title;
                            text = text.substring(0, 30);

                            color = taskFill(task, i);

                            domLine.style.top = ((element * kTimeLineItemMargin + height22) | 0) + 'px';

                            domLine.childNodes[0].textContent = text;
                            domLine.childNodes[0].title = task._title;
                            domLine.childNodes[0].style.left = '35px';
                            domLine.childNodes[0].style.fontWeight = '';
                            if (!lockProject) domLine.childNodes[0].style.color = color;

                            //domLine.childNodes[1].textContent = '';

                            if (showRowResp) {
                                domLine.childNodes[2].style.left = respPosX;
                                domLine.childNodes[2].textContent = taskResponsibles(task, this.noResponsible);
                                if (!lockProject) domLine.childNodes[2].style.color = color;

                                if (kElementCompleted === task._status) {
                                    domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                                }
                            }

                            if (showRowBeg) {
                                domLine.childNodes[3].textContent = task.beginFail ? addDate : formatTime(task.beginDate(), isTeamlabTime);
                                domLine.childNodes[3].style.left = beginDateX;
                                if (!lockProject) domLine.childNodes[3].style.color = color;

                                if (kElementCompleted === task._status) {
                                    domLine.childNodes[3].className = 'name-column-begin-date gantt-cell-disable';
                                }
                            }

                            if (showRowEnd) {
                                domLine.childNodes[4].textContent = task.endFail ? addDate : formatTime(task.endDate(), isTeamlabTime);
                                domLine.childNodes[4].style.left = endDateX;
                                if (!lockProject) domLine.childNodes[4].style.color = color;

                                if (kElementCompleted === task._status) {
                                    domLine.childNodes[4].className = 'name-column-end-date gantt-cell-disable';
                                }
                            }

                            if (showRowStat) {
                                domLine.childNodes[5].style.left = statusX;
                                // domLine.childNodes[5].style.color = color;

                                var cs = ASC.Projects.Master.customStatuses.find(function (item) {
                                    return task._customTaskStatus == item.id;
                                });

                                if (!cs) {
                                    cs = ASC.Projects.Master.customStatuses.find(function (item) {
                                        return task._status == item.statusType && item.isDefault;
                                    });
                                }

                                domLine.childNodes[5].textContent = cs.title;
                            }

                            if (task._priority && showRowPrior) {
                                domLine.childNodes[6].textContent = highPriority;
                                domLine.childNodes[6].style.left = priorityX;
                                if (!lockProject) domLine.childNodes[6].style.color = color;
                            }

                            if (lockProject) {
                                domLine.childNodes[0].style.color = kLockProjectColor;
                                domLine.childNodes[1].style.color = kLockProjectColor;
                                domLine.childNodes[2].style.color = kLockProjectColor;
                                domLine.childNodes[3].style.color = kLockProjectColor;
                                domLine.childNodes[4].style.color = kLockProjectColor;
                                domLine.childNodes[5].style.color = kLockProjectColor;
                                domLine.childNodes[6].style.color = kLockProjectColor;
                                domLine.childNodes[7].style.color = kLockProjectColor;

                                domLine.childNodes[0].className = 'name-column gantt-cell-pointer';
                                if (showRowResp) domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                                if (showRowEnd) domLine.childNodes[3].className = 'name-column-begin-date gantt-cell-disable';
                                if (showRowEnd) domLine.childNodes[4].className = 'name-column-end-date gantt-cell-disable';
                                if (showRowStat) domLine.childNodes[5].className = 'name-column-status gantt-cell-disable';
                                domLine.childNodes[6].className = 'name-column-priority gantt-cell-disable';
                            }

                            domLine.style.display = '';

                            this.collection.indexer.push({ p: j, m: i, t: it, text: text });

                            if (this.editBox.isEnable() && this.editBox.isValidAnchor(j, i, it)) {
                                this.editBox.element.style.left = editPosT;
                                this.editBox.element.style.top = (((element * kTimeLineItemMargin + height22) | 0) + 3) + 'px';
                                this.editBox.element.className = 'task-edit';
                                this.editBox.element.style.display = '';

                                domLine.style.background = '#E1E1E1';
                            }
                            else if (this.hit && this.hit.p === j && this.hit.m === i && this.hit.t === it) {
                                domLine.style.background = '#E1E1E1';
                            }

                            ++element;
                            if (elements < element) {
                                afterElementComplete();
                                this.elInner.style.display = '';
                                // console.log('Execution time: ' + (performance.now() - start));
                                return;
                            }
                        }

                        ++curLineInd;
                    }
                }

                // FREE TASKS LABEL

                if (curLineInd >= firstVisibleLine) {

                    //

                    domLine = this.collection.elements[element];

                    domLine.style.top = ((element * kTimeLineItemMargin + height22) | 0) + 'px';

                    domLine.childNodes[0].textContent = noneMilestones;
                    domLine.childNodes[0].title = '';
                    domLine.childNodes[0].style.fontWeight = 'bold';
                    domLine.childNodes[0].style.left = '26px';

                    domLine.childNodes[1].textContent = numberOfValidTasks(p[j].t);
                    //domLine.childNodes[2].textContent = '';
                    //domLine.childNodes[3].textContent = '';
                    //domLine.childNodes[4].textContent = '';

                    domLine.childNodes[7].style.display = '';

                    // collapse button

                    if (p[j].t.length) {

                        if (p[j].collapse)
                            domLine.childNodes[7].className = 'button-colapse-close';
                        else
                            domLine.childNodes[7].className = 'button-colapse-open';

                        domLine.childNodes[7].style.display = '';
                    } else {
                        domLine.childNodes[7].style.display = 'none';
                    }

                    if (lockProject) {
                        domLine.childNodes[0].style.color = kLockProjectColor;
                        domLine.childNodes[1].style.color = kLockProjectColor;
                        domLine.childNodes[0].className = 'name-column gantt-cell-disable';
                    } else {
                        domLine.childNodes[0].className = 'name-column gantt-cell-disable';
                    }

                    domLine.style.display = '';

                    this.collection.indexer.push({ p: j, m: undefined, t: undefined, text: text, disabled: true, freeTasks: true });

                    ++element;
                    if (elements < element) {
                        afterElementComplete();
                        this.elInner.style.display = '';
                        return;
                    }

                    ++curLineInd;
                }

                ++curLineInd;

                if (p[j].collapse) { continue; }

                // TASKS IN FREE ZONE

                for (it = 0; it < p[j].t.length; ++it) {

                    if (p[j].t[it].filter) { continue; }

                    if (curLineInd >= firstVisibleLine) {

                        //

                        domLine = this.collection.elements[element];

                        task = p[j].t[it];

                        text = task._title;
                        text = text.substring(0, 30);

                        color = taskFill(task, -1);

                        domLine.style.top = ((element * kTimeLineItemMargin + height22) | 0) + 'px';

                        domLine.childNodes[0].textContent = text;
                        domLine.childNodes[0].title = task._title;
                        domLine.childNodes[0].style.left = '35px';
                        domLine.childNodes[0].style.fontWeight = '';
                        if (!lockProject) domLine.childNodes[0].style.color = color;

                        //domLine.childNodes[1].textContent = '';

                        if (showRowResp) {
                            domLine.childNodes[2].style.left = respPosX;
                            domLine.childNodes[2].textContent = taskResponsibles(task, this.noResponsible);
                            if (!lockProject) domLine.childNodes[2].style.color = color;

                            if (kElementCompleted === task._status) {
                                domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                            }
                        }

                        if (showRowBeg) {
                            domLine.childNodes[3].textContent = task.beginFail ? addDate : formatTime(task.beginDate(), isTeamlabTime);
                            domLine.childNodes[3].style.left = beginDateX;
                            if (!lockProject) domLine.childNodes[3].style.color = color;

                            if (kElementCompleted === task._status) {
                                domLine.childNodes[3].className = 'name-column-begin-date gantt-cell-disable';
                            }
                        }

                        if (showRowEnd) {
                            domLine.childNodes[4].textContent = task.endFail ? addDate : formatTime(task.endDate(), isTeamlabTime);
                            domLine.childNodes[4].style.left = endDateX;
                            if (!lockProject) domLine.childNodes[4].style.color = color;

                            if (kElementCompleted === task._status) {
                                domLine.childNodes[4].className = 'name-column-end-date gantt-cell-disable';
                            }
                        }

                        if (showRowStat) {
                            domLine.childNodes[5].style.left = statusX;
                            //domLine.childNodes[5].style.color = color;

                            var cs = findCustomStatus(function (item) {
                                return task._customTaskStatus == item.id;
                            });

                            if (!cs) {
                                cs = findCustomStatus(function (item) {
                                    return task._status == item.statusType && item.isDefault;
                                });
                            }

                            domLine.childNodes[5].textContent = cs.title;
                        }

                        if (task._priority && showRowPrior) {
                            domLine.childNodes[6].textContent = highPriority;
                            domLine.childNodes[6].style.left = priorityX;
                            domLine.childNodes[6].style.color = color;
                        }

                        if (lockProject) {
                            domLine.childNodes[0].style.color = kLockProjectColor;
                            domLine.childNodes[1].style.color = kLockProjectColor;
                            domLine.childNodes[2].style.color = kLockProjectColor;
                            domLine.childNodes[3].style.color = kLockProjectColor;
                            domLine.childNodes[4].style.color = kLockProjectColor;
                            domLine.childNodes[5].style.color = kLockProjectColor;

                            domLine.childNodes[0].className = 'name-column gantt-cell-pointer';
                            if (showRowResp) domLine.childNodes[2].className = 'name-column-resp gantt-cell-disable';
                            if (showRowBeg) domLine.childNodes[3].className = 'name-column-begin-date gantt-cell-disable';
                            if (showRowEnd) domLine.childNodes[4].className = 'name-column-end-date gantt-cell-disable';
                            if (showRowStat) domLine.childNodes[5].className = 'name-column-status gantt-cell-disable';
                            domLine.childNodes[6].className = 'name-column-priority gantt-cell-disable';
                        }

                        domLine.style.display = '';

                        // index

                        this.collection.indexer.push({ p: j, m: undefined, t: it, text: text });

                        if (this.editBox.isEnable() && this.editBox.isValidAnchor(j, undefined, it)) {
                            this.editBox.element.style.left = editPosT;
                            this.editBox.element.style.top = (((element * kTimeLineItemMargin + height22) | 0) + 3) + 'px';
                            this.editBox.element.className = 'task-edit';
                            this.editBox.element.style.display = '';

                            domLine.style.background = '#E1E1E1';
                        }
                        else if (this.hit && this.hit.p === j && this.hit.m === undefined && this.hit.t === it) {
                            domLine.style.background = '#E1E1E1';
                        }

                        ++element;
                        if (elements < element) {
                            afterElementComplete();
                            this.elInner.style.display = '';
                            return;
                        }
                    }

                    ++curLineInd;
                }
            }

            afterElementComplete();

            // this.elInner.style.display = '';
        },
        getWidth: function () {
            return this.width;
        },

        // available rows

        addRowAvailable: function (row) {
            var width = 0, i = 0;
            for (i = 0; i < this.visibleRows.length; ++i)
                width += this.visibleRows[i].width + this.distanceBetweenCells;

            if (undefined === row) {
                this.visibleRows = [];

                for (i = 1; i < this.rows.length; ++i) {
                    this.rows[i].posX = -1000;
                    this.rows[i].el.style.left = this.rows[i].posX + 'px';
                    this.hideVisibleRow(i, true);
                }

                this.btnHiddenFlds.style.display = 'none';
            }
            else if (row === 'Responsibility') {
                this.visibleRows.push({ width: this.rows[1].minWidth, name: 'Responsibility' });
                this.rows[1].posX = this.rows[0].minWidth + width;
                this.rows[1].translate = window['Gantt']['Localize_strings']['responsibles2'] || 'Responsible(-s)';
                this.rows[1].el.style.left = (this.rows[1].posX + this.paddingLeftCellContent) + 'px';
                this.rows[1].el.style.width = this.rows[1].minWidth + 'px';
                this.rows[1].el.childNodes[0].textContent = this.rows[1].translate;
            }
            else if (row === 'BeginDate') {
                this.visibleRows.push({ width: this.rows[2].minWidth, name: 'BeginDate' });
                this.rows[2].posX = this.rows[0].minWidth + width;
                this.rows[2].translate = window['Gantt']['Localize_strings']['beginDate'] || 'Begin Date';
                this.rows[2].el.style.left = (this.rows[2].posX + this.paddingLeftCellContent) + 'px';
                this.rows[2].el.style.width = this.rows[2].minWidth + 'px';
                this.rows[2].el.childNodes[0].textContent = this.rows[2].translate;
            }
            else if (row === 'EndDate') {
                this.visibleRows.push({ width: this.rows[3].minWidth, name: 'EndDate' });
                this.rows[3].posX = this.rows[0].minWidth + width;
                this.rows[3].translate = window['Gantt']['Localize_strings']['endDate'] || 'End Date';
                this.rows[3].el.style.left = (this.rows[3].posX + this.paddingLeftCellContent) + 'px';
                this.rows[3].el.style.width = this.rows[3].minWidth + 'px';
                this.rows[3].el.childNodes[0].textContent = this.rows[3].translate;
            }
            else if (row === 'Status') {
                this.visibleRows.push({ width: this.rows[4].minWidth, name: 'Status' });
                this.rows[4].posX = this.rows[0].minWidth + width;
                this.rows[4].translate = window['Gantt']['Localize_strings']['status'] || 'Status';
                this.rows[4].el.style.left = (this.rows[4].posX + this.paddingLeftCellContent) + 'px';
                this.rows[4].el.style.width = this.rows[4].minWidth + 'px';
                this.rows[4].el.childNodes[0].textContent = this.rows[4].translate;
            }
            else if (row === 'Priority') {
                this.visibleRows.push({ width: this.rows[5].minWidth, name: 'Priority' });
                this.rows[5].posX = this.rows[0].minWidth + width;
                this.rows[5].translate = window['Gantt']['Localize_strings']['priotity'] || 'Priotity';
                this.rows[5].el.style.left = (this.rows[5].posX + this.paddingLeftCellContent) + 'px';
                this.rows[5].el.style.width = this.rows[5].minWidth + 'px';
                this.rows[5].el.childNodes[0].textContent = this.rows[5].translate;
            }

            width = 0;
            for (i = 0; i < this.visibleRows.length; ++i)
                width += this.visibleRows[i].width + this.distanceBetweenCells;

            this.width = this.rows[0].minWidth + width + this.btnFlds.offsetWidth + this.distanceBetweenCells;
            this.maxWidth = this.width;

            this.el.style.width = this.width + 'px';
            this.header.style.width = this.width + 'px';
            this.btnFlds.style.left = (this.width - this.btnFlds.offsetWidth - 14) + 'px';
            this.btnFldsFake.style.left = this.btnFlds.style.left;

            this.pfX = this.btnFlds.offsetLeft + this.btnFlds.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
            this.pfY = this.btnFlds.offsetTop + this.btnFlds.offsetHeight + this.timeline.ctx.canvas.offsetTop;

            this.splitter.style.left = (this.width - 16) + 'px';
        },
        deleteAvailableRow: function (row) {
            var cells = [], i = 0, j = 0;
            cells.push(undefined);

            for (; i < this.visibleRows.length; ++i) {
                if (row !== this.visibleRows[i].name) {
                    cells.push(this.visibleRows[i].name);
                }
            }

            for (; j < cells.length; ++j) {
                this.addRowAvailable(cells[j]);
            }

            this.rebuildContent(this.timeline.rightScroll.value());
            this.timeline.needUpdate = true;
        },
        getAvailableRows: function () {
            var i = 0, cells = [];

            for (; i < this.visibleRows.length; ++i) {
                cells.push(this.visibleRows[i].name);
            }

            return cells;
        },

        // hidden rows

        showHiddenRows: function (rows) {
            var j = 0, t = this;

            function getFieldByName(name) {
                for (var k = t.rows.length - 1; k >= 0; --k) {
                    if (t.rows[k].name === name)
                        return t.rows[k];
                }

                return null;
            }

            if (rows) {
                if (rows.length !== this.visibleRows.length) {
                    for (j = 0; j < this.visibleRows.length; ++j) {
                        this.hideVisibleRow(this.visibleRows[j].name, false);
                    }
                }

                for (j = 0; j < rows.length; ++j) {
                    this.hideVisibleRow(rows[j], true);
                }

                var width = 0, i = 0;
                for (i = 0; i < this.visibleRows.length; ++i) {
                    var field = getFieldByName(this.visibleRows[i].name);
                    if (field && !field.isHidden) {
                        width += this.visibleRows[i].width + this.distanceBetweenCells;
                    }
                }

                if (rows.length !== this.visibleRows.length) {
                    width += 21 + 1;
                    this.btnHiddenFlds.style.display = '';
                    this.btnHiddenFlds.style.left = (this.rows[0].minWidth + width - this.btnFlds.offsetWidth - 39 + 21 + 6 + 3) + 'px';
                }

                this.width = this.rows[0].minWidth + width + this.btnFlds.offsetWidth + this.distanceBetweenCells;
                //this.maxWidth = this.width;

                this.el.style.width = this.width + 'px';
                this.header.style.width = this.width + 'px';
                this.btnFlds.style.left = (this.width - this.btnFlds.offsetWidth - 14) + 'px';
                this.btnFldsFake.style.left = this.btnFlds.style.left;

                this.pfX = this.btnFlds.offsetLeft + this.btnFlds.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
                this.pfY = this.btnFlds.offsetTop + this.btnFlds.offsetHeight + this.timeline.ctx.canvas.offsetTop;

                this.pfHX = this.btnHiddenFlds.offsetLeft + this.btnHiddenFlds.offsetWidth + this.timeline.ctx.canvas.offsetLeft;
                this.pfHY = this.btnHiddenFlds.offsetTop + this.btnHiddenFlds.offsetHeight + this.timeline.ctx.canvas.offsetTop;

                this.splitter.style.left = (this.width - 16) + 'px';
            }
        },
        hideVisibleRow: function (rowName, isShow) {
            if (rowName === 'Responsibility' || 1 === rowName) {
                this.rows[1].el.style.display = isShow ? '' : 'none';
                this.rows[1].isHidden = !isShow;
            }
            else if (rowName === 'BeginDate' || 2 === rowName) {
                this.rows[2].el.style.display = isShow ? '' : 'none';
                this.rows[2].isHidden = !isShow;
            }
            else if (rowName === 'EndDate' || 3 === rowName) {
                this.rows[3].el.style.display = isShow ? '' : 'none';
                this.rows[3].isHidden = !isShow;
            }
            else if (rowName === 'Status' || 4 === rowName) {
                this.rows[4].el.style.display = isShow ? '' : 'none';
                this.rows[4].isHidden = !isShow;
            }
            else if (rowName === 'Priority' || 5 === rowName) {
                this.rows[5].el.style.display = isShow ? '' : 'none';
                this.rows[5].isHidden = !isShow;
            }
        },
        getHiddenRows: function () {
            var field = null, i = 0, cells = [], t = this;

            function getFieldByName(name) {
                for (var k = t.rows.length - 1; k >= 0; --k) {
                    if (t.rows[k].name === name)
                        return t.rows[k];
                }

                return null;
            }

            for (; i < this.visibleRows.length; ++i) {
                field = getFieldByName(this.visibleRows[i].name);
                if (field && field.isHidden) {
                    cells.push(this.visibleRows[i].name);
                }
            }

            return cells;
        },
        getVisibleRows: function (existName) {
            var cells = [];

            var t = this;

            function getFieldByName(name) {
                for (var k = t.rows.length - 1; k >= 0; --k) {
                    if (t.rows[k].name === name)
                        return t.rows[k];
                }

                return null;
            }

            for (var i = 0; i < this.visibleRows.length; ++i) {
                var field = getFieldByName(this.visibleRows[i].name);
                if (field && !field.isHidden) {
                    if (existName === this.visibleRows[i].name) continue;
                    cells.push(this.visibleRows[i].name);
                }
            }

            return cells;
        },

        getRowWithName: function (name) {
            for (var k = this.rows.length - 1; k >= 0; --k) {
                if (this.rows[k].name === name)
                    return this.rows[k];
            }

            return null;
        },

        showEditBox: function (context) {
            //this.OK = true;

            var t = context.timeline;
            if (t.readMode) {
                return false;
            }

            var select = deepCopy(context.select);

            t.editBox.cancel();

            if (undefined !== context.select.p) {
                if (kOpenProject !== t.storage.getProject(context.select.p).status())
                    return false;
            }

            context.select = deepCopy(select);

            var indexer = null, element = null, addX = 0;

            if (undefined !== context.select.t) {
                element = t.storage.getTask(context.select.p, context.select.m, context.select.t);
                if (element) {
                    if (kElementCompleted !== element._status) {

                        this.editBox.setBoldText(false);

                        indexer = t.storage.taskIds(context.select.p, context.select.m, context.select.t);

                        // undo

                        t._undoManager.add(kOperationChangeTitleTask,
                            {
                                p: context.select.p, m: context.select.m, t: element, index: context.select.t,
                                taskId: indexer.t, milestoneId: indexer.m, projectId: indexer.p,
                                silentUpdateMode: true
                            });

                    } else {
                        element = null;
                        this.select = undefined;
                    }
                }
            } else {
                element = t.storage.getMilestone(context.select.p, context.select.m);
                if (element) {
                    if (kElementCompleted - 1 !== element._status) {
                        context.editBox.setBoldText(true);

                        addX = 2;

                        indexer = t.storage.milestoneIds(context.select.p, context.select.m);

                        // undo

                        t._undoManager.add(kOperationChangeTitleMilestone,
                            {
                                p: this.select.p, m: context.select.m, t: element, index: context.select.t,
                                taskId: indexer.t, milestoneId: indexer.m, projectId: indexer.p,
                                silentUpdateMode: true
                            });
                    } else {
                        element = null;
                        this.select = undefined;
                    }
                }
            }

            if (element) {

                context.editBox.element.style.width = '175px';
                context.editBox.element.style.height = (t.itemMargin * 0.6) + 'px';

                context.editBox.setReference(element, this.select.p, this.select.m, this.select.t);
                context.editBox.setValue(element._title);

                t.hitTask = (undefined === context.editBox.anchor.t) ? -1 : context.editBox.anchor.t;
                t.hitMilestone = (undefined === context.editBox.anchor.m) ? -1 : context.editBox.anchor.m;
                t.hitProject = context.editBox.anchor.p;
                t.needUpdate = true;

                context.editBox.show(true);

                context.rebuildContent(undefined);

                return true;
            }

            return false;
        },
        closeEditing: function (forse, clearHit) {
            this.editBox.close();

            if (this.select && this.select.line) {
                this.collection.elements[this.select.line].style.background = '#F3F3F3';
                if (clearHit) return;
            }

            this.select = undefined;
            this.hit = undefined;

            if (forse) {
                this.rebuildContent(undefined);
            }
        },

        updateTimeLineHitLine: function () {

            if (this.hit && this.hit.line) {
                if (this.timeline.editBox.isEnable()) {

                } else {

                    this.timeline.hitTask = (undefined === this.hit.t) ? -1 : this.hit.t;
                    this.timeline.hitMilestone = (undefined === this.hit.m) ? -1 : this.hit.m;
                    this.timeline.hitProject = this.hit.p;
                }
                this.timeline.needUpdate = true;
            } else {
                this.timeline.hitTask = -1;
                this.timeline.hitMilestone = -1;
                this.timeline.hitProject = -1;

                this.timeline.needUpdate = true;
            }
        },
        highlightElement: function (p, m, t) {
            if (this.hit) {
                if (this.hit.line) {
                    this.collection.elements[this.hit.line].style.background = '#F3F3F3';
                    this.collection.elements[this.hit.line].childNodes[8].style.display = 'none';
                    //this.splitterLine.style.display = 'none';
                    this.hit = undefined;

                    if (undefined !== this.dragIndex) {
                        this.collection.elements[this.dragIndex].childNodes[8].style.display = 'none';
                        this.dragIndex = undefined;
                    }
                }
            }

            if (this.timeline.editBox.isEnable()) {
                p = this.timeline.editBox.p;
                m = -1 === this.timeline.editBox.m ? undefined : this.timeline.editBox.m;
                t = -1 === this.timeline.editBox.t ? undefined : this.timeline.editBox.t;
            }

            for (var i = this.collection.indexer.length - 1; i >= 0; --i) {
                if (this.collection.indexer[i].p === p &&
                    this.collection.indexer[i].m === m &&
                    this.collection.indexer[i].t === t) {

                    this.hit = { p: p, m: m, t: t, line: i };
                    this.collection.elements[i].style.background = '#E1E1E1';

                    if (undefined === this.dragMode && undefined !== this.hit.t) {
                        var task = this.timeline.storage.getTask(this.hit.p, this.hit.m, this.hit.t);
                        if (kElementCompleted !== task._status)
                            this.collection.elements[i].childNodes[8].style.display = '';
                    }
                    //this.splitterLine.style.top = this.collection.elements[i].style.top;

                    if (undefined !== this.dragIndex) {
                        this.collection.elements[this.dragIndex].childNodes[8].style.display = 'none';
                        this.dragIndex = undefined;
                    }

                    return;
                }
            }

            //this.splitterLine.style.display = 'none';
        },
        clearHighlightElement: function () {
            if (-1 !== this.timeline.hitTask || -1 !== this.timeline.hitMilestone || -1 !== this.timeline.hitProject) {
                this.timeline.hitTask = -1;
                this.timeline.hitMilestone = -1;
                this.timeline.hitProject = -1;
                //this.splitterLine.style.display = 'none';
                this.timeline.needUpdate = true;
            }
            if (undefined !== this.dragIndex) {
                this.collection.elements[this.dragIndex].childNodes[8].style.display = 'none';
                this.dragIndex = undefined;
            }
        }
    };

    function LeftPanelController(timeline, model, baseElement) {
        this.timeline = timeline;
        this.model = model;
        this.baseElement = baseElement;
    }
    LeftPanelController.prototype = {

        // internal

        model: undefined,
        panel: undefined,

        // handlers

        'onfieldsfilter': null,
        'onhiddenfieldsfilter': null,
        'onchangetimes': null,

        // public

        init: function () {
            this.panel = new LeftPanel(this.timeline, this.baseElement, this.model);
            if (this.panel) {
                this.panel.init(this);
            }
        },
        scrollContent: function (value) {
            if (this.panel) {
                this.panel.rebuildContent(value);
            }
        },
        rebuildContent: function () {
            if (this.panel) {
                this.panel.rebuildContent(undefined);
            }
        },
        addRowsAvailable: function (rows) {

            this.panel.addRowAvailable(undefined);

            if (rows) {
                for (var i = 0; i < rows.length; ++i) {
                    this.panel.addRowAvailable(rows[i]);
                }
            }

            this.panel.rebuildContent(this.timeline.rightScroll.value());
            this.timeline.needUpdate = true;
        },
        showHiddenRows: function (rows) {
            this.panel.showHiddenRows(rows);
            this.panel.rebuildContent(this.timeline.rightScroll.value());
            this.timeline.needUpdate = true;
        },

        getPanel: function () {
            return this.panel;
        },
        clearFocus: function (forse, clearHit) {
            this.panel.closeEditing(forse, clearHit);
        },
        updateFocus: function (p, m, t) {
            this.panel.highlightElement(p, m, t);
        },
        clearHighlight: function () {
            if (undefined !== this.panel.dragIndex) {
                this.panel.collection.elements[this.panel.dragIndex].childNodes[8].style.display = 'none';
                this.panel.dragIndex = undefined;
            }
        },

        // commands

        elementCentering: function (select) {

            this.panel.closeEditing();

            // milestone begin centering

            if (undefined === select.t && undefined !== select.m && undefined !== select.p) {

                var milestone = this.timeline.storage.getMilestone(select.p, select.m);
                this.timeline.animator.moveCenterToX(milestone.endTime);
            }

            // task in milestone centering

            if (undefined !== select.t && undefined !== select.m && undefined !== select.p) {

                var task = this.timeline.storage.getTask(select.p, select.m, select.t);
                this.timeline.animator.moveCenterToX(task.beginTime);
            }

            // free task centering

            if (undefined !== select.t && undefined === select.m && undefined !== select.p) {

                var taskFree = this.timeline.storage.getTask(select.p, undefined, select.t);
                this.timeline.animator.moveCenterToX(taskFree.beginTime);
            }

            // project centering

            if (undefined === select.t && undefined === select.m && undefined !== select.p) {
                var i, j, t = this.timeline;

                var project = this.timeline.storage.getProject(select.p);

                for (i = 0; i < project.m.length; ++i) {
                    for (j = 0; j < project.m[i].t.length; ++j) {
                        if (kElementCompleted != project.m[i].t[j].status()) {
                            project.setFullCollapse(false);
                            project.m[i].setCollapse(false);

                            // сначала отрисуем все объекты с изменениям схлопывания, а затем выполним центрирование

                            t.updateData();
                            t.needUpdateContent = true;
                            t.drawScene();

                            t.viewController().centeringElement(project.m[i].t[j].id(), false, function () { t.updateContent() });

                            return;
                        }
                    }
                }

                for (j = 0; j < project.t.length; ++j) {
                    if (kElementCompleted != project.t[j].status()) {
                        project.setCollapse(false);
                        project.setFullCollapse(false);

                        // сначала отрисуем все объекты с изменениям схлопывания, а затем выполним центрирование

                        t.updateData();
                        t.needUpdateContent = true;
                        t.drawScene();

                        t.viewController().centeringElement(project.t[j].id(), false, function () { t.updateContent() });

                        return;
                    }
                }
            }
        },
        changeResponsibles: function (e, select, domElement) {
            if (undefined !== select.p) {
                if (kOpenProject !== this.timeline.storage.getProject(select.p).status())
                    return false;
            }

            this.panel.closeEditing();

            var task = null,
                milestone = null,
                 offsets = domElement.getBoundingClientRect(),
                coords = { left: offsets.left, top: offsets.top + offsets.height * 1.2 },
                handler = this.timeline.handlers[kHanderShowRespPopUpMenuWindow];

            if (!handler) {
                return false;
            }

            if (undefined === select.t && undefined !== select.m && undefined !== select.p) {
                milestone = this.timeline.storage.getMilestone(select.p, select.m);
                if (kElementCompleted - 1 === this.timeline.storage.p[select.p].m[select.m].status())
                    return false;

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: undefined,
                    status: this.timeline.storage.p[select.p].m[select.m].status(), ref: this.timeline.storage.p[select.p].m[select.m],
                    ids: this.timeline.storage.milestoneIds(select.p, select.m)
                };

                stopSystemEvent(e);
                handler(milestone, coords, true);
                return true;
            }

            if (undefined !== select.t && undefined !== select.m && undefined !== select.p) {
                task = this.timeline.storage.getTask(select.p, select.m, select.t);
                if (kElementCompleted === task.status())
                    return false;

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: select.t,
                    status: task.status(), ref: task, isTask: true,
                    ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                };

                handler(task, coords, true);
                return true;
            }

            if (undefined !== select.t && undefined === select.m && undefined !== select.p) {
                task = this.timeline.storage.getTask(select.p, undefined, select.t);
                if (kElementCompleted === task.status())
                    return false;

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: select.t,
                    status: task.status(), ref: task, isTask: true,
                    ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                };

                handler(task, coords, true);
                return true;
            }

            return false;
        },
        changeStatus: function (e, select, domElement) {
            this.panel.closeEditing();

            if (undefined !== select.p) {
                if (kOpenProject !== this.timeline.storage.getProject(select.p).status())
                    return false;
            }

            var task = null,
                milestone = null,
                handler = this.timeline.handlers[kHanderShowTaskPopUpWindow],
                offsets = domElement.getBoundingClientRect(),
                coords = { left: offsets.left, top: offsets.top + offsets.height * 1.2 };

            if (!handler) {
                return false;
            }

            if (undefined === select.t && undefined !== select.m && undefined !== select.p) {
                milestone = this.timeline.storage.getMilestone(select.p, select.m);

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: undefined,
                    status: this.timeline.storage.p[select.p].m[select.m].status(), ref: this.timeline.storage.p[select.p].m[select.m],
                    ids: this.timeline.storage.milestoneIds(select.p, select.m)
                };

                handler(milestone, coords, true);
                return true;
            }

            if (undefined !== select.t && undefined !== select.m && undefined !== select.p) {
                task = this.timeline.storage.getTask(select.p, select.m, select.t);
                task.project = this.timeline.storage.getProject(select.p);

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: select.t,
                    status: task.status(), ref: task, isTask: true,
                    ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                };

                handler = this.timeline.handlers[kHanderShowTaskPopUpCustomWindow];
                handler(task, coords, true);
                delete task.project;
                return true;
            }

            if (undefined !== select.t && undefined === select.m && undefined !== select.p) {
                task = this.timeline.storage.getTask(select.p, undefined, select.t);
                task.project = this.timeline.storage.getProject(select.p);

                this.timeline._modelController.statusElement = {
                    p: select.p, m: select.m, t: select.t,
                    status: task.status(), ref: task, isTask: true,
                    ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                };

                handler = this.timeline.handlers[kHanderShowTaskPopUpCustomWindow];
                handler(task, coords, true);
                delete task.project;
                return true;
            }

            return false;
        },
        changeTimes: function (e, select, domElement, direction) {
            if (this['onchangetimes']) {
                this.panel.closeEditing();

                if (undefined !== select.p) {
                    if (kOpenProject !== this.timeline.storage.getProject(select.p).status())
                        return false;
                }

                var task = null,
                    milestone = null,
                    handler = null,
                    offsets = domElement.getBoundingClientRect(),
                    coords = { left: offsets.left, top: offsets.top + offsets.height * 1.2 };

                // milestone 

                if (undefined === select.t && undefined !== select.m && undefined !== select.p) {
                    milestone = this.timeline.storage.getMilestone(select.p, select.m);
                    if (kElementCompleted - 1 === this.timeline.storage.p[select.p].m[select.m].status())
                        return false;

                    this.timeline._modelController.statusElement = {
                        p: select.p, m: select.m, t: undefined,
                        status: this.timeline.storage.p[select.p].m[select.m].status(), ref: this.timeline.storage.p[select.p].m[select.m],
                        ids: this.timeline.storage.milestoneIds(select.p, select.m)
                    };

                    this['onchangetimes'](coords, milestone, false);

                    return true;
                }

                // task in milestone 

                if (undefined !== select.t && undefined !== select.m && undefined !== select.p) {
                    task = this.timeline.storage.getTask(select.p, select.m, select.t);
                    if (kElementCompleted === task.status())
                        return false;

                    this.timeline._modelController.statusElement = {
                        p: select.p, m: select.m, t: select.t,
                        status: task.status(), ref: task, isTask: true,
                        ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                    };

                    handler = this.timeline.handlers[kHanderShowTaskPopUpWindow];
                    if (handler) {
                        this['onchangetimes'](coords, task, direction);
                        return true;
                    }
                }

                // free task 

                if (undefined !== select.t && undefined === select.m && undefined !== select.p) {
                    task = this.timeline.storage.getTask(select.p, undefined, select.t);
                    if (kElementCompleted === task.status())
                        return false;

                    this.timeline._modelController.statusElement = {
                        p: select.p, m: select.m, t: select.t,
                        status: task.status(), ref: task, isTask: true,
                        ids: this.timeline.storage.taskIds(select.p, select.m, select.t)
                    };

                    this['onchangetimes'](coords, task, direction);

                    return true;
                }
            }

            return false;
        },
        collapseElement: function (e, select) {
            if (this.timeline.editBox.isEnable())
                return false;

            var milestone = null,
                project = null;

            if (undefined === select.t && undefined !== select.m && undefined !== select.p) {
                milestone = this.timeline.storage.getMilestone(select.p, select.m);

                if (e.ctrlKey || e.metaKey) {
                    this.timeline.viewController().collapse(!milestone.collapse);
                } else {
                    milestone.setCollapse(!milestone.collapse);
                }

                this.timeline.rightScroll.save(true);

                this.timeline.needUpdateContent = true;
                this.timeline.updateData();

                return true;
            }

            if (undefined !== select.p && undefined !== select.freeTasks) {

                project = this.timeline.storage.getProject(select.p);

                if (e.ctrlKey || e.metaKey) {
                    this.timeline.viewController().collapse(!project.collapse);
                } else {
                    project.setCollapse(!project.collapse);
                }

                this.timeline.rightScroll.save(true);

                this.timeline.needUpdateContent = true;
                this.timeline.updateData();

                return true;
            }

            if (undefined !== select.p && undefined === select.freeTasks && undefined === select.t && undefined === select.m) {

                project = this.timeline.storage.getProject(select.p);

                if (e.ctrlKey || e.metaKey) {
                    this.timeline.viewController().collapseProjects(!project.fullCollapse);
                } else {
                    project.setFullCollapse(!project.fullCollapse);
                }

                this.timeline.rightScroll.save(true);

                this.timeline.needUpdateContent = true;
                this.timeline.updateData();

                return true;
            }

            return false;
        },
        showElementMenu: function (e, select, domElement) {
            if (kOpenProject !== this.timeline.storage.getProject(select.p).status() && domElement) {
                return false;
            }

            var task = null,
                milestone = null,
                handler = this.timeline.handlers[kHanderShowEditPopUpMenuWindow],
                offsets = domElement.getBoundingClientRect(),
                coords = { left: e.clientX - this.timeline.ctx.canvas.offsetLeft, top: offsets.top + offsets.height * 1.0 };

            if (!handler) {
                return false;
            }

            this.panel.closeEditing();

            // milestone 

            if (undefined === select.t && undefined !== select.m && undefined !== select.p) {

                milestone = this.timeline.storage.getMilestone(select.p, select.m);

                if (this.timeline.handlers[kHanderShowEditPopUpMenuWindow] && !this.timeline.readMode) {
                    this.timeline.modelController().statusElement = {
                        p: select.p, m: select.m, t: undefined,
                        ids: this.timeline.storage.milestoneIds(select.p, select.m),
                        isTask: false, milestone: milestone, ref: milestone
                    };

                    handler(coords, milestone, false, this.timeline.storage.getProject(select.p).id());
                    return true;
                }
            }

            // task in milestone 

            if (undefined !== select.t && undefined !== select.m && undefined !== select.p) {

                task = this.timeline.storage.getTask(select.p, select.m, select.t);

                if (this.timeline.handlers[kHanderShowEditPopUpMenuWindow] && !this.timeline.readMode) {
                    this.timeline.modelController().statusElement = {
                        p: select.p, m: select.m, t: select.t,
                        ids: this.timeline.storage.taskIds(select.p, select.m, select.t),
                        isTask: true, task: task, ref: task
                    };

                    handler(coords, task, true, this.timeline.storage.getProject(select.p).id());
                    return true;
                }
            }

            // free task 

            if (undefined !== select.t && undefined === select.m && undefined !== select.p) {

                task = this.timeline.storage.getTask(select.p, undefined, select.t);

                if (this.timeline.handlers[kHanderShowEditPopUpMenuWindow] && !this.timeline.readMode) {
                    this.timeline.modelController().statusElement = {
                        p: select.p, m: undefined, t: select.t,
                        ids: this.timeline.storage.taskIds(select.p, select.m, select.t),
                        isTask: true, task: task, ref: task
                    };

                    handler(coords, task, true, this.timeline.storage.getProject(select.p).id());
                    return true;
                }
            }

            return false;
        }
    };

    function UndoManager(timeline) {
        this.init(timeline);
    }
    UndoManager.prototype = {
        init: function (timeline) {
            this.timeline = timeline;
            this.op = [];
            this.ind = -1;
            this.tempOperation = null;
        },
        reset: function () {
            this.front = null;
            this.op = [];
            this.ind = -1;

            this._updateDebug();
            this._updateUI();
        },

        currentOperation: function () {
            if (!this.op || !this.op.length)
                return null;

            return this.op[this.ind];
        },

        add: function (id, arg) {
            if (kOperationDummy === id && this.op.length) {
                if (this.op.last().id === kOperationDummy) {
                    this._updateDebug();
                    return;
                }
            }

            ++this.ind;
            this._sliceTop();

            this.op.push({ id: id, arg: deepCopy(arg) });

            if (this.op.length < 2)
                this._updateUI();

            this._updateDebug();
        },
        addTempOperation: function (id, arg) {

            // храним undo-операцию отдельно и в случае надобности добавим в стек undo

            this.tempOperation = { id: id, arg: deepCopy(arg) };
        },
        deleteTempOperation: function () {

            this.tempOperation = null;
        },
        applyTempOperation: function () {

            // добавим временную операцию в стек undo

            if (this.tempOperation) {
                this.add(this.tempOperation.id, this.tempOperation.arg);
                this.tempOperation = null;
            }
        },

        performTop: function (validate) {          // notifiy
            if (-1 === this.ind || !this.timeline || !this.op.length)
                return;

            this.perform(this.ind, undefined, validate);

            this._updateDebug();
            this._updateUI();
        },
        updateOperation: function (validate, ref, items) {
            if (-1 === this.ind || !this.timeline || !this.op.length)
                return;

            if (ref && this.ind < this.op.length && this.ind >= 0) {
                var cur = this.op[this.ind];
                switch (cur.id) {
                    case kOperationAddTask:
                        if (!ref._isMilestone) {
                            cur.arg.oldTitle = cur.arg.t._title;
                            cur.arg.curTitle = ref._title;

                            cur.arg.t.setTimes(ref.beginTime, ref.endTime);
                            cur.arg.t._title = ref._title;
                        }
                        break;

                    case kOperationAddMilestone:
                        if (ref._isMilestone) {
                            cur.arg.oldTitle = cur.arg.t._title;
                            cur.arg.curTitle = ref._title;
                            cur.arg.t._title = ref._title;
                        }
                        break;

                    case kOperationChangeTitleMilestone:
                        {
                            cur.arg.oldTitle = cur.arg.t._title;
                            cur.arg.curTitle = ref._title;
                        }
                        break;

                    case kOperationChangeTitleTask:
                        {
                            cur.arg.oldTitle = cur.arg.t._title;
                            cur.arg.curTitle = ref._title;
                        }
                        break;

                    case kOperationChangeTimeTask:
                        {

                        }
                        break;
                }
            }

            this.perform(this.ind, undefined, validate, ref, items);

            this._updateDebug();
            this._updateUI();
        },
        perform: function (i, reverse, validate, ref, items) {          // notifiy
            if (i > this.op.length - 1 || i < 0)
                return;

            var taskTreeChangeTimes = null;
            var title, status, beginTime, endTime, child, parent, isQueryChange = false;
            var deadline = false;
            var oneDay = false;
            var beginFail = false;
            var storage = this.timeline.storage;
            var cur = this.op[i];
            switch (cur.id) {
                case kOperationAddTask:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerDeleteTask])
                            this.timeline.handlers[kHandlerDeleteTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t, cur.arg.linksToRemove);
                    } else {
                        if (this.timeline.handlers[kHandlerAddTask])
                            this.timeline.handlers[kHandlerAddTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t, cur.arg.linksToRemove);
                    }
                    break;

                case kOperationDeleteTask:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerAddTask])
                            this.timeline.handlers[kHandlerAddTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t, cur.arg.linksToRemove);
                    } else {
                        if (this.timeline.handlers[kHandlerDeleteTask])
                            this.timeline.handlers[kHandlerDeleteTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t, cur.arg.linksToRemove);
                    }
                    break;

                case kOperationAddMilestone:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerDeleteMilestone])
                            this.timeline.handlers[kHandlerDeleteMilestone](cur.arg.projectId, cur.arg.milestoneId, cur.arg.t);
                    } else {
                        if (this.timeline.handlers[kHandlerAddMilestone])
                            this.timeline.handlers[kHandlerAddMilestone](cur.arg.projectId, cur.arg.milestoneId, cur.arg.t);
                    }
                    break;

                case kOperationDeleteMilestone:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerAddMilestone])
                            this.timeline.handlers[kHandlerAddMilestone](cur.arg.projectId, cur.arg.milestoneId, cur.arg.t);
                    } else {
                        if (this.timeline.handlers[kHandlerDeleteMilestone])
                            this.timeline.handlers[kHandlerDeleteMilestone](cur.arg.projectId, cur.arg.milestoneId, cur.arg.t);
                    }
                    break;

                case kOperationChangeTitleTask:
                    if (!ref) {
                        if (reverse) {
                            title = cur.arg.t._title;
                            cur.arg.t._title = cur.arg.oldTitle;
                        } else {
                            title = cur.arg.t._title;
                            cur.arg.t._title = cur.arg.curTitle;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeTitleTask])
                        this.timeline.handlers[kHandlerChangeTitleTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t);

                    if (!ref) {
                        if (reverse) {
                            cur.arg.t._title = title;
                        } else {
                            cur.arg.t._title = title;
                        }
                    }
                    break;

                case kOperationChangeTitleMilestone:
                    if (!ref) {
                        if (reverse) {
                            title = cur.arg.t._title;
                            cur.arg.t._title = cur.arg.oldTitle;
                        } else {
                            title = cur.arg.t._title;
                            cur.arg.t._title = cur.arg.curTitle;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeTitleMilestone])
                        this.timeline.handlers[kHandlerChangeTitleMilestone](cur.arg.projectId, cur.arg.milestoneId, ref ? ref : cur.arg.t);

                    if (!ref) {
                        if (reverse) {
                            cur.arg.t._title = title;
                        } else {
                            cur.arg.t._title = title;
                        }
                    }
                    break;

                case kOperationChangeTimeTask:

                    if (items) {

                        taskTreeChangeTimes = [];

                        if (items.left) {
                            items.left.walkElements(ref ? ref : cur.arg.t, function (task, node) {
                                if (node) {
                                    if (node['change']) {
                                        if (true === node['change']) {
                                            taskTreeChangeTimes.push(node);
                                            isQueryChange = true;
                                        }
                                    }
                                }
                            });
                        }
                        if (items.right) {
                            items.right.walkElements(ref ? ref : cur.arg.t, function (task, node) {
                                if (node) {
                                    if (node['change']) {
                                        if (true === node['change']) {
                                            taskTreeChangeTimes.push(node);
                                            isQueryChange = true;
                                        }
                                    }
                                }
                            });
                        }
                    } else {
                        items = cur.arg.queryMoveLinks;
                        if (items) {
                            if (items.length) {
                                taskTreeChangeTimes = [];
                                for (i = 0; i < items.length; ++i) {
                                    taskTreeChangeTimes.push(storage.getTask(items[i].p, items[i].m, items[i].t));
                                }
                            }
                        }
                    }

                    // если связные задачи изменились нет смысла дальше проводить валидацию

                    if (validate && !isQueryChange) {
                        beginTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginTime;
                        endTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endTime;

                        if (beginTime === cur.arg.t.beginTime && endTime === cur.arg.t.endTime) {
                            this.flushPop(0);
                            //console.log('kOperationChangeTimeTask');
                            return;
                        }
                    }

                    if (!ref) {
                        if (reverse) {
                            deadline = cur.arg.t.endFail;
                            oneDay = cur.arg.t.oneDay;
                            beginFail = cur.arg.t.beginFail;
                            cur.arg.t.endFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail;
                            cur.arg.t.oneDay = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay;
                            cur.arg.t.beginFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail;
                        } else {
                            deadline = cur.arg.t.endFail;
                            cur.arg.t.endFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail;

                            oneDay = cur.arg.t.oneDay;
                            beginFail = cur.arg.t.beginFail;

                            beginTime = cur.arg.t.beginTime;
                            endTime = cur.arg.t.endTime;

                            cur.arg.t.setTimes(
                                storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginTime,
                                storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endTime);

                            cur.arg.t.oneDay = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay;
                            cur.arg.t.beginFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeTime])
                        this.timeline.handlers[kHandlerChangeTime](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t, taskTreeChangeTimes);

                    if (!ref) {
                        if (reverse) {
                            cur.arg.t.endFail = deadline;
                            cur.arg.t.oneDay = oneDay;
                            cur.arg.t.beginFail = beginFail;
                        } else {
                            cur.arg.t.endFail = deadline;
                            cur.arg.t.setTimes(beginTime, endTime);
                            cur.arg.t.oneDay = oneDay;
                            cur.arg.t.beginFail = beginFail;
                        }
                    }
                    break;

                case kOperationChangeTimeMilestone:
                    if (validate) { // check change time only milestone
                        beginTime = storage.getMilestone(cur.arg.p, cur.arg.m).beginTime;
                        endTime = storage.getMilestone(cur.arg.p, cur.arg.m).endTime;

                        if (beginTime === cur.arg.t.beginTime && endTime === cur.arg.t.endTime) {
                            this.flushPop(0);
                            //console.log('kOperationChangeTimeMilestone');
                            return;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeTime])
                        this.timeline.handlers[kHandlerChangeTime](cur.arg.projectId, cur.arg.milestoneId, undefined, ref ? ref : cur.arg.t);
                    break;

                case kOperationMoveTask:
                    if (this.timeline.handlers[kHandlerMoveTask])
                        this.timeline.handlers[kHandlerMoveTask](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId,
                            cur.arg.projectToId, cur.arg.milestoneToId, ref ? ref : cur.arg.t, cur.arg.linksToRemove, reverse);
                    break;

                case kOperationMoveGroupTasks:
                    if (this.timeline.handlers[kHandlerMoveGroupTasks])
                        this.timeline.handlers[kHandlerMoveGroupTasks]((reverse) ? cur.arg.sourceMilestone : cur.arg.destMilestone, cur.arg.tasksIds);
                    break;

                case kOperationChangeTaskStatus:
                    if (!ref) {
                        if (reverse) {
                            status = cur.arg.t._status;
                            cur.arg.t._status = cur.arg.oldStatus;
                        } else {
                            status = cur.arg.t._status;
                            cur.arg.t._status = cur.arg.newStatus;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeTaskStatus])
                        this.timeline.handlers[kHandlerChangeTaskStatus](cur.arg.projectId, cur.arg.milestoneId, cur.arg.taskId, ref ? ref : cur.arg.t);

                    if (!ref) {
                        if (reverse) {
                            cur.arg.t._status = status;
                        } else {
                            cur.arg.t._status = status;
                        }
                    }

                    break;

                case kOperationChangeMilestoneStatus:
                    if (!ref) {
                        if (reverse) {
                            status = cur.arg.t._status;
                            cur.arg.t._status = cur.arg.oldStatus;
                        } else {
                            status = cur.arg.t._status;
                            cur.arg.t._status = cur.arg.newStatus;
                        }
                    }

                    if (this.timeline.handlers[kHandlerChangeMilestoneStatus])
                        this.timeline.handlers[kHandlerChangeMilestoneStatus](cur.arg.projectId, cur.arg.milestoneId, ref ? ref : cur.arg.t);

                    if (!ref) {
                        if (reverse) {
                            cur.arg.t._status = status;
                        } else {
                            cur.arg.t._status = status;
                        }
                    }

                    break;

                case kOperationAddTaskLink:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerDeleteTaskLink]) {
                            if (ref) {
                                this.timeline.handlers[kHandlerDeleteTaskLink](ref);
                            } else {
                                parent = cur.arg.link.parentTaskId || cur.arg.link['parentTaskId'];          // minimizator fix
                                child = cur.arg.link.dependenceTaskId || cur.arg.link['dependenceTaskId'];  // minimizator fix

                                this.timeline.handlers[kHandlerDeleteTaskLink](cur.arg.link, storage.taskWithId(child).ref, storage.taskWithId(parent).ref);
                            }
                        }
                    } else {
                        if (this.timeline.handlers[kHandlerAddTaskLink]) {
                            if (ref) {
                                this.timeline.handlers[kHandlerAddTaskLink](ref);
                            } else {
                                parent = cur.arg.link.parentTaskId || cur.arg.link['parentTaskId'];          // minimizator fix
                                child = cur.arg.link.dependenceTaskId || cur.arg.link['dependenceTaskId'];  // minimizator fix

                                this.timeline.handlers[kHandlerAddTaskLink](cur.arg.link, storage.taskWithId(child).ref, storage.taskWithId(parent).ref);
                            }
                        }
                    }
                    break;

                case kOperationDeleteTaskLink:
                    if (reverse) {
                        if (this.timeline.handlers[kHandlerAddTaskLink]) {
                            if (ref) {
                                this.timeline.handlers[kHandlerAddTaskLink](ref);
                            } else {
                                parent = cur.arg.link.parentTaskId || cur.arg.link['parentTaskId'];          // minimizator fix
                                child = cur.arg.link.dependenceTaskId || cur.arg.link['dependenceTaskId'];  // minimizator fix

                                this.timeline.handlers[kHandlerAddTaskLink](cur.arg.link, storage.taskWithId(child).ref, storage.taskWithId(parent).ref);
                            }
                        }
                    } else {
                        if (this.timeline.handlers[kHandlerDeleteTaskLink]) {
                            if (ref) {
                                this.timeline.handlers[kHandlerDeleteTaskLink](ref);
                            } else {
                                parent = cur.arg.link.parentTaskId || cur.arg.link['parentTaskId'];          // minimizator fix
                                child = cur.arg.link.dependenceTaskId || cur.arg.link['dependenceTaskId'];  // minimizator fix

                                this.timeline.handlers[kHandlerDeleteTaskLink](cur.arg.link, storage.taskWithId(child).ref, storage.taskWithId(parent).ref);
                            }
                        }
                    }
                    break;

                case kOperationChangeResponsible:
                    if (validate) {

                    } else {
                        if (this.timeline.handlers[kHandlerChangeResponsible])
                            this.timeline.handlers[kHandlerChangeResponsible](ref);
                    }
                    break;
            }

            this._updateDebug();
            this._updateUI();
        },
        flushPop: function () {
            if (-1 === this.ind || !this.timeline || !this.op.length)
                return;

            --this.ind;
            this.op.splice(this.op.length - 1, 1);

            this._updateDebug();
            this._updateUI();
        },
        flushTopDummy: function () {
            if (this.op.length) {
                if (this.op[this.op.length - 1].id === kOperationDummy) {
                    this.flushPop(0);
                    this._updateDebug();

                    return true;
                }
            }

            return false;
        },

        undo: function () {
            if (this.timeline.editBox.enable) { this.timeline.editBox.cancel(true) }
            this.timeline.leftPanelController().clearFocus();

            if (-1 === this.ind || !this.timeline || !this.op.length) {
                this._updateUI();
                return;
            }

            var storage = this.timeline.storage;
            var cur = this.op[this.ind];
            var status, task, i, link, length, milestoneRef;
            var taskTreeChangeTimes = [], items = null, reference;

            switch (cur.id) {
                case kOperationAddTask:
                    this.timeline.rightScroll.save();
                    storage.projects()[cur.arg.p].removeTask(cur.arg.index, cur.arg.m);
                    break;

                case kOperationDeleteTask:
                    this.timeline.rightScroll.save();
                    storage.addTaskWithIndex(cur.arg.t, cur.arg.index);

                    // восстановим связи для которых текущая задача главная ( из нее идет связь )

                    length = cur.arg.linksToRemove.length;

                    if (undefined !== cur.arg.m) {
                        for (i = 0; i < length; ++i) {
                            link = cur.arg.linksToRemove[i];

                            if (link.link.dependenceTaskId !== cur.arg.taskId) {
                                task = storage.p[cur.arg.p].m[cur.arg.m].t[link.ind];

                                task.links.splice(link.index, 1, link.link);
                            }
                        }
                    } else {
                        for (i = 0; i < length; ++i) {
                            link = cur.arg.linksToRemove[i];

                            if (link.link.dependenceTaskId !== cur.arg.taskId) {
                                task = storage.p[cur.arg.p].t[link.ind];

                                task.links.splice(link.index, 1, link.link);
                            }
                        }
                    }

                    break;

                case kOperationChangeTitleTask:
                    title = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._title;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._title = cur.arg.t._title;
                    cur.arg.t._title = title;
                    break;

                case kOperationAddMilestone:
                    this.timeline.rightScroll.save();
                    storage.p[cur.arg.p].removeMilestone(cur.arg.index);
                    break;

                case kOperationDeleteMilestone:
                    this.timeline.rightScroll.save();
                    storage.p[cur.arg.p].t.splice(storage.p[cur.arg.p].t.length - cur.arg.t.t.length, cur.arg.t.t.length);
                    storage.addMilestoneWithIndex(cur.arg.t, cur.arg.index);
                    break;

                case kOperationChangeTitleMilestone:
                    var title = storage.getMilestone(cur.arg.p, cur.arg.m)._title;
                    storage.getMilestone(cur.arg.p, cur.arg.m)._title = cur.arg.t._title;
                    cur.arg.t._title = title;
                    break;

                case kOperationChangeTimeTask:
                    {
                        var beginTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginTime;
                        var endTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endTime;
                        var endFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail;
                        var oneDay = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay;
                        var beginFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail;

                        storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).setTimes(cur.arg.t.beginTime, cur.arg.t.endTime);
                        storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail = cur.arg.t.endFail;
                        storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay = cur.arg.t.oneDay;
                        storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail = cur.arg.t.beginFail;

                        cur.arg.t.beginTime = beginTime;
                        cur.arg.t.endTime = endTime;
                        cur.arg.t.endFail = endFail;
                        cur.arg.t.oneDay = oneDay;
                        cur.arg.t.beginFail = beginFail;

                        //  {p: p, m: m, t: j, ref: tasks[j], id: tasks[j]._id};

                        items = cur.arg.queryMoveLinks;
                        if (items) {
                            for (i = 0; i < items.length; ++i) {
                                task = items[i];

                                beginTime = storage.getTask(task.p, task.m, task.t).beginTime;
                                endTime = storage.getTask(task.p, task.m, task.t).endTime;
                                endFail = storage.getTask(task.p, task.m, task.t).endFail;
                                oneDay = storage.getTask(task.p, task.m, task.t).oneDay;
                                beginFail = storage.getTask(task.p, task.m, task.t).beginFail;

                                storage.getTask(task.p, task.m, task.t).setTimes(task.ref.beginTime, task.ref.endTime);
                                storage.getTask(task.p, task.m, task.t).endFail = task.ref.endFail;
                                storage.getTask(task.p, task.m, task.t).oneDay = task.ref.oneDay;
                                storage.getTask(task.p, task.m, task.t).beginFail = task.ref.beginFail;

                                task.ref.beginTime = beginTime;
                                task.ref.endTime = endTime;
                                task.ref.endFail = endFail;
                                task.ref.oneDay = oneDay;
                                task.ref.beginFail = beginFail;
                            }
                        }

                        if (undefined === cur.arg.m) { storage.p[cur.arg.p]._calcTimes(); }
                    }
                    break;

                case kOperationChangeTimeMilestone:
                    var save = deepCopy(storage.getMilestone(cur.arg.p, cur.arg.m));
                    storage.p[cur.arg.p].removeMilestone(cur.arg.index);
                    storage.addMilestoneWithIndex(cur.arg.t, cur.arg.index);
                    cur.arg.t = save;
                    break;

                case kOperationMoveTask:
                    storage.projects()[cur.arg.toProject].removeTask(cur.arg.place, cur.arg.toMilestone);
                    var copy_m = deepCopy(cur.arg.t);
                    if (undefined == cur.arg.fromMilestone) {
                        copy_m.milestone = -1;
                        storage.addTaskWithIndex(copy_m, cur.arg.index);
                    } else {
                        copy_m.milestone = cur.arg.milestoneId;
                        storage.addTaskWithIndex(copy_m, cur.arg.index);
                    }

                    // восстановим связи для которых текущая задача главная ( из нее идет связь )

                    if (cur.arg.linksToRemove) {
                        length = cur.arg.linksToRemove.length;

                        if (undefined !== cur.arg.fromMilestone) {
                            for (i = 0; i < length; ++i) {
                                link = cur.arg.linksToRemove[i];

                                task = storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].t[link.ind];
                                task.links.splice(link.index, 0, link.link);
                            }
                        } else {
                            for (i = 0; i < length; ++i) {
                                link = cur.arg.linksToRemove[i];

                                task = storage.p[cur.arg.fromProject].t[link.ind];
                                task.links.splice(link.index, 0, link.link);
                            }
                        }
                    }

                    break;

                case kOperationMoveGroupTasks:
                    var tasksNeedMove = cur.arg.group;

                    // в веху из свободной зоны

                    if ('MtoF' === cur.arg.type) {

                        milestoneRef = storage.p[cur.arg.toProject].m[cur.arg.fromMilestone];

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = cur.arg.sourceMilestone;
                            storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].addTaskWithIndex(tasksNeedMove[i].task, tasksNeedMove[i].index);
                        }

                        storage.p[cur.arg.toProject].t.splice(storage.p[cur.arg.toProject].t.length - tasksNeedMove.length, tasksNeedMove.length);

                        milestoneRef.updateTimes();
                        storage.p[cur.arg.toProject]._calcTimes();
                    }


                    // из вехи в веху

                    if ('MtoM' === cur.arg.type) {

                        milestoneRef = storage.p[cur.arg.toProject].m[cur.arg.toMilestone];

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = cur.arg.sourceMilestone;
                            storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].addTaskWithIndex(tasksNeedMove[i].task, tasksNeedMove[i].index);
                        }

                        storage.p[cur.arg.toProject].m[cur.arg.toMilestone].t.splice(milestoneRef.t.length - tasksNeedMove.length,
                            tasksNeedMove.length);

                        milestoneRef.updateTimes();
                        storage.p[cur.arg.toProject].m[cur.arg.toMilestone].updateTimes();
                        storage.p[cur.arg.toProject]._calcTimes();
                    }

                    // из свободной зоны в веху

                    if ('FtoM' === cur.arg.type) {

                        milestoneRef = storage.p[cur.arg.toProject].m[cur.arg.toMilestone];

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = -1;
                            storage.addTaskWithIndex(tasksNeedMove[i].task, tasksNeedMove[i].index);
                        }

                        storage.p[cur.arg.toProject].m[cur.arg.toMilestone].t.splice(milestoneRef.t.length - tasksNeedMove.length,
                            tasksNeedMove.length);

                        milestoneRef.updateTimes();
                        storage.p[cur.arg.toProject]._calcTimes();
                    }

                    break;

                case kOperationChangeTaskStatus:
                    status = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._status;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._status = cur.arg.t._status;
                    cur.arg.t._status = status;
                    break;

                case kOperationChangeMilestoneStatus:
                    status = storage.getMilestone(cur.arg.p, cur.arg.m)._status;
                    storage.getMilestone(cur.arg.p, cur.arg.m)._status = cur.arg.t._status;
                    cur.arg.t._status = status;
                    break;

                case kOperationAddTaskLink:
                    task = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.t);
                    task.links.splice(cur.arg.linkIndex, 1);
                    break;

                case kOperationDeleteTaskLink:
                    task = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.t);
                    task.links.splice(cur.arg.linkIndex, 0, cur.arg.link);
                    break;

                case kOperationChangeResponsible: {

                    if (cur.arg.isMilestone) {
                        reference = storage.getMilestone(cur.arg.p, cur.arg.m);
                    } else {
                        reference = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index);
                    }

                    if (reference)
                        reference._responsibles = deepCopy(cur.arg.oldResponsibles);
                }
                    break;
            }

            this.perform(this.ind, true, undefined, reference);

            // скрываем все всплывающие элементы

            this.timeline.offMenus();
            this.timeline.offWidgets();
            this.timeline.clearPopUp = true;

            this.timeline.updateWithScroll();
            --this.ind;

            this._updateDebug();
            this._updateUI();
        },
        redo: function () {
            if (this.timeline.editBox.enable) { this.timeline.editBox.cancel(true) }
            this.timeline.leftPanelController().clearFocus();

            if (!this.timeline || !this.op.length || this.ind === this.op.length - 1) {
                this._updateUI();
                return;
            }

            ++this.ind;

            var storage = this.timeline.storage;
            var cur = this.op[this.ind];
            var title, status, task, i, link, length;
            var taskTreeChangeTimes = [], items = null, reference;

            switch (cur.id) {
                case kOperationAddTask:
                    this.timeline.rightScroll.save();
                    storage.addTaskWithIndex(cur.arg.t, cur.arg.index);
                    break;

                case kOperationDeleteTask:
                    this.timeline.rightScroll.save();

                    // удалим связи для которых текущая задача главная ( из нее идет связь )

                    length = cur.arg.linksToRemove.length;
                    if (undefined !== cur.arg.m) {
                        for (i = 0; i < length; ++i) {
                            link = cur.arg.linksToRemove[i];

                            if (link.link.dependenceTaskId !== cur.arg.taskId) {
                                task = storage.p[cur.arg.p].m[cur.arg.m].t[link.ind];

                                task.links.splice(link.index, 1);
                            }
                        }
                    } else {
                        for (i = 0; i < length; ++i) {
                            link = cur.arg.linksToRemove[i];

                            if (link.link.dependenceTaskId !== cur.arg.taskId) {
                                task = storage.p[cur.arg.p].t[link.ind];

                                task.links.splice(link.index, 1);
                            }
                        }
                    }

                    // удаляем саму задачу

                    storage.projects()[cur.arg.p].removeTask(cur.arg.index, cur.arg.m);

                    break;

                case kOperationAddMilestone:
                    this.timeline.rightScroll.save();
                    storage.addMilestoneWithIndex(cur.arg.t, cur.arg.index);
                    break;

                case kOperationDeleteMilestone:
                    this.timeline.rightScroll.save();
                    var tasks = storage.p[cur.arg.p].t;

                    for (i = 0; i < cur.arg.t.t.length; ++i) {
                        tasks.push(cur.arg.t.t[i]);
                        tasks.last().milestone = -1;
                    }

                    storage.p[cur.arg.p].removeMilestone(cur.arg.index);
                    break;

                case kOperationChangeTitleTask:
                    title = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._title;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._title = cur.arg.t._title;
                    cur.arg.t._title = title;
                    break;

                case kOperationChangeTitleMilestone:
                    title = storage.getMilestone(cur.arg.p, cur.arg.m)._title;
                    storage.getMilestone(cur.arg.p, cur.arg.m)._title = cur.arg.t._title;
                    cur.arg.t._title = title;
                    break;

                case kOperationChangeTimeTask:
                    var beginTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginTime;
                    var endTime = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endTime;
                    var endFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail;
                    var oneDay = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay;
                    var beginFail = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail;

                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).setTimes(cur.arg.t.beginTime, cur.arg.t.endTime);
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).endFail = cur.arg.t.endFail;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).oneDay = cur.arg.t.oneDay;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index).beginFail = cur.arg.t.beginFail;

                    cur.arg.t.beginTime = beginTime;
                    cur.arg.t.endTime = endTime;
                    cur.arg.t.endFail = endFail;
                    cur.arg.t.oneDay = oneDay;
                    cur.arg.t.beginFail = beginFail;

                    //  {p: p, m: m, t: j, ref: tasks[j], id: tasks[j]._id};

                    items = cur.arg.queryMoveLinks;
                    if (items) {
                        for (i = 0; i < items.length; ++i) {
                            task = items[i];

                            beginTime = storage.getTask(task.p, task.m, task.t).beginTime;
                            endTime = storage.getTask(task.p, task.m, task.t).endTime;
                            endFail = storage.getTask(task.p, task.m, task.t).endFail;
                            oneDay = storage.getTask(task.p, task.m, task.t).oneDay;
                            beginFail = storage.getTask(task.p, task.m, task.t).beginFail;

                            storage.getTask(task.p, task.m, task.t).setTimes(task.ref.beginTime, task.ref.endTime);
                            storage.getTask(task.p, task.m, task.t).endFail = task.ref.endFail;
                            storage.getTask(task.p, task.m, task.t).oneDay = task.ref.oneDay;
                            storage.getTask(task.p, task.m, task.t).beginFail = task.ref.beginFail;

                            task.ref.beginTime = beginTime;
                            task.ref.endTime = endTime;
                            task.ref.endFail = endFail;
                            task.ref.oneDay = oneDay;
                            task.ref.beginFail = beginFail;
                        }
                    }

                    if (undefined === cur.arg.m) { storage.p[cur.arg.p]._calcTimes(); }
                    break;

                case kOperationChangeTimeMilestone:
                    var save = deepCopy(storage.getMilestone(cur.arg.p, cur.arg.m));
                    storage.p[cur.arg.p].removeMilestone(cur.arg.index);
                    storage.addMilestoneWithIndex(cur.arg.t, cur.arg.index);
                    cur.arg.t = save;
                    break;

                case kOperationMoveTask:

                    // удалим связи для которых текущая задача главная ( из нее идет связь )

                    if (cur.arg.linksToRemove) {
                        length = cur.arg.linksToRemove.length;
                        if (undefined !== cur.arg.fromMilestone) {
                            for (i = 0; i < length; ++i) {
                                link = cur.arg.linksToRemove[i];

                                task = storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].t[link.ind];
                                task.links.splice(link.index, 1);
                            }
                        } else {
                            for (i = 0; i < length; ++i) {
                                link = cur.arg.linksToRemove[i];

                                task = storage.p[cur.arg.fromProject].t[link.ind];
                                task.links.splice(link.index, 1);
                            }
                        }
                    }

                    var addInd = cur.arg.index;
                    if (undefined === cur.arg.fromMilestone && undefined === cur.arg.toMilestone) {
                        addInd = cur.arg.place;
                    } else if (undefined === cur.arg.fromMilestone && undefined !== cur.arg.toMilestone) {
                        addInd = cur.arg.place;
                    }

                    storage.projects()[cur.arg.toProject].removeTask(cur.arg.index, cur.arg.fromMilestone);
                    var copy_m = deepCopy(cur.arg.t);
                    if (undefined == cur.arg.fromMilestone) {
                        copy_m.milestone = (undefined === cur.arg.milestoneToId) ? -1 : cur.arg.milestoneToId;
                        storage.addTaskWithIndex(copy_m, addInd);
                    } else {
                        copy_m.milestone = cur.arg.milestoneToId ? cur.arg.milestoneToId : -1;
                        storage.addTaskWithIndex(copy_m, cur.arg.place);
                    }

                    break;

                case kOperationMoveGroupTasks:
                    var tasksNeedMove = cur.arg.group;

                    // из свободной зоны в веху

                    if ('MtoF' === cur.arg.type) {

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = -1;
                            storage.p[cur.arg.fromProject].addTask(tasksNeedMove[i].task, true);
                        }

                        for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                            storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].removeTask(tasksNeedMove[i].index);
                        }

                        storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].updateTimes();
                        storage.p[cur.arg.fromProject]._calcTimes();
                    }

                    //  из вехи в веху

                    if ('MtoM' === cur.arg.type) {

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = cur.arg.destMilestone;
                            storage.p[cur.arg.toProject].m[cur.arg.toMilestone].t.push(tasksNeedMove[i].task);
                        }

                        for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                            storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].removeTask(tasksNeedMove[i].index);
                        }

                        storage.p[cur.arg.fromProject].m[cur.arg.fromMilestone].updateTimes();
                        storage.p[cur.arg.toProject].m[cur.arg.toMilestone].updateTimes();
                    }

                    // из свободной зоны в веху

                    if ('FtoM' === cur.arg.type) {

                        var milestoneRef = storage.p[cur.arg.toProject].m[cur.arg.toMilestone];

                        for (i = 0; i < tasksNeedMove.length; ++i) {
                            tasksNeedMove[i].task.milestone = cur.arg.destMilestone;
                            storage.p[cur.arg.toProject].m[cur.arg.toMilestone].t.push(tasksNeedMove[i].task);
                        }

                        for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                            storage.p[cur.arg.fromProject].removeTask(tasksNeedMove[i].index);
                        }

                        milestoneRef.updateTimes();
                        storage.p[cur.arg.fromProject]._calcTimes();
                    }

                    break;

                case kOperationChangeTaskStatus:
                    status = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._status;
                    storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index)._status = cur.arg.t._status;
                    cur.arg.t._status = status;
                    break;

                case kOperationChangeMilestoneStatus:
                    status = storage.getMilestone(cur.arg.p, cur.arg.m)._status;
                    storage.getMilestone(cur.arg.p, cur.arg.m)._status = cur.arg.t._status;
                    cur.arg.t._status = status;
                    break;

                case kOperationAddTaskLink:
                    task = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.t);

                    cur.arg.link['dependenceTaskId'] = storage.getTask(cur.arg.depIndexer.p, cur.arg.depIndexer.m, cur.arg.depIndexer.t)._id;
                    cur.arg.link['parentTaskId'] = storage.getTask(cur.arg.parIndexer.p, cur.arg.parIndexer.m, cur.arg.parIndexer.t)._id;

                    task.links.push(cur.arg.link);

                    break;

                case kOperationDeleteTaskLink:
                    task = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.t);
                    task.links.splice(cur.arg.linkIndex, 1);
                    break;

                case kOperationChangeResponsible: {

                    if (cur.arg.isMilestone) {
                        reference = storage.getMilestone(cur.arg.p, cur.arg.m);
                    } else {
                        reference = storage.getTask(cur.arg.p, cur.arg.m, cur.arg.index);
                    }

                    if (reference)
                        reference._responsibles = deepCopy(cur.arg.newResponsibles);
                }
                    break;
            }

            this.perform(this.ind, undefined, undefined, reference);

            // скрываем все всплывающие элементы

            this.timeline.offMenus();
            this.timeline.offWidgets();
            this.timeline.clearPopUp = true;

            this.timeline.updateWithScroll();

            this._updateDebug();
            this._updateUI();
        },

        // private

        _sliceTop: function () {
            if (!this.op.length)
                return;

            if (this.ind < 1)
                this.op = [];

            this.op.splice(this.ind, this.op.length - (this.ind));
        },
        _updateUI: function () {

            if (this.timeline.handlers[kHandlerUndoRedoUpdateUI]) {
                if (this.op.length) {
                    if (this.op.last().arg.silentUpdateMode) {
                        this.op.last().arg.silentUpdateMode = undefined;
                        return;
                    }
                }

                this.timeline.handlers[kHandlerUndoRedoUpdateUI](
                        (this.ind > -1 && this.ind < this.op.length) && this.op.length > 0,
                        (this.op.length - this.ind > 1) && this.op.length > 0);
            }
        },
        _updateDebug: function () {
            //  NOTE: отдельно переопределять
        }
    };

    function ZoomBar(dom, delegate) {
        this.init(dom, delegate);
    }
    ZoomBar.prototype = {
        init: function (dom, delegate) {
            var t = this;

            this._dom = dom;
            this.delegate = delegate;

            if (!(dom instanceof HTMLCanvasElement)) {
                this._canvas = document.createElement('canvas');
                this._canvas.id = 'ZoomBarId';
                this._canvas.width = this._dom.clientWidth;
                this._canvas.height = this._dom.clientHeight;
                this._dom.style.backgroundColor = '#ffffff';

                this._dom.appendChild(this._canvas);
            }

            if (this._canvas) {

                this.ctx = this._canvas.getContext('2d');
                this.ctxWidth = this.ctx.canvas.width;
                this.ctxHeight = this.ctx.canvas.height;
                this.bounding = this.ctx.canvas.getBoundingClientRect();

                this._dom.onmousemove = function (e) { t.onmousemove(e); };
                this._dom.onmousedown = function (e) { t.onmousedown(e); };
                this._dom.onmouseup = function (e) { t.onmouseup(e); };
                this._dom.ondblclick = function (e) { t.ondblclick(e); };
                this._dom.oncontextmenu = function () { return false; };

                // mozill scroll fix

                if ('onwheel' in this._dom) {
                    this._dom.addEventListener('wheel', function (e) { t.onmousewheel(e); }, false);
                } else if ('onmousewheel' in document) {
                    this._dom.addEventListener('mousewheel', function (e) { t.onmousewheel(e); }, false);
                } else {
                    this._dom.addEventListener('MozMousePixelScroll', function (e) { t.onmousewheel(e); }, false);
                }

                this.timeScale = delegate.timeScale;
                this.needUpdate = true;

                this.curDate = new Date();
                this.curDate.setHours(0);
                this.curDate.setMilliseconds(0);
                this.curDate.setMinutes(0);
                this.curDate.setSeconds(0);

                this.rightDate = new Date(this.curDate.getTime() + 12 * 30 * 24 * 3600000);     //  off half year
                this.leftDate = new Date(this.curDate.getTime() - 12 * 30 * 24 * 3600000);     //  add half year

                // this.rightDate.setUTCMonth(0);
                this.rightDate.setUTCDate(0);
                this.rightDate.setHours(0);
                this.rightDate.setMilliseconds(0);
                this.rightDate.setMinutes(0);
                this.rightDate.setSeconds(0);

                //this.leftDate.setUTCMonth(0);
                this.leftDate.setUTCDate(0);
                this.leftDate.setHours(0);
                this.leftDate.setMilliseconds(0);
                this.leftDate.setMinutes(0);
                this.leftDate.setSeconds(1);

                this.rightTextDate = new Date(this.rightDate.getTime() - 1);
                this.leftTextDate = new Date(this.leftDate.getTime());

                this.duration = 0;
                this.timeCur = 0;

                // UI

                this.isLBMDown = false;
                this.mouse = { x: 0, y: 0 };
                this.offmouse = { x: 0, y: 0 };
                this.handler = 0;
                this.anchor = 0;

                this.thumb = { begin: 0.0, end: 0.0, band: 0.0 };

                // styles

                this.indent = 4;
                this.indentX = (this.indent + 2) * 0.5;

                this.button = 0;

                this.clickHandler = 0;

                this.leftThumbX = 0;
                this.rightThumbX = 0;
                this.thumbW = 0;
                this.thumbX = 0;
                this.thumbC = 0;

            }
        },

        dom: function () {
            return this._dom;
        },
        canvas: function () {
            return this._canvas;
        },
        needRepaint: function () {
            this.repaint = true;
        },

        render: function () {
            if (this.needUpdate) {

                if (this.ctx) {

                    this.bounding = this.ctx.canvas.getBoundingClientRect();

                    this.ctxWidth = this.ctx.canvas.width;
                    this.ctxHeight = this.ctx.canvas.height;

                    this.calc();

                    if (this.repaint) {
                        this.repaint = false;

                        this.ctx.clearRect(0, 0, this.ctxWidth, this.ctxHeight);

                        if (this.thumbW !== kZoomBarThumbMinWidth) { this.ctx.fillStyle = kZoomBarThumbColor; } else { this.ctx.fillStyle = kZoomBarThumbHandlesColor; }

                        this.ctx.fillRect(this.thumbX + 0.5, 2, this.thumbW, this.ctxHeight - 4);

                        if (this.thumbW !== kZoomBarThumbMinWidth) {
                            this.ctx.fillStyle = kZoomBarThumbHandlesColor;

                            this.ctx.fillRect(this.thumbX + 0.5 - 1, 2, 1, this.ctxHeight - 4);
                            this.ctx.fillRect(Math.floor(this.thumbX + this.thumbW), 2, 1, this.ctxHeight - 4);

                            // боковые треугольники

                            this.ctx.beginPath();
                            this.ctx.moveTo(this.thumbX + 0.5, this.ctxHeight * 0.5 - this.indent);
                            this.ctx.lineTo(this.thumbX - this.indent - 0.5, this.ctxHeight * 0.5);
                            this.ctx.lineTo(this.thumbX + 0.5, this.ctxHeight * 0.5 + this.indent);
                            this.ctx.closePath();

                            this.ctx.fill();

                            this.ctx.beginPath();
                            this.ctx.moveTo(Math.floor(this.thumbX + this.thumbW), this.ctxHeight * 0.5 - this.indent);
                            this.ctx.lineTo(Math.floor(this.thumbX + this.thumbW + this.indent + 1), this.ctxHeight * 0.5);
                            this.ctx.lineTo(Math.floor(this.thumbX + this.thumbW), this.ctxHeight * 0.5 + this.indent);
                            this.ctx.closePath();

                            this.ctx.fill();
                        }

                        //

                        this.renderMilestoneLines();

                        // линия текущего дня

                        this.ctx.fillStyle = kZoomBarCurDayLineColor;
                        this.ctx.fillRect(this.dX, 0, 1, this.ctxHeight);

                        this.renderDates();
                    }
                }
            }
        },
        renderDates: function () {

            this.ctx.fillStyle = kDateScaleTextColor;

            function format(data) {
                var s = '', day = data.getDate(), month = data.getMonth() + 1;

                if (day < 10) s += '0' + day; else s += day;
                if (month < 10) s += '.0' + month; else s += '.' + month;

                s += '.' + String(data.getUTCFullYear()).substring(2, 4);

                return s;
            }

            this.ctx.fillText(format(this.leftTextDate), 5, this.ctxHeight * 0.6);
            this.ctx.fillText(format(this.rightTextDate), this.ctxWidth - 50, this.ctxHeight * 0.6);
        },
        renderMilestoneLines: function () {
            var i, j, time, color, begin = true, end = false, mc;

            var p = this.delegate.storage.projects();
            var pc = p.length;
            if (!pc) { return; }

            var cWidth = this.regionSize();

            var fx = cWidth * this.fraction + this.indentX;
            var hY = this.ctxHeight - this.indent;
            var fD = cWidth / this.duration;

            this.ctx.lineWidth = 1;
            this.ctx.strokeStyle = kZoomBarThumbColor;

            for (j = 0; j < pc; ++j) {
                mc = p[j].m.length;

                for (i = 0; i < mc; ++i) {

                    time = ((fD * p[j].m[i].endTime + fx) >> 0) + 0.5;

                    if (p[j].m[i].endTime <= 0) {
                        color = kMilestoneOverdueColor;
                    } else {
                        color = kMilestoneColor;
                    }

                    if (1 === p[j].m[i]._status) {
                        color = kMilestoneCompleteColor;
                    }

                    if (color !== this.ctx.strokeStyle) {
                        end = true;

                        if (i > 0) { this.ctx.stroke(); }

                        this.ctx.beginPath();
                        this.ctx.strokeStyle = color;
                    }

                    if (begin) {
                        this.ctx.beginPath();
                        this.ctx.strokeStyle = color;

                        begin = false;
                    }

                    this.ctx.moveTo(time, this.indent);
                    this.ctx.lineTo(time, hY);
                }
            }

            if (end) { this.ctx.stroke(); }
        },
        releaseCapture: function () {
            this.isLBMDown = false;
            this.handler = 0;
        },
        setRangeTimes: function (timeLeft) {

            if (timeLeft) {

                // + один полный месяц дополнительно что бы было место для маневров
                // на максимальной шкале месяцев будет хотть один полный видимый месяц

                var timeMargin = (31 + 31 + 1) * 24 * 3600000,

                 rightTs = this.curDate.getTime() + Math.abs(timeLeft * 3600000) + timeMargin,
                 leftTs = this.curDate.getTime() - Math.abs(timeLeft * 3600000) - timeMargin;

                if (rightTs > this.rightDate.getTime() || leftTs < this.leftDate.getTime()) {
                    this.leftDate = new Date(leftTs);
                    this.rightDate = new Date(rightTs);

                    this.rightDate.setUTCDate(0);
                    this.rightDate.setHours(0);
                    this.rightDate.setMilliseconds(0);
                    this.rightDate.setMinutes(0);
                    this.rightDate.setSeconds(0);

                    this.leftDate.setUTCDate(0);
                    this.leftDate.setHours(0);
                    this.leftDate.setMilliseconds(0);
                    this.leftDate.setMinutes(0);
                    this.leftDate.setSeconds(1);

                    this.rightTextDate = new Date(this.rightDate.getTime() - 1);
                    this.leftTextDate = new Date(this.leftDate.getTime());
                }
            }

            this.repaint = true;
        },

        calc: function () {
            var t = this.delegate;

            this.duration = (this.rightDate.getTime() - this.leftDate.getTime()) / 3600000;      //  hours
            this.timeCur = (this.curDate.getTime() - this.leftDate.getTime()) / 3600000;        //  hours
            this.fraction = ((this.curDate.getTime() - this.leftDate.getTime()) / 3600000) / this.duration;

            var cWidth = this.regionSize();

            var oneHourInPixels = this.timeScale.hourInPixels / this.timeScale.scaleX;
            var hours = (t.ctxWidth - t.visibleLeft) / oneHourInPixels;

            var leftHours = t.visibleLeft / oneHourInPixels;

            this.dX = Math.floor(Math.abs(this.timeCur / this.duration * cWidth) + this.indentX);

            this.thumb.begin = (this.timeCur - this.timeScale._fX * this.timeScale.scaleX + leftHours) / this.duration;
            this.thumb.end = (this.timeCur - this.timeScale._fX * this.timeScale.scaleX + hours + leftHours) / this.duration;
            this.thumb.band = hours / this.duration;

            this.leftThumbX = this.thumb.begin * cWidth + this.indentX;
            this.rightThumbX = this.thumb.end * cWidth + this.indentX;

            this.leftHandler = Math.floor(this.leftThumbX);
            this.rightHandler = Math.floor(this.rightThumbX);
            this.bandwidth = Math.floor(this.thumb.band * cWidth);

            this.thumbW = this.rightThumbX - this.leftThumbX;
            this.thumbX = Math.floor(this.leftHandler) + 0.5;
            this.thumbC = (this.rightThumbX + this.leftThumbX) * 0.5;

            if (this.thumbW <= kZoomBarThumbMinWidth) {
                this.thumbW = kZoomBarThumbMinWidth;
                this.thumbX = Math.floor(this.thumbC - this.thumbW * 0.5) + 0.5;

                var cX = (this.thumb.end + this.thumb.begin) * 0.5;
                cX = 0.5 + (cX - 0.5) * (1.0 + (this.thumb.end - this.thumb.begin));
                this.thumbX = (cWidth - this.thumbW) * cX + this.indentX;
            }
        },
        worldToContol: function (x, y) {
            return {
                x: x - this.bounding.left * (this.ctxWidth / this.bounding.width),
                y: y - this.bounding.top * (this.ctxHeight / this.bounding.height)
            };
        },
        isCapture: function (e) {

            this.mouse = this.worldToContol(e.clientX, e.clientY);

            if (this.thumbW === kZoomBarThumbMinWidth) {

                // в режиме 'узкого ползунка'

                return (this.mouse.x > this.thumbX && this.mouse.x < this.thumbX + this.thumbW);
            }

            return (this.mouse.x >= this.leftHandler && this.mouse.x <= this.rightHandler);
        },
        calcMouseHandler: function (e) {

            this.mouse = this.worldToContol(e.clientX, e.clientY);

            if (this.thumbW === kZoomBarThumbMinWidth) {

                // в режиме 'узкого ползунка'

                if (this.mouse.x > this.thumbX && this.mouse.x < this.thumbX + this.thumbW)
                    this.handler = 0;

            } else {

                if (Math.abs(this.mouse.x - this.leftHandler) < 7)
                    this.handler = -1;
                else if (Math.abs(this.mouse.x - this.rightHandler) < 7)
                    this.handler = 1;
                else
                    this.handler = 0;
            }

            return this.handler;
        },
        regionSize: function () {
            return this.ctxWidth - this.indentX * 2;
        },

        moveToMilestone: function () {

            var i, j, time, color, mc, mx, height = 0;

            var t = this.delegate;

            var p = t.storage.projects();
            var pc = p.length;

            if (!pc) { return false; }

            var cWidth = this.regionSize();
            var fx = this.duration * this.fraction / this.duration * cWidth + this.indentX;
            var hY = this.ctxHeight - this.indent;
            var fD = cWidth / this.duration;

            var margin = t.itemMargin;

            for (j = 0; j < pc; ++j) {
                if (!p[j].isEmpty()) {

                    mc = p[j].m.length;

                    for (i = 0; i < mc; ++i) {
                        time = ((fD * p[j].m[i].endTime + fx) >> 0) + 0.5 - this.mouse.x;

                        if (abs2(time) < 5) {                                                          // a = Math.abs(X) -> a = i < 0 ? ~i++ : i;
                            t.animator.moveCenterToX(p[j].m[i].endTime);
                            if (i === 0 && j === 0) { height = kEps; }

                            var maxVal = floor2((t.rightScroll.maxValue()) / margin) * margin;

                            var scrollY = height;
                            if (!t.fullscreen) { scrollY = floor2(scrollY / margin) * margin - kEps; }

                            t.animator.moveToY(Math.min(scrollY, maxVal - kEps));
                            t.offMenus();
                            t.offWidgets();

                            this.repaint = true;

                            return true;
                        }

                        height += margin + p[j].m[i].t.length * margin;

                        if (0 === p[j].m[i].t.length && t.fullscreen) { height += t.itemMargin; }
                    }

                    if (p[j].t.length) {
                        height += p[j].t.length * margin + t.itemHeight;
                    }
                }
            }

            return false;
        },

        maxPos: function () {
            var t = this.delegate;

            var cWidth = this.regionSize();

            var oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
            var leftHours = t.visibleLeft / oneHourInPixels;

            var dx = ((this.bandwidth / cWidth - 1.0 + this.fraction) * this.duration) + leftHours;

            return dx * oneHourInPixels / this.timeScale.hourInPixels;
        },
        minPos: function () {
            var t = this.delegate;

            var oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
            var leftHours = t.visibleLeft / oneHourInPixels;

            var dx = this.fraction * this.duration + leftHours;

            return dx * oneHourInPixels / this.timeScale.hourInPixels;
        },

        // mouse events

        onmousedown: function (e) {
            this.handler = 0;
            this.button = e.button;

            if (this.isCapture(e) || 0 !== this.calcMouseHandler(e)) {

                this.isLBMDown = true;
                this.offmouse.x = this.mouse.x;

                this.calc();

                this.anchor = this.mouse.x - this.leftHandler;
            }

            this.clickHandler = this.handler;
        },
        onmousemove: function (e) {
            if (2 === this.button) {
                return;
            }

            var t = this.delegate,
                capture = this.isCapture(e),
                cWidth = this.regionSize(),
                oneHourInPixels, hours, mouseTx, tumbScaleX, thumbSizeX, leftCenterX, rightCenterX, oldScaleX,
                pos, bandHalf, fx, dx, leftHours,
                duration = this.duration,
                timeCur = this.timeCur,
                fraction = this.fraction,
                maxVal = 0,
                scrollY = 0;

            if (0 === t.storage.p.length) { // none projects
                return;
            }

            t.bottomScroll.onmouseup(e);
            t.rightScroll.onmouseup(e);

            if (this.isLBMDown) {

                oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
                leftHours = (t.visibleLeft) / oneHourInPixels;

                if (0 === this.handler) {

                    t.animator.stop();

                    pos = (this.mouse.x - this.anchor) / cWidth;
                    pos = (-fraction + 0.5) * (fraction - pos) / fraction + 0.5 * pos / fraction;

                    fx = 0.5 - pos;

                    dx = Math.max((Math.min(fx, fraction) * duration), (this.bandwidth / cWidth - 1.0 + fraction) * duration) + leftHours;

                    t.offsetX = dx * oneHourInPixels / this.timeScale.hourInPixels;

                    t.updateWithStrafe();

                    this._canvas.style.cursor = 'pointer';

                    this.repaint = true;

                } else if (-1 === this.handler) {

                    // двигаем за левую часть ползунка

                    t.animator.stop();

                    mouseTx = this.mouse.x / cWidth;

                    if (this.thumb.end - mouseTx > 0) {
                        thumbSizeX = this.thumb.end - mouseTx;
                        tumbScaleX = thumbSizeX / this.thumb.band;
                        leftCenterX = thumbSizeX + fraction - this.thumb.end;

                        oldScaleX = t.timeScale.scaleX;

                        t.timeScale.scaleX *= tumbScaleX;

                        if (leftCenterX >= fraction) {
                            t.scaleX = oldScaleX;
                            t.update();
                            t._viewController.changeTimeScaleEvent();
                        } else {
                            thumbSizeX = this.thumb.band * tumbScaleX;
                            leftCenterX = fraction - (this.thumb.end - thumbSizeX);

                            if (t.timeScale.scaleX <= kScaleUnitMinSize) {
                                tumbScaleX = kScaleUnitMinSize / t.timeScale.scaleX * tumbScaleX;
                                t.timeScale.scaleX = kScaleUnitMinSize;
                            }

                            if (t.timeScale.scaleX >= kScaleUnitFifteenDays) {
                                tumbScaleX = kScaleUnitFifteenDays / t.timeScale.scaleX * tumbScaleX;
                                t.timeScale.scaleX = kScaleUnitFifteenDays;
                            }

                            thumbSizeX = this.thumb.band * tumbScaleX;
                            leftCenterX = fraction - (this.thumb.end - thumbSizeX);

                            oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
                            hours = t.timeScale.ctxWidth / oneHourInPixels;
                            leftHours = t.visibleLeft / oneHourInPixels;

                            t.offsetX = (leftCenterX * duration + leftHours) * oneHourInPixels / t.timeScale.hourInPixels;
                            t.updateWithStrafe();
                            t._viewController.changeTimeScaleEvent();
                        }
                    }

                    this._canvas.style.cursor = 'w-resize';

                } else if (1 === this.handler) {

                    // двигаем за правую часть ползунка

                    t.animator.stop();

                    mouseTx = this.mouse.x / cWidth;

                    if (mouseTx - this.thumb.begin > 0) {

                        thumbSizeX = mouseTx - this.thumb.begin;
                        tumbScaleX = thumbSizeX / this.thumb.band;
                        leftCenterX = fraction - this.thumb.begin;

                        oldScaleX = t.timeScale.scaleX;

                        t.timeScale.scaleX *= tumbScaleX;

                        if (t.timeScale.scaleX < kScaleUnitMinSize + 1 || t.timeScale.scaleX > kScaleUnitFifteenDays || leftCenterX >= fraction) {
                            t.timeScale.scaleX = oldScaleX;
                            t.update();
                            t._viewController.changeTimeScaleEvent();
                        } else {
                            oneHourInPixels = t.timeScale.hourInPixels / t.timeScale.scaleX;
                            hours = t.timeScale.ctxWidth / oneHourInPixels;
                            leftHours = t.visibleLeft / oneHourInPixels;

                            t.offsetX = (leftCenterX * duration + leftHours) * oneHourInPixels / t.timeScale.hourInPixels;
                            t.updateWithStrafe();
                            t._viewController.changeTimeScaleEvent();
                        }
                    }

                    this._canvas.style.cursor = 'w-resize';
                }

                maxVal = floor2((t.rightScroll.maxValue()) / t.itemMargin) * t.itemMargin;
                scrollY = t.getTopElementInVisibleRange().height - t.itemMargin * (!t.fullscreen);
                if (!t.fullscreen) {
                    scrollY = floor2(scrollY / t.itemMargin) * t.itemMargin - kEps;
                }

                if (t.contentHeight >= t.ctxHeight - t.itemMargin * 2) {
                    t.animator.moveToY(Math.min(scrollY, maxVal - kEps));

                    t.offMenus();
                    t.offWidgets();
                }

                this.repaint = true;

                return;
            }

            if (capture) {
                this._canvas.style.cursor = 'pointer';
                return;
            } else if (this.calcMouseHandler(e)) {
                this._canvas.style.cursor = 'w-resize';
                return;
            }

            this._canvas.style.cursor = '';
        },
        onmouseup: function (e) {
            if (0 === this.delegate.storage.p.length) { // none projects
                return;
            }

            var t = this.delegate, cWidth, duration, fx, dx, bandHalf, fraction, pos;

            this.releaseCapture();

            if (2 === this.button) {
                this.moveToMilestone();
                this.button = 0;
            } else {
                if (t) {
                    cWidth = this.regionSize();
                    duration = this.duration;
                    fraction = this.fraction;
                    bandHalf = this.bandwidth * 0.5 / cWidth;

                    if ((0 === this.clickHandler) && ((this.mouse.x - this.leftHandler) < -7 || (this.mouse.x - this.rightHandler) > 7)) {
                        pos = this.mouse.x / cWidth;
                        pos = (0.5 - fraction) * (fraction - pos) / fraction + 0.5 * pos / fraction;

                        fx = 0.5 - pos;
                        dx = fx * duration;

                        dx = Math.max((Math.min(fx, fraction - bandHalf) * duration), (fraction - 1.0 + bandHalf) * duration);

                        t.animator.stop();
                        t.animator.moveCenterToX(-dx);
                        t.updateWithStrafe();

                        this.repaint = true;
                    }
                }
            }
        },
        ondblclick: function (e) {
            // this.releaseCapture();
            // this.delegate.animator.moveCenterToX(0);
        },
        onmousewheel: function (e) {

            if (0 === this.delegate.storage.p.length) { // none projects
                return;
            }

            e = e || window['event'];

            var delta = e['deltaY'] || e['detail'] || e['wheelDelta'];
            if (e['deltaY']) delta *= -1;
            if (e['detail']) delta *= -1;

            var scale = this.delegate.timeScale.scaleX;
            if (delta > 0) {
                scale -= 24;

                if (scale < kScaleUnitNormalizeDays && this.delegate.timeScale.scaleX > kScaleUnitNormalizeDays) {
                    this.delegate.viewController().scaleTo(kScaleUnitNormalizeDays);
                } else {
                    this.delegate.viewController().scaleTo(Math.min(Math.max(scale, kScaleUnitMinSize), kScaleUnitFifteenDays));
                }
            } else {
                scale += 24;

                if (this.delegate.timeScale.scaleX < kScaleUnitNormalizeDays)
                    this.delegate.viewController().scaleTo(kScaleUnitNormalizeDays);
                else
                    this.delegate.viewController().scaleTo(Math.min(Math.min(scale, kScaleUnitOneMonth), kScaleUnitFifteenDays));
            }

            this.repaint = true;
        }
    };

    function DateScale(delegate, factor) {

        this.delegate = delegate;
        this.marginY = kUIDateScaleUpMargin;

        this.zoom = 1;

        this.textDateLineHeight = 15 * this.zoom;
        this.hourInPixels = 35 * this.zoom;                             //  при максимальной развертке величина деления для часа (TODO: сделать до минут)
        this.fontHeight = 9 * this.zoom;
        this.font = this.fontHeight + 'pt ' + kDefaultFontName;
        //
        this.ctx = delegate.ctx;
        this.ctxWidth = this.ctx.canvas.width;
        this.ctxHeight = this.ctx.canvas.height;
        this.scaleX = factor;                                     //  единичный фактор, соотвествует максимальной развертки шкалы до часа

        this.dayUTC = 0;
        this.weekDay = 0;
        this.monthCur = 0;
        this.yearCur = 0;

        this._fX = 0.0;

        // внутренние данные
        this.scaleType = 0;                                          //   тип шкалы
        this.divisionInOneHours = 1;                                          //   кол-во часов в одном делении
        this.divisionInDays = 7;                                          //   кол-во дней видимых на шкале ( если меньше 7, то числами )

        this.lineSize = this.delegate.itemMargin;
        this.headTextPosY = this.lineSize * 0.55 + 1;
        this.baseTextPosY = this.lineSize * 1.65;
    }
    DateScale.prototype = {
        updateDefaults: function () {
            this.font = this.fontHeight + 'pt ' + kDefaultFontName;
        },

        strafe: function (value) {
            this._fX = value;
            this.calculateScaleType();
        },
        height: function () {
            return this.delegate.itemMargin * 2;
        },
        linesFromY: function () {
            return this.delegate.itemMargin;
        },
        weekend: function () {
            if (kTypeScaleMonth === this.scaleType) { return null; }

            var dayInd = (new Date()).getDay(); --dayInd;

            if (-1 === dayInd)
                dayInd = 6;

            return {
                x: (6 - dayInd - 1) * 24 * this.hourInPixels / this.scaleX,
                width: 2 * 24 * this.hourInPixels / this.scaleX,
                off: 7 * 24 * this.hourInPixels / this.scaleX
            };
        },
        scaleUnitStep: function () {
            return this.divisionInOneHours;
        },

        update: function () {

        },
        setZoom: function (value) {
            this.zoom = Math.max(0.1, value);

            this.textDateLineHeight = 15 * this.zoom;
            this.hourInPixels = 35 * this.zoom;
            this.fontHeight = 9 * this.zoom;
            this.font = this.fontHeight + 'pt ' + kDefaultFontName;

            this.lineSize = this.delegate.itemMargin;
            this.headTextPosY = this.lineSize * 0.55;
            this.baseTextPosY = this.lineSize * 1.65;
        },

        refresh: function () {
            this.ctxWidth = this.ctx.canvas.width;
            this.ctxHeight = this.ctx.canvas.height;

            var date = new Date();
            this.weekDay = date.getDay(); --this.weekDay;
            if (-1 === this.weekDay)
                this.weekDay = 6;

            this.dayUTC = date.getUTCDate();
            this.monthCur = date.getMonth();
            this.yearCur = date.getFullYear();

            this.calculateScaleType();
        },

        draw: function () {
            // this.calculateScaleType();

            var j, i, m, ind, offLine;

            var currentDate = new Date();
            var dayOff = currentDate.getDay();
            --dayOff;
            if (-1 === dayOff) dayOff = 6;

            var datInWeekOff = currentDate.getDate();
            var monthOff = currentDate.getMonth();

            var divFW = this.hourInPixels * this.scaleX;

            var divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours;
            var count = Math.floor(this.ctxWidth / divWX) + 3;
            var offX = this.hourInPixels * this._fX;

            var fromX = this._fX * this.scaleX / this.divisionInOneHours;
            var index = Math.floor(fromX);
            var offTL = fromX - index;

            this.ctx.strokeStyle = kLinesVerticalColor;
            this.ctx.lineWidth = 1;
            this.ctx.fillStyle = kDateScaleTextColor;
            this.ctx.font = this.font;

            this.ctx.beginPath();

            if (kTypeScaleWeek === this.scaleType) { this.drawWeekScheme(); } else
                if (kTypeScaleDays === this.scaleType) { this.drawDaysScheme(); } else
                    if (kTypeScaleHours === this.scaleType) { this.drawHoursScheme(); } else
                        if (kTypeScaleMonth === this.scaleType) { this.drawMonthScheme(); }

            this.ctx.closePath();
            this.ctx.stroke();
        },

        drawHoursScheme: function () {
            var w, j, h, m, y, ind, str, offLine;

            var dayNeedDraw = false;
            var ctxWidth = this.ctxWidth;

            var divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours;
            var count = this.ctxWidth / divWX + this.divisionInOneHours * 2;

            var fromX = this._fX * this.scaleX / this.divisionInOneHours;
            var index = floor2(fromX); // Math.floor(fromX);
            var offTL = fromX - index;

            var daysInEnumMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            var day = this.dayUTC;
            var hourCorrection = 0;

            var offX = this.hourInPixels * this._fX;
            var daysleft = floor2(offX / divWX);    //  Math.floor(offX /  divWX);

            m = this.monthCur;
            y = this.yearCur;

            for (j = 0, w = this.weekDay, h = hourCorrection; j < count - daysleft; ++j, h += this.divisionInOneHours) {
                offLine = (j + fromX) * divWX;

                if (offLine > ctxWidth + 100)
                    break;

                if (h >= 24) {
                    h = 0; ++w;

                    if (w > 6) w = 0; dayNeedDraw = true;

                    day++;

                    if (day >= daysInEnumMouth + 1) {

                        m++; if (m >= 12) { m = 0; y++; }

                        day -= daysInEnumMouth;
                        daysInEnumMouth = this.daysInMonth(m + 1, y);
                    }
                }

                if (offLine < -divWX)
                    continue;

                if (dayNeedDraw) {
                    this.ctx.moveTo(offLine + 0.5, 2 + this.marginY);
                    this.ctx.lineTo(offLine + 0.5, this.textDateLineHeight + 2 + this.marginY);

                    dayNeedDraw = false;
                }

                this.ctx.fillText(h, offLine + 0.5 + 2, this.textDateLineHeight * 1.7 + 2 + this.marginY);
            }

        },
        drawDaysScheme: function () {
            var i, j, m, y, offLine, str, w, label;

            var monthNeedDraw = false;
            var monthDivisionOff = 0;

            var ctxWidth = this.ctxWidth;

            var divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours;
            var count = ctxWidth / divWX;
            var offX = this.hourInPixels * this._fX;

            var fromX = this._fX * (this.scaleX) / this.divisionInOneHours;
            var daysleft = floor2(offX / divWX);    //  Math.floor(offX /  divWX);
            var daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            var day = this.dayUTC;
            var rightOffset = -divWX * this.divisionInDays;
            var leftCount = Math.floor(count - daysleft) + this.divisionInDays;

            var names = (this.divisionInDays >= 9) ? window['Gantt']['Teamlab_shortmonths'] : window['Gantt']['Teamlab_months'];
            var shortDates = this.scaleX > kScaleUnitMinMonth;

            var txtW0 = (divWX - this.ctx.measureText('0').width) * 0.5;
            var txtW10 = (divWX - this.ctx.measureText('10').width) * 0.5;

            var dayBold = true;

            if (1 === this.divisionInDays) {
                for (j = 0, m = this.monthCur + 1, y = this.yearCur, w = this.weekDay; j < count - daysleft; ++j, ++day, ++w) {
                    offLine = (j + fromX) * divWX;

                    if (day > daysInMouth) {
                        day = 1; m++; if (m > 12) { m = 1; ++y; }
                        daysInMouth = this.daysInMonth(m, y);
                    }

                    if (w > 6) w = 0;

                    if (offLine < -divWX * 3)
                        continue;

                    if (w == 5 || w == 6) { this.ctx.fillStyle = kDateScaleDayOffTextColor; } else { this.ctx.fillStyle = kDateScaleTextColor; }

                    if (1 === day) {
                        this.ctx.fillStyle = kLinesDateScaleTopColor;
                        this.ctx.fillRect(floor2(offLine), 0, 1, this.delegate.itemMargin);

                        this.ctx.fillStyle = kDateScaleTextColor;
                        this.ctx.fillText(names[m - 1] + ' ' + y, offLine + this.lineSize * 0.25, this.headTextPosY);
                    }

                    this.ctx.fillText(day, offLine + ((day >= 10) ? txtW10 : txtW0), this.baseTextPosY);
                }

                daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
                day = this.dayUTC;

                for (j = 0, m = this.monthCur + 1, y = this.yearCur, w = this.weekDay; j >= -daysleft - count; --j, --day, --w) {
                    offLine = (j + fromX) * divWX;

                    if (day < 1) {
                        --m; if (m < 1) { m = 12; --y; }
                        daysInMouth = this.daysInMonth(m, y);
                        day = this.daysInMonth(m, y);
                    }

                    if (w < 0) w = 6;

                    if (offLine < -divWX * 3 || offLine > ctxWidth)
                        continue;

                    if (w == 5 || w == 6) { this.ctx.fillStyle = kDateScaleDayOffTextColor; } else { this.ctx.fillStyle = kDateScaleTextColor; }

                    if (1 === day) {
                        this.ctx.fillStyle = kLinesDateScaleTopColor;
                        this.ctx.fillRect(floor2(offLine), 0, 1, this.delegate.itemMargin);

                        this.ctx.fillStyle = kDateScaleTextColor;
                        this.ctx.fillText(names[m - 1] + ' ' + y, offLine + this.lineSize * 0.25, this.headTextPosY);
                    }

                    if (dayBold) {
                        if (j === 0) {
                            this.ctx.fillStyle = kDateScaleCurrentDayColor;
                            this.ctx.font = 'bold ' + this.fontHeight + 'pt ' + kDefaultFontName;
                        }
                    }

                    this.ctx.fillText(day, offLine + ((day >= 10) ? txtW10 : txtW0), this.baseTextPosY);

                    if (dayBold) { dayBold = false; if (j === 0) this.ctx.font = this.font }
                }

                return;
            }

            daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            day = this.dayUTC;

            for (j = 0, m = this.monthCur, y = this.yearCur; j < leftCount; ++j, day += this.divisionInDays) {
                offLine = (j + fromX) * divWX;

                if (offLine > ctxWidth + 100)
                    break;

                if (day >= daysInMouth) {
                    monthNeedDraw = true;
                    monthDivisionOff = offLine;

                    m++; if (m >= 12) { m = 0; ++y; }

                    day -= daysInMouth;
                    daysInMouth = this.daysInMonth(m + 1, y);
                    monthDivisionOff = offLine - divWX * (1.0 / this.divisionInDays) * day;
                }

                if (offLine < -divWX) {
                    continue;
                }

                if (monthNeedDraw) {
                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(offLine), 0, 1, this.delegate.itemMargin);

                    this.ctx.fillStyle = kDateScaleTextColor;
                    label = (!shortDates ? y : y % 100); if (1 === label.length) label = '0' + label;
                    this.ctx.fillText(names[m] + ' ' + label, offLine + this.lineSize * 0.25, this.headTextPosY);
                    monthNeedDraw = false;
                }

                if (0 === day)++day;

                this.ctx.fillText(day, offLine + 0.5 + 2 - txtW0 * ((day > 10) ? 0.75 : 1), this.baseTextPosY);
            }

            daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            day = this.dayUTC;

            for (j = 0, m = this.monthCur + 1, y = this.yearCur; j >= -daysleft - count; --j, day -= this.divisionInDays) {
                offLine = (j + fromX) * divWX;
                if (offLine < rightOffset)
                    break;

                if (day < 1) {
                    monthNeedDraw = true;
                    monthDivisionOff = offLine;

                    monthDivisionOff = offLine - divWX * (1.0 / this.divisionInDays) * day;

                    --m; if (m < 0) { m = 11; --y; }
                    daysInMouth = this.daysInMonth(m + 1, y);

                    day += daysInMouth;
                }

                if (offLine > ctxWidth)
                    continue;

                if (monthNeedDraw) {
                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(offLine), 0, 1, this.delegate.itemMargin);

                    this.ctx.fillStyle = kDateScaleTextColor;
                    label = (!shortDates ? y : y % 100); if (1 === label.length) label = '0' + label;
                    this.ctx.fillText(names[m] + ' ' + label, offLine + this.lineSize * 0.25, this.headTextPosY);
                    monthNeedDraw = false;
                }

                if (0 === day)++day;
                this.ctx.fillText(day, offLine + 0.5 + 2 - txtW0 * ((day > 10) ? 0.75 : 1), this.baseTextPosY);
            }
        },
        drawWeekScheme: function () {
            var i, j, m, y, offLine, str, w, dateend, weekDayNumber;

            var needPrintMonth = false;
            var lastMonthOffX = 0;

            var ctxWidth = this.ctxWidth;

            var divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours;
            var count = ctxWidth / divWX;
            var offX = this.hourInPixels * this._fX;

            var fromX = this._fX * (this.scaleX) / this.divisionInOneHours;
            var daysleft = (offX / divWX) >> 0;    //  Math.floor(offX /  divWX);

            var daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            var day = this.dayUTC - this.weekDay;
            var dayYear = this.getDayOfYear(new Date());// + 1;
            var daysInYear = this.getDaysInYear(this.yearCur);

            var rightOffset = -divWX * this.divisionInDays;
            var leftCount = Math.floor(count - daysleft) + this.divisionInDays;

            var names = window['Gantt']['Teamlab_shortmonths'];
            var shortDates = this.scaleX > kScaleUnitMinMonth;

            var week = window['Gantt']['Localize_strings']['week'] || 'week';

            var week0 = week + ' ';
            var week10 = week + ' 10';

            var txtW0 = (divWX - this.ctx.measureText(week0).width) * 0.5;
            var txtW10 = (divWX - this.ctx.measureText(week10).width) * 0.5;

            var weekNumber = this.getWeekNumber(new Date());
            var pixelsInDay = divWX / this.divisionInDays;

            for (j = 0, m = this.monthCur, y = this.yearCur; j < leftCount; ++j, day += this.divisionInDays, dayYear += this.divisionInDays) {
                offLine = (j + fromX) * divWX;

                if (offLine > ctxWidth + divWX * 2)
                    break;

                if (dayYear - daysInYear >= 0) {
                    dateend = new Date('31 Dec ' + y);
                    weekDayNumber = dateend.getDay();

                    dayYear = weekDayNumber;
                    daysInYear = this.getDaysInYear(y + 1);
                    weekNumber = 1;
                }

                if (day >= daysInMouth) {

                    needPrintMonth = true;
                    lastMonthOffX = offLine;

                    m++; if (m >= 12) { m = 0; ++y; }

                    day -= daysInMouth;
                    daysInMouth = this.daysInMonth(m + 1, y);

                    //-----------------------------------------------------------------------------------------------------
                    // WeekDay      =   (7 - (day - 1));
                    // offMonthX    =   offLine + (7 - this.weekDay - 7 - 7 + (7 - (day - 1))) * divWX / this.divisionInDays
                    //-----------------------------------------------------------------------------------------------------

                    lastMonthOffX = offLine + (1 - day - this.weekDay) * pixelsInDay;

                    //                    if (m === 11) {
                    //                        this.ctx.strokeStyle    =   '#ff0000';
                    //                        this.ctx.lineWidth      =   2;
                    //
                    //                        this.ctx.beginPath();
                    //
                    //                        this.ctx.moveTo(offLine, 0);
                    //                        this.ctx.lineTo(offLine, 100);
                    //                        this.ctx.stroke();
                    //
                    //                        this.ctx.strokeStyle    =   '#0000ff';
                    //                        this.ctx.lineWidth      =   5;
                    //
                    //                        this.ctx.beginPath();
                    //                        this.ctx.moveTo(lastMonthOffX, 0);
                    //                        this.ctx.lineTo(lastMonthOffX, 100);
                    //
                    //                        this.ctx.stroke();
                    //                    }
                }

                if (offLine < -divWX * 2) {
                    ++weekNumber;
                    continue;
                }

                if (needPrintMonth) {
                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(lastMonthOffX), 0, 1, this.delegate.itemMargin);

                    this.ctx.fillStyle = kDateScaleTextColor;
                    this.ctx.fillText(names[m] + ' ' + y, lastMonthOffX + this.lineSize * 0.5, this.headTextPosY);
                    needPrintMonth = false;
                }

                // this.ctx.strokeStyle    =   '#ff0000';
                // this.ctx.lineWidth      =   2;

                // this.ctx.beginPath();

                // this.ctx.moveTo(offLine - this.weekDay * pixelsInDay, 0);
                // this.ctx.lineTo(offLine - this.weekDay * pixelsInDay, 100);

                // this.ctx.stroke();

                if (weekNumber >= 10) {
                    this.ctx.fillText(week + ' ' + weekNumber, offLine + txtW10 + divWX - this.weekDay * pixelsInDay, this.baseTextPosY);
                } else {
                    this.ctx.fillText(week + ' ' + weekNumber, offLine + txtW0 + divWX - this.weekDay * pixelsInDay, this.baseTextPosY);
                }

                ++weekNumber;
            }

            var first = true;

            weekNumber = this.getWeekNumber(new Date()) - 1;
            daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            day = this.dayUTC - this.weekDay;
            dayYear = this.getDayOfYear(new Date()) + 1;
            daysInYear = this.getDaysInYear(this.yearCur);

            for (j = 0, m = this.monthCur + 1, y = this.yearCur; j >= -daysleft - count; --j, day -= this.divisionInDays, dayYear += this.divisionInDays) {
                offLine = (j + fromX) * divWX;

                if (offLine < rightOffset)
                    break;

                if (weekNumber <= 0) {
                    weekNumber = this.getIsoWeeks(y - 1);
                }

                if (day < 0) {
                    needPrintMonth = true;
                    lastMonthOffX = offLine;

                    --m; if (m < 0) { m = 11; --y; }
                    daysInMouth = this.daysInMonth(m + 1, y);

                    day += daysInMouth;

                    lastMonthOffX = offLine + (daysInMouth - day - this.weekDay + 1 + m % 2) * pixelsInDay;

                    // this.ctx.strokeStyle    =   '#0000ff';
                    // this.ctx.lineWidth      =   3;

                    // this.ctx.beginPath();

                    // this.ctx.moveTo(offLine + (daysInMouth - day - this.weekDay + 1 + m % 2) * pixelsInDay, 0);
                    // this.ctx.lineTo(offLine + (daysInMouth - day - this.weekDay + 1 + m % 2) * pixelsInDay, 500);

                    // this.ctx.stroke();
                }

                if (offLine > ctxWidth) {
                    --weekNumber;
                    continue;
                }

                if (needPrintMonth) {
                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(lastMonthOffX), 0, 1, this.delegate.itemMargin);

                    this.ctx.fillStyle = kDateScaleTextColor;
                    this.ctx.fillText(names[m] + ' ' + y, lastMonthOffX + this.lineSize * 0.5, this.headTextPosY);
                    needPrintMonth = false;
                }

                if (first) { first = false; continue; }

                if (weekNumber >= 10) {
                    this.ctx.fillText(week + ' ' + weekNumber, offLine + txtW10 + divWX - this.weekDay * pixelsInDay, this.baseTextPosY);
                } else {
                    this.ctx.fillText(week + ' ' + weekNumber, offLine + txtW0 + divWX - this.weekDay * pixelsInDay, this.baseTextPosY);
                }

                --weekNumber;
            }
        },
        drawMonthScheme: function () {
            var i = 0,
                j = 0,
                m = 0,
                y = 0,
                offLine = 0,
                w = 0,
                label = '',
                margin = this.delegate.itemMargin,
                monthNeedDraw = false,
                yearNeedDraw = false,
                monthIntervalPixels = 0,
                ctxWidth = this.ctxWidth,
                divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours,
                count = ctxWidth / divWX,
                offX = this.hourInPixels * this._fX,
                fromX = this._fX * (this.scaleX) / this.divisionInOneHours,
                daysleft = floor2(offX / divWX),    //  Math.floor(offX /  divWX),
                daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur),
                day = this.dayUTC,
                rightOffset = -divWX * this.divisionInDays,
                leftCount = Math.floor(count - daysleft) + 32,
                names = (this.divisionInDays >= 9) ? window['Gantt']['Teamlab_shortmonths'] : window['Gantt']['Teamlab_months'],
                yearstr = window['Gantt']['Localize_strings']['year'] || 'year',
                shortDates = this.scaleX > kScaleUnitMinMonth,
                txtW0 = (divWX - this.ctx.measureText('0').width) * 0.5,
                txtW10 = (divWX - this.ctx.measureText('10').width) * 0.5,
                monthPixels = divWX * 24 * 2,
                pixelsInDay = divWX / this.divisionInDays,
                dayCount = 0,
                lastMonthOffX = fromX * divWX + (-day) * divWX,
                fromRight = undefined,
                baseDateCaption = true;

            for (j = 0, m = this.monthCur, y = this.yearCur; j <= leftCount; ++j, day += this.divisionInDays, ++dayCount) {
                offLine = (j + fromX) * divWX;

                if (offLine > ctxWidth + monthPixels)
                    break;

                if (day >= daysInMouth) {
                    monthNeedDraw = true;
                    monthIntervalPixels = lastMonthOffX;

                    m++; if (m >= 12) { m = 0; ++y; yearNeedDraw = true; }

                    day -= daysInMouth;
                    daysInMouth = this.daysInMonth(m + 1, y);

                    lastMonthOffX = fromX * divWX + (dayCount + 1) * divWX;
                    monthIntervalPixels = lastMonthOffX - monthIntervalPixels;
                }

                if (offLine < -divWX * 2) {
                    continue;
                }

                if (monthNeedDraw) {

                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(lastMonthOffX), margin, 1, this.ctxHeight);

                    label = (!shortDates ? y : y % 100); if (1 === label.length) label = '0' + label;
                    label = names[m] + ' ' + label;

                    // Caption Mounth

                    this.ctx.fillStyle = kDateScaleTextColor;
                    this.ctx.fillText(label, lastMonthOffX + monthIntervalPixels * 0.5 - this.ctx.measureText(label).width * 0.5, this.baseTextPosY);

                    if (yearNeedDraw) {
                        this.ctx.fillStyle = kLinesDateScaleTopColor;
                        this.ctx.fillRect(floor2(lastMonthOffX + monthIntervalPixels), 0, 1, margin);
                        yearNeedDraw = false;
                    }

                    // Caption Year

                    if (6 === m) {
                        label = yearstr + ' ' + y;
                        this.ctx.fillStyle = kDateScaleTextColor;
                        this.ctx.fillText(label, lastMonthOffX + monthIntervalPixels * 0.5 - this.ctx.measureText(label).width * 0.5, this.headTextPosY);
                    }

                    if (undefined === fromRight) {
                        fromRight = lastMonthOffX;
                    }

                    //this.ctx.strokeStyle = '#0000ff';
                    //this.ctx.lineWidth = 2;
                    //
                    //this.ctx.beginPath();
                    //
                    //this.ctx.moveTo(lastMonthOffX , 0);
                    //this.ctx.lineTo(lastMonthOffX , 100);
                    //
                    //this.ctx.stroke();

                    monthNeedDraw = false;
                }
            }

            daysInMouth = this.daysInMonth(this.monthCur + 1, this.yearCur);
            day = this.dayUTC;
            dayCount = this.weekDay - 1;
            lastMonthOffX = (this.daysInMonth(this.monthCur, this.yearCur) + fromX) * divWX;

            for (j = 0, m = this.monthCur + 1, y = this.yearCur; j >= -daysleft - count; --j, day -= this.divisionInDays, --dayCount) {
                offLine = (j + fromX) * divWX;

                if (offLine < -monthPixels)
                    break;

                if (day < 0) {
                    monthIntervalPixels = lastMonthOffX;

                    --m; if (m < 0) { m = 11; --y; yearNeedDraw = true; }
                    daysInMouth = this.daysInMonth(m + 1, y);
                    day += daysInMouth;

                    monthNeedDraw = true;
                    lastMonthOffX = fromX * divWX + dayCount * divWX + (m % 2 - 1) * divWX;

                    monthIntervalPixels = monthIntervalPixels - lastMonthOffX;
                }

                if (offLine > ctxWidth + monthPixels)
                    continue;

                if (monthNeedDraw) {

                    this.ctx.fillStyle = kLinesDateScaleTopColor;
                    this.ctx.fillRect(floor2(lastMonthOffX), margin, 1, this.ctxHeight);

                    label = (!shortDates ? y : y % 100); if (1 === label.length) label = '0' + label;
                    label = names[m] + ' ' + label;

                    this.ctx.fillStyle = kDateScaleTextColor;
                    if (baseDateCaption) {

                        if (fromRight) {
                            this.ctx.fillText(label, (lastMonthOffX + fromRight) * 0.5 - this.ctx.measureText(label).width * 0.5, this.baseTextPosY);
                        }

                        baseDateCaption = false;
                    } else {
                        this.ctx.fillText(label, lastMonthOffX + monthIntervalPixels * 0.5 - this.ctx.measureText(label).width * 0.5, this.baseTextPosY);
                    }

                    monthNeedDraw = false;

                    if (yearNeedDraw) {
                        this.ctx.fillStyle = kLinesDateScaleTopColor;
                        this.ctx.fillRect(floor2(lastMonthOffX + monthIntervalPixels), 0, 1, margin);
                        yearNeedDraw = false;
                    }

                    // Caption Year

                    if (6 === m) {
                        label = yearstr + ' ' + y;
                        this.ctx.fillStyle = kDateScaleTextColor;
                        this.ctx.fillText(label, lastMonthOffX + monthIntervalPixels * 0.5 - this.ctx.measureText(label).width * 0.5, this.headTextPosY);
                    }

                    //this.ctx.strokeStyle = '#ff0000';
                    //this.ctx.lineWidth = 2;
                    //
                    //this.ctx.beginPath();
                    //
                    //this.ctx.moveTo(lastMounthOffX , 0);
                    //this.ctx.lineTo(lastMounthOffX , 100);
                    //
                    //this.ctx.stroke();
                }
            }
        },

        drawLines: function () {
            if (kTypeScaleMonth === this.scaleType)
                return;

            var j, i, m, ind, offLine, tx;

            var currentDate = new Date();
            var dayOff = currentDate.getDay();
            --dayOff;
            if (-1 === dayOff) dayOff = 6;

            var datInWeekOff = currentDate.getDate();
            var monthOff = currentDate.getMonth();

            var divFW = this.hourInPixels * this.scaleX;

            var divWX = this.hourInPixels / this.scaleX * this.divisionInOneHours;
            var count = this.ctxWidth / divWX + 2;
            var offX = this.hourInPixels * this._fX;

            // TODO : optimize

            var fromX = this._fX * this.scaleX / this.divisionInOneHours;
            var fromY = this.linesFromY() + 0.5;
            var index = Math.floor(fromX);
            var offTL = fromX - index;
            this._offTL = fromX - index;

            this.ctx.strokeStyle = kLinesDateScaleTopColor;
            this.ctx.lineWidth = 1;

            this.ctx.beginPath();

            if (kTypeScaleWeek === this.scaleType) {
                var addX = (divWX / 7) * dayOff;
                for (i = 0; i < count; ++i) {
                    tx = Math.floor((i + offTL) * divWX - addX) + 0.5;

                    this.ctx.moveTo(tx, fromY);
                    this.ctx.lineTo(tx, this.ctxHeight);
                }
            }

            //else  if (kTypeScaleDays === this.scaleType || kTypeScaleHours === this.scaleType) {
            //  for (i = 0; i < count; ++i) {
            //      tx = Math.floor((i + offTL) * divWX) + 0.5;
            //      this.ctx.moveTo(tx, fromY);
            //      this.ctx.lineTo(tx, this.ctxHeight);
            //  }
            //}

            this.ctx.closePath();
            this.ctx.stroke();
        },

        calculateScaleType: function () {
            var t = this;

            t.scaleType = kTypeScaleHours;
            t.divisionInDays = 7;
            t.divisionInOneHours = 1;

            if (t.scaleX >= kScaleUnitTwoHours && t.scaleX < kScaleUnitFourHours) {
                t.scaleType = kTypeScaleHours;
                t.divisionInOneHours = 2;
            } else if (t.scaleX >= kScaleUnitFourHours && t.scaleX < kScaleUnitEightHours) {
                t.scaleType = kTypeScaleHours;
                t.divisionInOneHours = 4;
            } else if (t.scaleX >= kScaleUnitEightHours && t.scaleX < kScaleUnitTwentyHours) {
                t.scaleType = kTypeScaleHours;
                t.divisionInOneHours = 8;
            } else if (t.scaleX >= kScaleUnitTwentyHours && t.scaleX < kScaleUnitDay) {
                t.scaleType = kTypeScaleHours;
                t.divisionInOneHours = 12;
            } else if (this.scaleX >= kScaleUnitDay && t.scaleX < kScaleUnitTwoDays) {
                t.scaleType = kTypeScaleDays;
                t.divisionInOneHours = 24;
                t.divisionInDays = 1;
            } else if (this.scaleX >= kScaleUnitTwoDays && t.scaleX < kScaleUnitFourDays) {
                t.scaleType = kTypeScaleWeek;
                t.divisionInOneHours = 24 * 7;
                t.divisionInDays = 7;
            } else if (t.scaleX >= kScaleUnitFourDays && t.scaleX < kScaleUnitFifteenDays + 5) {
                t.scaleType = kTypeScaleMonth;
                t.divisionInOneHours = 24;
                t.divisionInDays = 1;
            } else if (t.scaleX >= kScaleUnitFifteenDays + 5 && t.scaleX < 50000) {

                // TODO :

                t.scaleType = kTypeScaleMonth;
                t.divisionInOneHours = 24 * 7;
                t.divisionInDays = 30;
            }
        },

        // help

        daysInMonth: function (month, year) {
            var MonthDays = [
                [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
                [31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
            ];

            if (month < 1 || month > 12) {
                return 0;
            }

            return MonthDays[((year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0))) ? 1 : 0][month - 1];
        },
        getWeekNumber: function (d) {
            // Copy date so don't modify original
            d.setHours(0, 0, 0);
            // Set to nearest Thursday: current date + 4 - current day number
            // Make Sunday's day number 7
            d.setDate(d.getDate() + 4 - (d.getDay() || 7));
            // Get first day of year
            var yearStart = new Date(d.getFullYear(), 0, 1);
            return Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
            // Calculate full weeks to nearest Thursday
            //var weekNo = Math.ceil(( ( (d - yearStart) / 86400000) + 1)/7)
            // Return array of year and week number
            //return //[d.getFullYear(), weekNo];
        },
        getDayOfYear: function (date) {

            date.setHours(0);
            date.setMinutes(0);
            date.setMilliseconds(0);

            var begin = new Date().setFullYear(new Date().getFullYear(), 0, 1);

            date.setHours(0);
            date.setMinutes(0);
            date.setMilliseconds(1);

            return Math.ceil((date.getTime() - begin) / 86400000);
        },
        getDaysInYear: function (year) {
            var begin = new Date().setFullYear(year, 0, 1),
                end = new Date().setFullYear(year + 1, 0, 1);

            return Math.ceil((end - begin) / 86400000);
        },
        getIsoWeeks: function (year) {
            var d = new Date('31 Dec ' + year),
                first = new Date(d.getFullYear(), 0, 4),
                dayms = 1000 * 60 * 60 * 24,
                numday = ((d - first) / dayms);

            return Math.ceil((numday + first.getDay() + 1) / 7);
        }
    };

    function Task(id, owner, title, performer, description, begin, end, status, customTaskStatus, milestone, priority, subtasks, responsibles, links, beginFail, createdBy) {
        this._id = parseInt(id);
        this._owner = parseInt(owner) || -1;
        this._title = title;
        this.performer = performer;
        this._responsibles = responsibles || [];
        this._description = description;
        this._beginDate = begin;
        this._endDate = end;
        this._status = status || kElementActive;
        this._customTaskStatus = customTaskStatus;
        this.milestone = (undefined !== milestone) ? parseInt(milestone) : -1;
        this._priority = parseInt(priority) || 0;
        this._subtasks = subtasks || 0;
        this.links = links || [];
        this.beginFail = beginFail || false;
        this.endFail = false;
        this.oneDay = false;
        if (end) {
            this.oneDay = (begin.getTime() === end.getTime());
        }

        if (undefined == end) {   //  deadline failed
            this.beginTime = this.calculateTime(this._beginDate);
            this.endTime = this.beginTime;
            this.endFail = true;
        } else {
            this.beginTime = Math.min(this.calculateTime(this._beginDate), this.calculateTime(this._endDate));
            this.endTime = Math.max(this.calculateTime(this._beginDate), this.calculateTime(this._endDate));
        }

        this.duration = this.endTime - this.beginTime;

        // test time
        this.duration = Math.max(this.duration, 24);
        this.endTime = Math.max(this.endTime, this.beginTime + this.duration);
        this.createdBy = createdBy;

        this._isMilestone = false;

        this.titleWidth = -1;

        this.isInEditMode = false;
        this.dropDownWidget = false;

        // cache split lines

        this.stringsBlocks = null;
        this.descStrings = null;

        this.isTask = true;

        if (this._title) { this._title = this._title.replace(/^\s+/, '').replace(/\s+$/, ''); }
        if (this._description) { this._description = this._description.replace(/^\s+/, '').replace(/\s+$/, ''); }

        this._update(true);
    }
    Task.prototype = {
        _id: -1,
        _owner: -1,
        milestone: -1,

        _title: '',
        performer: '',
        _status: kElementActive,
        _responsibles: [],
        _subtasks: 0,
        links: [],

        beginTime: null,
        endTime: null,
        _isMilestone: false,

        titleWidth: -1,

        isInEditMode: false,

        strafe: function (time) {
            var dur = this.endTime - this.beginTime;
            this.beginTime = time;
            this.endTime = time + dur;

            this._update();
        },

        updateBegin: function (time, clampRight, oneDay) {
            if (time + 24 < this.endTime || this.endFail) {          // TODO : min time task one day
                if (clampRight && this.endFail)
                    this.beginTime = Math.min(time, clampRight - 24);
                else
                    this.beginTime = time;

                this.duration = this.endTime - this.beginTime;

                if (this.duration < 0 && this.beginFail) {
                    this.beginFail = false;
                    this.duration = 24;
                    this.endTime = this.beginTime + this.duration;
                }
            } else {
                this.beginTime = this.endTime - 24;
                this.duration = this.endTime - this.beginTime;
            }

            if (oneDay && !this.endFail) {
                this.oneDay = oneDay;
                this.beginTime += 24;
                this.duration = 24;
                this.endTime = this.beginTime + 24;
            }

            this._update(oneDay);
        },
        updateEnd: function (time, oneDay) {
            if (time - 24 > this.beginTime) {                        // TODO : min time task one day
                this.endTime = time;
                this.duration = this.endTime - this.beginTime;
            } else {
                this.endTime = this.beginTime + 24;
                this.duration = this.endTime - this.beginTime;
            }

            if (oneDay) {
                this.oneDay = oneDay;
            }

            this._update(oneDay);
        },

        updateBeginWithDate: function (date, oneDay) {
            date.setHours(0);
            date.setMilliseconds(0);
            date.setMinutes(0);
            date.setSeconds(0);

            this.updateBegin((date.getTime() - Task.curTime) / 3600000, undefined, oneDay);
        },
        updateEndWithDate: function (date, oneDay) {
            date.setHours(0);
            date.setMilliseconds(0);
            date.setMinutes(0);
            date.setSeconds(0);

            this.updateEnd((date.getTime() - Task.curTime) / 3600000, oneDay);
        },

        setTimes: function (begin, end) {
            this.beginTime = begin;
            this.endTime = end;
            this.duration = this.endTime - this.beginTime;

            this._update();
        },

        calculateTime: function (date) {
            // переводит дату в вид понятный для контрола ( в часы от текущей даты )
            // учет на данный момент идет по дням
            if (undefined == Task.curDate) {
                Task.curDate = new Date();
                Task.curDate.setHours(0);
                Task.curDate.setMilliseconds(0);
                Task.curDate.setMinutes(0);
                Task.curDate.setSeconds(0);

                Task.curTime = Task.curDate.getTime();
            }

            return (date.getTime() - Task.curDate.getTime()) / 3600000;    // (1000 * 60) / 60;
        },
        update: function () {
            this.titleWidth = -1;
            this.stringsBlocks = null;
            this.descStrings = null;
        },

        formatBeginDate: function (fullYear) {
            this._beginDate.setHours(12);
            var format = (fullYear) ? this._beginDate.dateFormatFullYear() : this._beginDate.dateFormat();
            this._beginDate.setHours(0);

            return format;
        },
        formatEndDate: function (fullYear) {
            if (this.endFail)
                return '...';

            this._endDate.setHours(12);

            var d = new Date(this._endDate);
            if (this.oneDay) d.setDate(d.getDate() - 1);

            var format = (fullYear) ? d.dateFormatFullYear() : d.dateFormat();
            this._endDate.setHours(0);

            return format;
        },
        formatBeginEndDays: function () {

            this._beginDate.setHours(12);

            var day = this._beginDate.getUTCDate();
            if (day < 10) day = '0' + day.toString();
            var month = this._beginDate.getMonth() + 1;
            if (month < 10) month = '0' + month.toString();

            this._beginDate.setHours(0);

            var txt = day + '.' + month + '. - ';

            this._endDate.setHours(12);

            day = this._endDate.getUTCDate();
            if (day < 10) day = '0' + day.toString();
            month = this._endDate.getMonth() + 1;
            if (month < 10) month = '0' + month.toString();

            this._endDate.setHours(0);

            txt += day + '.' + month + ' (' + (this.endTime - this.beginTime) / 24;
            if ((this.endTime - this.beginTime) / 24 < 2) {
                txt += ' ' + window['Gantt']['Localize_strings']['day'] + ')';
            } else {
                txt += ' ' + window['Gantt']['Localize_strings']['days'] + ')';
            }

            return txt;
        },

        daysPastDue: function () {
            return -Math.floor((this.endTime - 24) / 24);
        },

        getLink: function (parent) {
            var parentTaskId;
            for (var i = this.links.length - 1; i >= 0; --i) {
                parentTaskId = this.links[i].parentTaskId || this.links[i]['parentTaskId'];
                if (parentTaskId == parent)
                    return this.links[i];
            }

            return undefined;
        },

        _update: function (doNotClearOneDay) {
            if (undefined == Task.curDate) {
                Task.curDate = new Date();
                Task.curDate.setHours(0);
                Task.curDate.setMilliseconds(0);
                Task.curDate.setMinutes(0);
                Task.curDate.setSeconds(0);

                Task.curTime = Task.curDate.getTime();
            }

            this._beginDate = new Date((Task.curTime + (this.beginTime + 12) * 3600000));
            this._endDate = new Date((Task.curTime + (this.endTime) * 3600000));

            this._beginDate.setHours(0);
            this._beginDate.setMilliseconds(0);
            this._beginDate.setMinutes(0);
            this._beginDate.setSeconds(0);

            this._endDate.setHours(0);
            this._endDate.setMilliseconds(0);
            this._endDate.setMinutes(0);
            this._endDate.setSeconds(0);

            if (!doNotClearOneDay) { this.oneDay = false; this.beginFail = false; }
        },

        // api

        title: function () {
            return this._title;
        },
        id: function () {
            return this._id;
        },
        owner: function () {
            return this._owner;
        },
        beginDate: function () {
            return this._beginDate;
        },
        endDate: function () {
            if (this.oneDay) return this._beginDate;

            return this._endDate;
        },
        description: function () {
            return this._description;
        },
        isMilestone: function () {
            return this._isMilestone;
        },
        status: function () {
            return this._status;
        },
        customTaskStatus: function () {
            return this._customTaskStatus;
        },
        subtasks: function () {
            return this._subtasks;
        },
        responsibles: function () {
            return this._responsibles;
        },
        isUndefinedEndTime: function () {
            return this.endFail;
        },
        isUndefinedBeginTime: function () {
            return this.beginFail;
        }
    };

    function taskWithIds(p, m, t) { // p, m, t - id
        return new Task(t, p, '', '', '', new Date(), new Date(), kElementActive, undefined, m);
    }

    function Milestone(id, owner, title, description, responsible, deadline, status, isKey) {
        this._id = parseInt(id);
        this._owner = parseInt(owner);
        this._title = title;
        this._description = description;
        this._responsibles = responsible || null;
        this._beginDate = deadline;
        this._endDate = deadline;
        this._status = status || 0;
        this._isKey = isKey || false;

        this.beginTime = this.calculateTime(deadline);
        this.endTime = this.beginTime + 0;
        this.durationMin = kMilestoneMinDurationInHours;

        this.t = [];
        this.sortedByDates = true;

        this.duration = 0;

        this._isMilestone = true;
        this.titleWidth = -1;
        this.titleWithDate = '';

        this.dropDownWidget = false;

        // cache split lines
        this.stringsBlocks = null;
        this.descStrings = null;

        if (this._title) { this._title = this._title.replace(/^\s+/, '').replace(/\s+$/, ''); }
        if (this._description) { this._description = this._description.replace(/^\s+/, '').replace(/\s+$/, ''); }

        this.updateTimes();

        // for second view mode

        this.collapse = false;          // если в вехе нету задач то она всегда раскрыта

        this.isInEditMode = false;
    }
    Milestone.prototype = {
        _id: -1,
        _owner: -1,
        _title: '',
        performer: '',

        _isMilestone: true,

        beginTime: null,
        endTime: null,

        t: [],
        sortedByDates: false,

        titleWidth: -1,
        titleWithDate: '',

        collapse: false,

        add: function (t) {

            // сортировка

            //if (this.sortedByDates) {
            //    var up = false;
            //    for (var i = 0; i < this.t.length; ++i) {
            //        if (t.beginTime < this.t[i].beginTime) {
            //            this.t.splice(i, 0, t); up = true;
            //            break;
            //        }
            //    }
            //
            //    if (!this.t.length || !up)
            //         this.t.push(t);
            // } else {
            this.t.push(t);
            //}

            this.beginTime = Math.min(this.endTime - this.durationMin, Math.min(t.beginTime, this.beginTime));
            this.duration = this.endTime - this.beginTime;

            if (t.endFail) {
                t.updateEnd(this.endTime);
            }

            this.titleWidth = -1;
            this._update();
        },
        addTaskWithIndex: function (t, ind) {
            t.milestone = this._id;
            this.t.splice(ind, 0, t);
        },
        removeTask: function (i) {
            this.t.splice(i, 1);
            this.updateTimes();
        },
        update: function () {
            this.titleWidth = -1;
            this.stringsBlocks = null;
            this.descStrings = null;

            this._update();
        },

        strafe: function (time) {
            var dur = this.endTime - this.beginTime;
            this.beginTime = time;
            this.endTime = time + dur;

            this.titleWidth = -1;

            this._update();
        },

        updateBegin: function (time) {

        },
        updateEnd: function (time) {
            var internalChangeTime = false;

            if (time < this.endTime) {
                var add = this.endTime - time;
                for (var i = 0; i < this.t.length; ++i) {
                    if (kElementCompleted === this.t[i]._status) { continue; }
                    if (this.endTime >= this.t[i].endTime && time <= this.t[i].endTime) {
                        var dur = this.t[i].duration;
                        var end = this.t[i].endTime;
                        var begin = this.t[i].beginTime;

                        this.t[i].endTime = time;
                        this.t[i].beginTime = time - dur;
                        this.t[i]._update();

                        if (end !== this.t[i].endTime || begin !== this.t[i].beginTime)
                            internalChangeTime = true;
                    }
                }
            }

            for (var j = 0; j < this.t.length; ++j) {
                if (this.t[j].endFail) {
                    this.t[j].updateEnd(this.endTime);
                }
            }

            this.endTime = time;
            this.duration = this.endTime - this.beginTime;

            this.updateTimes();

            return internalChangeTime;
        },
        updateEndWithDate: function (date) {
            date.setHours(0);
            date.setMilliseconds(0);
            date.setMinutes(0);
            date.setSeconds(0);

            this.updateEnd((date.getTime() - Task.curTime) / 3600000);
        },

        calculateTime: function (date) {
            // переводит дату в вид понятный для контрола ( в часы от текущей даты )
            // учет на данный момент идет по дням
            if (undefined == Task.curDate) {
                Task.curDate = new Date();
                Task.curDate.setHours(0);
                Task.curDate.setMilliseconds(0);
                Task.curDate.setMinutes(0);
                Task.curDate.setSeconds(0);

                Task.curTime = Task.curDate.getTime();
            }

            return (date.getTime() - Task.curDate.getTime()) / 3600000;    // (1000 * 60) / 60;
        },

        updateTimes: function () {
            this.beginTime = this.endTime;
            this.beginTime = this.endTime - this.durationMin;

            for (var i = 0; i < this.t.length; ++i) {
                this.beginTime = Math.min(this.endTime - this.durationMin, Math.min(this.t[i].beginTime, this.beginTime));
            }

            this.duration = this.endTime - this.beginTime;

            var internalChangeTime = false;

            for (var j = 0; j < this.t.length; ++j) {
                if (this.t[j].endFail) {
                    if (this.t[j].updateEnd(this.endTime))
                        internalChangeTime = true;
                }
            }

            this.titleWidth = -1;
            this._update();

            return internalChangeTime;
        },

        formatBeginDate: function () {
            return this._beginDate.dateFormat();
        },
        formatEndDate: function () {

            this._endDate.setHours(12);

            var format = this._endDate.dateFormat();

            this._endDate.setHours(0);

            return format;
        },

        daysPastDue: function () {
            return -Math.floor((this.endTime - 24) / 24);
        },

        completeTasks: function () {
            var i = 0, count = 0, length = this.t.length;
            for (i = 0, count = 0; i < length; ++i) {
                if (kElementCompleted === this.t[i]._status) {
                    ++count;
                }
            }

            return count;
        },

        _update: function () {
            if (undefined == Task.curDate) {
                Task.curDate = new Date();
                Task.curDate.setHours(0);
                Task.curDate.setMilliseconds(0);
                Task.curDate.setMinutes(0);
                Task.curDate.setSeconds(0);

                Task.curTime = Task.curDate.getTime();
            }

            this._beginDate = new Date((Task.curTime + this.beginTime * 3600000));      //  * 1000 * 60 * 60
            this._endDate = new Date((Task.curTime + this.endTime * 3600000));

            var endMonth = this._endDate.getMonth() + 1;

            this.titleWithDate = this._title +
                ' ' + ((this._endDate.getDate() > 9) ? this._endDate.getDate() : ('0' + this._endDate.getDate())) +
                '.' + ((endMonth > 9) ? endMonth : ('0' + endMonth)) +
                '.' + (this._endDate.getFullYear() % 100);
        },

        setCollapse: function (value) {
            this.collapse = value;

            if (0 === this.t.length) { this.collapse = false; }
        },

        // api

        title: function () {
            return this._title;
        },
        id: function () {
            return this._id;
        },
        tasks: function () {
            return this.t;
        },
        isKey: function () {
            return this._isKey;
        },
        owner: function () {
            return this._owner;
        },
        beginDate: function () {
            return this._beginDate;
        },
        endDate: function () {
            return this._endDate;
        },
        description: function () {
            return this._description;
        },
        isMilestone: function () {
            return this._isMilestone;
        },
        status: function () {
            return this._status;
        },
        responsibles: function () {
            return this._responsibles;
        }
    };
    function milestoneWithIds(p, m) { // p, m - id
        return new Milestone(m, p, '', '', '', new Date(), kElementActive - 1);
    }

    function Project(id, title, description, responsible, creationDate, indexer, isprivate, status) {
        this._id = parseInt(id);
        this._title = title;
        this._description = description ? description : '';
        this._responsibles = responsible || null;
        this.creationDate = creationDate ? creationDate : new Date();

        this.beginTime = null;
        this.endTime = null;
        this.duration = 0;
        this.endFail = false;

        this.descStrings = null;
        this.dropDownWidget = false;

        this.m = [];
        this.t = [];

        this.titleWidth = -1;
        this.sort = true;

        if (this._title) { this._title = this._title.replace(/^\s+/, '').replace(/\s+$/, ''); }
        if (this._description) { this._description = this._description.replace(/^\s+/, '').replace(/\s+$/, ''); }

        // for second view mode

        this.collapse = false; // free tasks
        this.fullCollapse = false; // all content collapse

        this.milestoneSortPush = false;
        this._indexer = indexer;
        this._isprivate = isprivate;
        this._status = status || 0;            //  0 - open, 1 - close, 2 - pause, 1000 - read_only

        if (this.isLocked()) {
            this.collapse = true; // free tasks
            this.fullCollapse = true; // all content collapse
        }
    }
    Project.prototype = {

        addTask: function (t) {

            for (var i = 0; i < this.m.length; ++i) {
                if (this.m[i]._id == t.milestone) {
                    this.m[i].add(t);
                    return -1;
                }
            }

            this.t.push(t);
            this._calcTimes();

            return this.t.length;
        },
        addTaskWithIndex: function (t, ind) {
            this.t.splice(ind, 0, t);
        },

        addMilestone: function (m) {
            this.m.push(m);
            this._calcTimes();

            //            if (this.milestoneSortPush) {
            //                for (var i = 0; i < this.t.length; ++i) {
            //                    if (m._id == this.t[i].milestone) {
            //                        m.add(this.t[i]);
            //                        this.t.splice(i, 1);
            //                        --i;
            //                    }
            //                }
            //
            //                var up = false;
            //                for (var j = 0; j < this.m.length; ++j) {
            //                    if (m.beginTime < this.m[j].beginTime) {
            //                        this.m.splice(j, 0, m); up = true;
            //                        break;
            //                    }
            //                }
            //
            //                if (!this.m.length || !up)
            //                    this.m.push(m);
            //
            //            } else {
            //                this.m.push(m);
            //            }
        },
        addMilestoneWithIndex: function (m, ind) {
            this.m.splice(ind, 0, m);
        },

        removeMilestone: function (i) {
            if (i >= 0 && i < this.m.length)
                this.m.splice(i, 1);
        },
        removeTask: function (i, m) {
            if (undefined === m) {
                if (i >= 0 && i < this.t.length)
                    this.t.splice(i, 1);

                this._calcTimes();
                return;
            }

            if (m >= 0 && m < this.m.length)
                this.m[m].removeTask(i);
        },

        endMilestone: function () {
            if (0 === this.m.length)
                return undefined;

            return this.m.last();
        },
        getMilestone: function (ind) {
            return this.m[ind];
        },

        isEmpty: function () {
            // TODO :
            return !(this.t.length || this.m.length);
        },
        refresh: function () {
            //if (this.m < 1) {return;}
            //this.m = this.sortMilestones();
        },
        formatDate: function () {
            this.creationDate.setHours(12);
            var format = this.creationDate.dateFormat();
            this.creationDate.setHours(0);

            return format;
        },

        completeTasks: function () {
            var i = 0, count = 0, length = this.t.length;
            for (i = 0, count = 0; i < length; ++i) {
                if (kElementCompleted === this.t[i]._status) {
                    ++count;
                }
            }

            return count;
        },

        _calcTimes: function () {
            if (this.t.length) {
                this.endFail = false;

                this.beginTime = this.t[0].beginTime;
                this.endTime = this.t[0].endTime;
                if (this.t[0].endFail) this.endFail = true;

                for (var i = this.t.length - 1; i >= 0; --i) {
                    this.beginTime = Math.min(this.t[i].beginTime, this.beginTime);
                    this.endTime = Math.max(this.t[i].endTime, this.endTime);

                    if (this.t[i].endFail) this.endFail = true;
                }
            }
        },

        setCollapse: function (value) {
            this.collapse = value;

            if (this.isLocked()) { this.collapse = true; }
        },
        setFullCollapse: function (value) {
            this.fullCollapse = value;

            if (this.isLocked()) { this.fullCollapse = true; }
        },
        isLocked: function () {
            return (1 === this._status || 2 === this._status);
        },
        isInReadMode: function () {
            return (kReadModeProject === this._status);
        },

        // api

        title: function () {
            return this._title;
        },
        id: function () {
            return this._id;
        },
        description: function () {
            return this._description;
        },
        responsibles: function () {
            return this._responsibles;
        },
        status: function () {
            return this._status;
        }
    };

    function Storage(timeline) {
        this.p = [];
        this.m = [];
        this.t = [];

        this.pId = -1;
        this.mId = -1;
        this.tId = -1;

        this.sortedByDates = true;

        this.minTime = null;
        this.maxTime = null;

        this.needBuildIndexer = true;
    }
    Storage.prototype = {
        addProject: function (p) {
            this.pId = Math.max(this.pId, p._id);

            this.p.push(p);

            var i, j;
            for (j = 0; j < this.m.length; ++j) {
                if (this.m[j]._owner == p._id) {
                    p.addMilestone(this.m[j]);
                    this.m.splice(this.m.indexOf(j), 1);
                }
            }
            for (i = 0; i < this.t.length; ++i) {
                if (p._id === this.t[i]._owner) {
                    for (j = p.m.length - 1; j >= 0; --j) {
                        if (p.m[j]._id === this.t[i].milestone) {
                            p.m[j].add(this.t[i]);
                            p.refresh();
                            return;
                        }
                    }

                    this.p[i].add(this.t[i]);
                }
            }

            p.refresh();
        },

        addMilestone: function (m) {
            this.mId = Math.max(this.mId, m._id);

            if (!this.minTime) {
                this.minTime = m.beginTime;
            } else {
                this.minTime = Math.min(m.beginTime, this.minTime);
            }

            if (!this.maxTime) {
                this.maxTime = m.endTime;
            } else {
                this.maxTime = Math.max(m.endTime, this.maxTime);
            }

            for (i = 0; i < this.t.length; ++i) {
                if (this.t[i].milestone == m._id) {
                    m.add(this.t[i]);
                    this.t.splice(this.t.indexOf(i), 1);
                }
            }

            for (var i = this.p.length - 1; i >= 0; --i) {
                if (this.p[i]._id == m._owner) {
                    this.p[i].addMilestone(m);
                    this.p[i].refresh();
                    return;
                }
            }

            this.m.push(m);
        },
        addMilestoneWithIndex: function (m, ind) {
            for (var i = this.p.length - 1; i >= 0; --i) {
                if (this.p[i]._id == m._owner) {
                    this.p[i].m.splice(ind, 0, m);
                    return true;
                }
            }

            return false;
        },

        addTask: function (t) {
            this.tId = Math.max(this.tId, t._id);

            if (!this.minTime) {
                this.minTime = t.beginTime;
            } else {
                this.minTime = Math.min(t.beginTime, this.minTime);
            }

            if (!this.maxTime) {
                this.maxTime = t.endFail ? t.beginTime : t.endTime;
            } else {
                this.maxTime = Math.max(t.endFail ? t.beginTime : t.endTime, this.maxTime);
            }

            for (var i = this.p.length - 1; i >= 0; --i) {
                if (this.p[i]._id === t._owner) {
                    for (var j = this.p[i].m.length - 1; j >= 0; --j) {
                        if (this.p[i].m[j]._id === t.milestone) {
                            this.p[i].m[j].add(t);
                            this.p[i].refresh();
                            return true;
                        }
                    }

                    this.p[i].addTask(t);
                    return true;
                }
            }

            this.t.push(t);
            return false;
        },

        addTaskFront: function (t) {
            for (var i = this.p.length - 1; i >= 0; --i) {
                if (this.p[i]._id === t._owner) {
                    for (var j = this.p[i].m.length - 1; j >= 0; --j) {
                        if (this.p[i].m[j]._id === t.milestone) {
                            this.p[i].m[j].t.splice(0, 0, t);
                            // this.p[i].m[j].updateTimes();
                            // this.p[i].m[j].add(t);
                            // this.p[i].refresh();
                            return true;
                        }
                    }
                }
            }

            return false;
        },
        addTaskWithIndex: function (t, ind) {
            for (var i = this.p.length - 1; i >= 0; --i) {
                if (this.p[i]._id === t._owner) {

                    if (-1 === t.milestone || undefined === t.milestone) {
                        this.p[i].t.splice(ind, 0, t);
                        return true;
                    }

                    for (var j = this.p[i].m.length - 1; j >= 0; --j) {
                        if (this.p[i].m[j]._id === t.milestone) {
                            this.p[i].m[j].t.splice(ind, 0, t);
                            // this.p[i].m[j].updateTimes();
                            // this.p[i].m[j].add(t);
                            // this.p[i].refresh();
                            return true;
                        }
                    }
                }
            }

            return false;
        },

        clear: function () {
            this.p = [];
            this.m = [];
            this.t = [];
        },

        projects: function () {
            return this.p;
        },
        endProject: function () {
            if (0 === this.p.length)
                return undefined;

            return this.p.last();
        },

        getTask: function (p, m, t) {

            if (p >= this.p.length)
                return undefined;

            if (undefined === m) {
                if (t >= this.p[p].t.length)
                    return undefined;

                return this.p[p].t[t];
            }

            if (m >= this.p[p].m.length)
                return undefined;

            if (t >= this.p[p].m[m].t.length)
                return undefined;

            return this.p[p].m[m].t[t];
        },
        getMilestone: function (p, m) {
            return this.p[p].m[m];
        },
        getProject: function (p) {
            if (p >= this.p.length)
                return undefined;

            return this.p[p];
        },
        getTasWithInd: function (p, m, t) {
            if (-1 === p || -1 === t)
                return null;

            if (-1 === m) {
                return this.p[p].t[t];
            }

            return this.p[p].m[m].t[t];
        },

        // get element with Id

        taskWithId: function (id) {
            var i, j, p;

            for (p = this.p.length - 1; p >= 0; --p) {

                for (i = this.p[p].t.length - 1; i >= 0; --i) {
                    if (this.p[p].t[i]._id === id) {

                        return {
                            p: p, m: undefined, t: i,
                            taskId: this.p[p].t[i]._id, milestoneId: undefined, projectId: this.p[p]._id,
                            ref: this.p[p].t[i]
                        };
                    }
                }

                for (i = this.p[p].m.length - 1; i >= 0; --i) {

                    for (j = this.p[p].m[i].t.length - 1; j >= 0; --j) {

                        if (this.p[p].m[i].t[j]._id === id) {

                            return {
                                p: p, m: i, t: j,
                                taskId: this.p[p].m[i].t[j]._id, milestoneId: this.p[p].m[i]._id, projectId: this.p[p]._id,
                                ref: this.p[p].m[i].t[j]
                            };
                        }
                    }
                }
            }

            return undefined;
        },
        milestoneWithId: function (id) {
            var i, j, p;

            for (p = this.p.length - 1; p >= 0; --p) {

                for (i = this.p[p].m.length - 1; i >= 0; --i) {
                    if (this.p[p].m[i]._id === id) {

                        return {
                            p: p, m: i,
                            milestoneId: id, projectId: this.p[p]._id,
                            ref: this.p[p].m[i]
                        };
                    }
                }
            }

            return undefined;
        },

        // get ids
        milestoneIds: function (p, m) {
            return { p: this.p[p]._id, m: this.p[p].m[m]._id };
        },
        taskIds: function (p, m, t) {
            if (undefined === m) return { p: this.p[p]._id, t: this.p[p].t[t]._id };

            return { p: this.p[p]._id, m: this.p[p].m[m]._id, t: this.p[p].m[m].t[t]._id };
        },

        // ids
        genRefId: function () {
            return ++this.tId;
        },

        // api
        throughIndexerTasks: function (index) {
            var i, j, length, indexes = [];

            if (this.p.length) {
                for (i = 0; i < this.p[index].m.length; ++i) {

                    for (j = 0; j < this.p[index].m[i].t.length; ++j) {
                        indexes.push(this.p[index].m[i].t[j]._id);
                    }
                }

                length = this.p[index].t.length;

                for (j = 0; j < length; ++j) {
                    indexes.push(this.p[index].t[j]._id);
                }
            }

            return indexes;
        },
        throughIndexerMilestones: function (index) {
            var i, j, length, indexes = [];

            if (this.p.length) {

                length = this.p[index].m.length;

                for (i = 0; i < length; ++i) {
                    indexes.push(this.p[index].m[i]._id);
                }
            }

            return indexes;
        },
        throughIdsIndexes: function (index) {
            return { 'tasks': this.throughIndexerTasks(index), 'milestones': this.throughIndexerMilestones(index) };
        },
        buildWithThroughIndexer: function () {

            var i, j, k, ids, project, tasks, id, length, count, baseIndex = 0, baseEnd = 0, c, s;

            for (i = 0; i < this.p.length; ++i) {
                project = this.p[i];

                if (project._indexer) {

                    // order milestones

                    var milestones = [], haveIndex = [];

                    ids = this.p[i]._indexer['milestones'];
                    if (undefined === ids)
                        continue;

                    if (ids.length) {

                        length = ids.length;

                        for (j = 0; j < length; ++j) {
                            id = ids[j];
                            count = project.m.length;

                            for (k = 0; k < count; ++k) {
                                if (project.m[k].id() === id) {
                                    milestones.push(project.m[k]);
                                    haveIndex.push(k);
                                    break;
                                }
                            }
                        }

                        // есть вехи не имеющие индексов (добавляем в конец)

                        if (this.p[i].m.length > milestones.length) {

                            for (j = 0; j < count; ++j) {
                                if (-1 == haveIndex.indexOf(j)) {
                                    milestones.push(project.m[j]);
                                }
                            }

                            haveIndex = [];
                        }

                        this.p[i].m = milestones;
                    }

                    // order tasks

                    ids = this.p[i]._indexer['tasks'];

                    if (ids.length) {
                        count = project.m.length;

                        for (k = 0; k < count; ++k) {
                            tasks = [];
                            haveIndex = [];

                            length = project.m[k].t.length;
                            if (length) {

                                baseEnd = baseIndex + length;

                                for (c = baseIndex; c < baseEnd; ++c) {
                                    id = ids[c];
                                    for (s = 0; s < length; ++s) {
                                        if (project.m[k].t[s].id() === id) {
                                            tasks.push(project.m[k].t[s]);
                                            haveIndex.push(s);
                                            break;
                                        }
                                    }
                                }

                                // есть задачи не имеющие индексов (добавляем в конец)

                                if (length > tasks.length) {

                                    for (j = 0; j < length; ++j) {
                                        if (-1 == haveIndex.indexOf(j)) {
                                            tasks.push(project.m[k].t[j]);
                                        }
                                    }

                                    haveIndex = [];
                                }

                                project.m[k].t = tasks;
                                baseIndex += length;
                            }
                        }

                        tasks = [];
                        haveIndex = [];

                        length = project.t.length;
                        if (length) {

                            baseEnd = baseIndex + length;

                            for (c = baseIndex; c < baseEnd; ++c) {

                                id = ids[c];

                                for (s = 0; s < length; ++s) {
                                    if (project.t[s].id() === id) {
                                        tasks.push(project.t[s]);
                                        haveIndex.push(s);
                                        break;
                                    }
                                }
                            }

                            // есть задачи не имеющие индексов (добавляем в конец)

                            if (length > tasks.length) {

                                for (j = 0; j < length; ++j) {
                                    if (-1 == haveIndex.indexOf(j)) {
                                        tasks.push(project.t[j]);
                                    }
                                }

                                haveIndex = [];
                            }

                            project.t = tasks;
                        }
                    }
                }
            }
        }
    };

    function ModelController(delegate) {
        this.delegate = delegate;
        this.top = null;
        this.statusElement = null;
    }
    ModelController.prototype = {
        addMilestoneOperation: function (id, p, m) {
            var t = this.delegate;
            if (t) {
                var ids = t.storage.milestoneIds(p, m);
                var milestoneRef = t.storage.getMilestone(p, m);
                this.top = { id: id, p: p, m: m, t: undefined, ids: ids };

                switch (id) {
                    case kHandlerBeforeDeleteMilestone: {
                        if (t.handlers[id]) {
                            t.handlers[id](ids.p, ids.m, milestoneRef);
                        } else {
                            this.deleteElement(this.top);
                        }
                    } break;

                    case kHandlerBeforeChangeMilestoneStatus: {
                        if (t.handlers[id]) {
                            t.handlers[id](ids.p, ids.m, milestoneRef);
                        } else {
                            this.completeElement(this.top)
                        }
                    } break;
                }
            }
        },
        addTaskOperation: function (id, p, m, t, cs) {
            var tr = this.delegate;
            if (tr) {

                var taskRef, milestoneRef, ids, childTask, parentTask;

                if (kHandlerBeforeDeleteTaskLink === id) {
                    this.top = { id: id, link: p };
                    if (tr.handlers[id]) {
                        childTask = tr.storage.taskWithId(p['dependenceTaskId']);
                        parentTask = tr.storage.taskWithId(p['parentTaskId']);
                        tr.handlers[id](p, childTask.ref, parentTask.ref);    //  p - link
                    }

                    return;
                } else if (kHandlerBeforeAddTaskLink === id) {
                    this.top = { id: id, link: p };
                    if (tr.handlers[id]) {
                        tr.handlers[id](p, p['task'], p['parent']);    //  p - linkLineEdit
                    }

                    return;
                } else if (kHandlerBeforeMoveTaskWithLinks === id) {
                    this.top = { id: id, move: p };
                    if (tr.handlers[id]) {
                        tr.handlers[id](p);    //  p - moveData {}
                    }

                    return;
                } else if (kHandlerBeforeChangeResponsibles === id) {
                    this.top = { id: id, element: p };
                    if (tr.handlers[id]) {
                        tr.handlers[id](p);    //  p - task or milestone {}
                    }

                    return;
                }

                ids = tr.storage.taskIds(p, m, t);
                taskRef = tr.storage.getTask(p, m, t);

                this.top = { id: id, p: p, m: m, t: t, ids: ids };

                switch (id) {
                    case kHandlerBeforeDeleteTask: {
                        if (tr.handlers[id]) {
                            tr.handlers[id](ids.p, ids.m, ids.t, taskRef, this.collectLinks(p, m, taskRef._id));
                        } else {
                            this.deleteElement(this.top);
                        }
                    } break;

                    case kHandlerBeforeChangeTaskStatus: {
                        if (tr.handlers[id]) {
                            tr.handlers[id](ids.p, ids.m, ids.t, taskRef, cs);
                        } else {
                            this.completeElement(this.top);
                        }
                    } break;
                }
            }
        },

        // apply operation

        finalize: function (arg) {
            if (this.top) {
                switch (this.top.id) {
                    case kHandlerBeforeDeleteTask:
                        { this.deleteElement(this.top); } break;

                    case kHandlerBeforeDeleteMilestone:
                        { this.deleteElement(this.top); } break;

                    case kHandlerBeforeChangeMilestoneStatus:
                        { this.completeElement(this.top); } break;

                    case kHandlerBeforeChangeTaskStatus:
                        { this.completeElement(this.top, arg); } break;

                    case kHandlerBeforeAddTaskLink:
                        { this.addLink(this.top.link); } break;

                    case kHandlerBeforeDeleteTaskLink:
                        { this.deleteLink(this.top.link); } break;

                    case kHandlerBeforeMoveTaskWithLinks:
                        {
                            if ('SaveConnections' === arg) {
                                this.moveGroupTasks(this.top.move);
                            } else {
                                this.moveTask(this.top.move);
                            }
                        } break;
                }

                this.top = null;
            }
        },
        clear: function () {
            var t = this.delegate;
            if (t) {
                t._undoManager.reset();
                t.storage.clear();
                t.hitLink = undefined;
                t.zoomBar.needRepaint();
                t.offWidgets();
                t.offMenus();
                t.updateContent();

                t.viewController().strafeToDay();
            }
        },

        // operations

        deleteElement: function (element) {
            var t = this.delegate;
            if (t) {

                var ref = null;
                var linksToRemove = null;
                var i = 0;

                if (element.p === undefined) element.p = -1;
                if (element.m === undefined) element.m = -1;
                if (element.t === undefined) element.t = -1;

                t.rightScroll.save();

                // delete milestone

                if (-1 !== element.m && -1 === element.t && -1 !== element.p) {
                    ref = t.storage.getMilestone(element.p, element.m);
                    if (ref) {

                        ref.dropDownWidget = false;

                        // undo

                        t._undoManager.add(kOperationDeleteMilestone,
                            {
                                p: element.p, m: element.m, t: ref, index: element.m,
                                taskId: undefined, milestoneId: element.ids.m, projectId: element.ids.m
                            });

                        t._undoManager.updateOperation(0, ref);

                        var tasks = t.storage.p[element.p].t;
                        for (i = 0; i < ref.t.length; ++i) {
                            tasks.push(ref.t[i]); tasks.last().milestone = -1;
                        }

                        t.storage.p[element.p].removeMilestone(element.m);
                    }
                }

                // delete project task in free zone

                if (-1 === element.m && -1 !== element.t && -1 !== element.p) {
                    t.taskDescWidget.check(null);

                    ref = t.storage.getTask(element.p, undefined, element.t);
                    if (ref) {
                        linksToRemove = this.collectLinks(element.p, undefined, element.ids.t);      // NOTE: optimize

                        // undo

                        // NOTE: отключено временно

                        //t._undoManager.add(kOperationDeleteTask,
                        //    {p: element.p, m: undefined, t: ref, index: element.t,
                        //        taskId: element.ids.t, milestoneId: element.ids.m, projectId: element.ids.m,
                        //        linksToRemove: linksToRemove});

                        //t._undoManager.updateOperation(0, ref);

                        this.removeLinks(element.p, undefined, element.ids.t);

                        t.storage.p[element.p].removeTask(element.t);

                        t._undoManager.reset();
                    }
                }

                // delete milestone task

                if (-1 !== element.m && -1 !== element.t && -1 !== element.p) {
                    t.taskDescWidget.check(null);

                    ref = t.storage.getTask(element.p, element.m, element.t);
                    if (ref) {

                        linksToRemove = this.collectLinks(element.p, element.m, element.ids.t);    // NOTE: optimize

                        // undo

                        // NOTE: отключено временно

                        //t._undoManager.add(kOperationDeleteTask,
                        //    {p: element.p, m: element.m, t: ref, index: element.t,
                        //        taskId: element.ids.t, milestoneId: element.ids.m, projectId: element.ids.m,
                        //        linksToRemove: linksToRemove});

                        //t._undoManager.updateOperation(0, ref);

                        this.removeLinks(element.p, element.m, element.ids.t);

                        t.storage.p[element.p].m[element.m].removeTask(element.t);

                        t._undoManager.reset();
                    }
                }

                t.offMenus();
                t.offWidgets();
                t.painter.clearZones(true);

                t.needUpdateContent = true;
                t.leftPanelController().clearFocus();
                t.update();
            }
        },
        completeElement: function (element, cs) {
            var t = this.delegate, ref = null, oldStatus, newStatus;
            if (t) {
                if (element.p === undefined) element.p = -1;
                if (element.m === undefined) element.m = -1;
                if (element.t === undefined) element.t = -1;

                // milestone

                if (-1 !== element.m && -1 === element.t && -1 !== element.p) {
                    ref = t.storage.getMilestone(element.p, element.m);
                    if (ref) {

                        newStatus = ref._status;
                        oldStatus = ref._status;

                        if (newStatus != kElementCompleted - 1) {
                            newStatus = kElementCompleted - 1;
                        } else {
                            newStatus = kElementActive - 1;
                        }

                        t._undoManager.add(kOperationChangeMilestoneStatus, {
                            p: element.p,
                            m: element.m,
                            t: ref,
                            index: element.m,
                            newStatus: newStatus,
                            oldStatus: oldStatus,
                            taskId: undefined,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        ref._status = newStatus;

                        //

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                // task in free zone

                if (-1 === element.m && -1 !== element.t && -1 !== element.p) {
                    ref = t.storage.getTask(element.p, undefined, element.t);
                    if (ref) {

                        newStatus = cs;
                        oldStatus = ref._status;

                        // undo

                        t._undoManager.add(kOperationChangeTaskStatus,
                            {
                                p: element.p, m: undefined, t: ref, index: element.t,
                                newStatus: newStatus, oldStatus: oldStatus,
                                taskId: element.ids.t, milestoneId: element.ids.m, projectId: element.ids.m
                            });

                        var cs1 = findCustomStatus(function (item) {
                            return item.id === newStatus;
                        });

                        ref._status = cs1.statusType;
                        ref._customTaskStatus = newStatus;

                        //

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                // milestone task

                if (-1 !== element.m && -1 !== element.t && -1 !== element.p) {

                    ref = t.storage.getTask(element.p, element.m, element.t);
                    if (ref) {

                        newStatus = cs;
                        oldStatus = ref._status;

                        // undo

                        t._undoManager.add(kOperationChangeTaskStatus,
                            {
                                p: element.p, m: element.m, t: ref, index: element.t,
                                newStatus: newStatus, oldStatus: oldStatus,
                                taskId: element.ids.t, milestoneId: element.ids.m, projectId: element.ids.m
                            });

                        var cs = ASC.Projects.Master.customStatuses.find(function (item) {
                            return item.id === newStatus;
                        });

                        ref._status = cs.statusType;
                        ref._customTaskStatus = newStatus;

                        //

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                t.leftPanelController().clearFocus();
                t.updateData();
            }
        },
        checkStatus: function (element) {

            // только для задач в вехе

            var t = this.delegate, task = null, milestone = null;
            if (t) {
                if (element.p === undefined) element.p = -1;
                if (element.m === undefined) element.m = -1;
                if (element.t === undefined) element.t = -1;

                if (-1 !== element.m && -1 !== element.t && -1 !== element.p) {

                    // task = t.storage.getTask(element.p, element.m, element.t);

                    milestone = t.storage.getMilestone(element.p, element.m);
                    if (milestone) {
                        if (1 === milestone._status) {
                            return false;
                        }
                    }
                }
            }

            return true;
        },
        changeTime: function (element, date, direction, oneday) {
            var t = this.delegate, ref = null, oldStatus, newStatus;
            if (t) {

                if (element.p === undefined) element.p = -1;
                if (element.m === undefined) element.m = -1;
                if (element.t === undefined) element.t = -1;

                // milestone

                if (-1 !== element.m && -1 === element.t && -1 !== element.p) {
                    ref = t.storage.getMilestone(element.p, element.m);
                    if (ref) {

                        t._undoManager.add(kOperationChangeTimeMilestone, {
                            p: element.p,
                            m: element.m,
                            t: ref,
                            index: element.m,
                            newStatus: newStatus,
                            oldStatus: oldStatus,
                            taskId: undefined,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        ref.updateEndWithDate(date);

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                // task in free zone

                if (-1 === element.m && -1 !== element.t && -1 !== element.p) {
                    ref = t.storage.getTask(element.p, undefined, element.t);
                    if (ref) {

                        // undo

                        t._undoManager.add(kOperationChangeTimeTask, {
                            p: element.p,
                            m: undefined,
                            t: ref, index: element.t,
                            newStatus: newStatus,
                            oldStatus: oldStatus,
                            taskId: element.ids.t,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        if (direction) {
                            ref.updateEndWithDate(date, oneday);
                            ref.endFail = false;
                        } else {
                            ref.updateBeginWithDate(date, oneday);
                        }

                        //

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                // milestone task

                if (-1 !== element.m && -1 !== element.t && -1 !== element.p) {
                    ref = t.storage.getTask(element.p, element.m, element.t);
                    if (ref) {

                        // undo

                        t._undoManager.add(kOperationChangeTimeTask,
                            {
                                p: element.p, m: element.m, t: ref, index: element.t,
                                newStatus: newStatus, oldStatus: oldStatus,
                                taskId: element.ids.t, milestoneId: element.ids.m, projectId: element.ids.m
                            });

                        if (direction) {
                            ref.updateEndWithDate(date, oneday);
                            ref.endFail = false;
                        } else {
                            ref.updateBeginWithDate(date, oneday);
                        }

                        //

                        t._undoManager.updateOperation(0, ref);
                    }
                }

                t.updateData();
            }

        },

        changeResponsible: function (element, data) {

            function isEqualResponsiblies(l, r) {
                if (l.length != r.length) return false;

                var length = l.length, uids = [], i = 0;

                for (i = 0; i < length; ++i) {
                    uids.push(l[i].id);
                }

                for (i = 0; i < length; ++i) {
                    if (-1 === uids.indexOf(r[i].id))
                        return false;
                }

                return true;
            }

            var t = this.delegate, ref = null, oldStatus, newStatus;
            if (t) {

                if (element.p === undefined) element.p = -1;
                if (element.m === undefined) element.m = -1;
                if (element.t === undefined) element.t = -1;

                // milestone

                if (-1 !== element.m && -1 === element.t && -1 !== element.p) {
                    ref = t.storage.getMilestone(element.p, element.m);
                    if (ref) {
                        if (element.ref._responsibles && data) {
                            if (element.ref._responsibles.id === data.id) {
                                return;
                            }
                        }

                        t._undoManager.add(kOperationChangeResponsible, {
                            p: element.p,
                            m: element.m,
                            isMilestone: true,
                            index: element.m,
                            newResponsibles: data,
                            oldResponsibles: element.ref._responsibles,
                            taskId: undefined,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        ref._responsibles = data;

                        //

                        t._undoManager.updateOperation(true, ref);
                    }
                }

                // task in free zone

                if (-1 === element.m && -1 !== element.t && -1 !== element.p) {
                    ref = t.storage.getTask(element.p, undefined, element.t);
                    if (ref) {

                        if (isEqualResponsiblies(element.ref._responsibles, data)) {
                            return;
                        }

                        // undo

                        t._undoManager.add(kOperationChangeResponsible, {
                            p: element.p,
                            m: undefined,
                            index: element.t,
                            newResponsibles: data,
                            oldResponsibles: element.ref._responsibles,
                            taskId: element.ids.t,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        ref._responsibles = data;

                        //

                        t._undoManager.updateOperation(true, ref);
                    }
                }

                // milestone task

                if (-1 !== element.m && -1 !== element.t && -1 !== element.p) {

                    ref = t.storage.getTask(element.p, element.m, element.t);
                    if (ref) {

                        if (isEqualResponsiblies(element.ref._responsibles, data)) {
                            return;
                        }

                        // undo

                        t._undoManager.add(kOperationChangeResponsible, {
                            p: element.p,
                            m: element.m,
                            index: element.t,
                            newResponsibles: data,
                            oldResponsibles: element.ref._responsibles,
                            taskId: element.ids.t,
                            milestoneId: element.ids.m,
                            projectId: element.ids.m
                        });

                        ref._responsibles = data;

                        //

                        t._undoManager.updateOperation(true, ref);
                    }
                }

                t.updateData();
            }
        },

        applyNewAddTaskId: function (id) {
            var t = this.delegate, head = null, ref = null;
            if (t) {
                id = parseInt(id);
                head = t._undoManager.currentOperation();
                if (head && !isNaN(id)) {
                    head.arg.taskId = id;
                    head.arg.t._id = id;

                    ref = t.storage.getTask(head.arg.p, head.arg.m, head.arg.index);
                    if (ref) {
                        ref._id = id
                    }
                }
            }
        },
        applyNewAddMilestoneId: function (id, responsibles) {
            var t = this.delegate, head, ref;
            if (t) {
                id = parseInt(id);
                head = t._undoManager.currentOperation();
                if (head && !isNaN(id)) {
                    head.arg.milestoneId = id;
                    head.arg.t._id = id;
                    if (responsibles) {
                        head._responsibles = responsibles;
                    }

                    ref = t.storage.getMilestone(head.arg.p, head.arg.index);
                    if (ref) {
                        ref._id = id;
                        if (responsibles) {
                            ref._responsibles = responsibles;
                        }
                    }

                    t.updateData();
                }
            }
        },

        // create chart items

        addTask: function (id, owner, title, performer, description, begin, end, status, customTaskStatus, milestone, priority, subtasks, responsibles, links, isUndoOperation, beginFail, createdBy) {
            var t = this.delegate,
                task = null,
                element = null;

            if (t) {
                task = new Task(id, owner, title, performer, description, begin, end, status, customTaskStatus, milestone, priority, subtasks, responsibles, links, beginFail, createdBy);
                t.storage.addTask(task);

                // undo

                if (isUndoOperation) {
                    element = t.storage.taskWithId(id);
                    if (element) {
                        t._undoManager.add(kOperationAddTask, {
                            p: element.p, m: element.m, t: element.ref, index: element.t,
                            taskId: element.taskId, milestoneId: element.milestoneId, projectId: element.projectId
                        });
                    }
                }
            }
        },
        addMilestone: function (id, owner, title, description, responsible, deadline, status, isKey, isUndoOperation) {
            var t = this.delegate,
                milestone = null,
                element = null;

            if (t) {
                milestone = new Milestone(id, owner, title, description, responsible, deadline, status, isKey);
                t.storage.addMilestone(milestone);

                // undo

                if (isUndoOperation) {
                    element = t.storage.milestoneWithId(id);
                    if (element) {
                        t._undoManager.add(kOperationAddMilestone, {
                            p: element.p,
                            m: element.m,
                            t: element.ref,
                            index: element.m,
                            milestoneId: element.milestoneId,
                            projectId: element.projectId
                        });
                    }
                }
            }
        },
        addProject: function (id, title, description, responsible, creationDate, indexer, isprivate, status) {
            var t = this.delegate, project = null;
            if (t) {
                project = new Project(id, title, description, responsible, creationDate, indexer, isprivate, status);
                t.storage.addProject(project);
            }
        },

        // move task

        moveTask: function (data) {
            var t = this.delegate, addInd = 0, scrollY = 0, milestoneRef = null;
            if (t) {

                // из вехи в зону свободных задач

                if ('MtoF' === data.type) {

                    this.removeLinks(data.fromProject, data.fromMilestone, data.taskId);

                    data.t.milestone = -1;

                    addInd = t.storage.p[0].addTask(data.t);
                    t.storage.p[0].m[data.m].removeTask(data.index);

                    scrollY = t.getElementPosVertical(0, -1, addInd);
                    t.animator.moveToY(Math.min(Math.max(0, scrollY - t.ctxHeight * 0.5), t.rightScroll._maxValue - t.rightScroll.viewWidth));

                    data.place = addInd - 1;
                }

                //  из вехи в веху

                if ('MtoM' === data.type) {

                    this.removeLinks(data.fromProject, data.fromMilestone, data.taskId);

                    t.storage.p[data.toProject].m[data.toMilestone].addTaskWithIndex(data.t, data.place);
                    t.storage.p[data.fromProject].m[data.fromMilestone].removeTask(data.index);
                    t.storage.p[data.fromProject].m[data.fromMilestone].updateTimes();
                    t.storage.p[data.toProject].m[data.toMilestone].updateTimes();
                }

                // из свободной зоны в веху

                if ('FtoM' === data.type) {

                    this.removeLinks(data.fromProject, data.fromMilestone, data.taskId);

                    milestoneRef = t.storage.p[data.toProject].m[data.toMilestone];
                    if (milestoneRef) {
                        if (data.t.endFail && data.t.beginTime >= milestoneRef.endTime) {
                            data.t.updateBegin(milestoneRef.endTime - 24);
                        }
                    }

                    t.storage.p[data.toProject].m[data.toMilestone].addTaskWithIndex(data.t, data.place);
                    t.storage.p[data.fromProject].removeTask(data.index);
                    milestoneRef.updateTimes();

                    // undo

                    t._undoManager.add(kOperationMoveTask, data);

                    if (data.t.endFail && data.t.beginTime >= milestoneRef.endTime) {
                        data.t.updateBegin(milestoneRef.endTime - 24);
                    }

                    t._undoManager.performTop();
                    t.updateData();

                    return;
                }

                t._undoManager.add(kOperationMoveTask, data);
                t._undoManager.performTop();

                t.updateData();
            }
        },
        moveGroupTasks: function (data) {
            var t = this.delegate, i = 0, indTo, indCur, ids, tasksNeedMove, scrollY, milestoneRef;
            if (t) {

                ids = this.collectRelatedItems(data.fromProject, data.fromMilestone, data.taskId);
                tasksNeedMove = this.collectTasksWithIds(data.fromProject, data.fromMilestone, ids);

                data.group = tasksNeedMove;
                data.tasksIds = ids;

                // из вехи в зону свободных задач

                if ('MtoF' === data.type) {

                    data.sourceMilestone = t.storage.p[data.fromProject].m[data.fromMilestone]._id;
                    data.destMilestone = undefined;

                    for (i = 0; i < tasksNeedMove.length; ++i) {
                        tasksNeedMove[i].task.milestone = -1;
                        indCur = t.storage.p[data.fromProject].addTask(tasksNeedMove[i].task, true);

                        if (data.taskId === tasksNeedMove[i].id) { indTo = indCur; }
                    }

                    for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                        t.storage.p[data.fromProject].m[data.fromMilestone].removeTask(tasksNeedMove[i].index);
                    }

                    t.storage.p[data.fromProject].m[data.fromMilestone].updateTimes();
                    t.storage.p[data.fromProject]._calcTimes();

                    scrollY = t.getElementPosVertical(0, -1, indTo);
                    t.animator.moveToY(Math.min(Math.max(0, scrollY - t.ctxHeight * 0.5), t.rightScroll._maxValue - t.rightScroll.viewWidth));
                }

                //  из вехи в веху

                if ('MtoM' === data.type) {

                    data.sourceMilestone = t.storage.p[data.fromProject].m[data.fromMilestone]._id;
                    data.destMilestone = t.storage.p[data.toProject].m[data.toMilestone]._id;

                    for (i = 0; i < tasksNeedMove.length; ++i) {
                        tasksNeedMove[i].task.milestone = data.destMilestone;
                        t.storage.p[data.toProject].m[data.toMilestone].t.push(tasksNeedMove[i].task);
                    }

                    for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                        t.storage.p[data.fromProject].m[data.fromMilestone].removeTask(tasksNeedMove[i].index);
                    }

                    t.storage.p[data.fromProject].m[data.fromMilestone].updateTimes();
                    t.storage.p[data.toProject].m[data.toMilestone].updateTimes();
                }

                // из свободной зоны в веху

                if ('FtoM' === data.type) {

                    data.sourceMilestone = undefined;
                    data.destMilestone = t.storage.p[data.toProject].m[data.toMilestone]._id;

                    milestoneRef = t.storage.p[data.toProject].m[data.toMilestone];

                    for (i = 0; i < tasksNeedMove.length; ++i) {
                        if (tasksNeedMove[i].task.endFail && tasksNeedMove[i].task.beginTime >= milestoneRef.endTime) {
                            tasksNeedMove[i].task.updateBegin(milestoneRef.endTime - 24);
                        }

                        tasksNeedMove[i].task.milestone = data.destMilestone;
                        t.storage.p[data.toProject].m[data.toMilestone].t.push(tasksNeedMove[i].task);
                    }

                    for (i = tasksNeedMove.length - 1; i >= 0; --i) {
                        t.storage.p[data.fromProject].removeTask(tasksNeedMove[i].index);
                    }

                    milestoneRef.updateTimes();
                    t.storage.p[data.fromProject]._calcTimes();
                }

                t._undoManager.add(kOperationMoveGroupTasks, data);
                t._undoManager.performTop();
            }
        },

        // link

        addLink: function (link) {
            var t = this.delegate,
                linkType = kLinkBeginEnd,
                parentId = 0,
                dependenceId = 0,
                task = null,
                linkObj = {},
                taskExt = null,
                depIndexer = null,
                parIndexer = null;

            if (t) {
                if (kTaskSideLeft === link['parentSide'] && kTaskSideRight === link['side']) {
                    task = link['parent'];
                    parentId = link['task']._id;
                    dependenceId = link['parent']._id;
                } else {
                    if (kTaskSideLeft === link['parentSide'] && kTaskSideLeft === link['side']) {
                        linkType = kLinkBeginBegin;
                    } else if (kTaskSideRight === link['parentSide'] && kTaskSideRight === link['side']) {
                        linkType = kLinkEndEnd;
                    }

                    task = link['task'];
                    parentId = link['parent']._id;
                    dependenceId = link['task']._id;
                }

                if (kEndBeginLinksEditMode) {
                    if (kLinkBeginEnd !== linkType) {
                        return;
                    }
                }

                linkObj = {};

                linkObj['dependenceTaskId'] = dependenceId;
                linkObj['linkType'] = linkType;
                linkObj['parentTaskId'] = parentId;
                linkObj['type'] = 'link';

                task.links.push(linkObj);

                taskExt = t.storage.taskWithId(dependenceId);
                if (taskExt) {
                    depIndexer = t.storage.taskWithId(dependenceId);
                    parIndexer = t.storage.taskWithId(parentId);

                    t._undoManager.add(kOperationAddTaskLink, {
                        p: taskExt.p,
                        m: taskExt.m,
                        t: taskExt.t,
                        link: taskExt.ref.links[taskExt.ref.links.length - 1],
                        linkIndex: taskExt.ref.links.length - 1,
                        taskId: taskExt.taskId,
                        milestoneId: taskExt.milestoneId,
                        projectId: taskExt.projectId,
                        depIndexer: depIndexer,
                        parIndexer: parIndexer
                    });
                }
            }
        },
        deleteLink: function (link) {
            var t = this.delegate, i = 0, dep, par, refdep, refpar, base, task = null, indexLink = -1;
            if (t) {
                if (link) {
                    base = link.dependenceTaskId || link['dependenceTaskId'];
                    task = t.storage.taskWithId(base);
                    if (task) {
                        for (i = task.ref.links.length - 1; i >= 0; --i) {
                            dep = link.dependenceTaskId || link['dependenceTaskId'];   // minimizator fix
                            par = link.parentTaskId || link['parentTaskId'];           // minimizator fix

                            refdep = task.ref.links[i].dependenceTaskId || task.ref.links[i]['dependenceTaskId'];   // minimizator fix
                            refpar = task.ref.links[i].parentTaskId || task.ref.links[i]['parentTaskId'];           // minimizator fix

                            if (refdep === dep && refpar === par) {
                                indexLink = i;
                                break;
                            }
                        }

                        if (-1 !== indexLink) {

                            // if (t.hitLink) {

                            //dep = t.hitLink.dependenceTaskId || t.hitLink['dependenceTaskId'];   // minimizator fix
                            //par = t.hitLink.parentTaskId || t.hitLink['parentTaskId'];           // minimizator fix

                            // if (t.hitLink)
                            // t.hitLink = undefined;
                            // }

                            // undo только для открытых задач

                            //var fromT = t.storage.taskWithId(par);
                            //if (kElementCompleted !== task.ref.status() && kElementCompleted !== fromT.ref.status()) {

                            // undo

                            t._undoManager.add(kOperationDeleteTaskLink, {
                                p: task.p,
                                m: task.m,
                                t: task.t,
                                link: task.ref.links[indexLink],
                                linkIndex: indexLink,
                                taskId: task.taskId,
                                milestoneId: task.milestoneId,
                                projectId: task.projectId
                            });
                            // }

                            task.ref.links.splice(indexLink, 1);

                            t.updateData();
                        }
                    }
                }
            }
        },

        collectLinks: function (p, m, id) {        // id - taskId
            var t = this.delegate,
                links = [],
                i = 0,
                link = null,
                linkInd = 0,
                dependenceTaskId = 0,
                parentTaskId = 0,
                milestone = null,
                project = null;

            if (t) {
                if (undefined !== m) {
                    milestone = t.storage.getMilestone(p, m);
                    for (i = milestone.t.length - 1; i >= 0; --i) {
                        for (linkInd = milestone.t[i].links.length - 1; linkInd >= 0; --linkInd) {

                            link = milestone.t[i].links[linkInd];
                            parentTaskId = link.parentTaskId || link['parentTaskId'];
                            dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];

                            if ((id === dependenceTaskId || id === parentTaskId) && milestone.t[i].id() === dependenceTaskId) {
                                links.push({ link: link, index: linkInd, ind: i });
                            }
                        }
                    }
                } else {
                    project = t.storage.p[p];
                    for (i = project.t.length - 1; i >= 0; --i) {
                        for (linkInd = project.t[i].links.length - 1; linkInd >= 0; --linkInd) {

                            link = project.t[i].links[linkInd];
                            parentTaskId = link.parentTaskId || link['parentTaskId'];
                            dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];

                            if ((id === dependenceTaskId || id === parentTaskId) && project.t[i].id() === dependenceTaskId) {
                                links.push({ link: link, index: linkInd, ind: i });
                            }
                        }
                    }
                }
            }

            return links;
        },
        removeLinks: function (p, m, id) {        // id - taskId
            var t = this.delegate,
                i = 0,
                link = null,
                linkInd = 0,
                dependenceTaskId = 0,
                parentTaskId = 0,
                milestone = null,
                project = null;

            if (t) {
                if (undefined !== m) {
                    milestone = t.storage.getMilestone(p, m);
                    for (i = milestone.t.length - 1; i >= 0; --i) {
                        for (linkInd = milestone.t[i].links.length - 1; linkInd >= 0; --linkInd) {

                            link = milestone.t[i].links[linkInd];
                            parentTaskId = link.parentTaskId || link['parentTaskId'];
                            dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];

                            if (id === dependenceTaskId || id === parentTaskId) {
                                milestone.t[i].links.splice(linkInd, 1);
                            }
                        }
                    }
                } else {
                    project = t.storage.p[p];
                    for (i = project.t.length - 1; i >= 0; --i) {
                        for (linkInd = project.t[i].links.length - 1; linkInd >= 0; --linkInd) {

                            link = project.t[i].links[linkInd];
                            parentTaskId = link.parentTaskId || link['parentTaskId'];
                            dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];

                            if (id === dependenceTaskId || id === parentTaskId) {
                                link = project.t[i].links.splice(linkInd, 1);
                            }
                        }
                    }
                }
            }
        },

        collectRelatedItems: function (p, m, id) {
            var t = this.delegate,
                i,
                j,
                k,
                needAdd,
                link,
                linkInd,
                tasks,
                items = [],
                hash = {},
                hashLinks,
                parentTaskId,
                dependenceTaskId,
                cur = null,
                subItems = [],
                internal = [];

            if (t) {
                tasks = this.getTasks(p, m);
                if (0 === tasks.length) {
                    return items;
                }

                // разобьем на элементы в которых есть однозначное соотвествение с элементов его вся связи

                for (i = tasks.length - 1; i >= 0; --i) {
                    for (linkInd = tasks[i].links.length - 1; linkInd >= 0; --linkInd) {

                        link = tasks[i].links[linkInd];
                        parentTaskId = link.parentTaskId || link['parentTaskId'];           // js-minimizator
                        dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];   // js-minimizator

                        if (!hash[dependenceTaskId]) {
                            hash[dependenceTaskId] = {
                                links: []
                            };
                        }

                        hashLinks = hash[dependenceTaskId].links;
                        if (hashLinks) {
                            needAdd = true;
                            for (k = hashLinks.length - 1; k >= 0; --k) {
                                if (hashLinks[k] === parentTaskId) {
                                    needAdd = false; break;
                                }
                            }

                            if (needAdd) {
                                hashLinks.push(parentTaskId);
                            }
                        }

                        if (!hash[parentTaskId]) {
                            hash[parentTaskId] = { links: [] };
                        }
                        hashLinks = hash[parentTaskId].links;
                        if (hashLinks) {
                            needAdd = true;
                            for (k = hashLinks.length - 1; k >= 0; --k) {
                                if (hashLinks[k] === dependenceTaskId) {
                                    needAdd = false; break;
                                }
                            }

                            if (needAdd) {
                                hashLinks.push(dependenceTaskId);
                            }
                        }
                    }
                }

                cur = hash[id];
                if (!cur) { return []; }

                subItems = [], internal = [];

                // добавляем текущий индекс (основной элемент)
                items.push(id);
                subItems.push(id);

                // вся связи с основным элементом
                for (i = cur.links.length - 1; i >= 0; --i) {
                    subItems.push(cur.links[i]);
                    items.push(cur.links[i]);
                }

                while (true) {

                    internal = [];

                    for (i = subItems.length - 1; i >= 0; --i) {
                        if (id === subItems[i]) {
                            continue;
                        }

                        cur = hash[subItems[i]];

                        for (j = cur.links.length - 1; j >= 0; --j) {
                            if (-1 === items.indexOf(cur.links[j])) {
                                internal.push(cur.links[j]);
                                items.push(cur.links[j]);
                            }
                        }
                    }

                    if (0 === internal.length) {
                        break;
                    }

                    subItems = internal;
                }

                return items;
            }

            return [];
        },
        getTasks: function (p, m) {
            if (undefined !== m) { return this.delegate.storage.getMilestone(p, m).t; }
            return this.delegate.storage.p[p].t;
        },
        collectTasksWithIds: function (p, m, ids) {
            var i = 0,
                tasks = [],
                allTasks = this.getTasks(p, m), length = allTasks.length;

            for (i = 0; i < length; ++i) {
                if (-1 !== ids.indexOf(allTasks[i]._id)) {
                    tasks.push({
                        task: allTasks[i],
                        index: i,
                        id: allTasks[i]._id
                    });
                }
            }

            return tasks;
        },

        collectBeginEndItems: function (p, m, id) {

            // собираем все элементы для данной задачи с связью Begin-End

            var i = 0,
                j = 0,
                k = 0,
                link = null,
                index = -1,
                line = [],
                indexer = [],
                nextNode = null,
                cycleExist = false,
                maxDepth = 0,
                curDepth = 0,
                hash = null,
                curTask = this.delegate.storage.taskWithId(id).ref,
                tasks = this.getTasks(p, m),

                // все элементы в виде дерева справа

                rightTree = new TreeNode(),
                leftTree = new TreeNode();

            function countOfTasksWithLinks(arr) {
                var cn = 0;
                for (var counter = arr.length - 1; counter >= 0; --counter) {
                    if (arr[counter].links.length) {
                        ++cn;
                    }
                }

                return cn;
            }

            // NOTE: разбить рекурсию на блоки

            function walkInternalRight(tree, _id) {
                ++curDepth;

                var fi = 0;

                for (fi = 0; fi < tasks.length; ++fi) {

                    if (tasks[fi].links.length) {

                        if (tasks[fi]._id !== tree.data.id && _id !== tasks[fi]._id) {

                            index = hash.indexOf(tasks[fi]._id);

                            if (-1 !== index) {

                                link = tasks[fi].getLink(tree.data.id);
                                if (link) {

                                    //  console.log(link.dependenceTaskId + ' <- ' + link.parentTaskId);

                                    var linkType04 = link.linkType || link['linkType']; // js-minimizator

                                    if (linkType04 == kLinkBeginEnd && tasks[fi]._status !== kElementCompleted) {

                                        indexer = { p: p, m: m, t: fi, ref: tasks[fi], id: tasks[fi]._id };

                                        var nextNode = tree.createNode();

                                        nextNode.root = tasks[fi];
                                        nextNode.data = indexer;

                                        if (maxDepth < curDepth) {
                                            cycleExist = true;
                                            return;
                                        }

                                        walkInternalRight(nextNode, tasks[fi]._id);

                                        if (cycleExist)
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }

                --curDepth;
            }

            hash = this.collectBeginEndLinksRight(p, m, id);
            if (hash.length) {
                rightTree.root = curTask;
                maxDepth = countOfTasksWithLinks(tasks); // TODO: еще больше оптимизации, максимальная глубина это размер звена   //  hash.length + 1;
                curDepth = 0;

                for (k = 0; k < tasks.length; ++k) {
                    if (tasks[k].links.length) {
                        if (tasks[k]._id !== id) {

                            index = hash.indexOf(tasks[k]._id);

                            if (-1 !== index) {

                                link = tasks[k].getLink(id);
                                if (link) {

                                    var linkType02 = link.linkType || link['linkType']; // js-minimizator

                                    if (linkType02 == kLinkBeginEnd && tasks[k]._status !== kElementCompleted) {
                                        indexer = { p: p, m: m, t: k, ref: tasks[k], id: tasks[k]._id };

                                        nextNode = rightTree.createNode();

                                        nextNode.root = tasks[k];
                                        nextNode.data = indexer;

                                        walkInternalRight(nextNode, tasks[k]._id);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            function walkInternalLeft(tree, _id) {
                ++curDepth;

                var index = 0;
                for (index = 0; index < tree.data.ref.links.length; ++index) {
                    link = tree.data.ref.links[index];
                    if (link) {

                        var linkType2 = link.linkType || link['linkType']; // js-minimizator

                        if (linkType2 == kLinkBeginEnd) {
                            for (j = 0; j < tasks.length; ++j) {

                                var link1 = link.parentTaskId || link['parentTaskId']; // js-minimizator

                                if (tasks[j]._id === link1 && tasks[j]._status !== kElementCompleted) {
                                    indexer = { p: p, m: m, t: j, ref: tasks[j], id: tasks[j]._id };

                                    nextNode = tree.createNode();

                                    nextNode.root = tasks[j];
                                    nextNode.data = indexer;

                                    if (maxDepth < curDepth) {
                                        cycleExist = true;
                                        return;
                                    }

                                    if (tasks[j].links.length) {
                                        walkInternalLeft(nextNode, tasks[j]._id);
                                    }

                                    if (cycleExist)
                                        return;

                                    break;
                                }
                            }
                        }
                    }
                }

                --curDepth;
            }

            if (cycleExist) { return null; }

            leftTree.root = curTask;
            maxDepth = countOfTasksWithLinks(tasks); // TODO: еще больше оптимизации, максимальная глубина это размер звена // curTask.links.length;
            curDepth = 0;

            for (k = 0; k < curTask.links.length; ++k) {
                link = curTask.links[k];
                if (link) {
                    var linkType0 = link.linkType || link['linkType']; // js-minimizator
                    if (linkType0 == kLinkBeginEnd) {
                        for (j = 0; j < tasks.length; ++j) {

                            var link0 = link.parentTaskId || link['parentTaskId']; // js-minimizator

                            if (tasks[j]._id === link0 && tasks[j]._status !== kElementCompleted) {
                                indexer = { p: p, m: m, t: j, ref: tasks[j], id: tasks[j]._id };

                                nextNode = leftTree.createNode();

                                nextNode.root = tasks[j];
                                nextNode.data = indexer;

                                if (tasks[j].links.length) {
                                    walkInternalLeft(nextNode, tasks[j]._id);
                                } else {
                                    //  nextNode.data   =   null;   // NOTE ! понять почему раньше это было выставлено, а сейчас взял да закоментил Bug 20521
                                }

                                break;
                            }
                        }
                    }
                }
            }

            if (cycleExist) { return null; }

            return { right: rightTree, left: leftTree };
        },
        collectBeginEndLinksRight: function (p, m, id) {
            var t = this.delegate,
                i, j, k,
                needAdd,
                link,
                linkInd,
                tasks,
                items = [],
                hash = {},
                hashLinks,
                linkType,
                dependenceTaskId,
                parentTaskId,
                cur = null,
                indexDel = -1,
                curTask = null,
                subItems = [],
                internal = [];

            if (t) {

                tasks = this.getTasks(p, m);

                if (0 === tasks.length) {
                    return items;
                }

                // разобьем на элементы в которых есть однозначное соотвествение с элементов его вся связи

                for (i = tasks.length - 1; i >= 0; --i) {
                    for (linkInd = tasks[i].links.length - 1; linkInd >= 0; --linkInd) {
                        link = tasks[i].links[linkInd];

                        linkType = link.linkType || link['linkType'];                               // js-minimizator

                        if (linkType !== kLinkBeginEnd) {
                            continue;
                        }

                        dependenceTaskId = link.dependenceTaskId || link['dependenceTaskId'];   // js-minimizator
                        parentTaskId = link.parentTaskId || link['parentTaskId'];           // js-minimizator

                        if (!hash[dependenceTaskId]) {
                            hash[dependenceTaskId] = {
                                links: []
                            };
                        }
                        hashLinks = hash[dependenceTaskId].links;
                        if (hashLinks) {
                            needAdd = true;
                            for (k = hashLinks.length - 1; k >= 0; --k) {
                                if (hashLinks[k] === parentTaskId) {
                                    needAdd = false; break;
                                }
                            }

                            if (needAdd) {
                                hashLinks.push(parentTaskId);
                            }
                        }

                        if (!hash[parentTaskId]) {
                            hash[parentTaskId] = {
                                links: []
                            };
                        }
                        hashLinks = hash[parentTaskId].links;
                        if (hashLinks) {
                            needAdd = true;
                            for (k = hashLinks.length - 1; k >= 0; --k) {
                                if (hashLinks[k] === dependenceTaskId) {
                                    needAdd = false; break;
                                }
                            }

                            if (needAdd) {
                                hashLinks.push(dependenceTaskId);
                            }
                        }
                    }
                }

                cur = hash[id];
                if (!cur) {
                    return [];
                }

                indexDel = -1;
                curTask = t.storage.taskWithId(id);

                // if collect left, remove parent

                for (linkInd = curTask.ref.links.length - 1; linkInd >= 0; --linkInd) {
                    parentTaskId = curTask.ref.links[linkInd].parentTaskId || curTask.ref.links[linkInd]['parentTaskId'];
                    indexDel = cur.links.indexOf(parentTaskId);

                    if (-1 !== indexDel) {
                        cur.links.splice(indexDel, indexDel);
                    }
                }

                // if right

                subItems = [], internal = [];

                // добавляем текущий индекс (основной элемент)
                items.push(id);
                subItems.push(id);

                // вся связи с основным элементом
                for (i = cur.links.length - 1; i >= 0; --i) {
                    subItems.push(cur.links[i]);
                    items.push(cur.links[i]);
                }

                while (true) {

                    internal = [];

                    for (i = subItems.length - 1; i >= 0; --i) {
                        if (id === subItems[i]) { continue; }

                        cur = hash[subItems[i]];

                        for (j = cur.links.length - 1; j >= 0; --j) {
                            if (-1 === items.indexOf(cur.links[j])) {
                                internal.push(cur.links[j]);
                                items.push(cur.links[j]);
                            }
                        }
                    }

                    if (0 === internal.length) { break; }

                    subItems = internal;
                }

                return items;
            }

            return [];
        },

        // filtering

        setFilter: function (func) {
            var t = this.delegate;
            if (t) {
                t._undoManager.reset();
                t.hitLink = undefined;

                this.clearFilter(false);
                this.updateFilter(func);

                t.updateContent(true);
                t.viewController().toVisibleElementMove();
                t.offWidgets();
            }
        },
        updateFilter: function (func) {
            var i, j, p, x, length, projects = this.delegate.storage.p, ids, tasks;
            if (projects.length) {
                for (p = projects.length - 1; p >= 0; --p) {
                    for (i = projects[p].m.length - 1; i >= 0; --i) {

                        func(projects[p].m[i]);

                        for (j = projects[p].m[i].t.length - 1; j >= 0; --j) {
                            func(projects[p].m[i].t[j])
                        }
                    }

                    for (i = projects[p].t.length - 1; i >= 0; --i) {
                        func(projects[p].t[i]);
                    }
                }

                // только для задач
                // каждый элемент который имеет связи и попал под фильтр надо проверить
                // на полный путь от данного элемента до элемента без фильтра в связных с этим элементом

                for (p = projects.length - 1; p >= 0; --p) {
                    for (i = projects[p].m.length - 1; i >= 0; --i) {

                        for (j = projects[p].m[i].t.length - 1; j >= 0; --j) {

                            if (projects[p].m[i].t[j].filter) {

                                // TODO: need optimize

                                ids = this.collectRelatedItems(p, i, projects[p].m[i].t[j].id());
                                tasks = this.collectTasksWithIds(p, i, ids);

                                for (x = tasks.length - 1; x >= 0; --x) {
                                    if (!tasks[x].task.filter) {
                                        projects[p].m[i].t[j].filter = undefined;
                                        projects[p].m[i].t[j].linkFilter = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    for (i = projects[p].t.length - 1; i >= 0; --i) {

                        if (projects[p].t[i].filter) {

                            // TODO: need optimize

                            ids = this.collectRelatedItems(p, undefined, projects[p].t[i].id());
                            tasks = this.collectTasksWithIds(p, undefined, ids);

                            for (x = tasks.length - 1; x >= 0; --x) {
                                if (!tasks[x].task.filter) {
                                    projects[p].t[i].filter = undefined;
                                    projects[p].t[i].linkFilter = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        },
        clearFilter: function (repaint) {

            // убираем метки фильтра у всех элементов

            var i, j, p, length, projects = this.delegate.storage.p;

            if (projects.length) {

                for (p = projects.length - 1; p >= 0; --p) {
                    for (i = projects[p].m.length - 1; i >= 0; --i) {

                        projects[p].m[i].filter = undefined;

                        for (j = projects[p].m[i].t.length - 1; j >= 0; --j) {

                            projects[p].m[i].t[j].filter = undefined;
                            projects[p].m[i].t[j].linkFilter = undefined;
                        }
                    }

                    for (i = projects[p].t.length - 1; i >= 0; --i) {
                        projects[p].t[i].filter = undefined;
                        projects[p].t[i].linkFilter = undefined;
                    }
                }
            }

            if (repaint) {
                this.delegate.updateContent(true);
                this.delegate.viewController().toVisibleElementMove();
            }
        },

        // api

        addLinkWithIds: function (link) {
            var t = this.delegate, taskExt = null, linkObj = {};
            if (t) {
                taskExt = t.storage.taskWithId(link['dependenceTaskId']);
                if (taskExt) {
                    linkObj['dependenceTaskId'] = link['dependenceTaskId'];
                    linkObj['linkType'] = link['linkType'];
                    linkObj['parentTaskId'] = link['parentTaskId'];
                    linkObj['type'] = 'link';

                    taskExt.ref.links.push(linkObj);

                    t._undoManager.add(kOperationAddTaskLink, {
                        p: taskExt.p,
                        m: taskExt.m,
                        t: taskExt.t,
                        link: taskExt.ref.links[taskExt.ref.links.length - 1],
                        linkIndex: taskExt.ref.links.length - 1,
                        taskId: taskExt.taskId,
                        milestoneId: taskExt.milestoneId,
                        projectId: taskExt.projectId
                    });
                }
            }
        },
        finalizeStatus: function (status) {
            if (this.statusElement) {
                if (undefined !== this.statusElement.t) {
                    if (status !== this.statusElement.status) {
                        this.addTaskOperation(kHandlerBeforeChangeTaskStatus, this.statusElement.p, this.statusElement.m, this.statusElement.t, status);
                    }
                } else {
                    if (status) {
                        status = kElementCompleted - 1;
                    } else {
                        status = kElementActive - 1;
                    }
                    if (status !== this.statusElement.status) {
                        this.addMilestoneOperation(kHandlerBeforeChangeMilestoneStatus, this.statusElement.p, this.statusElement.m);
                    }
                }

                this.statusElement = null;
            }
        },
        finalizeOperation: function (type, data) {
            var t = this.delegate;
            if (t) {
                if (this.statusElement) {
                    if (this.statusElement.isTask) {
                        if ('edit' === type) {
                            t.editElementTitle(this.statusElement.p, this.statusElement.m === undefined ? -1 : this.statusElement.m, this.statusElement.t);
                        } else if ('delete' === type) {
                            this.addTaskOperation(kHandlerBeforeDeleteTask, this.statusElement.p, this.statusElement.m, this.statusElement.t);
                        } else if (typeof(type) === "number") {
                            if (this.statusElement.ref) {
                                if (type !== this.statusElement.ref.customTaskStatus()) {
                                    this.addTaskOperation(kHandlerBeforeChangeTaskStatus, this.statusElement.p, this.statusElement.m, this.statusElement.t, type);
                                }
                            }
                        } else if ('addlink' === type) {
                            if (kElementCompleted !== this.statusElement.task._status) {
                                if (t.handlers[kHandlerBeforeMenuAddTaskLink]) {
                                    var ts = t.storage.getTask(this.statusElement.p, this.statusElement.m, this.statusElement.t);
                                    t.handlers[kHandlerBeforeMenuAddTaskLink](ts);
                                }
                            }
                        } else if ('responsible' === type) {
                            this.changeResponsible(this.statusElement, data);
                        } else if ('timechanage' === type) {

                            var refDate, checkDate,
                                isOneDay = false,
                                setDate = data['time'];

                            if (data['direction']) {

                                // изменяем конечную дату

                                checkDate = this.statusElement.ref.endDate();

                                if (checkDate.getUTCDate() === setDate.getUTCDate() &&
                                    checkDate.getMonth() === setDate.getMonth() &&
                                    checkDate.getFullYear() === setDate.getFullYear()) { return; }

                                refDate = this.statusElement.ref.beginDate();

                                isOneDay = (refDate.getUTCDate() === setDate.getUTCDate() &&
                                    refDate.getMonth() === setDate.getMonth() &&
                                    refDate.getFullYear() === setDate.getFullYear());

                                if (this.statusElement.ref.beginDate().getTime() >= data['time'].getTime()) {
                                    isOneDay = true;
                                }
                            } else {

                                // изменяем начальную дату

                                checkDate = this.statusElement.ref.beginDate();

                                if (checkDate.getUTCDate() === setDate.getUTCDate() &&
                                    checkDate.getMonth() === setDate.getMonth() &&
                                    checkDate.getFullYear() === setDate.getFullYear()) { return; }

                                refDate = this.statusElement.ref.endDate();

                                isOneDay = (refDate.getUTCDate() === setDate.getUTCDate() &&
                                    refDate.getMonth() === setDate.getMonth() &&
                                    refDate.getFullYear() === setDate.getFullYear());

                                if (this.statusElement.ref.endDate().getTime() <= data['time'].getTime()) {
                                    isOneDay = true;
                                }
                            }
                            this.changeTime({ p: this.statusElement.p, m: this.statusElement.m, t: this.statusElement.t, ids: this.statusElement.ids }, data['time'], data['direction'], isOneDay);
                        }
                    } else {
                        if ('edit' === type) {
                            t.editElementTitle(this.statusElement.p, this.statusElement.m, -1);
                        } else if ('delete' === type) {
                            this.addMilestoneOperation(kHandlerBeforeDeleteMilestone, this.statusElement.p, this.statusElement.m);
                        } else if ('open' === type) {
                            if (this.statusElement.ref) {
                                if (kElementCompleted - 1 === this.statusElement.ref.status()) {
                                    this.addMilestoneOperation(kHandlerBeforeChangeMilestoneStatus, this.statusElement.p, this.statusElement.m);
                                }
                            }

                        } else if ('closed' === type) {
                            if (this.statusElement.ref) {
                                if (kElementCompleted - 1 !== this.statusElement.ref.status()) {
                                    this.addMilestoneOperation(kHandlerBeforeChangeMilestoneStatus, this.statusElement.p, this.statusElement.m);
                                }
                            }

                        } else if ('addMilestoneTask' === type) {
                            t.addTaskToMilestone(this.statusElement.p, this.statusElement.m);
                        } else if ('fitToScreen' === type) {
                            this.delegate.viewController().zoomToFitMilestone(this.statusElement.p, this.statusElement.m);
                        } else if ('responsible' === type) {
                            this.changeResponsible(this.statusElement, data);
                        } else if ('timechanage' === type) {
                            if (this.statusElement.ref.endDate().getTime() !== data['time'].getTime()) {
                                this.changeTime({ p: this.statusElement.p, m: this.statusElement.m, t: undefined, ids: this.statusElement.ids }, data['time']);
                            }
                        }
                    }

                    this.statusElement = null;
                }
            }
        },
        throughIdsIndexes: function (index) {
            if (undefined === index) {
                index = 0;
            } else {
                for (var i = this.delegate.storage.p.length - 1; i >= 0; --i) {
                    if (index === this.delegate.storage.p[i]._id) {
                        index = i;
                        break;
                    }
                }
            }

            return this.delegate.storage.throughIdsIndexes(index);
        },
        buildWithThroughIndexer: function () {
            return this.delegate.storage.buildWithThroughIndexer();
        }

        //  TODO: add custom operations
        //        updateTaskCustomOptions: function (id, options) {
        //            if (options) {
        //
        //            }
        //        },
        //        updateMilestoneWithOptions: function (id, options) {
        //
        //            var t = this.delegate;
        //            if (t) {
        //                if (options) {
        //
        //                }
        //            }
        //        }
    };

    function ViewController(delegate) {
        this.delegate = delegate;
        this['onchangetimescale'] = null;
        this['onshowhint'] = null;

        this.disableScaleEvents = false;
        this.position = null;
    }
    ViewController.prototype = {
        scaleTo: function (scale, forse, disableEvents) {
            var t = this.delegate;
            if (t) {

                //  отключаем отсылку сообщение об изменении масштаба
                this.disableScaleEvents = (undefined !== disableEvents);

                if (forse) {
                    t.animator.scale(scale, undefined, undefined, true);
                } else {
                    t.animator.scale(scale);
                }
            }
        },
        strafeToDay: function () {
            var t = this.delegate;
            if (t) {
                this.strafe(t.centerScreen / t.timeScale.hourInPixels);
            }
        },
        strafe: function (value) {
            var t = this.delegate;
            if (t) {
                t.offsetX = value;

                if (t.timeScale)
                    t.timeScale.strafe(value);

                t.needUpdate = true;
            }
        },
        toVisibleElementMove: function () {
            var t = this.delegate,
                scrollContext = null,
                maxScrollValue = 0,
                element = null,
                timeRange = 0;

            if (t) {
                if (t.zoomBar) {
                    t.zoomBar.calc();

                    scrollContext = t.getTopElementInVisibleRange();
                    if (!t.fullscreen) {
                        scrollContext.height = Math.max(kEps, scrollContext.height - t.itemMargin);
                    }

                    if (!scrollContext.empty) {
                        if (!t.fullscreen) {
                            scrollContext.height = floor2(scrollContext.height / t.itemMargin) * t.itemMargin - kEps;
                        }

                        maxScrollValue = floor2((t.rightScroll.maxValue()) / t.itemMargin) * t.itemMargin - kEps;

                        t.rightScroll.setWorldValue(Math.min(scrollContext.height, maxScrollValue), true);
                    } else {
                        element = t.getLeftMostElement();
                        if (element.x && element.height) {
                            t.offsetX = (-element.x + 24) / t.timeScale.scaleX + t.visibleLeft / t.timeScale.hourInPixels;
                            t.timeScale.strafe(t.offsetX);
                        }
                    }

                    timeRange = -Math.max(Math.abs(!t.storage.minTime ? 0 : t.storage.minTime),
                        Math.abs(!t.storage.maxTime ? 0 : t.storage.maxTime));

                    t.zoomBar.setRangeTimes(timeRange);
                }
            }
        },
        centeringElement: function (id, isMilestone, afterFuncY, disableMoveX) {
            var t = this.delegate;
            if (t && id) {
                var element = null;

                if (isMilestone)
                    element = t.storage.milestoneWithId(id);
                else
                    element = t.storage.taskWithId(id);

                if (element) {

                    var scrollY = t.getElementPosVertical(element.p, element.m > -1 ? element.m : -1, element.t > -1 ? element.t : -1) - t.ctxHeight * 0.5;

                    var maxVal = floor2((t.rightScroll.maxValue()) / t.itemMargin) * t.itemMargin;
                    scrollY = floor2(scrollY / t.itemMargin) * t.itemMargin;

                    t.animator.moveToY(Math.min(Math.max(kEps, scrollY), maxVal), afterFuncY);
                    if (undefined === disableMoveX)
                        t.animator.moveCenterToX(element.ref.beginTime);
                }
            }
        },

        // create zoom bar control

        buildZoomBar: function (dom) {
            var t = this.delegate;
            if (t && dom) {
                if (!t.zoomBar) {
                    t.zoomBar = new ZoomBar(dom, t);
                    t.needUpdateContent = true;
                }

                return t.zoomBar;
            }
            return null;
        },

        // smooth animations

        enableSlipAnimation: function (enable) {
            var t = this.delegate;
            if (t) {
                t.animator.strafeTo = enable;
                t.animator.scaleTo = enable;
                t.animator.scrollTo = enable;
            }
        },

        zoomToFitMilestone: function (p, m) {
            var t = this.delegate, beginTime = 0, endTime = 0, duration = 0, from = 0, to = 0, mul, leftHours;
            if (t) {
                if (t.zoomBar) {
                    beginTime = t.storage.getMilestone(p, m).beginTime;
                    endTime = t.storage.getMilestone(p, m).endTime;

                    duration = (t.zoomBar.rightDate.getTime() - t.zoomBar.leftDate.getTime()) / 3600000;            //  hours
                    from = (t.zoomBar.thumb.begin - 0.5) * duration;
                    to = (t.zoomBar.thumb.end - 0.5) * duration;

                    if ((to - from) < (endTime - beginTime)) {

                        // включаем отсылку сообщение об изменении масштаба
                        this.disableScaleEvents = false;

                        mul = t.timeScale.scaleX * ((endTime - beginTime) / (to - from) * 1.05);
                        mul = Math.min(Math.max(mul, kScaleUnitMinSize), kScaleUnitFifteenDays);

                        t.animator.moveScale(mul, (endTime + beginTime) * 0.5);

                        //leftHours   =   t.visibleLeft / t.timeScale.hourInPixels * mul;
                        //t.animator.scale(mul, true, -beginTime + t.timeScale.scaleX + leftHours * 1.05);                                   // disable centering animation
                    } else {
                        t.animator.moveCenterToX((endTime + beginTime) * 0.5);
                    }
                }
            }
        },
        fullscreen: function () {
            var t = this.delegate;
            if (t) {
                t.offMenus();
                t.offWidgets();
                t.painter.clearZones(true);

                t.needUpdateContent = true;
                t.needUpdate = true;
            }
        },

        disableUserEvents: function (enable) {
            if (undefined === enable) return;

            var t = this.delegate;
            if (t) {
                t.userEvents = !(enable || false);

                t.offMenus();
                t.isLBMDown = false;
                t.needUpdate = true;
            }
        },

        // mode2

        collapse: function (enable) {
            var t = this.delegate;
            if (t) {
                if (!t.fullscreen) {
                    var set = enable || false;
                    for (var i = t.storage.p.length - 1; i >= 0; --i) {
                        for (var j = t.storage.p[i].m.length - 1; j >= 0; --j) {
                            t.storage.p[i].m[j].setCollapse(set);
                        }

                        t.storage.p[i].setCollapse(set);
                    }
                }
            }
        },
        collapseProjects: function (enable) {
            var t = this.delegate;
            if (t) {
                if (!t.fullscreen) {
                    var set = enable || false;
                    for (var i = t.storage.p.length - 1; i >= 0; --i) {
                        t.storage.p[i].setFullCollapse(set);
                    }
                }
            }
        },
        collapseFreeTasks: function (enable) {
            var t = this.delegate;
            if (t) {
                if (!t.fullscreen) {
                    var set = enable || false;
                    for (var i = t.storage.p.length - 1; i >= 0; --i) {
                        t.storage.p[i].collapse = set;
                    }
                }
            }
        },
        scaleValue: function () {
            return this.delegate.timeScale.scaleX;
        },

        saveViewState: function () {
            var t = this.delegate;
            if (t) {
                this.position = { posX: t.offsetX, posY: t.rightScroll.value(), collapse: [], freecollapse: false };

                if (t.storage.p.length) {
                    var count = t.storage.p[0].m.length;
                    for (var i = 0; i < count; ++i) {
                        this.position.collapse.push({ id: t.storage.p[0].m[i].id(), status: t.storage.p[0].m[i].collapse });
                    }

                    this.position.freecollapse = t.storage.p[0].collapse;
                }
            }
        },
        loadViewState: function (reset) {
            var t = this.delegate;
            if (t) {
                if (this.position && undefined === reset) {
                    t.offsetX = this.position.posX;
                    t.rightScroll.setWorldValue(this.position.posY);

                    if (t.storage.p.length) {
                        var collapse = this.position.collapse;
                        var milestones = t.storage.p[0].m;
                        var count = collapse.length, mc = t.storage.p[0].m.length;

                        if (mc > 0 && count > 0) {
                            for (var i = 0; i < count; ++i) {
                                for (var j = 0; j < mc; ++j) {
                                    if (collapse[i].id === milestones[j].id()) { milestones[j].collapse = collapse[i].status; }
                                }
                            }
                        }

                        t.storage.p[0].collapse = this.position.freecollapse;
                    }
                }
            }

            this.position = null;
        },

        //

        changeTimeScaleEvent: function () {
            if (!this.disableScaleEvents) {
                if (this['onchangetimescale']) {
                    this['onchangetimescale'](this.delegate.timeScale.scaleX);
                }
            }
        },
        changeHintEvent: function (type, show, coords) {
            if (this['onshowhint']) {
                this['onshowhint'](type, show, coords);
            }
        }
    };

    function Animator(delegate) {
        this.delegate = delegate;

        this.strafeTo = false;
        this.strafeOffset = 0;
        this.scaleTo = false;
        this.scaleOffset = 0;
        this.scrollTo = false;
        this.scrollOffset = 0;
        this.scrollFrom = 0;
        this.strafeForseTo = 0;

        this.afterFunc = null;
        this.strafeCentering = true;
        this.scaleToCenterX = undefined;

        this.boundView = { from: 0, to: 0 };
    }
    Animator.prototype = {
        update: function () {
            var t = this.delegate;
            if (t) {
                if (0 != this.strafeOffset) {
                    var moveRange = this.strafeOffset - t.offsetX;
                    if (moveRange > 0) {
                        if (moveRange < 0.01) {
                            t.offsetX = this.strafeOffset;
                            this.strafeOffset = 0;
                        } else {
                            t.offsetX += moveRange * kAnimationScaleFactor;
                        }
                    } else {
                        if (moveRange > -0.01) {
                            t.offsetX = this.strafeOffset;
                            this.strafeOffset = 0;
                        } else {
                            t.offsetX += moveRange * kAnimationScaleFactor;
                        }
                    }

                    t.timeScale.strafe(t.offsetX);
                    t.needUpdate = true;
                    if (t.zoomBar) { t.zoomBar.repaint = true; }
                }

                if (0 != this.scrollOffset) {
                    var scrollRange = this.scrollOffset - this.scrollFrom;
                    if (scrollRange > 0) {
                        if (scrollRange < 0.5) {
                            this.scrollFrom = Math.max(this.scrollOffset, 0);
                            this.scrollOffset = 0;

                            if (this.afterFunc) {
                                this.afterFunc();
                                this.afterFunc = null;
                            }
                        } else {
                            this.scrollFrom += scrollRange * kAnimationScrollFactor;
                        }
                    } else {
                        if (scrollRange > -0.5) {
                            this.scrollFrom = Math.max(this.scrollOffset, 0);
                            this.scrollOffset = 0;

                            if (this.afterFunc) {
                                this.afterFunc();
                                this.afterFunc = null;
                            }
                        } else {
                            this.scrollFrom += scrollRange * kAnimationScrollFactor;
                        }
                    }

                    t.rightScroll.setWorldValue(this.scrollFrom);
                    t.needUpdate = true;
                    if (t.zoomBar) { t.zoomBar.repaint = true; }

                    if (t._leftPanelController) { t._leftPanelController.scrollContent(t.rightScroll.value()); }
                }

                // Плавное масштабирование

                if (0 != this.scaleOffset) {

                    var mul = t.timeScale.scaleX;
                    var scaleRange = this.scaleOffset - t.timeScale.scaleX;
                    if (scaleRange > 0) {
                        if (scaleRange < kAnimationScaleFactor) {
                            t.timeScale.scaleX = this.scaleOffset;
                            this.scaleOffset = 0;
                            this.strafeCentering = true;
                            this.scaleToCenterX = undefined;
                        } else {
                            t.timeScale.scaleX += scaleRange * kAnimationScaleFactor;
                        }
                    } else {
                        if (scaleRange > -kAnimationScaleFactor) {
                            t.timeScale.scaleX = this.scaleOffset;
                            this.scaleOffset = 0;
                            this.strafeCentering = true;
                            this.scaleToCenterX = undefined;
                        } else {
                            t.timeScale.scaleX += scaleRange * kAnimationScaleFactor;
                        }
                    }

                    if (this.strafeCentering) {
                        mul /= t.timeScale.scaleX;

                        var pixelsMove = -t.offsetX * t.timeScale.hourInPixels + t.centerScreen;
                        t.offsetX = (t.centerScreen - pixelsMove * mul) / t.timeScale.hourInPixels;

                        // корректируем положение таким образом что бы не вышли из зоны доступного просмотра
                        //console.log('from : ' + t.offsetX);
                        if (this.boundView.from !== this.boundView.to) {

                            this.boundView.from = t.zoomBar.minPos();
                            this.boundView.to = t.zoomBar.maxPos();

                            t.offsetX = Math.max(t.offsetX, this.boundView.to);
                            t.offsetX = Math.min(t.offsetX, this.boundView.from);
                        }
                        /// console.log('to : ' + t.offsetX);
                        t.timeScale.strafe(t.offsetX);
                    } else {
                        if (undefined != this.scaleToCenterX) {
                            mul /= t.timeScale.scaleX;

                            var dX = t.timeScale.scaleX * t.offsetX;
                            var sX = Math.abs(this.scaleToCenterX - dX);
                            if (dX < this.scaleToCenterX) {
                                t.offsetX += (dX - sX * (1.0 - mul)) / t.timeScale.scaleX;
                                dX = t.timeScale.scaleX * t.offsetX;

                                if (dX > this.scaleToCenterX) {
                                    t.offsetX = this.scaleToCenterX / t.timeScale.scaleX;
                                }

                            } else {
                                t.offsetX = this.scaleToCenterX / t.timeScale.scaleX;
                            }

                            t.timeScale.strafe(t.offsetX);
                        }
                    }

                    if (this.strafeForseTo) {
                        this.strafeOffset = t.centerScreen / t.timeScale.hourInPixels - this.strafeForseTo / t.timeScale.scaleX;
                    }

                    t._viewController.changeTimeScaleEvent();
                    t.needUpdate = true;
                    if (t.zoomBar) { t.zoomBar.repaint = true; }
                }
            }
        },
        stop: function () {
            this.strafeOffset = 0;
            this.afterFunc = null;
            this.strafeForseTo = 0;
        },

        moveCenterToX: function (x) {
            var t = this.delegate;
            if (t) {
                var to = t.centerScreen / t.timeScale.hourInPixels - x / t.timeScale.scaleX;
                this.strafeForseTo = 0;
                if (this.strafeTo) {
                    this.strafeOffset = to;
                } else {
                    t.offsetX = to;
                    t.timeScale.strafe(to);
                    t.update();
                }
            }
        },
        addMovementX: function (x) {
            var t = this.delegate;
            if (t) {
                var to = (t.offsetX - x);
                this.strafeForseTo = 0;
                if (this.strafeTo) {
                    this.strafeOffset = to;
                } else {
                    t.offsetX = to;
                    t.timeScale.strafe(to);
                    t.update();
                }
            }
        },
        moveToY: function (x, afterFunc, disableAnimator) {
            if (afterFunc) {
                if (this.afterFunc) { return; }

                this.afterFunc = afterFunc;
            }

            var t = this.delegate;
            if (t) {
                if (this.scrollTo && !disableAnimator) {
                    this.scrollFrom = t.rightScroll.value();
                    this.scrollOffset = x;
                } else {
                    this.scrollOffset = x;
                    this.scrollFrom = x;
                    t.rightScroll.forwardMoveY(x);
                }
            }
        },
        scale: function (scale, disableCentering, centerX, forse) {
            var t = this.delegate;
            if (t && scale) {
                if (disableCentering) {
                    this.strafeCentering = false;
                }
                if (centerX) {
                    this.scaleToCenterX = centerX;
                }
                this.strafeForseTo = 0;

                var mul = t.timeScale.scaleX / scale;

                if (this.scaleTo && !forse) {
                    this.scaleOffset = scale;
                    this.needUpdate = true;

                    if (t.zoomBar) {
                        this.boundView.from = t.zoomBar.minPos();
                        this.boundView.to = t.zoomBar.maxPos();
                    }

                    return;
                }

                this._status = 1;

                t.timeScale.scaleX = Math.max(1.0, scale);
                if (centerX) { t.offsetX = centerX / t.timeScale.scaleX; }

                if (this.strafeCentering) {
                    var pixelsMove = -t.offsetX * t.timeScale.hourInPixels + t.centerScreen;
                    t.offsetX = (t.centerScreen - pixelsMove * mul) / t.timeScale.hourInPixels;

                    if (t.timeScale) { t.timeScale.strafe(t.offsetX); }
                }
                t._viewController.changeTimeScaleEvent();
                t.needUpdate = true;
            }
        },

        moveScale: function (scale, x) {
            var t = this.delegate;
            if (t) {
                if (this.strafeTo && this.scaleTo) {
                    this.strafeForseTo = x;
                    this.strafeOffset = t.centerScreen / t.timeScale.hourInPixels - this.strafeForseTo / t.timeScale.scaleX;
                    this.scaleOffset = scale;
                } else {
                    t.timeScale.scaleX = scale;
                    t.offsetX = t.centerScreen / t.timeScale.hourInPixels - x / t.timeScale.scaleX;
                    t.timeScale.strafe(t.offsetX);
                    t._viewController.changeTimeScaleEvent();
                }

                t.needUpdate = true;
            }
        },

        status: function () {
            return (0 !== this.scaleOffset);
        },
        isScroll: function () {
            return (0 !== this.scrollOffset);
        }
    };

    function ElementRenderer(delegate) {
        this.t = delegate;
        this.worldToScreen = 1;
        this.tx = 0;
        this.te = 0;
        this.rel = 0;

        this.infinityW = 15;
        this.infinity = new Image();
        this.infinity.src = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAOCAYAAADwikbvAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAOVJREFUeNpibO7oZSAXMDFQAKii2QCIdwDxdyD+AsTzkdSgy60FYmWQBAsQGwHxISDmRtJwBUpjkwsCYlsgNgfZ3A2V3AnEakCsBcSToK6ahUXuMBCLAnETyGYHqIkJQPwCyYYUIDbGIpcKxDeA2BFk+n+o4G8kjQJA3AaV80cz9BeU/g/SvBfKATlVEIhFgHgB1GmLgHgTlA2T64eq3wVydhEQHwPiKCCOBGJGqORLIK6AsidgkasH2XwViPWgUfARiN8C8VIgNoM6lxWI2bDIPWGBmvQQiENwpAVQWIQOruQJEGAAibMzZdYXHDkAAAAASUVORK5CYII=';
    }
    ElementRenderer.prototype = {
        prepare: function () {
            this.offX = this.t.timeScale.hourInPixels * this.t.offsetX;
            this.scrollY = floor2(this.t.rightScroll.value());
            this.worldToScreen = this.t.timeScale.hourInPixels / this.t.timeScale.scaleX;
        },

        drawTask: function (task, offY, free, p, i, j, tasks) {
            this.tx = this.offX + task.beginTime * this.worldToScreen;
            this.te = task.duration * this.worldToScreen;
            this.rel = offY - this.scrollY;

            var tH = this.t.itemHeight;

            if (this.rel + this.t.itemHeight <= this.t.visibleUp) {
                return;
            }

            // позиция значка 'бесконечность' у задач без конечной даты

            if (task.endFail && i === -1) {

                this.te = -this.tx + this.t.ctxWidth - kTaskEndFailSetup.smallClampPx;
                this.te = Math.max(this.te, kTaskEndFailSetup.smallClampPx);

                if (this.tx <= this.t.visibleLeft - 1) {
                    this.te = Math.min(-this.tx + this.t.ctxWidth * kTaskEndFailSetup.bigClampPct, this.te);
                }

                // this.te     =   (Math.floor ( (this.te / this.worldToScreen) / 24 ) * 24) * this.worldToScreen;
            }

            // настроки цветов

            var showDates = false;
            var taskInLinkEditMode = false;
            var active = false;
            var inEditMode = false;

            var taskColor = kTaskCompleteColor;
            var priorityColor = kTaskNormalPriorityArrowColor;
            var borderColor = kTaskCompleteBorderColor;

            if (kElementCompleted === task._status) {
                taskColor = kTaskCompleteColor;
                priorityColor = kTaskCompletePriorityArrowColor;
                borderColor = kTaskCompleteBorderColor;

                if (!this.t.editBox.enable && !this.t._leftPanelController.getPanel().editBox.isEnable()) {
                    if ((this.t.hitTask === j && this.t.hitMilestone === i && this.t.hitProject === p)) {
                        taskColor = kTaskCompleteHitColor;
                        borderColor = kTaskCompleteBorderHitColor;
                        active = true;
                    }

                    if (free && (this.t.hitTask === j && this.t.hitMilestone === -1 && this.t.hitProject === p)) {
                        taskColor = kTaskCompleteHitColor;
                        borderColor = kTaskCompleteBorderHitColor;
                        active = true;
                    }

                    if (this.t.capTask === j && this.t.capMilestone === i && this.t.capProject === p) {
                        taskColor = kTaskCompleteHitColor;
                        borderColor = kTaskCompleteBorderHitColor;
                        showDates = true;
                        active = true;
                    }
                }

            } else {
                if ((task.endTime <= 0) && (!task.endFail || i !== -1)) {
                    taskColor = kTaskOverdueColor;
                    priorityColor = kTaskOverduePriorityArrowColor;
                    borderColor = kTaskOverdueBorderColor;

                    if (!this.t.editBox.enable && !this.t._leftPanelController.getPanel().editBox.isEnable()) {
                        if (this.t.hitTask === j && this.t.hitMilestone === i && this.t.hitProject === p) {
                            taskColor = kTaskOverdueHitColor;
                            borderColor = kTaskOverdueBorderHitColor;
                            active = true;
                        }

                        if (free && (this.t.hitTask === j && this.t.hitMilestone === -1 && this.t.hitProject === p)) {
                            taskColor = kTaskOverdueHitColor;
                            borderColor = kTaskOverdueBorderHitColor;
                            active = true;
                        }

                        if (this.t.capTask === j && this.t.capMilestone === i && this.t.capProject === p) {
                            taskColor = kTaskOverdueHitColor;
                            borderColor = kTaskOverdueBorderHitColor;
                            showDates = true;
                            active = true;
                        }
                    }
                } else {
                    taskColor = kTaskNormalColor;
                    priorityColor = kTaskNormalPriorityArrowColor;
                    borderColor = kTaskNormalBorderColor;

                    if (!this.t.editBox.enable && !this.t._leftPanelController.getPanel().editBox.isEnable()) {
                        if (free) {
                            taskColor = kTaskNoEndTimeColor;
                        }

                        if ((this.t.hitTask === j && this.t.hitMilestone === i && this.t.hitProject === p)) {
                            taskColor = kTaskNormalHitColor;
                            borderColor = kTaskNormalBorderHitColor;
                            active = true;
                        }

                        if (free && (this.t.hitTask === j && this.t.hitMilestone === -1 && this.t.hitProject === p)) {
                            taskColor = kTaskNormalHitColor;
                            borderColor = kTaskNormalBorderHitColor;
                            active = true;
                        }

                        if (this.t.capTask === j && this.t.capMilestone === i && this.t.capProject === p) {
                            taskColor = kTaskNormalHitColor;
                            borderColor = kTaskNormalBorderHitColor;
                            showDates = true;
                            active = true;
                        }
                    }
                }
            }

            // иконка для быстрого перемещения к задачке

            if (this.tx > this.t.ctxWidth - 24) {                    // right
                if (this.t.visibleUp < this.rel) {
                    this.t.ctx.fillStyle = taskColor;
                    this.t.painter.drawArrowLeft(this.t.ctxWidth - 30, this.rel + 2, 16, 16);
                }
            }

            if (this.tx + this.te < this.t.visibleLeft + 2) {        // left
                if (this.t.visibleUp < this.rel) {
                    this.t.ctx.fillStyle = taskColor;
                    this.t.painter.drawArrowRight(this.t.visibleLeft - 2, this.rel + 2, 16, 16);
                }
            }

            if (this.tx > this.t.ctxWidth || this.tx + this.te < 0 || this.rel > this.t.ctxHeight) {
                if (task.dropDownWidget) { this.t.taskDescWidget.set(this.tx, this.rel + this.t.itemHeight, task, null); }
                return;
            }

            // устанавливаем bounds для окна редактирование

            if (this.t.editBox.enable) {
                if (p == this.t.editBox.p && i == this.t.editBox.m && j === this.t.editBox.t) {

                    this.t.editBox.setBound(this.tx, this.rel, this.te, this.t.itemHeight);

                    this.t.ctx.font = this.t.titlesFont;
                    this.t.ctx.fillStyle = kTaskTextUnderEditColor;
                    this.t.ctx.strokeStyle = kTaskTextUnderEditColor;

                    this.t.ctx.fillText(task.formatBeginEndDays(), this.tx, this.rel + this.t.itemHeight + this.t.fontPx);

                    return;
                }
            }

            if (this.rel <= 0) { return; }

            if (this.t.visibleUp > 0) {
                tH = (this.rel + tH) - this.t.visibleUp - 1;
                if (tH <= 0) { return; }

                if (tH < this.t.itemHeight) {
                    this.rel += this.t.itemHeight - tH;
                } else {
                    tH = this.t.itemHeight;
                }
            }

            if (kEditModeAddLink === this.t.editMode) {
                if (this.t.linkLineEdit) {
                    if (this.t.linkLineEdit.pp === p && this.t.linkLineEdit.pm === i && this.t.linkLineEdit.pt === j) {
                        taskInLinkEditMode = true;
                    }
                    if (this.t.linkLineEdit.p === p && this.t.linkLineEdit.m === i && this.t.linkLineEdit.t === j) {
                        taskInLinkEditMode = true;
                    }

                    //this.linkLineEdit.pp = this.hitProject;
                    //this.linkLineEdit.pm = this.hitMilestone;
                    //this.linkLineEdit.pt = this.hitTask;
                }
            }

            var bound = { x: floor2(this.tx) + 0.5, y: floor2(this.rel) + 0.5, w: floor2(this.te), h: floor2(tH) };

            if (this.t.animator.status()) {
                bound = { x: (this.tx) + 0.5, y: (this.rel) + 0.5, w: (this.te) + 0.5, h: (tH) + 0.5 }; // дрожание рамки
            }

            // отрисовка самой задачи

            // если задача имеет свойство фильтр-связь, то делаем ее прозрачной

            if (task.linkFilter) {
                this.t.ctx.fillStyle = 'rgba(0, 0, 0, 0)';  //  this.t.ctx.globalAlpha = 0.50;   this.t.ctx.fillStyle    =   taskColor;
            } else {
                this.t.ctx.fillStyle = taskColor;
            }

            // отрисовка самой задачи (прямоугольник задачи)

            this.t.ctx.fillRect(bound.x, bound.y, bound.w, bound.h);

            //if (task.linkFilter) {
            //  this.t.ctx.globalAlpha = 1.0;
            //}

            // рисуем рамку вокруг задачи

            var isBorder = (task.links.length > 0);
            if (!isBorder) {
                // TODO: обязательно оптимизировать
                for (var ind = tasks.length - 1; ind >= 0; --ind) {
                    for (var linkInd = tasks[ind].links.length - 1; linkInd >= 0; --linkInd) {
                        if (task._id === (tasks[ind].links[linkInd].parentTaskId || tasks[ind].links[linkInd]['parentTaskId'])) {   // minimizator fix
                            isBorder = true;
                            break;
                        }
                    }
                }
            }

            if (isBorder || taskInLinkEditMode) {

                // пунктирная рамка вокруг задач со связями

                var dash = kTaskWithLinkBorderSettings.dash;
                var margin = 0;

                if (taskInLinkEditMode) {
                    // this.t.painter.drawStrokeRectangle(this.t.ctx, bound.x - 1, bound.y - 1, floor2(this.te) + 1, floor2(tH) + 1, taskColor);

                    this.t.ctx.strokeStyle = kLinkAddModeSettings.color;

                    dash = kLinkAddModeSettings.dash;
                    this.t.ctx.lineWidth = 2;
                    margin = 0.5;

                    bound = { x: floor2(this.tx), y: floor2(this.rel), w: floor2(this.te), h: floor2(tH) };

                } else {
                    this.t.painter.drawStrokeRectangle(this.t.ctx, bound.x, bound.y, floor2(this.te), floor2(tH), kTaskWithLinkBorderSettings.to);

                    this.t.ctx.strokeStyle = kTaskWithLinkBorderSettings.from;
                    this.t.ctx.lineWidth = 1;
                }

                if (this.t.animator.status()) {
                    bound = { x: floor2(this.tx) + 0.5, y: floor2(this.rel) + 0.5, w: floor2(this.te) + 0.5, h: floor2(tH) };   // дрожание рамки
                }

                this.t.painter.drawDashedLineH(this.t.ctx, bound.x, bound.y, bound.x + bound.w, bound.y, dash);
                this.t.painter.drawDashedLineH(this.t.ctx, bound.x, bound.y + bound.h, bound.x + bound.w, bound.y + bound.h, dash);
                this.t.painter.drawDashedLineV(this.t.ctx, bound.x, bound.y, bound.x, bound.y + bound.h, dash);
                this.t.painter.drawDashedLineV(this.t.ctx, bound.x + bound.w, bound.y, bound.x + bound.w, bound.y + bound.h, dash);

            } else {
                this.t.painter.drawStrokeRectangle(this.t.ctx, bound.x, floor2(this.rel) + 0.5, floor2(this.te), floor2(tH), borderColor);
            }

            var lockProject = (kOpenProject !== this.t.storage.getProject(p).status());

            // квадратики вне задача на связываемых задачах

            var main = false;
            if (active && !this.t.readMode && !lockProject) {

                var bY = floor2(bound.y) + 0.5;

                //if (!this.t._leftBox.hitBound) { // TODO: check hit in chart area
                if (kElementCompleted !== task._status) {
                    this.t.ctx.fillStyle = kLinkPinColor;
                    this.t.ctx.fillRect(floor2(bound.x - kLinkPinSize - 2), floor2(bY + tH * 0.5 - kLinkPinSize * 0.5 + 1), kLinkPinSize, kLinkPinSize);

                    this.t.ctx.fillStyle = '#FFFFFF';
                    this.t.ctx.fillRect(floor2(bound.x - kLinkPinSize * 0.5 - 3), floor2(bY + tH * 0.5 - 0.5), kLinkSmallPinSize, kLinkSmallPinSize);

                    main = true;

                    if (!task.endFail) {
                        this.t.ctx.fillStyle = kLinkPinColor;
                        this.t.ctx.fillRect(floor2(bound.x + 3 + bound.w), floor2(bY + tH * 0.5 - kLinkPinSize * 0.5 + 1), kLinkPinSize, kLinkPinSize);

                        this.t.ctx.fillStyle = '#FFFFFF';
                        this.t.ctx.fillRect(floor2(bound.x + 3 + bound.w + 3), floor2(bY + tH * 0.5 - 0.5), kLinkSmallPinSize, kLinkSmallPinSize);

                        main = true;
                    }
                }
                // }
            }

            if (!this.t.readMode && this.t._mouseInLinkZone && !main) {
                if (this.t._mouseInLinkZone.linkToTaskId) {
                    if (this.t._mouseInLinkZone.linkToTaskId === task._id) {

                        this.t.ctx.fillStyle = kLinkPinColor;
                        this.t.ctx.fillRect(floor2(bound.x - kLinkPinSize - 2), floor2(bound.y + tH * 0.5 - kLinkPinSize * 0.5 + 1), kLinkPinSize, kLinkPinSize);

                        this.t.ctx.fillStyle = '#FFFFFF';
                        this.t.ctx.fillRect(floor2(bound.x - kLinkPinSize * 0.5 - 3), floor2(bound.y + tH * 0.5), kLinkSmallPinSize, kLinkSmallPinSize);

                        if (!this.t._mouseInLinkZone.task.endFail) {
                            this.t.ctx.fillStyle = kLinkPinColor;
                            this.t.ctx.fillRect(floor2(bound.x + 3 + bound.w), floor2(bound.y + tH * 0.5 - kLinkPinSize * 0.5 + 1), kLinkPinSize, kLinkPinSize);

                            this.t.ctx.fillStyle = '#FFFFFF';
                            this.t.ctx.fillRect(floor2(bound.x + 3 + bound.w + 3), floor2(bound.y + tH * 0.5 - 0.5), kLinkSmallPinSize, kLinkSmallPinSize);
                        }
                    }
                }
            }

            // рисуем стрелку (приоритет)

            if (task._priority && !main) {
                if (this.te > this.t.itemHeight * 0.8) {
                    this.t.painter.drawIcoArrowUp(this.t.ctx, this.tx, this.rel, this.t.itemHeight, kTaskArrowPriorityBaseColor);
                }
            }

            // выпадающий список

            if (task.dropDownWidget) {
                this.t.taskDescWidget.set(this.tx, this.rel + this.t.itemHeight, task, borderColor);
                this.t.widgetY = offY;
                this.t.widgetX = this.tx;

            } else {
                if (this.t.fullscreen) {
                    if (this.tx + this.te > 0) {
                        this.t.ctx.fillStyle = kTaskTextColor;
                        this.t.ctx.strokeStyle = kTaskTextColor;
                        this.t.ctx.lineWidth = 1;

                        this.t.ctx.fillText(task._title, Math.max(this.tx + this.t.taskTextIndent * this.t.zoom, 0),
                                this.rel + this.t.itemHeight + this.t.fontPx);
                    }
                }

                if (-1 === task.titleWidth) {
                    task.titleWidth = this.t.ctx.measureText(task._title).width / this.t.zoom;
                }
            }

            // рисуем даты справа слева

            if (!this.t.readMode && showDates && !this.t.pressedButtonRight
                && kElementCompleted !== task._status &&
                this.t.editMode !== kEditModeAddLink && !lockProject) {

                if (!task.endFail) {

                    if (kTaskSideRight === this.t.capSide || kTaskSideNone == this.t.capSide) {
                        this.t.ctx.fillStyle = kTaskDateCaption.bkcolor;
                        this.t.painter.roundRect(this.t.ctx, bound.x + bound.w + kTaskDateCaption.offX,
                                this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5, kTaskDateCaption.width, kTaskDateCaption.height, 4);
                        this.t.ctx.fill();

                        this.t.ctx.strokeStyle = kTaskDateCaption.color;
                        this.t.painter.roundRect(this.t.ctx, bound.x + bound.w + kTaskDateCaption.offX,
                                this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5, kTaskDateCaption.width, kTaskDateCaption.height, 4);
                        this.t.ctx.stroke();

                        this.t.ctx.fillStyle = kTaskDateCaption.color;
                        this.t.ctx.fillText(task.formatEndDate(true), bound.x + bound.w + kTaskDateCaption.offX + 4,
                                this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5 + kTaskDateCaption.height - 4);
                    }
                }

                if (kTaskSideLeft === this.t.capSide || kTaskSideNone == this.t.capSide) {
                    this.t.ctx.fillStyle = kTaskDateCaption.bkcolor;
                    this.t.painter.roundRect(this.t.ctx, bound.x - kTaskDateCaption.width - kTaskDateCaption.offX,
                            this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5, kTaskDateCaption.width, kTaskDateCaption.height, 4);
                    this.t.ctx.fill();

                    this.t.ctx.strokeStyle = kTaskDateCaption.color;
                    this.t.painter.roundRect(this.t.ctx, bound.x - kTaskDateCaption.width - kTaskDateCaption.offX,
                            this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5, kTaskDateCaption.width, kTaskDateCaption.height, 4);
                    this.t.ctx.stroke();

                    this.t.ctx.fillStyle = kTaskDateCaption.color;
                    this.t.ctx.fillText(task.formatBeginDate(true), bound.x - kTaskDateCaption.width - kTaskDateCaption.offX + 4,
                            this.rel + (this.t.itemHeight - kTaskDateCaption.height) * 0.5 + kTaskDateCaption.height - 4);
                }
            }

            // бесконечная задача

            if (task.endFail) {
                this.t.ctx.drawImage(this.infinity, this.tx + this.te + 5, this.rel + 2);
            }

            // стрелки на задачах которы выходят одной стороной за границу видимой области

            if (this.tx < this.t.visibleLeft + 2 && this.tx + this.te > this.t.visibleLeft + 2) {  // left
                this.t.ctx.fillStyle = '#FFFFFF';
                this.t.painter.drawArrowRight(this.t.visibleLeft - 2, this.rel + 2, 16, 16);
            }

            //if (this.tx < this.t.ctxWidth - 24  && this.tx + this.te > this.t.ctxWidth - 24) {  // right
            //    // this.t.ctx.fillStyle = '#FFFFFF';
            //    // this.t.painter.drawArrowLeft(this.t.ctxWidth - 30, this.rel + 2, 16, 16);
            //}
        },
        drawMilestone: function (milestone, offY, p, i, realTasksCount) {

            var itemH = offY - this.scrollY;

            var mX = this.offX + milestone.beginTime * this.worldToScreen;
            var mW = milestone.duration * this.worldToScreen;
            var mY = itemH + this.t.itemHeight;

            if (mX > this.t.ctxWidth || mX + mW < 0 || itemH > this.t.ctxHeight) {
                if (milestone.dropDownWidget) { this.t.milestoneDescWidget.set(mX + mW, 0, milestone, null); }
                return;
            }

            var mH = (realTasksCount === 0 && this.t.fullscreen) ? this.t.itemMargin * 1.5 : (realTasksCount + 0.5) * this.t.itemMargin;
            mH += this.t.visibleUp;

            if (itemH + mH <= 0) { return; }
            if (!this.t.fullscreen && itemH + mH - this.t.itemMargin <= 2) { return; }

            if (this.t.backLightMilestone.enable || (this.t.backLightMilestone.p == p && this.t.backLightMilestone.m == i)) {

                // Подсветка вехи серым цветом

                if (!this.t.fullscreen) {
                    itemH = floor2(offY - this.scrollY - this.t.itemHeight * 0.5);
                    mY = itemH + this.t.itemHeight;
                    mH = (realTasksCount === 0 && this.t.fullscreen) ?
                        this.t.itemMargin * 1.5 : (realTasksCount + 0.5) * this.t.itemMargin;

                    if (mY < this.t.visibleUp) {
                        var y = this.t.itemMargin * 2 - mY;
                        mY = this.t.itemMargin * 2;
                        mH = mH - y;

                        if (mH < 0) { return }
                    }
                }

                if (!(!this.t.fullscreen && milestone.collapse)) {
                    this.t.ctx.fillStyle = kMilestoneColorBackLight;
                    this.t.ctx.fillRect(mX, mY, mW, mH);
                }
            }

            var lineY = itemH + this.t.itemHeight - this.t.milestoneLinewidth * this.t.zoom;
            if (lineY < this.t.visibleUp) { return; }

            if (milestone.endTime <= 0)
                this.t.ctx.fillStyle = kMilestoneOverdueColor;
            else
                this.t.ctx.fillStyle = kMilestoneColor;

            if (1 === milestone._status) {
                this.t.ctx.fillStyle = kMilestoneCompleteColor;
            }

            this.t.ctx.fillRect(mX, lineY, mW, this.t.milestoneLinewidth * this.t.zoom);

            this.t.ctx.beginPath();
            this.t.ctx.moveTo(mX + mW, lineY);
            this.t.ctx.lineTo(mX + mW, itemH + this.t.itemHeight + this.t.milestoneLinewidth * this.t.zoom * 2);
            this.t.ctx.lineTo(mX + mW - this.t.milestoneLinewidth * this.t.zoom * 2, itemH + this.t.itemHeight);
            this.t.ctx.closePath();
            this.t.ctx.fill();

            if (-1 === milestone.titleWidth) {
                milestone.titleWidth = this.t.ctx.measureText(milestone.titleWithDate).width;
            }

            var textW, txtX;

            if (this.t.editBox.enable && p == this.t.editBox.p && i == this.t.editBox.m && -1 === this.t.editBox.t) {
                this.t.editBox.setBound(mX + mW, itemH - this.t.milestoneLinewidth * this.t.zoom, -1, this.t.itemHeight);
                return;
            }

            textW = milestone.titleWidth + (milestone._isKey ? this.t.milestoneKeyLeftIndent * this.t.zoom : 0);
            txtX = mX + mW - textW;

            if (mX + mW > this.t.ctxWidth)
                txtX = this.t.ctxWidth - textW;

            // ключевая веха

            if (milestone._isKey) {
                this.t.painter.drawIcoKey(this.t.ctx, floor2(txtX + textW), floor2(itemH + this.t.itemHeight + 2 * (!this.t.fullscreen)), this.t.zoom);
            }

            if (milestone._status) {
                this.t.ctx.fillStyle = kMilestoneColor;
            }

            if (milestone.dropDownWidget) {
                this.t.milestoneDescWidget.set(mX + mW,
                        itemH + this.t.itemMargin * 0.5 + this.t.milestoneLinewidth * this.t.zoom - 1,
                    milestone,
                    milestone._status ? kMilestoneCompleteColor : this.t.ctx.fillStyle);

                this.t.widgetY = offY;
                this.t.widgetX = this.tx;
            }

            if (this.t.fullscreen) {
                this.t.ctx.fillText(milestone.titleWithDate, txtX, itemH + this.t.fontPx - this.t.milestoneLinewidth * this.t.zoom);
            }
        },

        drawLinks: function (tasks, offX, offY, scrollY) {
            if (!tasks.length) {
                return;
            }

            if (this.t.visibleUp > 0) {
                this.t.ctx.save();
                this.t.ctx.rect(this.t.visibleLeft, this.t.visibleUp, this.t.ctxWidth, this.t.ctxHeight);
                this.t.ctx.clip();
            }

            var yFrom, yTo, xFrom, xTo, task, linkId, linkType, linkColor;
            var selBeg = -1, selEnd = -1, selectedLink = undefined;
            if (this.t.capLink) {
                selBeg = this.t.capLink.dependenceTaskId || this.t.capLink['dependenceTaskId'];   // minimizator fix
                selEnd = this.t.capLink.parentTaskId || this.t.capLink['parentTaskId'];           // minimizator fix
            }

            var hitBeg = -1, hitEnd = -1, selectedHitLink = undefined;
            if (kHitLightLink) {
                if (this.t.linkWidget.link) {
                    hitBeg = this.t.linkWidget.link.dependenceTaskId || this.t.linkWidget.link['dependenceTaskId'];      // minimizator fix
                    hitEnd = this.t.linkWidget.link.parentTaskId || this.t.linkWidget.link['parentTaskId'];             // minimizator fix
                }
            }

            var editLink = null;

            var margin = this.t.itemMargin;
            var itemHalfH = floor2(this.t.itemHeight * 0.5);

            var linkFromCnY = 0, linkToCnY = 0;

            var rel = offY - scrollY;

            // this.t.ctx.beginPath();
            this.t.ctx.strokeStyle = kLinkNormalColor;
            this.t.ctx.lineWidth = 1;

            for (var to = 0; to < tasks.length; ++to) {
                task = tasks[to];

                if (task.filter) { linkFromCnY++; }

                for (var linkInd = 0; linkInd < task.links.length; ++linkInd) {

                    var link = task.links[linkInd];
                    linkId = link.parentTaskId || link['parentTaskId'];       // minimizator fix

                    if (tasks[to]._id === linkId) { continue; }

                    linkType = link.linkType || link['linkType'];                // minimizator fix

                    linkToCnY = 0;

                    for (var cur = 0; cur < tasks.length; ++cur) {

                        if (tasks[cur].filter) { linkToCnY++; }

                        if (tasks[cur].filter && tasks[to].filter) { continue; }

                        if (tasks[cur]._id === linkId) {

                            if (kLinkBeginEnd === linkType) {

                                xFrom = floor2(offX + (tasks[cur].beginTime + tasks[cur].duration) * this.worldToScreen + 5) - 0.5;
                                xTo = floor2(offX + (tasks[to].beginTime * this.worldToScreen) - 5) - 0.5;

                                yFrom = floor2(rel + margin * (cur - linkToCnY) + itemHalfH) + 0.5;
                                yTo = floor2(rel + margin * (to - linkFromCnY) + itemHalfH) + 0.5;

                                // рисуем связь в режиме добавления

                                if (this.t.pushLink) {
                                    if (linkType === this.t.pushLink.linkObj['linkType']
                                        && linkId == this.t.pushLink.linkObj['parentTaskId']
                                        && tasks[to]._id == this.t.pushLink.linkObj['dependenceTaskId']) {
                                        editLink = { linkType: linkType, xFrom: xFrom + 0.5, yFrom: yFrom + 0.5, xTo: xTo + 0.5, yTo: yTo + 0.5 };
                                        continue;
                                    }
                                }

                                if (tasks[cur].beginTime + tasks[cur].duration > tasks[to].beginTime) {
                                    //                                  NOTE : если нужно подсвечивать выделенную связь нужно разкоментить код

                                    //if (linkId === selEnd && task.id() === selBeg) {
                                    if (linkId === hitEnd && task.id() === hitBeg) {
                                        selectedLink = { linkType: linkType, xFrom: xFrom + 0.5, yFrom: yFrom + 0.5, xTo: xTo + 0.5, yTo: yTo + 0.5, linkColor: kLinkInvalidColor };
                                        continue;
                                    }

                                    //this.t.ctx.stroke();

                                    //this.t.ctx.beginPath();
                                    // this.t.ctx.lineWidth     =   2;
                                    this.t.ctx.strokeStyle = kTaskWithLinkBorderSettings.to;
                                    this.t._renderer.drawLink(linkType, xFrom, yFrom, xTo, yTo);

                                    // this.t.ctx.lineWidth     =   2;
                                    this.t.ctx.strokeStyle = kLinkInvalidColor;
                                    this.t._renderer.drawLink(linkType, xFrom, yFrom, xTo, yTo, true);  //  dash

                                    //this.t.ctx.stroke();

                                    // this.t.ctx.beginPath();
                                    //this.t.ctx.lineWidth    =   1;
                                    //this.t.ctx.strokeStyle  =   kLinkNormalColor;



                                    continue;
                                }

                            } else if (kLinkBeginBegin === linkType) {

                                xFrom = floor2(offX + tasks[cur].beginTime * this.worldToScreen) + 0.5;
                                xTo = floor2(offX + tasks[to].beginTime * this.worldToScreen) + 0.5;

                                yFrom = floor2(rel + margin * (cur - linkToCnY) + itemHalfH) + 0.5;
                                yTo = floor2(rel + margin * (to - linkFromCnY) + itemHalfH) + 0.5;

                            } else if (kLinkEndEnd === linkType) {

                                xFrom = floor2(offX + (tasks[cur].beginTime + tasks[cur].duration) * this.worldToScreen) + 0.5;
                                xTo = floor2(offX + (tasks[to].beginTime + tasks[to].duration) * this.worldToScreen) + 0.5;

                                yFrom = floor2(rel + margin * (cur - linkToCnY) + itemHalfH) + 0.5;
                                yTo = floor2(rel + margin * (to - linkFromCnY) + itemHalfH) + 0.5;
                            }

                            // рисуем связь в режиме добавления

                            if (this.t.pushLink) {
                                if (linkType === this.t.pushLink.linkObj['linkType'] && linkId == this.t.pushLink.linkObj['parentTaskId'] && tasks[to]._id == this.t.pushLink.linkObj['dependenceTaskId']) {
                                    editLink = { linkType: linkType, xFrom: xFrom + 0.5, yFrom: yFrom + 0.5, xTo: xTo + 0.5, yTo: yTo + 0.5 };
                                    continue;
                                }
                            }

                            // NOTE : если нужно подсвечивать выделенную связь нужно разкоментить код
                            //if (linkId === selEnd && task.id() === selBeg) {
                            if (linkId === hitEnd && task.id() === hitBeg) {
                                selectedLink = { linkType: linkType, xFrom: xFrom + 0.5, yFrom: yFrom + 0.5, xTo: xTo + 0.5, yTo: yTo + 0.5, linkColor: kLinkNormalColor };
                                continue;
                            }

                            this.t.ctx.strokeStyle = kTaskWithLinkBorderSettings.to;
                            this.t._renderer.drawLink(linkType, xFrom, yFrom, xTo, yTo);

                            this.t.ctx.strokeStyle = kLinkNormalColor;
                            this.t._renderer.drawLink(linkType, xFrom, yFrom, xTo, yTo, true);  //  dash
                        }
                    }
                }
            }

            //this.t.ctx.stroke();

            if (editLink) {
                this.t.ctx.beginPath();

                this.t.ctx.lineWidth = 2;
                this.t.ctx.strokeStyle = kLinkAddEditColor;

                kTaskWithLinkBorderSettings.dash = [6, 2];

                this.t._renderer.drawLink(editLink.linkType, editLink.xFrom, editLink.yFrom, editLink.xTo, editLink.yTo, true);

                kTaskWithLinkBorderSettings.dash = [4, 2];
                this.t.ctx.stroke();
            }

            // NOTE : если нужно подсвечивать выделенную связь нужно разкоментить код
            if (selectedLink) {
                this.t.ctx.beginPath();

                this.t.ctx.lineWidth = 2;

                //kTaskWithLinkBorderSettings.dash = [6, 2];
                this.t.ctx.strokeStyle = kTaskWithLinkBorderSettings.to;
                this.t._renderer.drawLink(selectedLink.linkType, floor2(selectedLink.xFrom), floor2(selectedLink.yFrom),
                    floor2(selectedLink.xTo), floor2(selectedLink.yTo), true);

                this.t.ctx.strokeStyle = selectedLink.linkColor;
                this.t._renderer.drawLink(selectedLink.linkType, floor2(selectedLink.xFrom), floor2(selectedLink.yFrom),
                    floor2(selectedLink.xTo), floor2(selectedLink.yTo), true);

                //kTaskWithLinkBorderSettings.dash = [4, 2];

                this.t.ctx.stroke();
            }

            if (this.t.visibleUp > 0) {
                this.t.ctx.restore();
            }
        },
        drawLink: function (type, xFrom, yFrom, xTo, yTo, dash) {
            if (kLinkBeginEnd === type) {
                if (yFrom < yTo) {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom - 5, yFrom);
                            this.t.ctx.lineTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xFrom, yTo);
                            this.t.ctx.lineTo(xTo + 5 + 1, yTo);        // fix  + 1px

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xFrom, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yTo, xTo + 5 + 1, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom - 5, yFrom);
                            this.t.ctx.lineTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xFrom, yTo - this.t.itemHeight);
                            this.t.ctx.lineTo(xTo, yTo - this.t.itemHeight);
                            this.t.ctx.lineTo(xTo, yTo);
                            this.t.ctx.lineTo(xTo + 5 + 1, yTo);        // fix  + 1px

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xFrom, yTo - this.t.itemHeight, kTaskWithLinkBorderSettings.dash);

                            this.t.painter.lineTo(this.t.ctx, xTo, yTo - this.t.itemHeight, xFrom, yTo - this.t.itemHeight, kTaskWithLinkBorderSettings.dash);  // NOTE:

                            this.t.painter.lineTo(this.t.ctx, xTo, yTo - this.t.itemHeight, xTo, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xTo + 5 + 1, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                } else {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom - 5, yFrom);
                            this.t.ctx.lineTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xFrom, yTo);
                            this.t.ctx.lineTo(xTo + 5, yTo);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yTo, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yTo, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xTo + 5 + 1, yTo);        // fix  + 1px
                            this.t.ctx.lineTo(xTo, yTo);
                            this.t.ctx.lineTo(xTo, yFrom - this.t.itemHeight);
                            this.t.ctx.lineTo(xFrom, yFrom - this.t.itemHeight);
                            this.t.ctx.lineTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xFrom - 5, yFrom);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xTo + 5 + 1, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xTo, yFrom - this.t.itemHeight, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo, yFrom - this.t.itemHeight, xFrom, yFrom - this.t.itemHeight, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom - this.t.itemHeight, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom, yFrom, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                }
            } else if (kLinkBeginBegin === type) {
                if (yFrom < yTo) {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom + 1, yFrom);        // fix  + 1px
                            this.t.ctx.lineTo(xFrom - 5, yFrom);
                            this.t.ctx.lineTo(xFrom - 5, yTo);
                            this.t.ctx.lineTo(xTo + 5, yTo);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom + 1, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom - 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yTo, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom + 1, yFrom);        // fix  + 1px
                            this.t.ctx.lineTo(xTo - 5, yFrom);
                            this.t.ctx.lineTo(xTo - 5, yTo);
                            this.t.ctx.lineTo(xTo + 1, yTo);            // fix  + 1px

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yFrom, xFrom + 1, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yFrom, xTo - 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yTo, xTo + 1, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                } else {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom + 1, yFrom);        // fix  + 1px
                            this.t.ctx.lineTo(xFrom - 5, yFrom);
                            this.t.ctx.lineTo(xFrom - 5, yTo);
                            this.t.ctx.lineTo(xTo + 5, yTo);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yFrom, xFrom + 1, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yTo, xFrom - 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom - 5, yTo, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xTo + 1, yTo);            // fix  + 1px
                            this.t.ctx.lineTo(xTo - 5, yTo);
                            this.t.ctx.lineTo(xTo - 5, yFrom);
                            this.t.ctx.lineTo(xFrom + 1, yFrom);        // fix  + 1px

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yTo, xTo + 1, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yTo, xTo - 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo - 5, yFrom, xFrom + 1, yFrom, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                }
            } else if (kLinkEndEnd === type) {
                if (yFrom < yTo) {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xTo, yTo);
                            this.t.ctx.lineTo(xTo + 5, yTo);
                            this.t.ctx.lineTo(xTo + 5, yFrom);
                            this.t.ctx.lineTo(xFrom, yFrom);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo + 5, yFrom, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xTo + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            // this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xTo, yTo);
                            this.t.ctx.lineTo(xFrom + 5, yTo);
                            this.t.ctx.lineTo(xFrom + 5, yFrom);
                            this.t.ctx.lineTo(xFrom, yFrom);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xFrom + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom + 5, yFrom, xFrom + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xFrom + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                } else {
                    if (xFrom < xTo) {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xTo + 5, yFrom);
                            this.t.ctx.lineTo(xTo + 5, yTo);
                            this.t.ctx.lineTo(xTo, yTo);

                            this.t.ctx.stroke();
                        } else {
                            //this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xTo + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo + 5, yTo, xTo + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xTo + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    } else {
                        if (!dash) {
                            this.t.ctx.beginPath();

                            this.t.ctx.moveTo(xFrom, yFrom);
                            this.t.ctx.lineTo(xFrom + 5, yFrom);
                            this.t.ctx.lineTo(xFrom + 5, yTo);
                            this.t.ctx.lineTo(xTo, yTo);

                            this.t.ctx.stroke();
                        } else {
                            // this.t.ctx.lineWidth = 2;
                            this.t.painter.lineTo(this.t.ctx, xFrom, yFrom, xFrom + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xFrom + 5, yTo, xFrom + 5, yFrom, kTaskWithLinkBorderSettings.dash);
                            this.t.painter.lineTo(this.t.ctx, xTo, yTo, xFrom + 5, yTo, kTaskWithLinkBorderSettings.dash);
                            //this.t.ctx.lineWidth = 1;
                        }
                    }
                }
            }
        },

        drawBackLightFreeTask: function (p, i, j, rel) {
            if (this.t.editBox.enable && p === this.t.editBox.p && -1 === this.t.editBox.m && this.t.editBox.t === j) {
                if (this.t.itemMargin * 2 < rel) {
                    this.t.ctx.fillStyle = kTaskSelectedBackColor;

                    if (this.t.fullscreen) {
                        this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                    } else {
                        this.t.ctx.fillRect(0, rel - this.t.itemHeight * 0.5, this.t.ctxWidth, this.t.itemMargin);
                    }
                }

                this.t.currentElementY = rel + this.t.itemHeight;
            }

            if (this.t._leftPanelController.getPanel().editBox.isEnable()) {
                if (this.t._leftPanelController.getPanel().editBox.anchor.p === p && this.t._leftPanelController.getPanel().editBox.anchor.m === undefined && i === -1 && this.t._leftPanelController.getPanel().editBox.anchor.t === j) {

                    if (this.t.itemMargin * 2 < rel) {
                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }
                    }

                    this.t.currentElementY = rel + this.t.itemHeight;
                    return;
                }
            }

            if ((this.t.hitTask === j && this.t.hitMilestone === -1 && this.t.hitProject === p) ||
                (this.t.capTask === j && this.t.capMilestone === -1 && this.t.capProject === p)) {

                if (!this.t.editBox.enable) {
                    if (this.t.itemMargin * 2 < rel) {

                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, rel - this.t.itemHeight * 0.5, this.t.ctxWidth, this.t.itemMargin);
                        }
                    }

                    this.t.currentElementY = rel + this.t.itemHeight;
                }
            }
        },
        drawBackLightTask: function (p, i, j, rel) {
            if (this.t.editBox.enable && p == this.t.editBox.p && i == this.t.editBox.m && this.t.editBox.t === j) {

                if (this.t.itemMargin * 2 < rel) {
                    this.t.ctx.fillStyle = kTaskSelectedBackColor;

                    if (this.t.fullscreen) {
                        this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                    } else {
                        this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                    }
                }

                this.t.currentElementY = rel + this.t.itemHeight;
            }

            if (this.t._leftPanelController.getPanel().editBox.isEnable()) {
                if (this.t._leftPanelController.getPanel().editBox.anchor.p === p && this.t._leftPanelController.getPanel().editBox.anchor.m === i && this.t._leftPanelController.getPanel().editBox.anchor.t === j) {

                    if (this.t.itemMargin * 2 < rel) {
                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }
                    }

                    this.t.currentElementY = rel + this.t.itemHeight;
                    return;
                }
            }

            if ((this.t.hitTask === j && this.t.hitMilestone === i && this.t.hitProject === p) ||
                (this.t.capTask === j && this.t.capMilestone === i && this.t.capProject === p)) {

                if (!this.t.editBox.enable) {

                    if (this.t.itemMargin * 2 < rel) {

                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }
                    }

                    this.t.currentElementY = rel + this.t.itemHeight;
                }
            }
        },
        drawBackLightMilestone: function (p, i, j, rel) {
            if (this.t.editBox.enable && p == this.t.editBox.p && i == this.t.editBox.m && this.t.editBox.t === j) {

                if (this.t.itemMargin * 2 < rel) {
                    this.t.ctx.fillStyle = kTaskSelectedBackColor;

                    if (this.t.fullscreen) {
                        this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                    } else {
                        this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                    }

                    return;
                }
            }

            if (this.t._leftPanelController.getPanel().editBox.isEnable()) {
                if (this.t._leftPanelController.getPanel().editBox.anchor.p === p && this.t._leftPanelController.getPanel().editBox.anchor.m === i && this.t._leftPanelController.getPanel().editBox.anchor.t === undefined && j === -1) {

                    if (this.t.itemMargin * 2 < rel) {
                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }

                        return;
                    }
                }
            }

            if (this.t.menuMileStone.ref) {
                if (this.t.menuMileStone.p === p && this.t.menuMileStone.m === i && !this.t.menuMileStone.hide) {
                    if (this.t.itemMargin * 2 < rel && !this.t.editBox.enable) {

                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }
                    }
                }
            }

            if (this.t.hitTask === j && this.t.hitMilestone === i && this.t.hitProject === p) {
                if (!this.t.editBox.enable && !this.t.milestoneChange) {

                    if (this.t.itemMargin * 2 < rel && !this.t.editBox.enable) {

                        this.t.ctx.fillStyle = kTaskSelectedBackColor;

                        if (this.t.fullscreen) {
                            this.t.ctx.fillRect(0, rel, this.t.ctxWidth, this.t.itemHeight);
                        } else {
                            this.t.ctx.fillRect(0, floor2(rel - this.t.itemHeight * 0.5) - 0.5, this.t.ctxWidth, this.t.itemMargin + 0.5);
                        }
                    }
                }
            }
        }
    };

    function LinkWidget(delegate) {
        this.t = delegate;
        this.bound = { x: 0, y: 0, w: 0, h: 0 };
        this.internalBound = { x: 0, y: 0, w: 0, h: 0 };
        this.direction = 0;
        this.handler = null;
        this.link = null;
        this.linkDays = 0;
        this.titleIndent = { x: 0, y: 0 };

        this.disableBound = false;
    }
    LinkWidget.prototype = {
        setBound: function (bound, direction, days, sign) {
            if (this.disableBound) return;

            this.bound = bound;

            // 0 - горизонтальное расположение
            // 1 - вертикальное расположение

            this.direction = direction;
            this.linkDays = days;

            this.days = '';
            this.titleIndent = { x: 0, y: 0 };

            if (this.linkDays > 0) {
                if (sign) {
                    this.days = '+';
                }

                this.days += this.linkDays + ' ' + window['Gantt']['Localize_strings']['dayShort'];

                this.titleIndent.x = kLinksWidgetSettings.titleFx * (this.days.length > 0);
                this.titleIndent.y = kLinksWidgetSettings.titleFy * (this.days.length > 0);

            } else if (this.linkDays < 0) {
                if (sign) {
                    this.days = '-';
                } else {
                    this.linkDays = Math.abs(this.linkDays);
                }

                this.days = this.linkDays + ' ' + window['Gantt']['Localize_strings']['dayShort'];

                this.titleIndent.x = kLinksWidgetSettings.titleFx * (this.days.length > 0);
                this.titleIndent.y = kLinksWidgetSettings.titleFy * (this.days.length > 0);
            }

            if (direction) {
                this.internalBound = {
                    x: this.bound.x - kLinksWidgetSettings.h * 0.5,
                    y: this.bound.y - kLinksWidgetSettings.trx - this.titleIndent.y,
                    w: kLinksWidgetSettings.h,
                    h: kLinksWidgetSettings.w - kLinksWidgetSettings.trx + this.titleIndent.y
                };
            } else {
                this.internalBound = {
                    x: this.bound.x + kLinksWidgetSettings.trx,
                    y: this.bound.y - kLinksWidgetSettings.h * 0.5,
                    w: kLinksWidgetSettings.w - kLinksWidgetSettings.trx + this.titleIndent.x,
                    h: kLinksWidgetSettings.h
                };
            }
        },
        setLink: function (link) {
            this.link = link;
            this.t.needUpdate = true;
        },
        render: function (x, y) {
            this.t.painter.clearZones(true);

            if (this.direction) {

                this.t.painter.addZone(this.internalBound.x - kLinksWidgetSettings.h,
                        this.internalBound.y - kLinksWidgetSettings.h,
                        this.internalBound.w + kLinksWidgetSettings.h,
                        this.internalBound.h + kLinksWidgetSettings.h, 5);

                // отрисовка виджета (задний серый фон)

                this.t.overlayctx.fillStyle = '#83888D';
                this.t.overlayctx.lineWidth = 1;
                this.t.overlayctx.beginPath();

                this.t.overlayctx.moveTo(floor2(this.internalBound.x), floor2(this.internalBound.y + kLinksWidgetSettings.h * 0.5 + this.titleIndent.y));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x + this.internalBound.w * 0.5), floor2(this.internalBound.y + this.internalBound.h));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x + this.internalBound.w + 2), floor2(this.internalBound.y + kLinksWidgetSettings.h * 0.5 + this.titleIndent.y));

                this.t.overlayctx.lineTo(floor2(this.internalBound.x + this.internalBound.w + 2), floor2(this.internalBound.y - kLinksWidgetSettings.h * 0.5));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x), floor2(this.internalBound.y - kLinksWidgetSettings.h * 0.5));

                this.t.overlayctx.closePath();
                this.t.overlayctx.fill();

                // белый прямоугольник для иконки

                this.t.overlayctx.fillStyle = '#FFFFFF';
                this.t.overlayctx.fillRect(floor2(this.bound.x - kLinksWidgetIcoSettings.s * 0.5 + 1.0),
                        floor2(this.internalBound.y + kLinksWidgetSettings.h * 0.5) - kLinksWidgetIcoSettings.yh - kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s);

                // иконка удаления

                this.t.overlayctx.fillStyle = '#83888D';
                this.t.overlayctx.beginPath();
                this.t.painter.drawIcoCross(this.t.overlayctx, floor2(this.bound.x - kLinksWidgetIcoSettings.s * 0.5 + 1.0),
                        floor2(this.internalBound.y + kLinksWidgetSettings.h * 0.5) - kLinksWidgetIcoSettings.yh - kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s);
                this.t.overlayctx.closePath();
                this.t.overlayctx.fill();

                // надпись с днями

                if (this.days.length && this.linkDays <= 9999 && this.linkDays >= -9999) {

                    this.t.overlayctx.fillStyle = '#FFFFFF';

                    if (abs2(this.linkDays) < 100)
                        this.t.overlayctx.font = '7pt ' + kDefaultFontName;
                    else if (abs2(this.linkDays) >= 100 && abs2(this.linkDays) < 1000)
                        this.t.overlayctx.font = '6pt ' + kDefaultFontName;
                    else
                        this.t.overlayctx.font = '5pt ' + kDefaultFontName;

                    this.t.overlayctx.fillText(this.days,
                            this.internalBound.x + (this.internalBound.w - this.t.overlayctx.measureText(this.days).width) * 0.5,
                            this.internalBound.y + kLinksWidgetIcoSettings.s * 1.3);
                }

            } else {

                this.t.painter.addZone(this.internalBound.x - kLinksWidgetSettings.h,
                    this.internalBound.y,
                        this.internalBound.w + kLinksWidgetSettings.h, this.internalBound.h, 5);

                // отрисовка виджета (задний серый фон)

                this.t.overlayctx.fillStyle = '#83888D';
                this.t.overlayctx.lineWidth = 1;
                this.t.overlayctx.beginPath();

                this.t.overlayctx.moveTo(floor2(this.internalBound.x), floor2(this.internalBound.y));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x - kLinksWidgetSettings.h * 0.5), floor2(this.internalBound.y + this.internalBound.h * 0.5));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x), floor2(this.internalBound.y + this.internalBound.h));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x + kLinksWidgetSettings.w - kLinksWidgetSettings.trx + this.titleIndent.x), floor2(this.internalBound.y + this.internalBound.h));
                this.t.overlayctx.lineTo(floor2(this.internalBound.x + kLinksWidgetSettings.w - kLinksWidgetSettings.trx + this.titleIndent.x), floor2(this.internalBound.y));

                this.t.overlayctx.closePath();
                this.t.overlayctx.fill();

                // белый прямоугольник для иконки

                this.t.overlayctx.fillStyle = '#FFFFFF';
                this.t.overlayctx.fillRect(floor2(this.bound.x + kLinksWidgetIcoSettings.xw + this.titleIndent.x),
                    floor2(this.internalBound.y + kLinksWidgetIcoSettings.yh),
                    kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s);

                // иконка удаления

                this.t.overlayctx.fillStyle = '#83888D';
                this.t.overlayctx.beginPath();
                this.t.painter.drawIcoCross(this.t.overlayctx, this.bound.x + kLinksWidgetIcoSettings.xw - 0.5 + this.titleIndent.x,
                        this.internalBound.y + kLinksWidgetIcoSettings.yh,
                    kLinksWidgetIcoSettings.s,
                    kLinksWidgetIcoSettings.s);

                this.t.overlayctx.closePath();
                this.t.overlayctx.fill();

                // надпись с днями

                if (this.days.length && this.linkDays <= 9999 && this.linkDays >= -9999) {

                    this.t.overlayctx.fillStyle = '#FFFFFF';

                    if (abs2(this.linkDays) < 100)
                        this.t.overlayctx.font = '7pt ' + kDefaultFontName;
                    else if (abs2(this.linkDays) >= 100 && abs2(this.linkDays) < 1000)
                        this.t.overlayctx.font = '6pt ' + kDefaultFontName;
                    else
                        this.t.overlayctx.font = '5pt ' + kDefaultFontName;

                    this.t.overlayctx.fillText(this.days,
                            this.internalBound.x + (this.internalBound.h - this.t.overlayctx.measureText(this.days).width) * 0.5 - 5,
                            this.internalBound.y + kLinksWidgetIcoSettings.s * 0.95);
                }
            }
        },
        reset: function () {
            this.bound = { x: 0, y: 0, w: 0, h: 0 };
            this.link = null;
        },
        isValid: function () {
            return (this.bound.w + this.bound.h > 0);
        },
        hide: function () {
            this.bound = { x: 0, y: 0, w: 0, h: 0 };
            this.internalBound = { x: 0, y: 0, w: 0, h: 0 };
            this.link = null;
            this.t.needUpdate = true;
        },

        // проверка на попадание в область баунд меню

        checkZone: function (x, y) {

            // вертикально оринтирован

            if (this.direction) {
                if (this.internalBound.x >= x || x >= this.internalBound.x + this.internalBound.w ||
                    this.internalBound.y - 10 >= y || y >= this.internalBound.y + this.internalBound.h) {      //  TODO:

                    return false;
                }

            } else {

                // горизонтально ориентирован

                if (this.internalBound.x - 20 > x || x > this.internalBound.x + this.internalBound.w + 7 ||
                    this.internalBound.y - 2 > y || y > this.internalBound.y + this.internalBound.h) {      //  TODO:

                    return false;
                }
            }

            return true;
        },

        // events

        onmousedown: function (x, y) {
            if (this.handler) {
                if (this.isValid()) {
                    if (this.direction) {
                        if (x >= this.bound.x - kLinksWidgetIcoSettings.s &&
                            x <= this.bound.x + kLinksWidgetIcoSettings.s &&
                            y <= this.internalBound.y + kLinksWidgetIcoSettings.s &&
                            y >= this.internalBound.y - kLinksWidgetIcoSettings.s) {

                            this.handler();
                            return true;
                        }
                    } else {
                        if (x >= this.bound.x + kLinksWidgetIcoSettings.xw + this.titleIndent.x &&
                            x <= this.bound.x + kLinksWidgetIcoSettings.xw + kLinksWidgetIcoSettings.s + this.titleIndent.x &&
                            y >= this.internalBound.y + kLinksWidgetIcoSettings.yh &&
                            y <= this.internalBound.y + kLinksWidgetIcoSettings.yh + kLinksWidgetIcoSettings.s) {

                            this.handler();
                            return true;
                        }
                    }
                }
            }

            return false;
        },
        onmousemove: function (x, y) {
            if (this.isValid()) {
                if (this.direction) {
                    if (x >= this.internalBound.x && x <= this.internalBound.x + this.internalBound.w &&
                        y >= this.internalBound.y - 15 && y <= this.internalBound.y + this.internalBound.h + 7) {

                        // if (x >= this.bound.x + kLinksWidgetIcoSettings.xw &&
                        //    x <= this.bound.x + kLinksWidgetIcoSettings.xw + kLinksWidgetIcoSettings.s &&
                        //    y >= this.internalBound.y + kLinksWidgetIcoSettings.yh &&
                        //    y <= this.internalBound.y + kLinksWidgetIcoSettings.yh + kLinksWidgetIcoSettings.s) {


                        this.t.overlay.style.cursor = 'pointer';
                        // }

                        return true;
                    }

                    this.hide();

                } else {
                    if (x >= this.internalBound.x - 20 && x <= this.internalBound.x + this.internalBound.w &&
                        y >= this.internalBound.y && y <= this.internalBound.y + this.internalBound.h) {

                        // if (x >= this.bound.x + kLinksWidgetIcoSettings.xw &&
                        //    x <= this.bound.x + kLinksWidgetIcoSettings.xw + kLinksWidgetIcoSettings.s &&
                        //    y >= this.internalBound.y + kLinksWidgetIcoSettings.yh &&
                        //    y <= this.internalBound.y + kLinksWidgetIcoSettings.yh + kLinksWidgetIcoSettings.s) {


                        this.t.overlay.style.cursor = 'pointer';
                        // }

                        return true;
                    }

                    this.hide();
                }

            }

            return false;
        }
    };

    function TaskWidget(delegate) {
        this.t = delegate;
        this.ref = null;
        this.bound = { x: 0, y: 0, w: 0, h: 0 };
        this.detailsLinkBound = { x: 0, y: 0, w: 0, h: 0 };
        this.href = null;
    }
    TaskWidget.prototype = {
        set: function (x, y, t, fill) {
            if (this.ref) {
                if (this.ref.t._id == t._id) {
                    this.ref.x = x;
                    this.ref.y = y;
                    this.ref.fill = fill;
                    return;
                }

                this.ref.t.dropDownWidget = false;
            }

            this.ref = { x: x, y: y, t: t, fill: fill };
        },
        check: function (t) {
            if (this.ref) {
                if (t && this.ref.t._id == t._id) {
                    this.ref.t.dropDownWidget = false;
                    this.ref = null;
                    return false;
                }

                this.ref.t.dropDownWidget = false;
                if (!t) this.ref = null;
            }
            return true;
        },
        checkBound: function (x, y) {
            return this.bound.x > x || x > this.bound.x + this.bound.w ||
                this.bound.y > y || y > this.bound.y + this.bound.h;
        },
        checkInBoundDetailsLink: function (x, y) {
            return this.detailsLinkBound.x < x && x < this.detailsLinkBound.x + this.detailsLinkBound.w &&
                this.detailsLinkBound.y < y && y < this.detailsLinkBound.y + this.detailsLinkBound.h;
        },
        checkAndOff: function (t) {
            if (this.ref) {
                if (t && this.ref.t._id == t._id) {
                    this.ref.t.dropDownWidget = false;
                    this.ref = null;
                }
            }
        },
        setLinkRef: function (s) { this.href = s; },
        isValid: function () {
            return (null !== this.ref);
        }
    };

    function TimeLine(canvas, overlay) {
        this.setZoom(1);
        this.initBase(canvas, overlay);
        this.initUI();
        this.initStyles();
        this.initWidgets();

        // mode settings

        this.readMode = false;
        this.sticking = true;
        this.userEvents = true;
        this.fullscreen = false;

        this._modeStrafe = false;

        this.centerScreen = 0;
        this.visibleLeft = 0;
        this.visibleUp = 0;
        this.offsetX = 0;

        this.needUpdate = true;
        this.needUpdateContent = true;
        this.needUpdateScrollContent = false;
        this.repaint = false;

        this.capProject = 0;
        this.capTask = -1;
        this.capSide = 0;
        this.capMilestone = -1;

        this.hitProject = -1;
        this.hitTask = -1;
        this.hitMilestone = -1;
        this.hitSide = 0;

        this.clickPosCentreing = null;

        this.isLBMDown = false;
        this.downMouse = { x: 0, y: 0 };
        this.moveMouse = { x: 0, y: 0 };

        this.hitLine = { p: -1, m: -1, t: -1 };       // mouse hit task area
        this.anchorMouse = { x: 0, y: 0 };            // mouse offset
        this.pressMouse = { x: 0, y: 0, change: false };

        this.enableTouch = false;
        this.createMode = null;

        this.currentElementY = 0;

        // handlers for events

        this.handlers = [];

        // edit

        this.editMode = kEditModeNoUse;

        // for links mode

        this.hitLink = undefined;
        this.linkLineEdit = undefined;
        this.pushLink = undefined;

        // drag&drop mode

        this.dragBound = { x: 0, y: 0, width: 0, height: 0, dx: 0, dy: 0, time: 0, failEnd: false };
        this.itemToDrop = { t: -1, m: -1, p: -1 };
        this.dragTo = { t: -1, m: -1, p: -1 };

        //
        this.clearPopUp = false;

        //
        this.queryMoveLinks = null;

        //
        this.leftBoxMousePressed = 0;
        this.pressedButtonRight = false;
        this.pressedButtonLeft = false;

        //
        this.printVersion = false;

        //

        this.projectsLines = [];
        this.projectsLinesY = [];


        //

        this._needDrawFlashBackground = false;
    }
    TimeLine.prototype = {

        initBase: function (canvas, overlay) {
            var t = this;

            if (canvas) {
                this.canvas = canvas;
                this.ctx = canvas.getContext('2d');

                this.ctxWidth = this.ctx.canvas.width;
                this.ctxHeight = this.ctx.canvas.height;
                this.bounding = this.ctx.canvas.getBoundingClientRect();

                if (overlay) {
                    this.overlay = overlay;
                    this.overlayctx = this.overlay.getContext('2d');
                    this.updateDragDrop = false;

                    // если выходим за границу контрола то скрываем все всплывающие менюшки

                    this.overlay.onmouseout = function (e) {
                        t.leftPanelController().updateFocus();
                        t.leftPanelController().clearHighlight();
                        t.rightScroll.focus(false);
                        t.hitProject = -1;
                        t.hitMilestone = -1;
                        t.hitTask = -1;
                        t.offMenus();
                        t.painter.clearZones(true);
                        t.needUpdate = true;
                    };
                }

                lockImg.src = "/Products/Projects/App_Themes/default/images/small-icons.png"; // PRIVATE ICON

                this._userDefaults = new UserDefaults(this);
                this.storage = new Storage(this);
                this._undoManager = new UndoManager(this);
                this._viewController = new ViewController(this);
                this._modelController = new ModelController(this);
                this.painter = new Painter(this.ctx, this.overlayctx);
                this.animator = new Animator(this);
                this._renderer = new ElementRenderer(this);

                this._leftPanelController = new LeftPanelController(this, this.storage, document.getElementById('ganttCanvasContainer'));
                if (this._leftPanelController) {
                    this._leftPanelController.init();
                }

                // данны для связи
                this._mouseInLinkZone = null;

                this.repaint = true;
            }
        },
        initStyles: function () {
            this.taskTextIndent = 11;     //  px

            this.milestoneMenuOffY = 3;
            this.menuArrowSize = 4;
            this.menuHeight = 24;

            this.menuLeftIndent = 1;//7;
            this.menuTopIndent = 4;
            this.menuIndentMiddle = 20;

            this.milestoneLinewidth = 3;      //  px
            this.milestoneKeyLeftIndent = 26;     //  px
        },
        initWidgets: function () {
            // popup menus

            var t = this;

            this.menuTask = {
                offx: 0,
                offy: 0,
                p: -1,
                m: -1,
                t: -1,
                ref: null,
                _status: kElementActive,
                bound: { x: 0, y: 0, w: 0, h: 0 },
                futureBound: { x: 0, y: 0, w: 0, h: 0 },
                handlers: [],
                painter: new Painter(t.overlayctx, null),
                disableSetter: false,

                //
                set: function (p, m, t, ref, x, y) {

                    if (this.ref) {
                        if (ref && (this.ref._id === ref._id)) {
                            this.bound = this.futureBound;
                            return;
                        }
                    }

                    if (-1 !== p && -1 !== t && m === -1) {
                        this.p = p;
                        this.m = m;
                        this.t = t;
                    } else {
                        if (-1 !== p) this.p = p;
                        if (-1 !== m) this.m = m;
                        if (-1 !== t) this.t = t;
                    }

                    if (ref) {
                        this.ref = ref;
                        this._status = this.ref._status;
                        this.bound = this.futureBound;
                    }
                },
                setBound: function (x, y, w, h) {
                    this.futureBound = { x: x, y: y, w: w, h: h };
                },
                check: function (x, y) {
                    if (this.bound.x > x || x > this.bound.x + this.bound.w ||
                        this.bound.y > y || y > this.bound.y + this.bound.h
                        + t.itemHeight) {

                        this.bound.w = 0;
                        this.bound.h = 0;

                        return false;
                    }

                    return true;
                },

                // проверка на попадание в область баунд меню

                checkZone: function (x, y) {
                    var cw, cx = this.bound.x + this.bound.w * 0.5;

                    if (kElementActive == this._status) {
                        cw = kMenuSettings.elementsWidth5X * 0.5;
                    } else {
                        cw = kMenuSettings.elementsWidth2X * 0.5;
                    }

                    if (cx - cw > x || x > cx + cw || this.bound.y > y || y > this.bound.y + this.bound.h)
                        return false;

                    return true;
                },

                reset: function () {
                    this.bound.w = 0;
                    this.bound.h = 0;
                    this.futureBound.w = 0;
                    this.futureBound.h = 0;
                    this.p = -1;
                    this.m = -1;
                    this.t = -1;
                    this.ref = null;
                },
                addHandler: function (f) {
                    this.handlers.push(f);
                },
                value: function (that, x, y, parts) {
                    if (y - this.bound.y <= (that.menuTopIndent + kMenuSettings.icoSize) * that.zoom && this.bound.w > 0)
                        return floor2((x - this.bound.x) / floor2(this.bound.w / parts));

                    return -1;
                },

                //

                draw: function () {
                    if (0 < this.bound.w && 0 < this.bound.h) {

                        var sizeW = kMenuSettings.elementsWidth5X * t.zoom;
                        if (this._status == kElementCompleted) {
                            sizeW = kMenuSettings.elementsWidth2X * t.zoom;
                        }

                        var dX = this.bound.x + this.bound.w * 0.5 - sizeW * 0.5;
                        var h = kMenuSettings.icoSize + kMenuSettings.borderSz * 2;

                        //                        t.overlayctx.fillStyle = kMenuSettings.borderColor;
                        //                        t.overlayctx.fillRect(dX - 1, this.bound.y, sizeW + 2, this.bound.h + 2);
                        //
                        //                        t.overlayctx.beginPath();
                        //                        t.overlayctx.moveTo(dX + sizeW * 0.5, this.bound.y + this.bound.h + t.menuArrowSize * t.zoom + 2);
                        //                        t.overlayctx.lineTo(dX + sizeW * 0.5 - t.menuArrowSize * t.zoom, this.bound.y + this.bound.h + 2);
                        //                        t.overlayctx.lineTo(dX + sizeW * 0.5 + t.menuArrowSize * t.zoom, this.bound.y + this.bound.h + 2);
                        //                        t.overlayctx.lineTo(dX + sizeW * 0.5 + t.menuArrowSize * t.zoom, this.bound.y + this.bound.h + 2);
                        //                        t.overlayctx.closePath();
                        //                        t.overlayctx.fill();

                        t.overlayctx.fillStyle = kMenuSettings.backgroundColor;
                        t.overlayctx.fillRect(dX - kMenuSettings.borderSz, this.bound.y + 1, sizeW + kMenuSettings.borderSz * 2, h);

                        t.overlayctx.beginPath();
                        t.overlayctx.moveTo(dX + sizeW * 0.5, this.bound.y + h + t.menuArrowSize * t.zoom + 1);
                        t.overlayctx.lineTo(dX + sizeW * 0.5 - t.menuArrowSize * t.zoom, this.bound.y + h + 1);
                        t.overlayctx.lineTo(dX + sizeW * 0.5 + t.menuArrowSize * t.zoom, this.bound.y + h + 1);
                        t.overlayctx.closePath();
                        t.overlayctx.fill();

                        var sw = kMenuSettings.icoSize * t.zoom;
                        var ty = this.bound.y + kMenuSettings.borderSz * t.zoom + 1;
                        var icoCount = (this._status == kElementCompleted) ? 2 : 5;

                        for (var i = 0; i < icoCount; ++i) {
                            var tx = dX - 1 + (t.menuLeftIndent + i * t.menuIndentMiddle) * t.zoom;

                            t.overlayctx.fillStyle = kMenuSettings.borderColor;
                            t.overlayctx.fillRect(tx, ty, sw, sw);

                            if (4 === i) {
                                t.overlayctx.fillStyle = kMenuSettings.backgroundColor;
                                t.overlayctx.strokeStyle = kMenuSettings.backgroundColor;

                                this.painter.drawIconLink(tx, ty, sw, sw);

                                continue;
                            }

                            t.overlayctx.fillStyle = kMenuSettings.backgroundColor;
                            t.overlayctx.beginPath();

                            // pen
                            if (0 === i) {
                                if (this._status == kElementCompleted) {
                                    this.painter.drawPlay(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawPencil(tx, ty, sw, sw);
                                }
                            } else if (1 === i) {
                                if (this._status == kElementCompleted) {
                                    this.painter.drawDelete(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawIcoResponsible(tx, ty, sw, sw);
                                }
                            }

                            if (2 === i) {
                                this.painter.drawDelete(tx, ty, sw, sw);
                            }

                            if (3 === i) {
                                this.painter.drawComplete(tx, ty, sw, sw);
                            }

                            t.overlayctx.closePath();
                            t.overlayctx.fill();
                        }

                        return true;
                    }

                    return false;
                },

                // events

                onmousemove: function (x, y) {
                    if (0 === this.bound.w && 0 === this.bound.h)
                        return false;

                    if (this.check(x, y)) {
                        //if ((t.menuTopIndent +  kMenuPopUpSettings.icoSize) * t.zoom >= y - this.bound.y &&
                        //    (t.menuTopIndent) * t.zoom <= y - this.bound.y && this.bound.w > 0) {
                        t.overlay.style.cursor = 'pointer';
                        // } else {
                        //    if(y < this.bound.y + this.bound.h)
                        //         t.overlay.style.cursor = '';
                        //}

                        t.needUpdate = true;

                        return true;
                    }

                    return false;
                },
                onmousedown: function (x, y) {
                    if ((t.menuTopIndent + kMenuSettings.icoSize) * t.zoom >= y - this.bound.y &&
                        (t.menuTopIndent) * t.zoom <= y - this.bound.y &&
                        this.bound.w > 0) {

                        var parts = (this._status == kElementCompleted) ? 2 : 5;
                        var menuInd = floor2((x - this.bound.x) / floor2(this.bound.w / parts));

                        if (menuInd < this.handlers.length && menuInd >= 0) {
                            this.handlers[menuInd](this.p, this.m, -1);
                        }

                        return true
                    }

                    return false;
                }
            };

            this.menuTask.addHandler(function () {
                if (kElementCompleted === t.menuTask.ref._status) {
                    //if (t._modelController.checkStatus({p: t.menuTask.p, m: t.menuTask.m, t: t.menuTask.t})) {
                    var cs = findCustomStatus(function (item) {
                        return item.statusType === 1 && item.isDefault;
                    });
                    t._modelController.addTaskOperation(kHandlerBeforeChangeTaskStatus, t.menuTask.p, t.menuTask.m == -1 ? undefined : t.menuTask.m, t.menuTask.t, cs.id);
                    //}
                    t.menuTask.reset();
                } else {
                    var ts = t.storage.getTask(t.menuTask.p, t.menuTask.m == -1 ? undefined : t.menuTask.m, t.menuTask.t);
                    if (ts) t.taskDescWidget.checkAndOff(ts);

                    if (!t.showPopUpEditElement(t.menuTask.p, t.menuTask.m === -1 ? undefined : t.menuTask.m, t.menuTask.t))
                        t.editElementTitle(t.menuTask.p, t.menuTask.m, t.menuTask.t);

                    t.offWidgets();
                    t.needUpdate = true;
                }
            });
            this.menuTask.addHandler(function () {
                if (kElementCompleted === t.menuTask.ref._status) {
                    t._modelController.addTaskOperation(kHandlerBeforeDeleteTask, t.menuTask.p, t.menuTask.m === -1 ? undefined : t.menuTask.m, t.menuTask.t);
                } else {

                    var posX = t.menuTask.bound.x + t.menuTask.bound.w * 0.5;

                    // назначения отвественных для задачи

                    t.showPopUpResp(t.menuTask.p, t.menuTask.m === -1 ? undefined : t.menuTask.m, t.menuTask.t, posX);
                }
                t.menuTask.reset();
                t.isLBMDown = false;
            });
            this.menuTask.addHandler(function () {
                t._modelController.addTaskOperation(kHandlerBeforeDeleteTask, t.menuTask.p, t.menuTask.m === -1 ? undefined : t.menuTask.m, t.menuTask.t);
                t.menuTask.reset();
                t.isLBMDown = false;
            });
            this.menuTask.addHandler(function () {
                var cs = findCustomStatus(function (item) {
                    return item.statusType === 2 && item.isDefault;
                });
                t._modelController.addTaskOperation(kHandlerBeforeChangeTaskStatus, t.menuTask.p, t.menuTask.m == -1 ? undefined : t.menuTask.m, t.menuTask.t, cs.id);
                t.menuTask.reset();
            });
            this.menuTask.addHandler(function () {
                if (kElementCompleted !== t.menuTask.ref._status) {
                    if (t.handlers[kHandlerBeforeMenuAddTaskLink]) {
                        var ts = t.storage.getTask(t.menuTask.p, t.menuTask.m == -1 ? undefined : t.menuTask.m, t.menuTask.t);
                        t.handlers[kHandlerBeforeMenuAddTaskLink](ts);

                        t.offWidgets();
                        t.needUpdate = true;
                    }
                }
            });

            this.menuMileStone = {
                offx: 0,
                offy: 0,
                p: -1,
                m: -1,
                ref: null,
                _status: kElementActive,
                bound: { x: 0, y: 0, w: 0, h: 0 },
                hide: true,
                handlers: [],
                painter: new Painter(t.overlayctx, null),
                disable: false,

                //
                set: function (p, m, ref) {
                    if (-1 !== p) { this.p = p; }
                    if (-1 !== m) { this.m = m; }

                    if (ref) {
                        this.ref = ref;
                        this._status = this.ref._status;
                    }
                },
                check: function (x, y) {
                    if (this.bound.x > x || x > this.bound.x + this.bound.w || this.bound.y > y || y > this.bound.y + this.bound.h + 10) {

                        this.bound.w = 0;
                        this.bound.h = 0;
                        this.hide = true;
                        // t.needUpdate = true;

                        return false;
                    }

                    // t.menuTask.reset();
                    return true;
                },
                reset: function () {
                    this.bound.w = 0;
                    this.bound.h = 0;
                    this.p = -1;
                    this.m = -1;
                    this.t = -1;
                    this.hide = true;
                    this.ref = null;
                    this._status = kElementActive - 1;
                    this.disable = false;
                },
                addHandler: function (f) {
                    this.handlers.push(f);
                },
                setBound: function (x, y, w, h) {
                    this.bound = { x: x, y: y, w: w, h: h };
                },
                //
                // проверка на попадание в область баунд меню

                checkZone: function (x, y) {
                    if (this.disable) { return false; }
                    var cw, cx = this.bound.x + this.bound.w * 0.5;

                    if (kElementActive - 1 == this._status) {
                        cw = kMenuSettings.elementsWidth6X * 0.5;
                    } else {
                        cw = kMenuSettings.elementsWidth3X * 0.5;
                    }

                    if (cx - cw > x || x > cx + cw || this.bound.y > y || y > this.bound.y + this.bound.h)
                        return false;

                    return true;
                },
                draw: function () {

                    if (0 < this.bound.w && 0 < this.bound.h && !this.hide && !this.disable) {

                        var sizeW = kMenuSettings.elementsWidth6X * t.zoom;
                        if (this._status == kElementCompleted - 1) {
                            sizeW = kMenuSettings.elementsWidth3X * t.zoom;
                        }

                        var dX = this.bound.x + this.bound.w * 0.5 - sizeW * 0.5;
                        var h = kMenuSettings.icoSize + kMenuSettings.borderSz * 2;

                        //                        t.overlayctx.fillStyle = kMenuSettings.borderColor;
                        //                        t.overlayctx.fillRect(dX - 1, this.bound.y, sizeW + 2, this.bound.h + 2 - t.menuArrowSize * t.zoom * 2);
                        //
                        //                        t.overlayctx.beginPath();
                        //                        t.overlayctx.moveTo(dX + sizeW * 0.5,
                        //                            this.bound.y + this.bound.h - t.menuArrowSize * t.zoom + 2);
                        //                        t.overlayctx.lineTo(dX + sizeW * 0.5 - t.menuArrowSize * t.zoom,
                        //                            this.bound.y + this.bound.h- t.menuArrowSize * t.zoom * 2 + 2);
                        //                        t.overlayctx.lineTo(dX + sizeW * 0.5 + t.menuArrowSize * t.zoom,
                        //                            this.bound.y + this.bound.h - t.menuArrowSize * t.zoom * 2 + 2);
                        //                        t.overlayctx.closePath();
                        //                        t.overlayctx.fill();

                        t.overlayctx.fillStyle = kMenuSettings.backgroundColor;
                        t.overlayctx.fillRect(dX - kMenuSettings.borderSz, this.bound.y + 1, sizeW + kMenuSettings.borderSz * 2, h);

                        h += kMenuSettings.borderSz;

                        t.overlayctx.beginPath();
                        t.overlayctx.moveTo(dX + sizeW * 0.5,
                                this.bound.y + h + 1 - t.menuArrowSize * t.zoom);
                        t.overlayctx.lineTo(dX + sizeW * 0.5 - t.menuArrowSize * t.zoom,
                                this.bound.y + h + 1 - t.menuArrowSize * t.zoom * 2);
                        t.overlayctx.lineTo(dX + sizeW * 0.5 + t.menuArrowSize * t.zoom,
                                this.bound.y + h + 1 - t.menuArrowSize * t.zoom * 2);
                        t.overlayctx.closePath();
                        t.overlayctx.fill();

                        var sw = kMenuSettings.icoSize * t.zoom;
                        var ty = this.bound.y + kMenuSettings.borderSz * t.zoom + 1;
                        var icoCount = (this._status == kElementCompleted - 1) ? 3 : 6;

                        for (var i = 0; i < icoCount; ++i) {
                            var tx = dX - 1 + (t.menuLeftIndent + i * t.menuIndentMiddle) * t.zoom;

                            t.overlayctx.fillStyle = kMenuSettings.borderColor;
                            t.overlayctx.fillRect(tx, ty, sw, sw);

                            t.overlayctx.fillStyle = kMenuSettings.backgroundColor;
                            t.overlayctx.beginPath();

                            if (0 === i) {
                                if (this._status == kElementCompleted - 1) {
                                    this.painter.drawPlay(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawPencil(tx, ty, sw, sw);
                                }
                            } else if (1 === i) {
                                if (this._status == kElementCompleted - 1) {
                                    this.painter.drawDelete(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawIcoResponsible(tx, ty, sw, sw);
                                }
                            }

                            if (2 === i) {
                                if (this._status == kElementCompleted - 1) {
                                    this.painter.drawFitSw(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawDelete(tx, ty, sw, sw);
                                }
                            }

                            // add
                            if (3 === i) {
                                if (this._status == kElementCompleted - 1) {
                                    this.painter.drawFitSw(tx, ty, sw, sw);
                                } else {
                                    this.painter.drawAdd(tx, ty, sw, sw);
                                }
                            }

                            // fit zoom
                            if (4 === i) { this.painter.drawFitSw(tx, ty, sw, sw); }
                            if (5 == i) { this.painter.drawComplete(tx, ty, sw, sw); }

                            t.overlayctx.closePath();
                            t.overlayctx.fill();
                        }

                        return true;
                    }

                    return false;
                },

                // events

                onmousemove: function (x, y) {
                    if (this.disable) { t.overlay.style.cursor = 'pointer'; return false; }

                    if (0 === this.bound.w && 0 === this.bound.h) {
                        return false;
                    }

                    if (this.check(x, y)) {
                        // if ((t.menuTopIndent + kMenuSettings.icoSize) * t.zoom >= y - this.bound.y &&
                        //    (t.menuTopIndent) * t.zoom <= y - this.bound.y && this.bound.w > 0) {
                        t.overlay.style.cursor = 'pointer';
                        //     t.needUpdate = true;

                        // } else {
                        //     t.overlay.style.cursor = '';
                        t.needUpdate = true;

                        // }

                        return true;
                    }

                    return false;
                },
                onmousedown: function (x, y) {
                    if (this.disable) { return false; }
                    if ((t.menuTopIndent + kMenuSettings.icoSize) * t.zoom >= y - this.bound.y &&
                        (t.menuTopIndent) * t.zoom <= y - this.bound.y && this.bound.w > 0) {

                        if (y < this.bound.y + 6 * t.zoom) {
                            t.menuMileStone.reset();
                            return false;
                        }

                        var parts = (this._status == kElementCompleted - 1) ? 3 : 6;
                        var width = (this._status == kElementCompleted - 1) ? kMenuSettings.elementsWidth3X : kMenuSettings.elementsWidth6X;
                        var menuInd = floor2((x - this.bound.x) / floor2(width / parts));

                        // var parts = 6;
                        //var menuInd = floor2((x - this.bound.x) / floor2(this.bound.w / parts));
                        //if (this._status == kElementCompleted - 1) {
                        //    --menuInd; if (menuInd < 0) return false;
                        // }

                        if (menuInd < this.handlers.length && menuInd >= 0) {
                            this.handlers[menuInd](this.p, this.m, -1);
                        }

                        return true;
                    }

                    return false;
                }
            };

            this.menuMileStone.addHandler(function () {             // 0
                if (t.menuMileStone.ref.status() == kElementActive - 1) {
                    if (!t.showPopUpEditElement(t.menuMileStone.p, t.menuMileStone.m === -1 ? undefined : t.menuMileStone.m, -1))
                        t.editElementTitle(t.menuMileStone.p, t.menuMileStone.m, -1);

                    t.offWidgets();
                } else {
                    t._modelController.addMilestoneOperation(kHandlerBeforeChangeMilestoneStatus, t.menuMileStone.p, t.menuMileStone.m);
                }
                t.isLBMDown = false;
                t.menuMileStone.reset();
            });
            this.menuMileStone.addHandler(function () {               // 1

                if (t.menuMileStone.ref.status() == kElementActive - 1) {

                    var posX = t.menuMileStone.bound.x + t.menuMileStone.bound.w * 0.5;

                    // назначения отвественных для вехи

                    t.showPopUpResp(t.menuMileStone.p, t.menuMileStone.m, t.menuMileStone.t, posX);
                } else {
                    t._modelController.addMilestoneOperation(kHandlerBeforeDeleteMilestone, t.menuMileStone.p, t.menuMileStone.m, t.menuMileStone.t);
                }
                t.isLBMDown = false;
                t.menuMileStone.reset();
            });
            this.menuMileStone.addHandler(function () {                // 2
                if (t.menuMileStone.ref.status() == kElementActive - 1) {
                    t._modelController.addMilestoneOperation(kHandlerBeforeDeleteMilestone, t.menuMileStone.p, t.menuMileStone.m, t.menuMileStone.t);
                } else {
                    t._viewController.zoomToFitMilestone(t.menuMileStone.p, t.menuMileStone.m);
                }
                t.isLBMDown = false;
                t.menuMileStone.reset();

            });
            this.menuMileStone.addHandler(function () {                  // 3
                if (t.menuMileStone.ref.status() == kElementCompleted - 1) {
                    t._viewController.zoomToFitMilestone(t.menuMileStone.p, t.menuMileStone.m);
                } else {
                    t.addTaskToMilestone(t.menuMileStone.p, t.menuMileStone.m);
                    t.offWidgets();
                }
                t.isLBMDown = false;
                t.menuMileStone.reset();
            });
            this.menuMileStone.addHandler(function () {                     // 4
                if (t.menuMileStone.ref.status() == kElementCompleted - 1) return;

                t._viewController.zoomToFitMilestone(t.menuMileStone.p, t.menuMileStone.m);
                t.menuMileStone.reset();
            });
            this.menuMileStone.addHandler(function () {                     // 5
                t._modelController.addMilestoneOperation(kHandlerBeforeChangeMilestoneStatus, t.menuMileStone.p, t.menuMileStone.m);
                t.isLBMDown = false;
                t.menuMileStone.reset();
            });

            // edit control for add task
            this.editBox = new EditBox(this);

            // task description widget

            this.taskDescWidget = new TaskWidget(this);

            // milestone description widget

            this.milestoneDescWidget = {
                ref: null,
                bound: { x: 0, y: 0, w: 0, h: 0 },

                set: function (x, y, t, fill) {
                    if (this.ref) {
                        if (this.ref.t._id == t._id) {
                            this.ref.x = x;
                            this.ref.y = y;
                            this.ref.fill = fill;
                            return;
                        }

                        this.ref.t.dropDownWidget = false;
                    }

                    this.ref = { x: x, y: y, t: t, fill: fill };
                },
                check: function (t) {
                    if (this.ref) {
                        if (t && this.ref.t._id == t._id) {
                            this.ref.t.dropDownWidget = false;
                            this.ref = null;
                            return false;
                        }

                        this.ref.t.dropDownWidget = false;
                        if (!t) this.ref = null;
                    }
                    return true;
                },
                checkBound: function (x, y) {
                    return this.bound.x > x || x > this.bound.x + this.bound.w ||
                        this.bound.y > y || y > this.bound.y + this.bound.h;
                },
                checkAndOff: function (t) {
                    if (this.ref) {
                        if (t && this.ref.t._id == t._id) {
                            this.ref.t.dropDownWidget = false;
                            this.ref = null;
                        }
                    }
                },
                isValid: function () {
                    return (null !== this.ref);
                }
            };

            // always light milestones

            this.backLightMilestone = {
                enable: true,
                p: -1,
                m: -1,
                up: function (pi, mi) {
                    if (pi != this.p || mi != this.m) {
                        this.p = pi; this.m = mi; this.update = true;
                    }
                },
                update: false
            };

            this.linkWidget = new LinkWidget(this);
            this.linkWidget.handler = (function () {
                t._modelController.addTaskOperation(kHandlerBeforeDeleteTaskLink, this.link);
                this.hide();
            });

            this.oldRef = null;
        },
        initUI: function () {
            this.rightScroll = new ScrollBar(this.ctx, this);
            this.bottomScroll = new BottomScrollBar(this.ctx, this);
            this.timeScale = new DateScale(this, 1);

            this.zoomBar = null;
        },

        focus: function (fc, e) {
            if (!fc) {
                this.bottomScroll.onmouseup(e);
                this.rightScroll.onmouseup(e);
                this.zoomBar.releaseCapture();
                this.onmouseup(e, true);
                this.needUpdate = true;
            }
        },

        // controllers

        viewController: function () {
            return this._viewController;
        },
        modelController: function () {
            return this._modelController;
        },
        undoManager: function () {
            return this._undoManager;
        },
        userDefaults: function () {
            return this._userDefaults;
        },
        leftPanelController: function () {
            return this._leftPanelController;
        },

        setZoom: function (zoom) {
            //this.zoom             =   Math.max(1,zoom);
            this.zoom = Math.max(0.1, zoom);
            this.zoom = Math.min(8, this.zoom);

            this.itemHeight = kTimeLineItemHeight * this.zoom;
            this.itemMargin = kTimeLineItemMargin * this.zoom;
            this.itemProjectMargin = 0;
            this.fontHeight = kTimeLineItemFontHeight * this.zoom;
            this.titlesFont = this.fontHeight + 'pt ' + kDefaultFontName;
            this.fontPx = this.fontHeight * 1.5;

            if (this.timeScale) {
                this.timeScale.setZoom(this.zoom);
            }
            if (this.storage) {
                var i, j, k, mi, ti;

                var p = this.storage.projects();
                var pl = p.length;
                for (j = 0; j < pl; ++j) {
                    p[j].titleWidth = -1;
                    for (mi = p[j].m.length - 1; mi >= 0; --mi) {
                        p[j].m[mi].update();

                        for (ti = p[j].m[mi].t.length - 1; ti >= 0; --ti) {
                            p[j].m[mi].t[ti].update();
                        }
                    }

                    for (k = p[j].t.length - 1; k >= 0; --k) {
                        p[j].t[k].update();
                    }
                }

                this.updateContent();
            }
        },
        setReadMode: function (value) {
            this.readMode = value;
        },

        update: function () {
            if (this.ctx) {

                this.ctxWidth = this.ctx.canvas.width;
                this.ctxHeight = this.ctx.canvas.height;
                this.bounding = this.ctx.canvas.getBoundingClientRect();

                if (this._leftPanelController) { this._leftPanelController.getPanel().setHeight(this.ctxHeight); }

                if (this.fullscreen) {
                    this.centerScreen = this.ctxWidth * 0.5;
                    this.visibleLeft = 0;
                } else {

                    this.visibleLeft = this._leftPanelController.getPanel().getWidth();

                    this.centerScreen = (this.ctxWidth + this.visibleLeft) * 0.5;
                }
            }

            this.needUpdateContent = true;
            this.needUpdate = true;

            this.timeScale.update();
        },
        updateWithStrafe: function () {
            if (this.timeScale)
                this.timeScale.strafe(this.offsetX);

            this.update();
        },
        updateContent: function (rebuild) {
            this.rightScroll.position = 0;
            this.rightScroll._value = 0;

            this.projectsLines = [];
            this.projectsLinesY = [];

            // в случае когда нужно скролить до первого видимого горизонтального элемента нужно сделать пересчет высоты контента

            if (rebuild) {
                this.contentHeight = this.getContentHeight();
                if (!this.fullscreen) { this.contentHeight += this.itemMargin; }
                var viewHeight = this.ctxHeight - this.timeScale.height();
                this.rightScroll.init(this.timeScale.height(), viewHeight, this.contentHeight + !this.fullscreen * this.itemMargin, this.ctxWidth);
                this.rightScroll.clamp();

                if (this.zoomBar) { this.bottomScroll.updateThumb(this.zoomBar.thumb); }

                this.visibleUp = (!this.fullscreen) * (this.timeScale.height());
                this.needUpdateContent = false;
            }

            this.needUpdateContent = true;

            this.update();
        },
        updateWithScroll: function () {
            if (this.storage) {
                var i, j, k, mi, ti;

                var p = this.storage.projects();
                var pl = p.length;
                for (j = 0; j < pl; ++j) {
                    p[j].titleWidth = -1;
                    for (mi = 0; mi < p[j].m.length; ++mi) {
                        p[j].m[mi].updateTimes();
                    }
                }
            }

            this.needUpdateContent = true;
            this.update();
        },
        updateData: function () {
            this.needUpdate = true;
            if (this._leftPanelController) { this._leftPanelController.rebuildContent(); }
        },
        reset: function () {
            this.storage.reset();
            this._undoManager.reset();
        },
        offMenus: function () {
            this.menuTask.reset();
            this.menuMileStone.reset();
            this.linkWidget.setBound({ x: 0, y: 0, w: 0, h: 0 });
        },
        offWidgets: function () {
            this.milestoneDescWidget.check(null);
            this.taskDescWidget.check(null);
        },
        backLightTask: function () {
            if (this.editBox.enable) {
                return { p: this.editBox.p, m: this.editBox.m, t: this.editBox.t == -1 ? undefined : this.editBox.t };
            }

            if (this.isLBMDown) {
                return { p: this.capProject, m: this.capMilestone, t: this.capTask };
            }

            if (this.milestoneChange) {
                return { p: undefined, m: undefined, t: undefined };
            }

            if (this.menuMileStone.ref && !this.menuTask.ref) {
                return { p: this.menuMileStone.hide ? undefined : this.menuMileStone.p, m: this.menuMileStone.hide ? undefined : this.menuMileStone.m, t: undefined };
            }

            return { p: this.hitProject, m: this.hitMilestone, t: this.hitTask };
        },

        render: function () {

            if (this.zoomBar) {
                this.zoomBar.render();
            }

            this.animator.update();

            this.drawScene();
            this.drawEditLayer();

            // TODO: переделать
            if (this.mouse) {
                if (kEditModeElementDrag === this.editMode) {
                    if (this.mouse.y <= this.visibleUp) {
                        if (this.rightScroll.value() > 0) {
                            if (this.rightScroll.value() <= 0.0001) {
                                this.rightScroll.setTopPos();
                            } else {
                                var scrollTo = Math.max(floor2((this.rightScroll.value()) / this.itemMargin) * this.itemMargin - this.itemMargin, kEps);
                                this.animator.moveToY(scrollTo);
                            }
                            this.painter.clearZones(true);
                        }
                    }

                    if (this.ctxHeight - this.itemMargin < this.mouse.y) {

                        var maxVal = floor2((this.rightScroll.maxValue()) / this.itemMargin) * this.itemMargin;
                        var scrollToBottom = Math.min(floor2((this.rightScroll.value() + this.itemMargin) / this.itemMargin) * this.itemMargin + this.itemMargin,
                            maxVal);

                        this.animator.moveToY(scrollToBottom);
                        this.painter.clearZones(true);
                    }
                }
            }
        },

        // user events handlers

        onmousemove: function (e, forse) {
            if (!this.userEvents) return;

            var lockProject = false;

            var mouse = this.windowToCanvas(e.clientX, e.clientY);
            mouse.altKey = e.altKey;

            if (this.leftPanelController().getPanel().width >= mouse.x) {
                this.offMenus();
                this.mouse = mouse;
                this.mouse.baseEvent = e;
                return;
            }

            if (this.leftPanelController().getPanel().editBox.isEnable()) { return; }

            if (!this.mouse) { this.mouse = mouse; }
            if (this.mouse.x == mouse.x && this.mouse.y == mouse.y && !forse) return;
            this.mouse = mouse;
            this.mouse.baseEvent = e;

            if (this.mouse.y <= this.visibleUp) {

                this.linkWidget.onmousemove(mouse.x, mouse.y);
                this.menuTask.onmousemove(mouse.x, mouse.y);
                this.menuMileStone.onmousemove(mouse.x, mouse.y);

                // находимся на шкале дат, сбрасываем фокус с активного элемента

                if (!(this.menuTask.checkZone(mouse.x, mouse.y) || this.linkWidget.checkZone(mouse.x, mouse.y))) {
                    this.overlay.style.cursor = '';

                    if (-1 !== this.hitTask) {
                        this.hitTask = -1;
                        this.hitMilestone = -1;
                        this.needUpdate = true;
                    }
                }

                return;
            }

            if (this.isLBMDown) {
                this.pressMouse.change = !(mouse.x == this.pressMouse.x && mouse.y == this.pressMouse.y);
            }

            if (!this.isLBMDown) {
                if (this.bottomScroll.onmousemove(mouse)) {
                    return;
                }

                if (this.rightScroll.onmousemove(mouse)) {
                    return;
                }

                if (this.rightScroll.isHit(mouse)) {
                    this.hitMilestone = -1;
                    this.hitTask = -1;
                    this.offMenus();
                    return;
                }
            }

            if (kEditModeAddLink === this.editMode) {
                if (!this.readMode && !this.editBox.enable) {
                    if (this.linkLineEdit) {
                        this.overlay.style.cursor = 'pointer';
                        this.linkLineEdit.update = true;
                        this.moveMouse = this.mouse;

                        if (this.pushLink) {
                            this.pushLink.task.links.pop();
                            this.pushLink = undefined;
                        }

                        var pp = this.hitProject;
                        var pm = this.hitMilestone;
                        var pt = this.hitTask;

                        var taskTo = null, typeLink, linkAdd;

                        this.calculateHit(e, true, e.ctrlKey || e.metaKey);

                        if ('w-resize' === this.overlay.style.cursor)
                            this.overlay.style.cursor = 'pointer';

                        this.linkLineEdit.parent = null;

                        if (-1 != this.hitProject && -1 != this.hitTask && -1 === this.hitMilestone) {

                            taskTo = this.storage.getTask(this.hitProject, undefined, this.hitTask);
                            if (taskTo._status !== kElementCompleted && this.hitMilestone === this.linkLineEdit.m) {
                                this.linkLineEdit.parent = taskTo;

                                if (this.timeToSceneX(this.linkLineEdit.parent.beginTime) + Math.abs(this.timeToSceneX(this.linkLineEdit.parent.endTime) - this.timeToSceneX(this.linkLineEdit.parent.beginTime)) * 0.5 > this.mouse.x) {
                                    this.linkLineEdit.parentSide = kTaskSideLeft;
                                } else {
                                    this.linkLineEdit.parentSide = kTaskSideRight;
                                }

                                if (taskTo.endFail) { this.linkLineEdit.parentSide = kTaskSideLeft; }

                                if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideRight)
                                    typeLink = kLinkEndEnd;
                                else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideLeft)
                                    typeLink = kLinkBeginBegin;
                                else
                                    typeLink = kLinkBeginEnd;

                                this.pushLink = { task: taskTo };
                                this.pushLink.linkObj = {};
                                this.pushLink.linkObj['dependenceTaskId'] = taskTo.id();
                                this.pushLink.linkObj['linkType'] = typeLink;
                                this.pushLink.linkObj['parentTaskId'] = this.linkLineEdit.task.id();
                                this.pushLink.linkObj['type'] = 'link';

                                if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideLeft) {

                                    this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                    this.pushLink.task = this.linkLineEdit.task;
                                    this.linkLineEdit.task.links.push(this.pushLink.linkObj);

                                } else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideRight) {
                                    taskTo.links.push(this.pushLink.linkObj);
                                } else {
                                    this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                    this.pushLink.task = this.linkLineEdit.task;
                                    this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                }

                                if (this._mouseInLinkZone) {
                                    this._mouseInLinkZone.task = taskTo;
                                    this._mouseInLinkZone.linkToTaskId = taskTo.id();
                                    this._mouseInLinkZone.linkToTaskSide = this.linkLineEdit.parentSide;
                                }

                                this.linkLineEdit.pp = this.hitProject;
                                this.linkLineEdit.pm = this.hitMilestone;
                                this.linkLineEdit.pt = this.hitTask;

                                if (!this.isValidEditLink()) {
                                    this.linkLineEdit.pp = -1;
                                    this.linkLineEdit.pm = -1;
                                    this.linkLineEdit.pt = -1;

                                    if (this._mouseInLinkZone) {
                                        this._mouseInLinkZone.task = undefined;
                                        this._mouseInLinkZone.linkToTaskId = undefined;
                                        this._mouseInLinkZone.linkToTaskSide = undefined;
                                    }

                                    this.overlay.style.cursor = '';
                                } else {

                                    // связь имеет право на добавление

                                    if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideRight)
                                        typeLink = kLinkEndEnd;
                                    else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideLeft)
                                        typeLink = kLinkBeginBegin;
                                    else
                                        typeLink = kLinkBeginEnd;

                                    this.pushLink = { task: taskTo };
                                    this.pushLink.linkObj = {};
                                    this.pushLink.linkObj['dependenceTaskId'] = taskTo.id();
                                    this.pushLink.linkObj['linkType'] = typeLink;
                                    this.pushLink.linkObj['parentTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['type'] = 'link';

                                    if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideLeft) {

                                        this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                        this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                        this.pushLink.task = this.linkLineEdit.task;
                                        this.linkLineEdit.task.links.push(this.pushLink.linkObj);

                                    } else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideRight) {
                                        taskTo.links.push(this.pushLink.linkObj);
                                    } else {
                                        this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                        this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                        this.pushLink.task = this.linkLineEdit.task;
                                        this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                    }
                                }
                            } else { this.overlay.style.cursor = ''; }
                        } else if (-1 != this.hitProject && -1 != this.hitTask && -1 != this.hitMilestone) {

                            taskTo = this.storage.getTask(this.hitProject, this.hitMilestone, this.hitTask);

                            if (taskTo._status !== kElementCompleted && this.hitMilestone === this.linkLineEdit.m) {
                                this.linkLineEdit.parent = taskTo;

                                if (this.timeToSceneX(this.linkLineEdit.parent.beginTime) + Math.abs(this.timeToSceneX(this.linkLineEdit.parent.endTime) - this.timeToSceneX(this.linkLineEdit.parent.beginTime)) * 0.5 > this.mouse.x) {
                                    this.linkLineEdit.parentSide = kTaskSideLeft;
                                } else {
                                    this.linkLineEdit.parentSide = kTaskSideRight;
                                }

                                this.linkLineEdit.pp = this.hitProject;
                                this.linkLineEdit.pm = this.hitMilestone;
                                this.linkLineEdit.pt = this.hitTask;

                                if (taskTo.endFail) { this.linkLineEdit.parentSide = kTaskSideLeft; }

                                if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideRight)
                                    typeLink = kLinkEndEnd;
                                else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideLeft)
                                    typeLink = kLinkBeginBegin;
                                else
                                    typeLink = kLinkBeginEnd;

                                this.pushLink = { task: taskTo };
                                this.pushLink.linkObj = {};
                                this.pushLink.linkObj['dependenceTaskId'] = taskTo.id();
                                this.pushLink.linkObj['linkType'] = typeLink;
                                this.pushLink.linkObj['parentTaskId'] = this.linkLineEdit.task.id();
                                this.pushLink.linkObj['type'] = 'link';

                                if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideLeft) {
                                    this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                    this.pushLink.task = this.linkLineEdit.task;
                                    this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                } else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideRight) {
                                    taskTo.links.push(this.pushLink.linkObj);
                                } else {
                                    this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                    this.pushLink.task = this.linkLineEdit.task;
                                    this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                }

                                if (this._mouseInLinkZone) {
                                    this._mouseInLinkZone.task = taskTo;
                                    this._mouseInLinkZone.linkToTaskId = taskTo.id();
                                    this._mouseInLinkZone.linkToTaskSide = this.linkLineEdit.parentSide;
                                }

                                if (!this.isValidEditLink()) {
                                    this.linkLineEdit.pp = -1;
                                    this.linkLineEdit.pm = -1;
                                    this.linkLineEdit.pt = -1;

                                    if (this._mouseInLinkZone) {
                                        this._mouseInLinkZone.task = undefined;
                                        this._mouseInLinkZone.linkToTaskId = undefined;
                                        this._mouseInLinkZone.linkToTaskSide = undefined;
                                    }
                                    this.overlay.style.cursor = '';
                                } else {

                                    // связь имеет право на добавление

                                    this.pushLink = { task: taskTo };
                                    this.pushLink.linkObj = {};

                                    this.pushLink.linkObj['dependenceTaskId'] = taskTo.id();
                                    this.pushLink.linkObj['linkType'] = typeLink;
                                    this.pushLink.linkObj['parentTaskId'] = this.linkLineEdit.task.id();
                                    this.pushLink.linkObj['type'] = 'link';

                                    if (this.linkLineEdit.parentSide === kTaskSideRight && this.linkLineEdit.side === kTaskSideLeft) {
                                        this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                        this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                        this.pushLink.task = this.linkLineEdit.task;
                                        this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                    } else if (this.linkLineEdit.parentSide === kTaskSideLeft && this.linkLineEdit.side === kTaskSideRight) {
                                        taskTo.links.push(this.pushLink.linkObj);
                                    } else {
                                        this.pushLink.linkObj['dependenceTaskId'] = this.linkLineEdit.task.id();
                                        this.pushLink.linkObj['parentTaskId'] = taskTo.id();
                                        this.pushLink.task = this.linkLineEdit.task;
                                        this.linkLineEdit.task.links.push(this.pushLink.linkObj);
                                    }
                                }
                            } else { this.overlay.style.cursor = ''; }
                        } else {
                            this.linkLineEdit.pp = -1;
                            this.linkLineEdit.pm = -1;
                            this.linkLineEdit.pt = -1;

                            if (this._mouseInLinkZone) {
                                this._mouseInLinkZone.task = undefined;
                                this._mouseInLinkZone.linkToTaskId = undefined;
                                this._mouseInLinkZone.linkToTaskSide = undefined;
                            }
                            this.overlay.style.cursor = '';
                        }

                        this.hitProject = pp;
                        this.hitMilestone = pm;
                        this.hitTask = pt;

                        this.needUpdate = true;

                        return;
                    }
                }
            }
            else if (kEditModeElementDrag === this.editMode) {
                if (!this.readMode) {
                    this.moveMouse = mouse;

                    //this.dragBound.x = mouse.x;
                    //this.dragBound.y = mouse.y;

                    this.dragBound.y = floor2(mouse.y / this.itemMargin) * this.itemMargin + (this.itemMargin - this.itemHeight) * 0.5;

                    this.getTheMilestoneTaskToDrag(e);
                    this.updateDragDrop = true;

                    this.leftPanelController().clearFocus();

                    return;
                }
            }

            if (this.taskDescWidget.ref) {
                if (this.taskDescWidget.checkInBoundDetailsLink(mouse.x, mouse.y)) {
                    this.overlay.style.cursor = 'pointer'; return;
                } else {
                    this.overlay.style.cursor = '';
                }

                if (!this.readMode && !this.taskDescWidget.checkBound(mouse.x, mouse.y)) {
                    this.offMenus();
                    this.overlay.style.cursor = '';
                    return;
                }
            }

            if (this.milestoneDescWidget.ref && !this.readMode) {
                if (!this.milestoneDescWidget.checkBound(mouse.x, mouse.y)) {
                    this.offMenus();
                    this.overlay.style.cursor = '';
                    return;
                }
            }

            // находимся в всплывающем меню задачи

            if (this.menuTask.checkZone(mouse.x, mouse.y)) {

                this.moveMouse = this.mouse;
                this.needUpdate = true;

                this.menuTask.disableSetter = true;

                //this.calculateHit(e, false, e.ctrlKey || e.metaKey);

                this.hitProject = this.menuTask.p;
                this.hitMilestone = this.menuTask.m;
                this.hitTask = this.menuTask.t;

                this.menuTask.disableSetter = false;

                this.menuMileStone.reset();
                this.linkWidget.reset();
                this.menuTask.onmousemove(mouse.x, mouse.y);

                return;
            }

            // находимся в всплывающем меню задачи

            if (this.menuMileStone.checkZone(mouse.x, mouse.y)) {

                this.moveMouse = this.mouse;
                this.needUpdate = true;

                //this.menuTask.disableSetter = true;

                this.calculateHit(e, true, e.ctrlKey || e.metaKey);

                this.hitProject = this.menuTask.p;
                this.hitMilestone = this.menuTask.m;
                this.hitTask = this.menuTask.t;

                //this.menuTask.disableSetter = false;

                this.menuTask.reset();
                this.linkWidget.reset();

                this.menuMileStone.onmousemove(mouse.x, mouse.y);

                //                this.hitProject     =   this.menuMileStone.p;
                //                this.hitMilestone   =   this.menuMileStone.m;
                //
                //                this._leftPanelController.updateFocus(this.menuMileStone.p,
                //                    this.menuMileStone.m,
                //                    undefined);

                return;
            }

            if (this.linkWidget.checkZone(mouse.x, mouse.y)) {

                this.moveMouse = this.mouse;
                this.needUpdate = true;

                // this.calculateHit(e, true, e.ctrlKey || e.metaKey);

                this.menuMileStone.reset();
                this.menuTask.reset();
                this.linkWidget.onmousemove(mouse.x, mouse.y);

                return;
            }

            if (this.editBox.enable) {
                if (!this.pressedButtonRight) {

                    this.editBox.onmousemove(mouse);

                    if (this.editBox.milestoneRef && this.editBox.createMode) {
                        if (Math.abs(mouse.x - (this.editBox.bound.x + this.editBox.bound.w)) < kHitSidePixels &&
                            mouse.y > (this.editBox.bound.y + this.editBox.bound.h) &&
                            mouse.y < (this.editBox.bound.y + this.editBox.bound.h + this.itemMargin)) {
                            this.overlay.style.cursor = 'w-resize';

                            return;
                        }
                    }

                    if ((Math.abs(this.editBox.bound.x - mouse.x) < kHitSidePixels ||
                        Math.abs(this.editBox.bound.x + this.editBox.bound.w - mouse.x) < kHitSidePixels) &&
                        this.editBox.bound.y <= mouse.y && mouse.y <= this.editBox.bound.y + this.editBox.bound.h) {

                        this.overlay.style.cursor = 'w-resize';

                        return;
                    }

                    if (this.editBox.inBound(mouse.x, mouse.y) && mouse.x > this.visibleLeft || this.editBox.checkUp)
                        this.overlay.style.cursor = 'text';
                    else
                        this.overlay.style.cursor = '';

                    return;
                }
            }

            var divisionInPx = this.timeScale.hourInPixels / this.timeScale.scaleX;
            var scaleUnitStep = this.timeScale.scaleUnitStep();
            var task, milestone, time;

            if (this.isLBMDown) {
                // if (this.pressedButtonRight) return;
                scaleUnitStep = 24; //   DAYS ONLY

                var timeX, timeNX = (mouse.x - this.timeScale.hourInPixels * this.offsetX) /
                    (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX + this.anchorMouse.x;

                if (this.sticking) {
                    timeX = floor2(timeNX / scaleUnitStep) * scaleUnitStep;
                } else {
                    timeX = timeNX;
                }

                if (this.createMode) {
                    this.capProject = this.createMode.p;
                    this.capMilestone = this.createMode.m;
                    this.capTask = this.createMode.t;
                    this.capSide = kTaskSideRight;
                } else {
                    if (e.which === 1 && !this.editBox.enable && this.editBox.checkUp) {
                        return;
                    }
                }

                var timeTaskScreen;
                var endFailX = this.ctxWidth * kTaskEndFailSetup.bigClampPct;
                var worldToScreen, boundWidth, anchorX, dur, mouseX, endTime;
                lockProject = false;

                if (-1 !== this.capProject) {
                    lockProject = (kOpenProject !== this.storage.getProject(this.capProject).status());
                }

                if (-1 !== this.capProject && -1 !== this.capTask && -1 === this.capMilestone && !this.readMode && !lockProject) {

                    // двигаем задачу у проекта (непривязана к вехе)

                    task = this.storage.p[this.capProject].t[this.capTask];

                    if (undefined !== task && kElementCompleted != task._status && (!this.editBox.enable || this.editBox.createMode)) {

                        // вертикальный перенос задачи

                        if (this.moveMouse.y - mouse.y !== 0 && !this._modeStrafe && !this.editBox.enable && 0 === this.capSide) {

                            worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
                            boundWidth = task.duration * worldToScreen;
                            anchorX = this.anchorMouse.x * worldToScreen;
                            dur = task.duration;
                            mouseX = this.moveMouse.x;

                            if (task.endFail) {
                                boundWidth = -anchorX;
                                dur = boundWidth / worldToScreen;

                                timeTaskScreen = this.timeToSceneX(task.beginTime);
                                boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx;

                                if (timeTaskScreen > this.visibleLeft - 1) {
                                    boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx - timeTaskScreen;
                                } else {
                                    boundWidth = this.ctxWidth * kTaskEndFailSetup.bigClampPct - this.visibleLeft;
                                    anchorX = 0;
                                    mouseX = this.visibleLeft;
                                }
                            }

                            this.dragBound = {
                                x: mouseX, y: this.moveMouse.y,
                                width: boundWidth, height: this.itemHeight,
                                dy: 0, //this.anchorMouse.y,
                                dx: anchorX, time: dur, failEnd: task.endFail
                            };

                            this.itemToDrop = { t: this.capTask, m: this.capMilestone, p: this.capProject };

                            this.capTask = -1;
                            this.capMilestone = -1;
                            this.capProject = -1;
                            this.capSide = -1;

                            this.hitTask = -1;
                            this.hitMilestone = -1;
                            this.hitProject = -1;
                            this.hitSide = -1;

                            this.isLBMDown = false;
                            this.needUpdate = true;

                            this.editMode = kEditModeElementDrag;

                            return;
                        }

                        this._modeStrafe = true;

                        if (kTaskSideLeft === this.capSide) {

                            // при ведении влево больше чем сегодняшний день делаем принудительно увеличение на один день

                            if (timeNX <= 0) timeX -= 24;

                            task.updateBegin(timeX);
                        } else if (kTaskSideRight === this.capSide) {

                            // при ведении вправо больше чем сегодняшний день делаем принудительно увеличение на один день

                            if (timeNX >= 0) timeX += 24;

                            task.updateEnd(timeX);
                            task.endFail = false;
                        } else {
                            if (task.endFail) {
                                timeTaskScreen = this.timeToSceneX(task.beginTime);
                                endFailX = this.ctxWidth - kTaskEndFailSetup.smallClampPx;

                                if (timeTaskScreen <= this.visibleLeft - 1) {
                                    endFailX = this.ctxWidth * kTaskEndFailSetup.bigClampPct;
                                }

                                endTime = (Math.floor(this.worldToSceneX(endFailX) / 24) * 24);

                                task.updateEnd(endTime);
                            }

                            task.strafe(timeX);
                            task.endFail = false;
                        }

                        if (kEnableQueryTaskWithLinksMove) {
                            if (this.queryMoveLinks) {
                                if (this.queryMoveLinks) {

                                    if (this.queryMoveLinks.right && 1) {
                                        this.queryMoveLinks.right.walkElements(task, function (task, node) {
                                            if (task && node) {
                                                if (task._id !== node._id) {
                                                    if (task.endTime > node.beginTime) {
                                                        node.strafe(task.endTime);
                                                        node['change'] = true;
                                                    }
                                                }
                                            }
                                        });
                                    }

                                    if (this.queryMoveLinks.left && 1) {
                                        this.queryMoveLinks.left.walkElements(task, function (task, node) {
                                            if (task && node) {
                                                if (task._id !== node._id) {
                                                    if (task.beginTime < node.endTime) {
                                                        node.strafe(task.beginTime - (node.endTime - node.beginTime));
                                                        node['change'] = true;
                                                    }
                                                }
                                            }
                                        });
                                    }
                                }
                            }

                        }

                        this.storage.p[this.capProject]._calcTimes();

                        if (this.pressMouse.change)
                            this.taskDescWidget.checkAndOff(task);
                    }

                } else if (-1 != this.capProject && -1 != this.capTask && -1 !== this.capMilestone && !this.readMode && !lockProject) {

                    //  двигаем задачу у вехи

                    task = this.storage.getTask(this.capProject, this.capMilestone, this.capTask);

                    if (undefined !== task && kElementCompleted != task._status && (!this.editBox.enable || this.editBox.createMode)) {

                        // вертикальный перенос задачи

                        if (this.moveMouse.y - mouse.y !== 0 && !this._modeStrafe && !this.editBox.enable && 0 === this.capSide) {

                            worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
                            boundWidth = task.duration * worldToScreen;
                            anchorX = this.anchorMouse.x * worldToScreen;
                            dur = task.duration;
                            mouseX = this.moveMouse.x;

                            if (task.endFail) {
                                boundWidth = -anchorX;
                                dur = boundWidth / worldToScreen;

                                timeTaskScreen = this.timeToSceneX(task.beginTime);
                                boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx;

                                if (timeTaskScreen > this.visibleLeft - 1) {
                                    boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx - timeTaskScreen;
                                } else {
                                    boundWidth = this.ctxWidth * kTaskEndFailSetup.bigClampPct - this.visibleLeft;
                                    anchorX = 0;
                                    mouseX = this.visibleLeft;
                                }
                            }

                            this.dragBound = {
                                x: mouseX, y: this.moveMouse.y,
                                width: boundWidth, height: this.itemHeight,
                                dy: 0, //this.anchorMouse.y,
                                dx: anchorX, time: dur, failEnd: task.endFail
                            };

                            this.itemToDrop = { t: this.capTask, m: this.capMilestone, p: this.capProject };

                            this.capTask = -1;
                            this.capMilestone = -1;
                            this.capProject = -1;
                            this.capSide = -1;

                            this.hitTask = -1;
                            this.hitMilestone = -1;
                            this.hitProject = -1;
                            this.hitSide = -1;

                            this.isLBMDown = false;
                            this.needUpdate = true;

                            this.editMode = kEditModeElementDrag;

                            return;
                        }

                        this._modeStrafe = true;

                        milestone = this.storage.getMilestone(this.capProject, this.capMilestone);

                        if (kTaskSideLeft === this.capSide) {

                            // при ведении влево больше чем сегодняшний день делаем принудительно увеличение на один день

                            if (timeNX <= 0) timeX -= 24;

                            task.updateBegin(timeX, milestone.endTime);
                        } else if (kTaskSideRight === this.capSide) {

                            // при ведении вправо больше чем сегодняшний день делаем принудительно увеличение на один день

                            if (timeNX >= 0) timeX += 24;

                            task.updateEnd(timeX);

                            task.endFail = false;
                        } else {
                            if (task.endFail) {
                                task.updateEnd(milestone.endTime);
                            }

                            task.strafe(timeX);
                            task.endFail = false;

                            if (kEnableQueryTaskWithLinksMove) {
                                if (this.queryMoveLinks) {
                                    if (this.queryMoveLinks) {

                                        if (this.queryMoveLinks.right) {
                                            this.queryMoveLinks.right.walkElements(task, function (task, node) {
                                                if (task && node) {
                                                    if (task._id !== node._id) {
                                                        if (task.endTime > node.beginTime) {
                                                            node.strafe(task.endTime);
                                                            node['change'] = true;
                                                        }
                                                    }
                                                }
                                            });
                                        }

                                        if (this.queryMoveLinks.left) {
                                            this.queryMoveLinks.left.walkElements(task, function (task, node) {
                                                if (task && node) {
                                                    if (task._id !== node._id) {
                                                        if (task.beginTime < node.endTime) {
                                                            node.strafe(task.beginTime - (node.endTime - node.beginTime));
                                                            node['change'] = true;
                                                        }
                                                    }
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        this.storage.getMilestone(this.capProject, this.capMilestone).updateTimes();

                        if (this.pressMouse.change)
                            this.taskDescWidget.checkAndOff(task);
                    }

                } else if (-1 != this.capProject && -1 === this.capTask && -1 !== this.capMilestone && !this.readMode && !lockProject) {

                    //  двигаем веху

                    milestone = this.storage.getMilestone(this.capProject, this.capMilestone);

                    if (milestone && !milestone.isInEditMode) {
                        if (timeNX >= 0) timeX += 24;

                        milestone.updateEnd(timeX);
                    }

                } else {

                    //  двигаем весь TimeLine

                    this.pressedButtonRight = false;
                    if (e.buttons) {
                        this.pressedButtonRight = (e.buttons == 2); // The right mouse button is pressed. (IE9+)
                    } else {
                        this.pressedButtonRight = (e.button === 2);
                    }

                    this.offsetX += (mouse.x - this.moveMouse.x) / this.timeScale.hourInPixels;

                    this.offsetX = Math.max(this.offsetX, this.zoomBar.maxPos());
                    this.offsetX = Math.min(this.offsetX, this.zoomBar.minPos());

                    if (this.timeScale)
                        this.timeScale.strafe(this.offsetX);

                    if (this.enableTouch) {
                        var val = floor2((mouse.y - this.moveMouse.y) / this.itemMargin) * this.itemMargin;

                        this.rightScroll.forwardMoveY(val - kEps);
                    }

                    if (this.zoomBar) { this.bottomScroll.updateThumb(this.zoomBar.thumb); }
                }

                this.moveMouse = mouse;
                this.needUpdate = true;

                this.hitTask = -1;
                this.hitMilestone = -1;

                this.milestoneDescWidget.check(null);
                this.taskDescWidget.check(null);

                this.offMenus();


                if (this.zoomBar) { this.zoomBar.repaint = true; }
                return;
            }

            var hitItem = { t: this.hitTask, p: this.hitProject };

            this.calculateHit(e, true, e.ctrlKey || e.metaKey);
            this.moveMouse = this.mouse;
            lockProject = false;

            if (-1 !== this.hitProject) {
                lockProject = (kOpenProject !== this.storage.getProject(this.hitProject).status());
            }

            if (this.readMode) {
                if (hitItem.t != this.hitTask || hitItem.p != this.hitProject || this.backLightMilestone.update) {
                    this.needUpdate = true;
                    this.backLightMilestone.update = false;
                }

                return;
            }

            if (this.hitSide === kTaskSideNone) {

                this.menuTask.set(this.hitProject, this.hitMilestone, this.hitTask, this.storage.getTasWithInd(this.hitProject, this.hitMilestone, this.hitTask), mouse.x, mouse.y);

                if (-1 == this.hitProject) {
                    this.menuTask.reset();
                }

                if (-1 !== this.backLightMilestone.m)
                    this.menuMileStone.set(this.backLightMilestone.p, this.backLightMilestone.m, this.storage.getMilestone(this.backLightMilestone.p, this.backLightMilestone.m));
                if (-1 === this.backLightMilestone.p) {
                    this.menuMileStone.reset();
                }


            } else {
                if (this.hitSide === kTaskSideLeft || this.hitSide === kTaskSideRight) {
                    this.menuTask.reset();
                    this.menuMileStone.reset();

                    if (kElementCompleted !== this.menuTask._status && !lockProject)
                        this.overlay.style.cursor = 'w-resize';
                }
            }

            if (-1 === hitItem.t && -1 === this.hitTask)
                this.linkWidget.onmousemove(mouse.x, mouse.y);

            if (this.menuTask.onmousemove(mouse.x, mouse.y)) {
                this.linkWidget.reset();
            }

            if (this.menuMileStone.onmousemove(mouse.x, mouse.y)) {
                this.linkWidget.reset();
            }

            if (hitItem.t != this.hitTask || hitItem.p != this.hitProject || this.backLightMilestone.update) {
                if (this.backLightMilestone.update) {
                    if (!this.milestoneChange) {
                        this._leftPanelController.updateFocus(this.backLightMilestone.p,
                            this.backLightMilestone.m,
                            undefined);

                        // this.hitProject     =   this.backLightMilestone.p;
                        // this.hitMilestone   =   this.backLightMilestone.m;

                        if (-1 !== this.backLightMilestone.m)
                            this.menuMileStone.set(this.backLightMilestone.p, this.backLightMilestone.m, this.storage.getMilestone(this.backLightMilestone.p, this.backLightMilestone.m));
                    }

                } else {

                    if (this.editBox.isEnable()) {
                        this._leftPanelController.updateFocus(this.editBox.p,
                                this.editBox.m == -1 ? undefined : this.editBox.m,
                                this.editBox.t == -1 ? undefined : this.editBox.t);
                    } else {
                        this._leftPanelController.updateFocus(this.hitProject,
                                this.hitMilestone == -1 ? undefined : this.hitMilestone,
                                this.hitTask == -1 ? undefined : this.hitTask);
                    }
                }

                this.needUpdate = true;
                this.backLightMilestone.update = false;
                // this.linkWidget.reset();
            } else {
                this.leftPanelController().clearHighlight();
            }
        },
        onmousedown: function (e) {
            if (!this.userEvents) return;

            this._modeStrafe = false;

            this.animator.stop();
            if (this.zoomBar) this.zoomBar.releaseCapture();

            var tref = null;

            this.capTask = -1;
            this.capSide = 0;

            this.downMouse = this.windowToCanvas(e.clientX, e.clientY);
            this.pressMouse = { x: this.downMouse.x, y: this.downMouse.y, change: false };

            if (this._leftPanelController.getPanel().editBox.isEnable()) {
                if (!this._leftPanelController.getPanel().editBox.inBound(e)) { this._leftPanelController.clearFocus(); }
                return;
            }

            if (this.bottomScroll.onmousedown(this.downMouse)) return;
            if (this.rightScroll.onmousedown(this.downMouse)) return;

            this.leftBoxMousePressed = 0;
            this.moveMouse = { x: this.downMouse.x, y: this.downMouse.y };

            if (this.taskDescWidget.ref) {
                if (!this.taskDescWidget.checkBound(this.pressMouse.x, this.pressMouse.y)) {
                    if (this.taskDescWidget.checkInBoundDetailsLink(this.pressMouse.x, this.pressMouse.y)) {
                        this.taskDescWidget.setLinkRef('Tasks.aspx?prjID=' + this.taskDescWidget.ref.t._owner + '&id=' + this.taskDescWidget.ref.t._id);
                    }

                    this.offMenus();
                    this.overlay.style.cursor = '';
                    return;
                }

                this.needUpdate = true;
            }

            if (this.milestoneDescWidget.ref) {
                if (!this.milestoneDescWidget.checkBound(this.pressMouse.x, this.pressMouse.y)) {
                    this.offMenus();
                    this.overlay.style.cursor = '';
                    return;
                }
            }

            this.isLBMDown = true;

            if (e.buttons) {
                this.pressedButtonRight = (e.buttons == 2); // The right mouse button is pressed. (IE9+)
                //if (!this.pressedButtonRight)
                //    this.pressedButtonLeft = (e.buttons === 1);
            } else {
                this.pressedButtonRight = (e.button === 2);
                if (!this.pressedButtonRight)
                    this.pressedButtonLeft = (e.button === 1);
            }

            if (this.linkWidget.onmousedown(this.moveMouse.x, this.moveMouse.y)) return;
            if (this.menuTask.onmousedown(this.moveMouse.x, this.moveMouse.y)) return;
            if (this.menuMileStone.onmousedown(this.moveMouse.x, this.moveMouse.y)) return;

            if (this.mouse.y <= this.visibleUp) { return; }

            this.calculateHit(e, !this.editBox.enable, e.ctrlKey || e.metaKey);

            if (this.editBox.enable) {
                if (this.editBox.p === this.hitProject && this.editBox.m === this.hitMilestone && this.editBox.t === this.hitTask) {
                    this.capTask = this.hitTask;
                    this.capMilestone = this.hitMilestone;
                    this.capProject = this.hitProject;
                    this.capSide = this.hitSide;

                    this.editBox.onmousedown(this.moveMouse);
                    return;
                }

                if (-1 !== this.editBox.p && -1 !== this.editBox.m && this.editBox.t === -1) {
                    this.editBox.onmousedown(this.moveMouse);
                    if (this.editBox.inBound(this.moveMouse.x, this.moveMouse.y))
                        return;
                }

                this.editBox.onmousedown(this.moveMouse);

                if (this.editBox.checkUp) {
                    this.hitProject = -1;
                    this.hitMilestone = -1;
                    this.hitTask = -1;
                    this.capTask = -1;
                    this.capMilestone = -1;
                    this.capProject = -1;

                    return;
                }
            }

            this.capTask = this.hitTask;
            this.capMilestone = this.hitMilestone;
            this.capProject = this.hitProject;
            this.capSide = this.hitSide;

            if (this.hitLink) {
                this.capLink = this.hitLink;
            }

            // создаем начальную точку-связи

            if (this._mouseInLinkZone) {
                if (this.addEditLink()) {
                    this.editMode = kEditModeAddLink;
                }

                this.needUpdate = true;
                return;
            }

            if (-1 === this.menuMileStone.p && -1 === this.menuMileStone.m) {
                if (this.pressedButtonRight && -1 === this.capProject && -1 === this.capTask && -1 === this.capMilestone) {

                    this.anchorMouse = { x: 0, y: 0 };

                    if (this.exist && this.capLink || this.hitLink) {

                        this._modelController.addTaskOperation(kHandlerBeforeDeleteTaskLink, this.hitLink ? this.hitLink : this.capLink);

                    } else {

                        // при клике правой кнопнкой мыши создаем задачу

                        if (!this.editBox.enable) {

                            if (this.calculateLineHit(e)) {
                                if (-1 !== this.hitLine.p && kOpenProject === this.storage.getProject(this.hitLine.p).status()) {
                                    var line = this.hitLine.t + 1;
                                    if (-1 !== this.hitLine.m) {
                                        this.storage.getMilestone(this.hitLine.p, this.hitLine.m).setCollapse(false);
                                    }

                                    if (this.storage.p[this.hitLine.p].fullCollapse) {
                                        this.storage.p[this.hitLine.p].setFullCollapse(false);
                                    }

                                    if (e.ctrlKey || e.metaKey) {
                                        this.updateData();
                                        this.needUpdateContent = true;
                                        this.drawScene();

                                        this.createMilestone(this.hitLine.p, this.hitLine.bottomMilestoneInd ? this.hitLine.bottomMilestoneInd : this.hitLine.m);
                                    } else {
                                        if (this.storage.p[this.hitLine.p].collapse && -1 === this.hitLine.m) {
                                            this.storage.p[this.hitLine.p].setCollapse(false);

                                            this.updateData();
                                            this.needUpdateContent = true;
                                            this.drawScene();

                                            //    line = 0;
                                        }
                                        this.createTask(this.hitLine.p, this.hitLine.m, line);
                                        this.createMode = { p: this.hitLine.p, m: this.hitLine.m, t: line };
                                    }
                                }
                            } else {
                                if (this.downMouse.y > this.visibleUp) {
                                    if (this.storage.p.length) {
                                        if (kOpenProject === this.storage.getProject(0).status()) {
                                            this.storage.p[0].setCollapse(false);

                                            var ind = this.storage.p[0].t.length;

                                            this.createTask(0, -1, ind);
                                            this.createMode = { p: 0, m: -1, t: ind };
                                        }
                                    }
                                }
                            }
                        }
                    }

                    this.offMenus();
                    this.offWidgets();
                    return;
                }
            }

            if (e.ctrlKey || e.metaKey) {
                if (-1 != this.capProject && -1 != this.capTask && -1 === this.capMilestone) {
                    tref = this.storage.getTask(this.capProject, undefined, this.capTask);
                }

                if (-1 != this.capProject && -1 != this.capTask && -1 != this.capMilestone) {
                    tref = this.storage.getTask(this.capProject, this.capMilestone, this.capTask);
                }

                if (tref) {
                    if (kElementCompleted == tref._status || !(e.ctrlKey || e.metaKey))
                        return;

                    var worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
                    var boundWidth = tref.duration * worldToScreen;
                    var anchorX = this.anchorMouse.x * worldToScreen;
                    var dur = tref.duration;
                    var mouseX = this.moveMouse.x;

                    if (tref.endFail) {
                        boundWidth = -anchorX;
                        dur = boundWidth / worldToScreen;

                        var timeTaskScreen = this.timeToSceneX(tref.beginTime);
                        boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx;

                        if (timeTaskScreen > this.visibleLeft - 1) {
                            boundWidth = this.ctxWidth - kTaskEndFailSetup.smallClampPx - timeTaskScreen;
                        } else {
                            boundWidth = this.ctxWidth * kTaskEndFailSetup.bigClampPct - this.visibleLeft;
                            anchorX = 0;
                            mouseX = this.visibleLeft;
                        }
                    }

                    this.dragBound = {
                        x: mouseX, y: this.moveMouse.y,
                        width: boundWidth, height: this.itemHeight,
                        dy: 0, //this.anchorMouse.y,
                        dx: anchorX, time: dur, failEnd: tref.endFail
                    };

                    this.itemToDrop = { t: this.capTask, m: this.capMilestone, p: this.capProject };

                    this.capTask = -1;
                    this.capMilestone = -1;
                    this.capProject = -1;
                    this.capSide = -1;

                    this.hitTask = -1;
                    this.hitMilestone = -1;
                    this.hitProject = -1;
                    this.hitSide = -1;

                    this.isLBMDown = false;
                    this.needUpdate = true;

                    this.editMode = kEditModeElementDrag;
                }

                return;
            }

            if (this.editBox.enable)
                return;

            this.needUpdate = true;

            // undo

            var taskRef = null;
            var projectId = -1;
            var milestoneId = -1;
            var taskId = -1;

            var formatBatchTasks = [], silentUpdateMode = true;

            if (-1 !== this.capProject && -1 !== this.capTask && -1 === this.capMilestone && !this.readMode) {

                // задача в свободной зоне

                taskRef = this.storage.p[this.capProject].t[this.capTask];
                if (kElementCompleted === taskRef._status)
                    return;

                projectId = this.storage.p[this.capProject]._id;
                taskId = this.storage.p[this.capProject].t[this.capTask]._id;

                if (kEnableQueryTaskWithLinksMove) {

                    // собираем связи для данной задачи

                    this.queryMoveLinks = this._modelController.collectBeginEndItems(this.capProject, undefined, taskId);

                    if (this.queryMoveLinks) {
                        if (this.queryMoveLinks.right) {
                            this.queryMoveLinks.right.walkElements(taskRef, function (task, node, data) {
                                if (data) {
                                    formatBatchTasks.push(data);
                                }
                            });
                        }

                        if (this.queryMoveLinks.left) {
                            this.queryMoveLinks.left.walkElements(taskRef, function (task, node, data) {
                                if (data) {
                                    formatBatchTasks.push(data);
                                }
                            });
                        }
                    }
                }

                this._undoManager.addTempOperation(kOperationChangeTimeTask,
                    {
                        p: this.capProject, m: undefined, t: taskRef, index: this.capTask,
                        taskId: taskId, milestoneId: undefined, projectId: projectId, silentUpdateMode: silentUpdateMode,
                        queryMoveLinks: formatBatchTasks
                    });

            } else if (-1 != this.capProject && -1 != this.capTask && -1 !== this.capMilestone && !this.readMode) {

                // задача принадлежит вехе

                taskRef = this.storage.getTask(this.capProject, this.capMilestone, this.capTask);
                if (kElementCompleted === taskRef._status)
                    return;

                projectId = this.storage.p[this.capProject]._id;
                milestoneId = this.storage.p[this.capProject].m[this.capMilestone]._id;
                taskId = this.storage.p[this.capProject].m[this.capMilestone].t[this.capTask]._id;

                if (kEnableQueryTaskWithLinksMove) {

                    // собираем связи для данной задачи

                    this.queryMoveLinks = this._modelController.collectBeginEndItems(this.capProject, this.capMilestone, taskId);

                    if (this.queryMoveLinks) {
                        if (this.queryMoveLinks.right) {
                            this.queryMoveLinks.right.walkElements(taskRef, function (task, node, data) {
                                if (data) {
                                    formatBatchTasks.push(data);
                                }
                            });
                        }

                        if (this.queryMoveLinks.left) {
                            this.queryMoveLinks.left.walkElements(taskRef, function (task, node, data) {
                                if (data) {
                                    formatBatchTasks.push(data);
                                }
                            });
                        }
                    }
                }

                this._undoManager.addTempOperation(kOperationChangeTimeTask,
                    {
                        p: this.capProject, m: this.capMilestone, t: taskRef, index: this.capTask,
                        taskId: taskId, milestoneId: milestoneId, projectId: projectId, silentUpdateMode: silentUpdateMode,
                        queryMoveLinks: formatBatchTasks
                    });

            } else if (-1 != this.capProject && -1 === this.capTask && -1 !== this.capMilestone && !this.readMode) {

                var ids = this.storage.milestoneIds(this.capProject, this.capMilestone);
                var ref = this.storage.getMilestone(this.capProject, this.capMilestone);

                this._undoManager.add(kOperationChangeTimeMilestone,
                    {
                        p: this.capProject, m: this.capMilestone, t: ref, index: this.capMilestone,
                        taskId: undefined, milestoneId: ids.m, projectId: ids.p,
                        silentUpdateMode: silentUpdateMode
                    });

                this._undoManager.add(kOperationDummy, null);
            } else if (this.capLink) {
                this.needUpdate = true;
            }
        },
        onmouseup: function (e, focus) {
            if (!this.userEvents) return;

            this._modeStrafe = false;

            var coords;
            var lockproject = false;

            var taskId = -1;
            var projectId = -1;
            var milestoneId = -1;
            var p, t, m, clickOk = false;

            var t_ = this;
            var mouseChange = this.pressMouse.change;
            this.createMode = null;
            this.needUpdateScrollContent = true;

            this.isLBMDown = false;
            this.downMouse = this.windowToCanvas(e.clientX, e.clientY);

            if (this.leftBoxMousePressed) {
                this.leftBoxMousePressed--;
                this.offMenus();
                this.needUpdate = true;
                return;
            }

            if (focus) return;

            if (this.taskDescWidget.ref && this.taskDescWidget.href) {
                if (!this.taskDescWidget.checkBound(this.downMouse.x, this.downMouse.y)) {
                    if (this.taskDescWidget.checkInBoundDetailsLink(this.downMouse.x, this.downMouse.y)) {
                        window.open(this.taskDescWidget.href);
                        this.taskDescWidget.setLinkRef(null);
                    }

                    //this.offMenus();
                    // this.overlay.style.cursor = '';
                    // return;
                }
            }

            this.taskDescWidget.setLinkRef(null);

            if (this.pressMouse.y <= this.visibleUp) { return; }

            if (kEditModeAddLink === this.editMode) {
                this.applyEditLink();

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;
                this.capProject = -1;
                this.queryMoveLinks = null;


                this.capTask = -1;
                this.capMilestone = -1;
                this.capProject = -1;
                this.capSide = -1;

                return;
            } else if (kEditModeElementDrag === this.editMode) {
                this.painter.clearZones(true);
                this.moveElement(e);
                this.updateData();
                return;
            }

            if (this.editBox.enable) {
                this.editBox.onmouseup(this.windowToCanvas(e.clientX, e.clientY));
                return;
            }

            this.menuMileStone.reset();
            this.menuTask.reset();

            if (this.editBox.checkUp && e.which !== 3) {
                this.editBox.onmouseup(this.downMouse);
                return;
            }

            //this.downMouse = this.windowToCanvas(e.clientX, e.clientY);
            this.pressMouse = { x: this.downMouse.x, y: this.downMouse.y, change: this.pressMouse.change };

            var hitItem = { t: this.hitTask, m: this.hitMilestone, p: this.hitProject };
            this.calculateHit(e, true);

            if (-1 !== this.backLightMilestone.p && -1 !== this.backLightMilestone.m && -1 === this.hitTask) {
                this.hitProject = this.backLightMilestone.p;
                this.hitMilestone = this.backLightMilestone.m;
            }

            if (this.clickPosCentreing) {
                this.animator.moveCenterToX(this.storage.getTask(this.clickPosCentreing.p, this.clickPosCentreing.m, this.clickPosCentreing.t).beginTime);
                this.clickPosCentreing = null;

                this.offMenus();
                this.overlay.style.cursor = '';
            }

            function checkMouse(e) {
                var mouse = t_.windowToCanvas(e.clientX, e.clientY);
                return (mouse.x == t_.pressMouse.x && mouse.y == t_.pressMouse.y && !t_.pressMouse.change);
            }

            // task milestone - details widget show/hide or popup menus events

            if (-1 !== this.capProject && -1 !== this.capTask && -1 !== this.capMilestone) {

                lockproject = (kOpenProject !== this.storage.getProject(this.capProject).status());

                t = this.storage.getTask(this.capProject, this.capMilestone, this.capTask);

                if (this.pressedButtonRight && !focus && !lockproject) {
                    if (-1 !== this.hitTask) {
                        if (this.handlers[kHanderShowEditPopUpMenuWindow] && !this.readMode) {
                            this._modelController.statusElement = {
                                p: this.capProject, m: this.capMilestone, t: this.capTask,
                                ids: this.storage.taskIds(this.capProject, this.capMilestone, this.capTask),
                                isTask: true, task: t, ref: t
                            };

                            coords = { left: e.clientX - this.ctx.canvas.offsetLeft, top: this.currentElementY + this.ctx.canvas.offsetTop };

                            this.offWidgets();

                            this.handlers[kHanderShowEditPopUpMenuWindow](coords, t, true, this.storage.getProject(this.capProject).id());
                        }
                    }

                    this.menuMileStone.p = -1;
                    this.menuMileStone.m = -1;
                    this.queryMoveLinks = null;

                    this.capTask = -1;
                    this.capMilestone = -1;
                    this.capProject = -1;
                    this.capSide = -1;

                    this.offMenus();
                    this.leftPanelController().clearFocus();
                    return;
                }

                if (!mouseChange) {
                    if (this.taskDescWidget.check(t)) {
                        t.dropDownWidget = !t.dropDownWidget;
                        if (t.dropDownWidget) {
                            this.milestoneDescWidget.check(null);
                        }

                        clickOk = true;
                    }

                    // undo

                    this._undoManager.deleteTempOperation();

                } else {
                    projectId = this.storage.getProject(this.capProject)._id;
                    milestoneId = this.storage.getMilestone(this.capProject, this.capMilestone)._id;
                    taskId = t._id;

                    // undo

                    // проверяем все задачи на изменения времени

                    this._undoManager.applyTempOperation();
                    this._undoManager.updateOperation(true, t, this.queryMoveLinks);
                }

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;
                this.queryMoveLinks = null;

                //this.leftPanelController().clearFocus();
                this.leftPanelController().scrollContent(this.rightScroll.value());
            }

            // free task - details widget show/hide or popup menus events

            if (-1 !== this.capProject && -1 !== this.capTask && -1 === this.capMilestone) {

                lockproject = (kOpenProject !== this.storage.getProject(this.capProject).status());

                t = this.storage.getTask(this.capProject, undefined, this.capTask);

                if (this.pressedButtonRight && !focus && !lockproject) {
                    if (-1 !== this.hitTask) {
                        if (this.handlers[kHanderShowEditPopUpMenuWindow] && !this.readMode) {
                            this._modelController.statusElement = {
                                p: this.capProject, m: undefined, t: this.capTask,
                                ids: this.storage.taskIds(this.capProject, undefined, this.capTask),
                                isTask: true, task: t, ref: t
                            };

                            coords = { left: e.clientX - this.ctx.canvas.offsetLeft, top: this.currentElementY + this.ctx.canvas.offsetTop };

                            this.offWidgets();

                            this.handlers[kHanderShowEditPopUpMenuWindow](coords, t, true, this.storage.getProject(this.capProject).id());
                        }
                    }
                    this.menuMileStone.p = -1;
                    this.menuMileStone.m = -1;
                    this.queryMoveLinks = null;

                    this.capTask = -1;
                    this.capMilestone = -1;
                    this.capProject = -1;
                    this.capSide = -1;

                    this.offMenus();
                    this.leftPanelController().clearFocus();
                    return;
                }

                if (!mouseChange) {
                    if (this.taskDescWidget.check(t)) {
                        t.dropDownWidget = !t.dropDownWidget;
                        if (t.dropDownWidget) {
                            this.milestoneDescWidget.check(null);
                        }

                        clickOk = true;
                    }

                    // undo

                    this._undoManager.deleteTempOperation();

                } else {
                    projectId = this.storage.getProject(this.capProject)._id;
                    taskId = t._id;

                    // undo

                    // проверяем все задачи на изменения времени

                    this._undoManager.applyTempOperation();
                    this._undoManager.updateOperation(true, t, this.queryMoveLinks);
                }

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;
                this.capProject = -1;
                this.queryMoveLinks = null;

                // this.leftPanelController().clearFocus();
                this.leftPanelController().scrollContent(this.rightScroll.value());
            }

            // milestone - details widget show/hide or popup menus events

            if (-1 !== this.menuMileStone.p && -1 !== this.menuMileStone.m) {

                lockproject = (kOpenProject !== this.storage.getProject(this.menuMileStone.p).status());

                m = this.storage.getMilestone(this.menuMileStone.p, this.menuMileStone.m);

                if (this.pressedButtonRight && !focus && !lockproject) {
                    if (this.handlers[kHanderShowEditPopUpMenuWindow] && !this.readMode) {

                        this._modelController.statusElement = {
                            p: this.menuMileStone.p, m: this.menuMileStone.m, t: undefined,
                            ids: this.storage.milestoneIds(this.menuMileStone.p, this.menuMileStone.m),
                            isTask: false, milestone: m, ref: m
                        };

                        coords = {
                            left: e.clientX - this.ctx.canvas.offsetLeft,
                            top: this.getElementPosVertical(this.menuMileStone.p, this.menuMileStone.m, -1) + this.ctx.canvas.offsetTop - this.rightScroll.value()
                        };

                        this.offWidgets();

                        this.handlers[kHanderShowEditPopUpMenuWindow](coords, m, false, this.storage.getProject(this.menuMileStone.p).id());
                    }

                    this.menuMileStone.p = -1;
                    this.menuMileStone.m = -1;
                    this.queryMoveLinks = null;

                    this.capTask = -1;
                    this.capMilestone = -1;
                    this.capProject = -1;
                    this.capSide = -1;

                    this.offMenus();
                    this.leftPanelController().clearFocus();

                    return;
                }

                if (!mouseChange) {
                    m.dropDownWidget = !m.dropDownWidget;

                    if (m.dropDownWidget) {
                        this.taskDescWidget.check(null);
                    } else {
                        this.milestoneDescWidget.check(null);
                    }

                    clickOk = true;
                }

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;

                //this.leftPanelController().clearFocus(undefined, true);
                this.leftPanelController().scrollContent(this.rightScroll.value());
            }

            // milestone change time

            if (-1 !== this.capProject && -1 === this.capTask && -1 !== this.capMilestone) {

                if (mouseChange) {
                    if (this._undoManager.flushTopDummy()) {
                        var ref = this.storage.getMilestone(this.capProject, this.capMilestone);
                        this._undoManager.updateOperation(true, ref);
                    }
                } else {
                    if (this._undoManager.flushTopDummy()) {
                        this._undoManager.flushPop(0);
                    }
                }

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;

                this.leftPanelController().scrollContent(this.rightScroll.value());
            }

            if (this.pressMouse.change) clickOk = true;

            this.capTask = -1;
            this.capSide = 0;
            this.capMilestone = -1;
            this.pressMouse.change = true;

            this.queryMoveLinks = null;

            if (this.bottomScroll.onmouseup(e))
                return;

            if (this.rightScroll.onmouseup(e))
                return;

            if (!clickOk) {
                this.milestoneDescWidget.check(null);
                this.taskDescWidget.check(null);
            }

            this.needUpdate = true;
            this.needUpdateScrollContent = true;
        },
        ondblclick: function (e) {
            if (!this.userEvents) return;

            this.isLBMDown = false;

            if (e.ctrlKey || e.metaKey) return;

            if (this.editBox.enable) {
                this.editBox.ondblclick(this.windowToCanvas(e.clientX, e.clientY));
                return;
            }

            this.calculateHit(e);

            this.downMouse = this.windowToCanvas(e.clientX, e.clientY);

            if (this.bottomScroll.onmousedown(this.downMouse))
                return;

            if (this.rightScroll.onmousedown(this.downMouse))
                return;

            if (-1 !== this.hitMilestone && -1 !== this.hitTask) {
                //var t = this.calculateTaskHit();
                //if (t) {
                //    this.animatior.moveToX(t.beginTime);
                //}
                return;
            }

            if (this.menuTask.check(this.downMouse.x, this.downMouse.y) ||
                this.menuMileStone.check(this.downMouse.x, this.downMouse.y)
                || (-1 !== this.menuMileStone.p && -1 !== this.menuMileStone.m)) { }

            //this.animator.moveCenterToX(0);
        },
        onmousewheel: function (e) {
            if (!this.userEvents) return;

            var oldPosY = 0;
            if (this.editMode == kEditModeAddLink) {
                oldPosY = this.rightScroll.value();
            }

            this.offWidgets();

            if (e.altKey) {
                if (kEditModeElementDrag === this.editMode) {
                    return;
                }

                e = e || window['event'];
                var delta = e['deltaY'] || e['detail'] || e['wheelDelta'];
                if (e['deltaY']) delta *= -1;
                if (e['detail']) delta *= -1;

                this.setZoomUI((this.zoom - ((delta / Math.abs(delta)) >> 0)));
                this.calculateHit(e, true);

                if (this.zoomBar) { this.zoomBar.repaint = true; }

                return;
            }

            var scrollVertical = true;

            //

            if (undefined !== e['wheelDeltaX']) {
                var offAdd = 0, maxPos = this.zoomBar.maxPos(), minPos = this.zoomBar.minPos();

                if (e['wheelDeltaX'] > 0) {

                    offAdd = minPos - this.offsetX;

                    if (offAdd > 0) {
                        this.offWidgets();

                        this.offMenus();
                        this.painter.clearZones(true);
                        this.overlay.style.cursor = '';

                        offAdd = Math.min(Math.abs(offAdd), (this.ctxWidth / this.timeScale.hourInPixels * kUIScrollStepInWidthPercent));

                        this.animator.addMovementX(-offAdd);
                    }

                    scrollVertical = false;
                }

                if (e['wheelDeltaX'] < 0) {

                    offAdd = maxPos - this.offsetX;

                    if (offAdd < 0) {
                        this.offWidgets();
                        this.offMenus();
                        this.painter.clearZones(true);
                        this.overlay.style.cursor = '';

                        offAdd = Math.min(Math.abs(offAdd), (this.ctxWidth / this.timeScale.hourInPixels * kUIScrollStepInWidthPercent));

                        this.animator.addMovementX(offAdd);
                    }

                    scrollVertical = false;
                }
            }

            if (scrollVertical) {
                this.rightScroll.onmousewheel(e);
            }

            if (this.editMode == kEditModeAddLink) {
                this.linkLineEdit.posY += oldPosY - this.rightScroll.value();
            }

            this.calculateHit(e, true);
            this.linkWidget.setLink(this.hitLink);
            var mouse = this.windowToCanvas(e.clientX, e.clientY);
            this.menuTask.set(this.hitProject, this.hitMilestone, this.hitTask);

            if (0 < this.menuTask.bound.w && 0 < this.menuTask.bound.h) {
                this.menuTask.check(mouse.x, mouse.y);
            }

            if (0 < this.menuMileStone.bound.w && 0 < this.menuMileStone.bound.h) {
                this.menuMileStone.check(mouse.x, mouse.y);
            }
        },

        onkeypress: function (e) {
            if (!this.userEvents) return;

            if (this.editBox.enable) {
                this.editBox.onkeypress(e);
                return;
            }

            if (this._leftPanelController.getPanel().editBox.isEnable()) { return; }

            e = window.event || e;
            var keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;

            if ((e.ctrlKey || e.metaKey) && 26 == keyCode) {   // Ctr + Z
                this._undoManager.undo();
            }

            if ((e.ctrlKey || e.metaKey) && 25 == keyCode) {   // Ctr + Y
                this._undoManager.redo();
            }
        },
        onkeydown: function (e) {
            if (!this.userEvents) return;

            if (this.editBox.enable) {
                this.editBox.onkeydown(e);
                return;
            }

            if (this._leftPanelController.getPanel().editBox.isEnable()) { return; }

            e = window.event || e;
            var keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
            this.counter = 0;

            var hidePopUp = false;

            var mY = 0;
            var maxVal = floor2((this.rightScroll.maxValue()) / this.itemMargin) * this.itemMargin;

            var offAdd = 0, maxPos = this.zoomBar.maxPos(), minPos = this.zoomBar.minPos();

            switch (keyCode) {
                case 191: { if ((e.ctrlKey || e.metaKey) && e.shiftKey) { this.printVersion = !this.printVersion; this.needUpdate = true; } } break;
                case 40:  // down
                    {
                        hidePopUp = true;

                        mY = this.fullscreen ?
                            this.rightScroll.value() + kUIScrollBarStepMoveY :
                            floor2((this.rightScroll.value() + this.itemMargin) / this.itemMargin + 0.5) * this.itemMargin - kEps;

                        this.animator.moveToY(Math.min(mY, maxVal - kEps), undefined, !this.fullscreen);
                    }
                    break;
                case 38:  // up
                    {
                        hidePopUp = true;

                        mY = this.fullscreen ?
                            this.rightScroll.value() - kUIScrollBarStepMoveY :
                            floor2((this.rightScroll.value() - this.itemMargin) / this.itemMargin + 0.5) * this.itemMargin - kEps;

                        this.animator.moveToY(Math.max(mY, kEps), undefined, !this.fullscreen);
                    }
                    break;
                case 33:  // pageup
                    {
                        hidePopUp = true;

                        if (!this.fullscreen) {
                            mY = floor2((this.rightScroll.value() -
                                floor2((this.ctxHeight - this.timeScale.height()) / this.itemMargin + 0.5) * this.itemMargin)
                                / this.itemMargin) * this.itemMargin - kEps;

                            this.animator.moveToY(Math.max(mY, kEps));
                        } else {
                            this.animator.moveToY(Math.max((this.rightScroll.calculatePageMove(1) * this.rightScroll.step), kEps));
                        }
                    }
                    break;
                case 34:  // pagedown
                    {
                        hidePopUp = true;

                        if (!this.fullscreen) {
                            mY = floor2((this.rightScroll.value() +
                                floor2((this.ctxHeight - this.timeScale.height()) / this.itemMargin + 0.5) * this.itemMargin)
                                / this.itemMargin) * this.itemMargin - kEps;

                            this.animator.moveToY(Math.min(mY, maxVal - kEps));
                        } else {
                            this.animator.moveToY(Math.min((this.rightScroll.calculatePageMove(-1) * this.rightScroll.step), this.rightScroll.maxValue() - kEps));
                        }
                    }
                    break;
                case 36:  // home
                    hidePopUp = true;

                    this.rightScroll.pageMove(1, 'end');
                    break;
                case 35:  // end
                    hidePopUp = true;

                    if (!this.fullscreen) {
                        this.animator.moveToY(maxVal - kEps, undefined, true);
                    } else {
                        this.rightScroll.pageMove(-1, 'home');
                    }
                    break;
                case 37: // left
                    offAdd = minPos - this.offsetX;
                    if (offAdd > 0) {
                        if (kEditModeElementDrag === this.editMode) return;

                        this.offWidgets();

                        this.offMenus();
                        this.painter.clearZones(true);
                        this.overlay.style.cursor = '';

                        offAdd = Math.min(Math.abs(offAdd), (this.ctxWidth / this.timeScale.hourInPixels * kUIScrollStepInWidthPercent));

                        this.animator.addMovementX(-offAdd);
                        // this.animatior.addMovementX(-this.timeScale.hourInPixels / this.timeScale.scaleX * 8);
                    }
                    break;
                case 39: // right

                    offAdd = maxPos - this.offsetX;
                    if (offAdd < 0) {
                        if (kEditModeElementDrag === this.editMode) return;

                        this.offWidgets();
                        this.offMenus();
                        this.painter.clearZones(true);
                        this.overlay.style.cursor = '';

                        offAdd = Math.min(Math.abs(offAdd), (this.ctxWidth / this.timeScale.hourInPixels * kUIScrollStepInWidthPercent));

                        this.animator.addMovementX(offAdd);
                        // this.animatior.addMovementX(this.timeScale.hourInPixels / this.timeScale.scaleX * 8);
                    }
                    break;
            }

            if (hidePopUp) {

                // скрываем все всплывающие элементы

                this.offMenus();
                this.offWidgets();
                this.clearPopUp = true;
            }
        },
        onkeyup: function (e) {
            if (!this.userEvents) return;

            if (!this.readMode && !(e.ctrlKey || e.metaKey)) {
                this.applyEditLink();
            }
        },

        // clipborad

        onpaste: function (e) {
            if (!this.userEvents) return;

            if (this.editBox.enable) {
                this.editBox.onpaste(e);
            }
        },
        oncopy: function (e) {
            if (!this.userEvents) return;

            if (this.editBox.enable) {
                this.editBox.oncopy(e);
            }
        },

        // main paintings

        drawScene: function (print) {
            if (!this.needUpdate)
                return;

            this.needUpdate = false;

            if (this.ctx) {
                this.ctxWidth = this.ctx.canvas.width;
                this.ctxHeight = this.ctx.canvas.height;
                this.bounding = this.ctx.canvas.getBoundingClientRect();

                if (this.fullscreen) {
                    this.centerScreen = this.ctxWidth * 0.5;
                    this.visibleLeft = 0;
                } else {
                    this.visibleLeft = this._leftPanelController.getPanel().getWidth();
                    this.centerScreen = (this.ctxWidth + this.visibleLeft) * 0.5;
                }

                this.worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
                this.offX = this.timeScale.hourInPixels * this.offsetX;

                if (this.needUpdateContent) {
                    this.contentHeight = this.getContentHeight();
                    if (!this.fullscreen) { this.contentHeight += this.itemMargin; }
                    var viewHeight = this.ctxHeight - this.timeScale.height();
                    this.rightScroll.init(this.timeScale.height(), viewHeight, this.contentHeight + !this.fullscreen * this.itemMargin, this.ctxWidth);
                    this.rightScroll.clamp();

                    // if (this.zoomBar) {this.bottomScroll.updateThumb(this.zoomBar.thumb);}

                    this.visibleUp = (!this.fullscreen) * (this.timeScale.height());
                    this.needUpdateContent = false;

                    if (this._leftPanelController) { this._leftPanelController.scrollContent(this.rightScroll.value()); }
                }

                if (this.zoomBar) { this.bottomScroll.updateThumb(this.zoomBar.thumb, this.ctxWidth, this.ctxHeight); }

                this.timeScale.refresh();

                this.ctx.clearRect(0, 0, this.ctxWidth, this.ctxHeight);

                this.worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
                this.offX = this.timeScale.hourInPixels * this.offsetX;
                this.scrollY = (this.rightScroll.value()) >> 0;

                this.drawWeekend();
                this.timeScale.drawLines();
                this.timeScale.draw();

                this.drawElementHorLines();
                this.drawDayLine();
                this.drawWeekendLines();

                if (this.fullscreen) {
                    this.ctx.save();
                    this.ctx.rect(0, this.timeScale.height() + this.itemProjectMargin * 2, this.ctxWidth, this.ctxHeight);
                    this.ctx.clip();
                }

                this.drawProjects();

                if (this.fullscreen) {
                    this.ctx.restore();
                }

                this.drawPopWidgets();

                if (undefined === print) {
                    this.rightScroll.refresh();
                    this.bottomScroll.refresh();
                }

                if (this.ctx.canvas.width - this.ctxWidth > 1) {
                    this.ctx.fillStyle = '#ffffff';
                    this.ctx.fillRect(this.ctxWidth, 0, this.ctx.canvas.width - this.ctxWidth, this.ctxHeight);
                }

                this.updateScrollContent();

                if (this._needDrawFlashBackground) {
                    this.ctx.fillStyle = 'rgba(255,255,255,0.7)';
                    this.ctx.fillRect(0, 0, this.ctxWidth, this.ctxHeight);

                }

                if (this.printVersion) {
                    this.ctx.font = 'Bold 10pt ' + kDefaultFontName;
                    this.ctx.fillStyle = '#000000';
                    this.ctx.fillText('v' + kInternalVersion, this.ctxWidth - 70, this.ctxHeight - 15);
                }
            }
        },

        drawProjects: function () {
            var i, j, tx, tn, te, textW;

            var offY = floor2(this.timeScale.height());
            var scrollY = floor2(this.rightScroll.value());

            this.ctx.font = this.titlesFont;

            this._renderer.prepare();
            this.editBox.boundUpdate = false;

            this.projectsLines = [];

            var p = this.storage.projects();
            var length = p.length;

            for (j = 0; j < length; ++j) {
                var tasks = p[j].t;

                var tasksLength = tasks.length;

                // this.ctx.fillStyle = '#00ffff';
                // this.ctx.fillRect(0, offY - scrollY, this.ctxWidth, this.itemMargin);

                this.projectsLines.push(offY - scrollY + this.itemMargin * 0.5);

                offY += this.itemMargin;

                if (p[j].fullCollapse) { continue; }

                tx = this.offX + p[j].beginTime * this.worldToScreen;
                te = p[j].duration * this.worldToScreen;

                this.ctx.strokeStyle = kLinesVerticalColor;
                this.ctx.lineWidth = 1;

                offY += (this.itemMargin - this.itemHeight) * 0.5;

                if (j === this.capProject)
                    offY = this.drawProject(p[j], offY, this.capTask, scrollY, j);
                else
                    offY = this.drawProject(p[j], offY, -1, scrollY, j);

                if (offY - scrollY > this.ctxHeight) { break; }

                offY -= (this.itemMargin - this.itemHeight) * 0.5;

                // this.ctx.fillRect(0, offY, this.ctxWidth, this.itemMargin);

                if (0 === tasksLength)
                    offY += this.itemMargin;
            }

            if (!this.editBox.boundUpdate) { this.editBox.setBound(0, 0, 0, 0, true); }
        },
        drawProject: function (project, offY, capture, scrollY, p) {
            var i, j, x, tasksCount, length, free = false, outFactor = false;

            var m = project.m, realTasksCount;
            var count = m.length;

            for (i = 0; i < count; ++i) {
                tasksCount = m[i].t.length;

                //  в фильтре поэтому пропускаем

                if (m[i].filter) { continue; }

                // подсчитываем реально видимые задачи с учетом фильтра

                realTasksCount = tasksCount;

                for (x = tasksCount - 1; x >= 0; --x) {
                    if (m[i].t[x].filter) { --realTasksCount; }
                }

                this._renderer.drawBackLightMilestone(p, i, -1, offY - scrollY + 3);
                this._renderer.drawMilestone(m[i], offY, p, i, realTasksCount);

                // выходим за видимую область по высоте для вех с задачами, поэтому не рисуем свободные задачи

                if (offY - scrollY > this.ctxHeight) {
                    outFactor = true;
                    break;
                }

                offY += this.itemMargin;

                if (offY - scrollY + tasksCount * this.itemMargin > 0 && offY - scrollY < this.ctxHeight) {
                    if ((!this.fullscreen && !m[i].collapse) || this.fullscreen)
                        this._renderer.drawLinks(m[i].t, this.offX, offY, scrollY);
                }

                if (0 === tasksCount && this.fullscreen) { offY += this.itemMargin; }

                if (!this.fullscreen && m[i].collapse) { continue; }

                for (j = 0; j < tasksCount; ++j) {

                    //  в фильтре поэтому пропускаем

                    if (m[i].t[j].filter) { continue; }

                    this._renderer.drawBackLightTask(p, i, j, offY - scrollY + 3);         // TODO: fix
                    this._renderer.drawTask(m[i].t[j], offY, free, p, i, j, m[i].t);

                    offY += this.itemMargin;
                }
            }

            if (outFactor) {
                return offY;
            }

            length = project.t.length;
            if (length) {

                i = -1; // set milestone -1

                if (this.fullscreen) { offY += this.itemHeight; }

                free = true;

                if (!this.fullscreen) {
                    offY += this.itemMargin;

                    if (project.collapse) { return offY; }
                }

                if (offY - scrollY < this.ctxHeight) {
                    if ((!this.fullscreen && !project.collapse) || this.fullscreen)
                        this._renderer.drawLinks(project.t, this.offX, offY, scrollY);
                }

                for (j = 0; j < length; ++j) {

                    //  в фильтре поэтому пропускаем

                    if (project.t[j].filter) { continue; }

                    this._renderer.drawBackLightFreeTask(p, i, j, offY - scrollY + 3);     // TODO: fix
                    this._renderer.drawTask(project.t[j], offY, free, p, i, j, project.t);

                    offY += this.itemMargin;
                }
            }

            return offY;
        },
        drawPopWidgets: function () {
            var t = this, k = 0;

            function splitStrings(str, str2, maxstrings) {
                if (!str || 0 === str.length) return { lines: [], width: 0 };
                if (!str2) str2 = str;

                var lines = [];
                var maxBlockWidth = 0;

                var allTextWidth = Math.max(t.ctx.measureText(str).width, t.ctx.measureText(str2).width) / t.zoom;
                if (allTextWidth > kDetailsWidgetSettings.width) {
                    var i, j;

                    var widgetWidth = (allTextWidth > kDetailsWidgetSettings.maxWidth * 2) ?
                        kDetailsWidgetSettings.maxWidth : kDetailsWidgetSettings.width;

                    var strings = [], line = '';
                    var length = str.length;
                    var from = 0;

                    for (i = 0; i < length; ++i) {
                        if ('\n' === str[i]) {
                            strings.push(str.slice(from, i));
                            strings.push('\n'); ++i;
                            from = i;
                        }

                        if (' ' === str[i] && from !== i) {
                            strings.push(str.slice(from, i));
                            from = i;
                        }

                        if (i == length - 1) {
                            if (from !== i) {
                                strings.push(str.slice(from, i + 1));
                            }
                        }
                    }

                    length = strings.length;

                    for (j = 0; j < length; ++j) {

                        var appLine = line + strings[j];
                        var widthLine = t.ctx.measureText(appLine).width / t.zoom;

                        if (t.ctx.measureText(strings[j]).width / t.zoom > widgetWidth) {
                            line = strings[j].slice(0, 25).replace(/^\s+/, '') + '...';
                            lines.push(line);
                            maxBlockWidth = Math.max(t.ctx.measureText(line).width / t.zoom, maxBlockWidth);
                            break;
                        }

                        if (strings[j] === '\n') {
                            line = line.replace(/^\s+/, '');
                            lines.push(line);
                            maxBlockWidth = Math.max(t.ctx.measureText(line).width / t.zoom, maxBlockWidth);
                            line = '';
                            continue;
                        }

                        if (widthLine > widgetWidth) {
                            line = line.replace(/^\s+/, '');
                            lines.push(line);
                            maxBlockWidth = Math.max(t.ctx.measureText(line).width / t.zoom, maxBlockWidth);
                            line = strings[j];
                        } else {
                            line = appLine;
                        }

                        if (j == length - 1) {
                            line = line.replace(/^\s+/, '');
                            lines.push(line);
                            maxBlockWidth = Math.max(t.ctx.measureText(line).width / t.zoom, maxBlockWidth);
                        }

                        if (lines.length > maxstrings - 1 && -1 !== maxstrings) {
                            if (lines[lines.length - 1].length > 2) {
                                if (' ' == lines[lines.length - 1][lines[lines.length - 1].length - 2]) {
                                    lines[lines.length - 1] = lines[lines.length - 1].slice(0, -2);
                                }
                            }

                            lines.push('...'); break;
                        }
                    }

                    //console.log(lines);
                } else {
                    lines.push(str);
                    maxBlockWidth = kDetailsWidgetSettings.width;
                }

                maxBlockWidth += t.taskTextIndent * 2;

                return { lines: lines, width: maxBlockWidth * t.zoom };
            }

            function drawTaskWidget() {

                // calculate widget sizes

                var isOverdue = (kTaskOverdueColor == widget.fill);
                var isComplete = (kTaskCompleteColor == widget.fill);

                var tx = t.visibleLeft + (t.visibleLeft > 0 ? 5 : 0);
                widget.x = Math.max(tx, widget.x);

                var widgetTxtOffX = widget.x + t.taskTextIndent * t.zoom;
                var widthWidget = Math.max(widget.t.stringsBlocks.width, widget.t.descStrings.width);

                var heightWidget = widget.t.stringsBlocks.lines.length * t.fontPx;
                heightWidget += widget.t.descStrings.lines.length * t.fontPx;
                heightWidget += t.fontPx * 0.5;

                var openSubtaskCount = widget.t.subtasks();

                if (isOverdue) { heightWidget += t.fontPx; }
                if (widget.t.performer && widget.t.performer.length) { heightWidget += t.fontPx * 1.5; }
                if (openSubtaskCount) { heightWidget += t.fontPx * 1.5; }

                heightWidget += t.fontPx * 1.5; // responsible

                heightWidget += t.fontPx * 1.5; // details

                heightWidget += t.fontPx * 2.2;
                heightWidget /= t.zoom;

                // draw dates

                var dayTxt = widget.t.formatBeginDate() + ' - ' + widget.t.formatEndDate();
                if (widget.t.endTime > 0 && !isComplete) dayTxt += ' (' + window['Gantt']['Localize_strings']['statusOpenTask'] + ')';
                if (isComplete) dayTxt += ' (' + window['Gantt']['Localize_strings']['statusClosedTask'] + ')';
                widthWidget = Math.max((t.ctx.measureText(dayTxt).width + t.taskTextIndent * 2) * t.zoom, widthWidget);

                var dayOverdue = '';
                if (isOverdue) {
                    dayOverdue = window['Gantt']['Localize_strings']['taskOverdueText']['format'](widget.t.daysPastDue());
                    widthWidget = Math.max((t.ctx.measureText(dayOverdue).width + t.taskTextIndent * 2) * t.zoom, widthWidget);
                }

                t.taskDescWidget.bound.x = Math.max(1, widget.x);
                t.taskDescWidget.bound.y = widget.y;
                t.taskDescWidget.bound.w = widthWidget;
                t.taskDescWidget.bound.h = heightWidget;

                // draw bound widget

                t.ctx.fillStyle = kDetailsWidgetSettings.backgroundColor;
                t.ctx.fillRect(widget.x, widget.y, widthWidget, heightWidget * t.zoom);

                t.ctx.strokeStyle = widget.fill;
                t.ctx.lineWidth = 1;
                t.ctx.strokeRect(Math.floor(widget.x) + 0.5, Math.floor(widget.y) + 0.5,
                    Math.floor(widthWidget), Math.floor(heightWidget * t.zoom + 1));

                // draw text lines

                t.ctx.fillStyle = '#000000';
                t.ctx.strokeStyle = '#000000';

                var lineY = widget.y + t.fontPx + 1;

                for (k = 0; k < widget.t.stringsBlocks.lines.length; ++k) {
                    t.ctx.fillText(widget.t.stringsBlocks.lines[k], widget.x + t.taskTextIndent * t.zoom, lineY);

                    lineY += t.fontPx;
                }

                lineY += t.fontPx * 0.5;

                t.ctx.fillStyle = (isOverdue) ? widget.fill : '#000000';
                t.ctx.fillText(dayTxt, widgetTxtOffX, lineY);

                if (isOverdue) {
                    lineY += t.fontPx;
                    t.ctx.fillText(dayOverdue, widgetTxtOffX, lineY);
                }

                t.ctx.fillStyle = kDetailsWidgetSettings.titleColor;

                if (openSubtaskCount) {
                    lineY += t.fontPx * 1.5;
                    var text = openSubtaskCount == 1 ? window['Gantt']['Localize_strings']['taskDescSubtask'] : window['Gantt']['Localize_strings']['taskDescSubtasks'];
                    t.ctx.fillText(openSubtaskCount + ' ' + text, widgetTxtOffX, lineY);
                }

                lineY += t.fontPx * 1.5;

                var respCount = widget.t._responsibles.length;
                if (respCount > 0) {
                    var resp = widget.t.performer;

                    if (respCount > 1)
                        resp = window['Gantt']['Localize_strings']['responsibles']['format'](respCount);
                    else
                        resp = window['Encoder']['htmlDecode'](widget.t._responsibles[0]['displayName']);

                    t.ctx.fillText(resp, widgetTxtOffX, lineY);
                } else {
                    t.ctx.fillText(window['Gantt']['Localize_strings']['noResponsible'], widgetTxtOffX, lineY);
                }

                lineY += t.fontPx * 1.5;

                for (k = 0; k < widget.t.descStrings.lines.length; ++k) {
                    t.ctx.fillText(widget.t.descStrings.lines[k], widgetTxtOffX, lineY);

                    lineY += t.fontPx;
                }

                lineY += t.fontPx * 0.5;

                // details link
                var detailsText = window['Gantt']['Localize_strings']['details'];

                t.ctx.fillText(detailsText, widgetTxtOffX, lineY);

                t.ctx.beginPath();
                t.ctx.strokeStyle = kDetailsWidgetSettings.descriptionColor;
                t.ctx.lineWidth = Math.floor(t.zoom);
                t.ctx.moveTo(Math.floor(widgetTxtOffX) + 0.5, Math.floor(lineY + t.fontPx * 0.15) + 0.5);
                t.ctx.lineTo(Math.floor(widgetTxtOffX + t.ctx.measureText(detailsText).width) + 0.5, Math.floor(lineY + t.fontPx * 0.15) + 0.5);
                t.ctx.stroke();

                var textHeight = 14;
                t.taskDescWidget.detailsLinkBound.x = widgetTxtOffX;
                t.taskDescWidget.detailsLinkBound.y = lineY - textHeight;
                t.taskDescWidget.detailsLinkBound.w = t.ctx.measureText(detailsText).width;
                t.taskDescWidget.detailsLinkBound.h = textHeight;
            }

            function drawMilestoneWidget() {
                var isOverdue = (kMilestoneOverdueColor === widgetms.fill);
                var isComplete = (kMilestoneCompleteColor === widgetms.fill);

                var widthWidget = Math.max(widgetms.t.descStrings.width, kDetailsWidgetSettings.width * t.zoom);

                widgetms.x = Math.min(widgetms.x, t.ctxWidth - 5);
                var widgetTxtOffX = widgetms.x + t.taskTextIndent * t.zoom - widthWidget;

                var heightWidget = widgetms.t.descStrings.lines.length * t.fontPx;
                heightWidget += t.fontPx * 0.5;
                if (widgetms.t.t.length) { heightWidget += t.fontPx * 1.25; }
                if (widgetms.t._responsibles) { heightWidget += t.fontPx * 1.5; }
                heightWidget += t.fontPx * 2;

                t.milestoneDescWidget.bound.x = widgetms.x - widthWidget;
                t.milestoneDescWidget.bound.y = widgetms.y;
                t.milestoneDescWidget.bound.w = widthWidget;
                t.milestoneDescWidget.bound.h = heightWidget;

                // draw bound widget

                t.ctx.fillStyle = kDetailsWidgetSettings.backgroundColor;
                t.ctx.fillRect(widgetms.x, widgetms.y, -widthWidget, heightWidget);

                t.ctx.strokeStyle = widgetms.fill;
                t.ctx.lineWidth = 1;
                t.ctx.strokeRect(Math.floor(widgetms.x) + 0.5, Math.floor(widgetms.y) + 0.5,
                    -Math.floor(widthWidget), Math.floor(heightWidget + 1));

                // draw text lines

                var lineY = widgetms.y + t.fontPx * 1.25;

                var dayTxt = widgetms.t.formatEndDate();
                if (widgetms.t.endTime > 0 && !isComplete) dayTxt += ' (' + window['Gantt']['Localize_strings']['statusOpenMilestone'] + ')';
                if (isComplete) dayTxt += ' (' + window['Gantt']['Localize_strings']['statusClosedMilestone'] + ')';
                t.ctx.fillStyle = (isOverdue) ? widgetms.fill : '#000000';
                if (isOverdue) { dayTxt += ' (' + window['Gantt']['Localize_strings']['overdue'] + ')'; }
                t.ctx.fillText(dayTxt, widgetTxtOffX, lineY);

                if (widgetms.t.t.length) {
                    lineY += t.fontPx * 1.25;

                    t.ctx.fillStyle = t.windowsWidgetColorText;
                    t.ctx.fillText(widgetms.t.t.length + ' ' + window['Gantt']['Localize_strings']['tasks'], widgetTxtOffX, lineY);
                }

                t.ctx.fillStyle = t.windowsWidgetColorText;

                if (widgetms.t._responsibles) {
                    lineY += t.fontPx * 1.5;
                    t.ctx.fillText(window['Encoder']['htmlDecode'](widgetms.t._responsibles['displayName']), widgetTxtOffX, lineY);
                }

                lineY += t.fontPx * 1.5;

                for (k = 0; k < widgetms.t.descStrings.lines.length; ++k) {
                    t.ctx.fillText(widgetms.t.descStrings.lines[k], widgetTxtOffX, lineY);

                    lineY += t.fontPx;
                }

                lineY += t.fontPx * 0.5;
            }

            var widget = this.taskDescWidget.ref;
            if (widget) {

                if (null == widget.t.stringsBlocks) widget.t.stringsBlocks = splitStrings(widget.t._title, widget.t._description, kMaxShowTextStrings);
                if (null == widget.t.descStrings) widget.t.descStrings = splitStrings(widget.t._description, widget.t._title, kMaxShowTextStrings);

                drawTaskWidget();
            }

            var widgetms = this.milestoneDescWidget.ref;
            if (widgetms) {
                if (null == widgetms.t.descStrings) widgetms.t.descStrings = splitStrings(widgetms.t._description, -1);
                drawMilestoneWidget();
            }
        },

        drawDayLine: function () {
            if (kTypeScaleMonth === this.timeScale.scaleType) {
                var offX = floor2(this.timeToSceneX(0)) + 0.5;

                this.painter.drawLineH(offX, 0, offX, this.ctxHeight, kDayLineColor, 1);
            }
        },
        drawElementHorLines: function () {

            if (!this.fullscreen) {

                var divLine = (this.rightScroll.value()) % this.itemMargin;
                var height = this.timeScale.height() - divLine;

                var maxOffY = this.contentHeight - this.rightScroll.value();

                //if (this.storage.p.length) {
                //    //    maxOffY             -=   this.storage.p[0].collapse * this.itemMargin;
                //}

                if (maxOffY < height) {

                    // первая строчка более темная

                    this.ctx.beginPath();

                    this.ctx.strokeStyle = kLinesVerticalColor;
                    this.ctx.lineWidth = 1;

                    this.ctx.moveTo(0, this.itemMargin + 0.5);
                    this.ctx.lineTo(this.ctxWidth, this.itemMargin + 0.5);

                    this.ctx.moveTo(0, this.itemMargin * 2 + 0.5);
                    this.ctx.lineTo(this.ctxWidth, this.itemMargin * 2 + 0.5);

                    this.ctx.stroke();

                    return;
                }

                if (this.projectsLines.length !== this.storage.p.length) {
                    this.needUpdate = true;
                }

                var lineY = 0;
                var linePY = 0;

                this.projectsLinesY = [];

                this.ctx.beginPath();

                this.ctx.strokeStyle = kLinesVerticalColor;
                this.ctx.lineWidth = 1;

                for (; height <= this.ctxHeight; height += this.itemMargin) {

                    lineY = floor2(height) + 0.5;

                    if (lineY < this.visibleUp) {
                        linePY = lineY;
                        continue;
                    }

                    this.ctx.moveTo(0.5, lineY);
                    this.ctx.lineTo(this.ctxWidth + 0.5, lineY);

                    for (var j = this.projectsLines.length - 1; j >= 0; --j) {
                        if (this.projectsLines[j] > linePY && this.projectsLines[j] < lineY) {
                            this.projectsLinesY.push(linePY);
                            break;
                        }
                    }

                    linePY = lineY;

                    if (maxOffY < height) { break; }
                }

                this.ctx.stroke();

                // первая строчка более темная

                this.ctx.beginPath();

                this.ctx.strokeStyle = kLinesVerticalColor;
                this.ctx.lineWidth = 1;

                this.ctx.moveTo(0, this.itemMargin + 0.5);
                this.ctx.lineTo(this.ctxWidth, this.itemMargin + 0.5);

                this.ctx.moveTo(0, this.itemMargin * 2 + 0.5);
                this.ctx.lineTo(this.ctxWidth, this.itemMargin * 2 + 0.5);

                this.ctx.stroke();

                // project's white lines

                if (kTypeScaleDays !== this.timeScale.scaleType) {
                    var width = this.itemMargin;

                    this.ctx.fillStyle = '#ffffff';

                    var margin = this.itemMargin;

                    for (var i = this.projectsLinesY.length - 1; i >= 0; --i) {

                        lineY = this.projectsLinesY[i] + 1;

                        if (this.projectsLinesY[i] > margin * 2) {
                            this.ctx.fillRect(0, lineY, this.ctxWidth, margin - 2);
                        } else if (this.projectsLinesY[i] >= margin) {
                            this.ctx.fillRect(0, margin * 2 + 1, this.ctxWidth, lineY - margin - 2);
                        }
                    }
                }
            }
        },

        drawWeekend: function () {
            var weekend = this.timeScale.weekend();
            if (weekend) {
                var ind = -1;
                var offX = this.timeScale.hourInPixels * this.offsetX;
                if (1 === this.timeScale.divisionInDays && (kTypeScaleMonth !== this.timeScale.scaleType)) {
                    var y = this.itemMargin;
                    var height = this.ctxHeight - y;

                    var arrItems = this.ctxWidth / weekend.off + 2;

                    var stepRight = (offX / weekend.off - ((offX / weekend.off) >> 0)) * weekend.off + weekend.x;

                    this.ctx.fillStyle = kWeekendColor;

                    for (ind = -1; ind < arrItems; ++ind) {
                        this.ctx.fillRect(stepRight + ind * weekend.off, y, weekend.width, height);
                    }
                }

                offX = floor2(this.timeToSceneX(0)) + 0.5;
                this.ctx.fillStyle = kDayBandColor;
                this.ctx.fillRect(offX, this.itemMargin, weekend.width / 2, this.ctxHeight - this.itemMargin);
            }
        },
        drawWeekendLines: function () {
            var weekend = this.timeScale.weekend();
            if (weekend) {
                var ind = -1;
                var offX = this.timeScale.hourInPixels * this.offsetX;

                if (1 === this.timeScale.divisionInDays && (kTypeScaleMonth !== this.timeScale.scaleType)) {
                    var y = this.itemMargin;
                    var height = this.ctxHeight;

                    var arrItems = this.ctxWidth / weekend.off + 2;
                    var stepRight = (offX / weekend.off - ((offX / weekend.off) >> 0)) * weekend.off + weekend.x;

                    this.ctx.lineWidth = 1;
                    this.ctx.strokeStyle = kLinesVerticalColor;

                    this.ctx.beginPath();

                    for (ind = -1; ind < arrItems; ++ind) {

                        this.ctx.moveTo(floor2(stepRight + ind * weekend.off) + 0.5, floor2(this.itemMargin));
                        this.ctx.lineTo(floor2(stepRight + ind * weekend.off) + 0.5, floor2(height));

                        this.ctx.moveTo(floor2(stepRight + ind * weekend.off + weekend.width) + 0.5, floor2(this.itemMargin) + 0.5);
                        this.ctx.lineTo(floor2(stepRight + ind * weekend.off + weekend.width) + 0.5, floor2(height) + 0.5);
                    }

                    this.ctx.stroke();

                    this.ctx.lineWidth = 1;
                    this.ctx.strokeStyle = kLinesVerticalColor;

                    this.ctx.beginPath();

                    for (ind = -1; ind < arrItems; ++ind) {

                        for (var j = 3; j < 7; ++j) {
                            if (j === 5) { continue; }
                            this.ctx.moveTo(floor2(stepRight + ind * weekend.off + weekend.width * 0.5 * j) + 0.5, floor2(this.itemMargin));
                            this.ctx.lineTo(floor2(stepRight + ind * weekend.off + weekend.width * 0.5 * j) + 0.5, floor2(height));

                            this.ctx.moveTo(floor2(stepRight + ind * weekend.off + weekend.width + weekend.width * 0.5 * j) + 0.5, floor2(this.itemMargin) + 0.5);
                            this.ctx.lineTo(floor2(stepRight + ind * weekend.off + weekend.width + weekend.width * 0.5 * j) + 0.5, floor2(height) + 0.5);
                        }
                    }

                    this.ctx.stroke();

                    // project's white lines

                    this.ctx.fillStyle = '#ffffff';

                    var margin = this.itemMargin;
                    var lineY = 0;

                    for (var i = this.projectsLinesY.length - 1; i >= 0; --i) {

                        lineY = this.projectsLinesY[i] + 1;

                        if (this.projectsLinesY[i] > margin * 2) {
                            this.ctx.fillRect(0, lineY, this.ctxWidth, margin - 2);
                        } else if (this.projectsLinesY[i] >= margin) {
                            this.ctx.fillRect(0, margin * 2 + 1, this.ctxWidth, lineY - margin - 2);
                        }
                    }

                }
            }
        },

        // overlay paintings

        drawEditLayer: function () {
            if (this.clearPopUp) {
                if (this.mouse) {
                    this.onmousemove(this.mouse.baseEvent, true);
                }

                this.clearPopUp = false;
            }

            if (this.editBox.enable) {
                this.painter.clearZones(true);
                this.editBox.draw();
                this.painter.addBoundZone(this.editBox.bound, 10 * this.zoom);

                return;
            }

            if (kEditModeAddLink === this.editMode)
                this.drawLinkEditLines();
            else if (kEditModeElementDrag === this.editMode)
                this.drawDragElement();
            else
                this.drawPopUpMenu();
        },
        drawDragElement: function () {
            var t = this;

            function drawDragItem() {
                if (t.dragBound.failEnd) {

                    t.overlayctx.fillStyle = kTaskDragDropFillColor;
                    t.overlayctx.strokeStyle = (t.dragTo.p === t.itemToDrop.p) ? kTaskDragDropBorderColor : kTaskDragDropInvalidBorderColor;
                    t.overlayctx.lineWidth = 1;

                    if (t.dragBound.x + t.dragBound.dx <= t.visibleLeft) {
                        t.dragBound.width = t.dragBound.width + t.dragBound.x + t.dragBound.dx - t.visibleLeft;
                        t.dragBound.x = t.visibleLeft;
                        t.dragBound.dx = 0;
                    }

                    var x = floor2(t.dragBound.x + t.dragBound.dx) + 0.5;
                    var y = floor2(t.dragBound.y + t.dragBound.dy) + 0.5;

                    t.overlayctx.beginPath();
                    t.overlayctx.moveTo(x, y);
                    t.overlayctx.lineTo(x + t.dragBound.width, y);
                    t.overlayctx.lineTo(x + t.dragBound.width /*- t.dragBound.height * 0.5*/, y + t.dragBound.height * 0.5);
                    t.overlayctx.lineTo(x + t.dragBound.width, y + t.dragBound.height);
                    t.overlayctx.lineTo(x, y + t.dragBound.height);
                    t.overlayctx.closePath();
                    t.overlayctx.fill();
                    t.overlayctx.stroke();

                    t.overlayctx.drawImage(t._renderer.infinity, x + t.dragBound.width + 4, y + 2);

                } else {

                    if (t.dragBound.x + t.dragBound.dx <= t.visibleLeft) {
                        t.dragBound.width = t.dragBound.width + t.dragBound.x + t.dragBound.dx - t.visibleLeft;
                        t.dragBound.x = t.visibleLeft;
                        t.dragBound.dx = 0;
                    }

                    t.overlayctx.fillStyle = kTaskDragDropFillColor;

                    t.overlayctx.fillRect(t.dragBound.x + t.dragBound.dx,
                            t.dragBound.y + t.dragBound.dy,
                        t.dragBound.width,
                        t.dragBound.height);

                    t.overlayctx.strokeStyle = (t.dragTo.p === t.itemToDrop.p) ? kTaskDragDropBorderColor : kTaskDragDropInvalidBorderColor;;
                    t.overlayctx.lineWidth = 1;

                    t.overlayctx.strokeRect(floor2(t.dragBound.x + t.dragBound.dx) + 0.5,
                            floor2(t.dragBound.y + t.dragBound.dy) + 0.5,
                        floor2(t.dragBound.width),
                        floor2(t.dragBound.height));
                }

                t.painter.addZone(t.dragBound.x + t.dragBound.dx,
                        t.dragBound.y + t.dragBound.dy,
                    t.dragBound.width,
                    t.dragBound.height, 20);
            }
            function drawCaptureMilestone() {

                if (t.animator.isScroll()) {
                    return;
                }

                var length, tasksCount, realTasksCount, x, margin = t.itemMargin;

                var offY = Math.floor(t.timeScale.height());
                var clampX = Math.floor(t.visibleLeft);
                var clampY = Math.floor(t.timeScale.height() + t.itemProjectMargin * 2);

                var p = t.storage.projects();
                var pl = p.length;

                for (var j = 0; j < pl; ++j) {

                    // весь контент у проекты скрыт

                    if (p[j].fullCollapse) { offY += margin; continue; }

                    if (!p[j].isEmpty()) {

                        offY += margin;

                        length = p[j].m.length;

                        for (var i = 0; i < length; ++i) {

                            // применен фильтр

                            if (p[j].m[i].filter) { continue; }

                            // подсчитываем реально видимые задачи с учетом фильтра

                            tasksCount = p[j].m[i].t.length;
                            realTasksCount = tasksCount;

                            for (x = tasksCount - 1; x >= 0; --x) {
                                if (p[j].m[i].t[x].filter) { --realTasksCount; }
                            }

                            if (j === t.dragTo.p && i === t.dragTo.m) {

                                // если веха закрытая то ничего не рисуем
                                if (1 === p[j].m[i]._status) return;

                                // перенос работает в рамках одного проекта
                                if (t.dragTo.p !== t.itemToDrop.p) return;

                                var tx = t.offX + p[j].m[i].beginTime * t.worldToScreen;
                                var te = p[j].m[i].duration * t.worldToScreen;
                                var rel = Math.floor(offY - t.scrollY + t.itemHeight - t.milestoneLinewidth * t.zoom);
                                var th = (p[j].m[i].t.length !== 0) ? (realTasksCount + 0.5) * margin : 0.5 * margin;

                                if (!p[j].m[i].collapse) {
                                    th = (realTasksCount !== 0) ? (realTasksCount + 0.5) * margin : 0.5 * margin;
                                    th = (realTasksCount !== 0) ? (realTasksCount + 0.5) * margin : 0.5 * margin;
                                } else {
                                    th = margin * 0.5; //(realTasksCount !== 0) ? margin * 0.5 : margin * 0.5;
                                }

                                if (!t.backLightMilestone.enable) {
                                    t.overlayctx.fillStyle = kMilestoneColorBackLight;
                                    t.overlayctx.fillRect(tx, rel + t.itemHeight, te, th);
                                }

                                if (tx < clampX) { te -= clampX - tx; tx = clampX; }

                                if (tx + te > clampX) {
                                    // три линии для отрисовки той вехи в которую мы переносим задачу

                                    t.overlayctx.strokeStyle = kMilestoneBeforeDragColor;
                                    t.overlayctx.lineWidth = 1;

                                    t.overlayctx.beginPath();

                                    t.overlayctx.moveTo(Math.floor(tx) + 0.5, Math.max(clampY, rel - t.milestoneLinewidth * t.zoom + 0.5));
                                    t.overlayctx.lineTo(Math.floor(tx) + 0.5, rel + Math.floor(th) + 0.5);
                                    t.overlayctx.lineTo(Math.floor(tx + te) + 0.5, rel + Math.floor(th) + 0.5);
                                    t.overlayctx.lineTo(Math.floor(tx + te) + 0.5, Math.max(clampY, rel - t.milestoneLinewidth * t.zoom + 0.5));

                                    t.overlayctx.stroke();

                                    t.painter.addZone(tx, rel, te, th, margin);

                                    // console.log('milestone: ' + i);
                                }

                                return;
                            }

                            offY += margin;

                            if (!p[j].m[i].collapse) { offY += margin * realTasksCount; }
                        }

                        // свободные задачи в проекте скрыты

                        if (p[j].collapse) { offY += margin; continue; }

                        if (p[j].t.length) {

                            offY += margin;

                            for (i = p[j].t.length - 1; i >= 0; --i) {

                                // применен фильтр

                                if (p[j].t[i].filter) { continue; }

                                offY += margin;
                            }
                        } else {

                            // линия свободных задач

                            offY += margin;
                        }

                        if (offY - t.scrollY > t.ctxHeight)
                            return;

                    } else {

                        // пустой проект имеет только две линии - сам проект и свободные задачи

                        offY += margin * 2;
                    }
                }
            }

            if (this.updateDragDrop) {

                this.painter.clearZones(true);

                drawCaptureMilestone();
                drawDragItem();

                this.updateDragDrop = false;
            }
        },
        drawPopUpMenu: function () {

            this.painter.clearZones(true);

            if (this.readMode) return;

            if (-1 !== this.hitTask && 0 === this.hitSide) {

                this.menuTask.setBound(Math.floor(this.menuTask.offx - kMenuSettings.elementsWidth5X * 0.5 * this.zoom),
                        Math.floor(this.menuTask.offy) - (kMenuSettings.icoSize + kMenuSettings.borderSz * 2 + 5) * this.zoom,
                        kMenuSettings.elementsWidth5X * this.zoom,
                        (kMenuSettings.icoSize + kMenuSettings.borderSz * 2 + 5) * this.zoom);
            }

            if (-1 !== this.menuMileStone.m && -1 !== this.menuMileStone.p) {

                var width = (this.menuMileStone.ref._status == kElementCompleted - 1) ? kMenuSettings.elementsWidth3X : kMenuSettings.elementsWidth6X;

                this.menuMileStone.setBound(floor2(this.menuMileStone.offx - width * 0.5 * this.zoom),
                        floor2(this.menuMileStone.offy) - (kMenuSettings.icoSize + kMenuSettings.borderSz * 3 + this.milestoneMenuOffY * 2) * this.zoom,
                        (width) * this.zoom,
                        (kMenuSettings.icoSize + kMenuSettings.borderSz * 2 + this.milestoneMenuOffY * 2) * this.zoom);
            }

            if (this.linkWidget.isValid()) {
                this.linkWidget.render();
                return;
            }

            this.painter.clearZones(true);

            if (this.menuTask.draw()) {
                this.painter.
                    addZone(this.menuTask.bound.x,
                    this.menuTask.bound.y,
                    this.menuTask.bound.w,
                    this.menuTask.bound.h, 10 * this.zoom);
            }

            if (this.menuMileStone.draw()) {
                this.painter.
                    addZone(this.menuMileStone.bound.x,
                    this.menuMileStone.bound.y,
                    this.menuMileStone.bound.w,
                    this.menuMileStone.bound.h, 10 * this.zoom);
            }
        },
        drawLinkEditLines: function () {
            if (this.linkLineEdit) {
                if (this.linkLineEdit.update) {

                    this.painter.clearZones(true);

                    if (!this.pushLink) {

                        var xFrom, xTo, yFrom, yTo, itemHf = this.itemHeight * 0.5, itemH = this.itemHeight;
                        var mouseX = this.moveMouse.x, mouseY = this.moveMouse.y;
                        var linkX = this.linkLineEdit.posX, linkY = this.linkLineEdit.posY + 1;

                        this.overlayctx.lineWidth = 2;
                        this.overlayctx.strokeStyle = '#24547E';

                        if (kTaskSideLeft === this.linkLineEdit.side) {
                            if (mouseX < linkX) {

                                this.painter.drawDashedLineH(this.overlayctx,
                                    floor2(mouseX - 0.5), floor2(linkY),
                                    floor2(linkX + 0.5), floor2(linkY), [6, 2]);

                                this.painter.drawDashedLineV(this.overlayctx,
                                    floor2(mouseX), floor2(Math.min(linkY, mouseY) - 0.5),
                                    floor2(mouseX), floor2(Math.max(linkY, mouseY) + 0.5), [6, 2]);
                            } else {
                                if (mouseY < this.linkLineEdit.posY) {

                                    xFrom = floor2(linkX - itemHf - 0.5);
                                    xTo = floor2(mouseX + 0.5);

                                    if (xFrom < xTo) {

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            xFrom, floor2(linkY - itemH),
                                            xTo, floor2(linkY - itemH), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            xFrom, floor2(linkY),
                                            floor2(xFrom + itemHf), floor2(linkY), [6, 2]);

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            xFrom, floor2(linkY - itemH),
                                            xFrom, floor2(linkY), [6, 2]);
                                    }

                                    yFrom = floor2(Math.min(linkY, mouseY));
                                    yTo = floor2(Math.max(linkY - itemH, mouseY - itemH));

                                    if (yFrom < yTo) {

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            floor2(mouseX), yFrom,
                                            floor2(mouseX), yTo, [6, 2]);
                                    }
                                } else {
                                    xFrom = floor2(linkX - itemHf);
                                    xTo = floor2(mouseX);

                                    if (xFrom < xTo) {

                                        this.painter.drawDashedLineH(this.overlayctx, xFrom,
                                            floor2(linkY + itemH), xTo,
                                            floor2(linkY + itemH), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx, xFrom,
                                            floor2(linkY), floor2(xFrom + itemHf),
                                            floor2(linkY), [6, 2]);

                                        this.painter.drawDashedLineV(this.overlayctx, xFrom,
                                            floor2(linkY), xFrom,
                                            floor2(linkY + itemH), [6, 2]);
                                    }

                                    yFrom = floor2(Math.min(linkY + itemH, mouseY + itemH));
                                    yTo = floor2(Math.max(linkY + itemH, mouseY + itemH));

                                    if (yFrom < yTo) {

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            floor2(mouseX), yFrom,
                                            floor2(mouseX), yTo, [6, 2]);
                                    }
                                }
                            }
                        }

                        if (kTaskSideRight === this.linkLineEdit.side) {
                            if (mouseX > linkX) {

                                this.painter.drawDashedLineH(this.overlayctx,
                                    floor2(linkX - 0.5), floor2(linkY),
                                    floor2(mouseX + 0.5), floor2(linkY), [6, 2]);

                                this.painter.drawDashedLineV(this.overlayctx,
                                    floor2(mouseX), floor2(Math.min(linkY, mouseY) - 0.5),
                                    floor2(mouseX), floor2(Math.max(linkY, mouseY) + 0.5), [6, 2]);
                            } else {
                                if (mouseY < this.linkLineEdit.posY) {

                                    xFrom = floor2(mouseX - 0.5);
                                    xTo = floor2(linkX + itemHf + 0.5);

                                    if (xFrom < xTo) {

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            xFrom, Math.min(floor2(mouseY), floor2(linkY - itemH)),
                                            xFrom, Math.max(floor2(mouseY), floor2(linkY - itemH)), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            xFrom, floor2(linkY - itemH),
                                            xTo, floor2(linkY - itemH), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            floor2(xTo - itemHf), floor2(linkY),
                                            xTo, floor2(linkY), [6, 2]);

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            xTo, floor2(linkY - itemH),
                                            xTo, floor2(linkY), [6, 2]);
                                    }

                                } else {

                                    xFrom = floor2(mouseX - 0.5);
                                    xTo = floor2(linkX + itemHf + 0.5);

                                    if (xFrom < xTo) {

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            xFrom, Math.min(floor2(mouseY), floor2(linkY + itemH)),
                                            xFrom, Math.max(floor2(mouseY), floor2(linkY + itemH)), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            xFrom, floor2(linkY + itemH),
                                            xTo, floor2(linkY + itemH), [6, 2]);

                                        this.painter.drawDashedLineH(this.overlayctx,
                                            floor2(xTo - itemHf), floor2(linkY),
                                            xTo, floor2(linkY), [6, 2]);

                                        this.painter.drawDashedLineV(this.overlayctx,
                                            xTo, floor2(linkY),
                                            xTo, floor2(linkY + itemH), [6, 2]);
                                    }
                                }
                            }
                        }

                        this.painter.addZone(Math.min(linkX, mouseX),
                            Math.min(linkY, mouseY),
                                Math.max(linkX, mouseX) - Math.min(linkX, mouseX),
                                Math.max(linkY, mouseY) - Math.min(linkY, mouseY), 30);
                    }

                    this.linkLineEdit.update = false;
                }
            }
        },

        // элементы захвата и пересечений

        calculateHit: function (e, cursor, ctr) {

            this.clickPosCentreing = null;

            this.downMouse = this.windowToCanvas(e.clientX, e.clientY);
            this.moveMouse = { x: this.downMouse.x, y: this.downMouse.y };

            var worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
            this.downMouse.x -= this.timeScale.hourInPixels * this.offsetX;

            var offY = this.timeScale.height() - this.rightScroll.value() + this.itemHeight * 0.5;
            var scaleUnitStep = this.timeScale.scaleUnitStep();

            var offX = this.timeScale.hourInPixels * this.offsetX;
            var scrollY = floor2(this.rightScroll.value());

            this.hitProject = -1;
            this.hitMilestone = -1;
            this.hitTask = -1;
            this.hitSide = 0;
            this.hitLink = undefined;
            this.capLink = undefined;
            this.milestoneChange = undefined;
            var taskBeginY = 0;
            var taskEnY = 0;
            var margin = this.itemMargin;
            var mousedx = this.worldToSceneX(this.moveMouse.x);
            var mouseaddy = this.downMouse.y - this.itemHeight;

            var offLinkY = this.itemMargin * 0.5;   //(!this.fullscreen) * this.itemHeight * 0.5;

            var linkBeginY = 0;
            var linkMouseY = 0;

            var timeTaskScreen = 0;
            var timeTaskScreenEnd = 0;

            var endFailX = this.ctxWidth * kTaskEndFailSetup.bigClampPct;
            var timeEps = this.sceneXToTime(kTaskCaptureEps);
            var pinEps = this.sceneXToTime(kTaskPinCaptureEps);

            var i, j, k, mi, ti, mc, mbegin, realTasksCount, x, sideEps = 0, lockProject = false;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {
                var t = p[j].t;

                lockProject = p[j]._status;

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    offY += margin;

                    for (mi = 0; mi < p[j].m.length; ++mi) {

                        // применен фильтр

                        if (p[j].m[mi].filter) { continue; }

                        var m = p[j].m[mi];
                        mc = p[j].m[mi].t.length;

                        if (!this.fullscreen && m.collapse) { offY += margin; continue; }

                        // подсчитываем реально видимые задачи с учетом фильтра

                        realTasksCount = mc;

                        for (x = mc - 1; x >= 0; --x) {
                            if (m.t[x].filter) { --realTasksCount; }
                        }

                        linkBeginY = offY + scrollY;
                        linkMouseY = this.downMouse.y + scrollY - this.timeScale.height() + offLinkY;

                        var milestoneY = offY;
                        var milestoneBeginY = offY + margin * 0.25;
                        var milestoneEndY = offY + realTasksCount * margin + this.itemHeight * 1.5;

                        var billet = milestoneY - this.itemHeight * 0.3 - 2 + margin * 0.5 - margin * 0.3;

                        if (this.fullscreen) {
                            milestoneEndY += this.itemHeight;
                            if (0 === mc) { milestoneEndY += margin; }
                        }

                        mbegin = offY + (margin - this.itemHeight) * 0.5 + this.itemHeight * 0.5;
                        offY += margin;
                        offY += margin * realTasksCount;

                        // элементы в вехе

                        taskBeginY = mbegin + margin - this.itemHeight;
                        taskEnY = mbegin + margin;

                        for (ti = 0; ti < mc; ++ti) {

                            t = m.t[ti];

                            // применен фильтр

                            if (t.filter) { continue; }

                            // находимся на 'кнопках-стрелках' для центрирование позиций элементов

                            if ((this.downMouse.y >= taskBeginY && this.downMouse.y <= taskEnY)) {

                                if ((this.moveMouse.x > this.visibleLeft && this.moveMouse.x < this.visibleLeft + 16)
                                    || (this.moveMouse.x > this.ctxWidth - 32 && this.moveMouse.x < this.ctxWidth - 16)) {

                                    timeTaskScreen = this.timeToSceneX(t.beginTime);
                                    timeTaskScreenEnd = this.timeToSceneX(t.endTime);

                                    if (timeTaskScreen < this.visibleLeft || (timeTaskScreen < this.visibleLeft && timeTaskScreenEnd > this.visibleLeft)) {
                                        if (cursor) this.overlay.style.cursor = 'pointer';
                                        this.clickPosCentreing = { p: j, m: mi, t: ti };

                                        this.offMenus();

                                        return;
                                    }

                                    if (timeTaskScreen > this.ctxWidth) {
                                        if (this.moveMouse.x > this.ctxWidth - 32 && this.moveMouse.x < this.ctxWidth - 16) {
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.clickPosCentreing = { p: j, m: mi, t: ti };

                                            this.offMenus();

                                            return;
                                        }
                                    }
                                }
                            }

                            sideEps = pinEps; if (t._status === kElementCompleted) sideEps = 0;

                            if ((mousedx >= t.beginTime - sideEps && mousedx <= t.endTime + sideEps) &&
                                (this.downMouse.y >= taskBeginY && this.downMouse.y <= taskEnY)) {

                                this.hitTask = ti;
                                this.hitMilestone = mi;
                                this.hitProject = j;

                                this.hitSide = 0;

                                this.backLightMilestone.up(-1, -1);

                                this.anchorMouse.x = t.beginTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;
                                this.anchorMouse.y = taskBeginY - this.downMouse.y;

                                if (cursor) this.overlay.style.cursor = 'pointer';

                                this.menuTask.offx = this.moveMouse.x;
                                this.menuTask.offy = taskBeginY;

                                if (this.readMode || kOpenProject !== lockProject) this.menuTask.reset();

                                if (t._status !== kElementCompleted) {

                                    if (kHitSidePixels > abs2(this.downMouse.x - t.beginTime * worldToScreen)) {
                                        if (cursor && !this.readMode && kOpenProject === lockProject) {
                                            if (this.storage.getTask(j, mi, ti)._status != kElementCompleted && !ctr)
                                                this.overlay.style.cursor = 'w-resize';
                                        }

                                        this.hitSide = kTaskSideLeft;
                                    }

                                    if (kHitSidePixels > abs2(this.downMouse.x - t.endTime * worldToScreen)) {
                                        if (cursor && !this.readMode && kOpenProject === lockProject) {
                                            if (this.storage.getTask(j, mi, ti)._status != kElementCompleted && !ctr)
                                                this.overlay.style.cursor = 'w-resize';
                                        }

                                        this.hitSide = kTaskSideRight;
                                        this.anchorMouse.x = t.endTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;
                                    }

                                    // зона появления 'квадрата связи'

                                    if (this.editMode !== kEditModeAddLink) {

                                        timeTaskScreen = this.timeToSceneX(t.beginTime);
                                        timeTaskScreenEnd = this.timeToSceneX(t.endTime);

                                        if (timeTaskScreen - this.moveMouse.x > 2 && timeTaskScreen - this.moveMouse.x < kTaskPinCaptureEps) {
                                            if (!this._mouseInLinkZone) {
                                                this._mouseInLinkZone = { side: kTaskSideLeft };
                                                this.needUpdate = true;
                                            }
                                            this.hitSide = kTaskSideNone;
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.offMenus();
                                        } else if (this.moveMouse.x - timeTaskScreenEnd > 2 && this.moveMouse.x - timeTaskScreenEnd < kTaskPinCaptureEps && !t.endFail) {
                                            if (!this._mouseInLinkZone) {
                                                this._mouseInLinkZone = { side: kTaskSideRight };
                                                this.needUpdate = true;
                                            }
                                            this.hitSide = kTaskSideNone;
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.offMenus();
                                        } else {
                                            if (this._mouseInLinkZone) {
                                                this._mouseInLinkZone = null;
                                                this.needUpdate = true;
                                            }
                                        }
                                    }
                                }

                                return;
                            }

                            taskBeginY += margin;
                            taskEnY += margin;
                        }

                        // боковая левая часть (изменяем дедлайн вехи)

                        if (Math.abs(this.downMouse.x - m.endTime * worldToScreen) < kHitSidePixels && (this.downMouse.y >= milestoneBeginY && milestoneEndY >= this.downMouse.y)) {
                            if (p[j].m[mi]._status == kElementCompleted - 1) return;

                            if (this.editBox.enable) {
                                if (this.editBox.taskRef)
                                    return;
                            }

                            if (cursor && !this.readMode && kOpenProject === lockProject) this.overlay.style.cursor = 'w-resize';

                            this.milestoneChange = true;

                            this.hitMilestone = mi;
                            this.hitProject = j;

                            this.anchorMouse.x = m.endTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;

                            this.menuTask.offx = this.moveMouse.x;
                            this.menuTask.offy = milestoneY;
                            this.menuMileStone.hide = true;
                            this.menuMileStone.reset();

                            this.backLightMilestone.up(j, mi);

                            if (this.readMode || kOpenProject !== lockProject) this.menuTask.reset();

                            return;
                        }

                        // веха (показываем всплывающюю менюшку или при клике показываем виджет с описанием)

                        if (kEditModeNoUse === this.editMode) {
                            if ((mousedx >= m.beginTime && mousedx <= m.endTime) && (this.downMouse.y >= billet && this.downMouse.y <= billet + margin * 0.5)) {

                                this.backLightMilestone.up(j, mi);

                                //if (cursor) this.overlay.style.cursor = 'pointer';

                                this.menuMileStone.set(j, mi, this.storage.getMilestone(j, mi));

                                this.menuMileStone.offx = this.moveMouse.x;
                                this.menuMileStone.offy = milestoneY + this.itemHeight - (this.milestoneLinewidth + this.menuArrowSize) * this.zoom;
                                this.menuMileStone.hide = false;
                                this.menuMileStone.disable = lockProject;

                                return;
                            }
                        }

                        // проверка связей идет только после того как мы прочекали все задачи

                        if (!lockProject && this.calculateLinksHit(p[j].m[mi].t, offX, linkBeginY, scrollY, 1, mousedx, linkMouseY, 0)) {
                            if (cursor) { this.overlay.style.cursor = 'pointer'; } return;
                        }

                        //

                        if (0 === mc && this.fullscreen) offY += margin;
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) { offY += margin; continue; }

                    // свободные задачи

                    if (p[j].t.length) {

                        if (this.fullscreen) { offY += this.itemHeight; }
                        if (!this.fullscreen) { offY += margin; }

                        if (!this.fullscreen && p[j].collapse) { continue; }

                        linkBeginY = offY + scrollY;
                        linkMouseY = this.downMouse.y + scrollY - this.timeScale.height() + margin + offLinkY;

                        // подсчитываем реально видимые задачив свободной зоне

                        mc = p[j].t.length;
                        realTasksCount = mc;

                        for (x = mc - 1; x >= 0; --x) {
                            if (p[j].t[x].filter) { --realTasksCount; }
                        }

                        // элементы в вехе

                        mbegin = offY + margin * 0.5 - this.itemHeight;

                        taskBeginY = mbegin;
                        taskEnY = mbegin + this.itemHeight;

                        for (k = 0; k < p[j].t.length; ++k) {
                            t = p[j].t[k];

                            // применен фильтр

                            if (t.filter) { continue; }

                            var endTime = t.endTime;
                            if (t.endFail) endTime += 10000000;  // NOTE !

                            if (t.endFail) {
                                timeTaskScreen = this.timeToSceneX(t.beginTime);
                                endFailX = this.ctxWidth - kTaskEndFailSetup.smallClampPx;

                                if (timeTaskScreen <= this.visibleLeft - 1) {
                                    endFailX = this.ctxWidth * kTaskEndFailSetup.bigClampPct;
                                }

                                if (this.moveMouse.x - endFailX > kHitSidePixels + 32) {    //   TODO: need fix
                                    if (cursor) this.overlay.style.cursor = '';

                                    taskBeginY += margin;
                                    taskEnY += margin;

                                    continue;

                                    // return;
                                }

                                endTime = this.worldToSceneX(endFailX);
                            }

                            // находимся на 'кнопках-стрелках' для центрирование позиций элементов

                            if ((this.downMouse.y >= taskBeginY && this.downMouse.y <= taskEnY)) {

                                if ((this.moveMouse.x > this.visibleLeft && this.moveMouse.x < this.visibleLeft + 16)
                                    || (this.moveMouse.x > this.ctxWidth - 32 && this.moveMouse.x < this.ctxWidth - 16)) {

                                    timeTaskScreen = this.timeToSceneX(t.beginTime);
                                    timeTaskScreenEnd = this.timeToSceneX(endTime);

                                    if (timeTaskScreen < this.visibleLeft || (timeTaskScreen < this.visibleLeft && timeTaskScreenEnd > this.visibleLeft)) {
                                        if (cursor) this.overlay.style.cursor = 'pointer';
                                        this.clickPosCentreing = { p: j, m: undefined, t: k };
                                        this.offMenus();

                                        return;
                                    }

                                    if (timeTaskScreen > this.ctxWidth) {
                                        if (this.moveMouse.x > this.ctxWidth - 32 && this.moveMouse.x < this.ctxWidth - 16) {
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.clickPosCentreing = { p: j, m: undefined, t: k };
                                            this.offMenus();

                                            return;
                                        }
                                    }
                                }
                            }

                            sideEps = pinEps; if (t._status === kElementCompleted) sideEps = 0;

                            if ((mousedx >= t.beginTime - sideEps && mousedx <= endTime + sideEps) &&
                                (this.downMouse.y >= taskBeginY && this.downMouse.y <= taskEnY)) {

                                this.hitTask = k;
                                this.hitMilestone = -1;
                                this.hitProject = j;

                                this.hitSide = 0;

                                this.backLightMilestone.up(-1, -1);

                                this.anchorMouse.x = t.beginTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;
                                this.anchorMouse.y = taskBeginY - this.downMouse.y;

                                this.overlay.style.cursor = 'pointer';

                                this.menuTask.offx = this.moveMouse.x;
                                this.menuTask.offy = taskBeginY;

                                if (this.readMode || kOpenProject !== lockProject) this.menuTask.reset();

                                if (t._status !== kElementCompleted) {

                                    if (abs2(this.downMouse.x - t.beginTime * worldToScreen) < kHitSidePixels) {
                                        if (cursor && !this.readMode && kOpenProject === lockProject) {
                                            if (this.storage.getTask(j, undefined, k)._status != kElementCompleted && !ctr)
                                                this.overlay.style.cursor = 'w-resize';
                                        }
                                        this.hitSide = kTaskSideLeft;
                                    }

                                    if (t.endFail) {

                                        if (abs2(this.moveMouse.x - endFailX) < kHitSidePixels) {
                                            if (cursor && !this.readMode && kOpenProject === lockProject) {
                                                if (this.storage.getTask(j, undefined, k)._status != kElementCompleted && !ctr)
                                                    this.overlay.style.cursor = 'w-resize';
                                            }
                                            this.hitSide = kTaskSideRight;
                                            this.anchorMouse.x = endTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;
                                        }
                                    } else {


                                        if (abs2(this.downMouse.x - endTime * worldToScreen) < kHitSidePixels) {
                                            if (cursor && !this.readMode && kOpenProject === lockProject) {
                                                if (this.storage.getTask(j, undefined, k)._status != kElementCompleted && !ctr)
                                                    this.overlay.style.cursor = 'w-resize';
                                            }
                                            this.hitSide = kTaskSideRight;
                                            this.anchorMouse.x = endTime - this.downMouse.x / (this.timeScale.hourInPixels * scaleUnitStep) * scaleUnitStep * this.timeScale.scaleX;
                                        }
                                    }

                                    // зона появления 'кружка связи'

                                    if (this.editMode !== kEditModeAddLink) {
                                        timeTaskScreen = this.timeToSceneX(t.beginTime);
                                        timeTaskScreenEnd = this.timeToSceneX(endTime);

                                        if (timeTaskScreen - this.moveMouse.x > 2 && timeTaskScreen - this.moveMouse.x < kTaskPinCaptureEps) {
                                            if (!this._mouseInLinkZone) {
                                                this._mouseInLinkZone = { side: kTaskSideLeft };
                                                this.needUpdate = true;
                                            }
                                            this.hitSide = kTaskSideNone;
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.offMenus();
                                        } else if (this.moveMouse.x - timeTaskScreenEnd > 2 && this.moveMouse.x - timeTaskScreenEnd < kTaskPinCaptureEps && !t.endFail) {
                                            if (!this._mouseInLinkZone) {
                                                this._mouseInLinkZone = { side: kTaskSideRight };
                                                this.needUpdate = true;
                                            }
                                            this.hitSide = kTaskSideNone;
                                            if (cursor) this.overlay.style.cursor = 'pointer';
                                            this.offMenus();
                                        } else {
                                            if (this._mouseInLinkZone) {
                                                this._mouseInLinkZone = null;
                                                this.needUpdate = true;
                                            }
                                        }
                                    }
                                }

                                return;
                            }

                            taskBeginY += margin;
                            taskEnY += margin;
                        }

                        // проверка связей идет только после того как мы прочекали все задачи

                        if (!lockProject && this.calculateLinksHit(p[j].t, offX, linkBeginY, scrollY, 1, mousedx, linkMouseY, margin)) {
                            if (cursor) { this.overlay.style.cursor = 'pointer'; } return;
                        }

                        offY += margin * realTasksCount;
                    } else {

                        // линия свободных задач

                        offY += margin;
                    }
                } else {

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            this.backLightMilestone.up(-1, -1);

            if (this.editMode !== kEditModeAddLink) {
                this._mouseInLinkZone = null;
            }

            if (cursor) this.overlay.style.cursor = '';
            if (this.readMode) { this.menuTask.reset(); this.menuMileStone.reset(); }
        },
        calculateLineHit: function (e, dx, dy) {

            var i, j, k, m, begin, end;

            if (undefined != e) {
                this.downMouse = this.windowToCanvas(e.clientX, e.clientY);
            } else {
                this.downMouse = { x: dx, y: dy };
            }

            this.moveMouse = { x: this.downMouse.x, y: this.downMouse.y };
            this.downMouse.x -= this.timeScale.hourInPixels * this.offsetX;

            var margin = this.itemMargin;
            var offY = -this.rightScroll.value();
            var mouseY = this.downMouse.y - margin * 2;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                // строка проекта

                if (offY <= mouseY && mouseY <= offY + margin) {

                    // если проект пуст, то индекс вехи (-1), иначе указываем на первую веху (0)

                    this.hitLine = { p: j, m: p[j].m.length ? 0 : -1, t: -1 };
                    return true;
                }

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) {

                    offY += margin;
                    continue;
                }

                if (!p[j].isEmpty()) {

                    offY += this.itemMargin;

                    for (m = 0; m < p[j].m.length; ++m) {

                        // применен фильтр

                        if (p[j].m[m].filter) { continue; }

                        if (offY <= mouseY && mouseY <= offY + margin) {
                            this.hitLine = { p: j, m: m, t: -1 };
                            return true;
                        }

                        offY += this.itemMargin;

                        if (p[j].m[m].collapse) {
                            continue;
                        }

                        for (i = 0; i < p[j].m[m].t.length; ++i) {

                            // применен фильтр

                            if (p[j].m[m].t[i].filter) { continue; }

                            if (offY <= mouseY && mouseY <= offY + margin) {
                                this.hitLine = { p: j, m: m, t: i - 1 };
                                return true;
                            }

                            offY += margin;
                        }
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) {

                        if (offY <= mouseY && mouseY <= offY + margin) {

                            // если проект пуст, то индекс вехи (-1), иначе указываем на первую веху (0)

                            this.hitLine = { p: j, m: -1, t: -1 };
                            return true;
                        }

                        offY += margin;
                        continue;
                    }

                    if (p[j].t.length) {

                        if (offY <= mouseY && mouseY <= offY + margin) {
                            this.hitLine = { p: j, m: -1, t: -1 };
                            return true;
                        }

                        offY += this.itemMargin;

                        for (i = 0; i < p[j].t.length; ++i) {

                            // применен фильтр

                            if (p[j].t[i].filter) { continue; }

                            if (offY <= mouseY && mouseY <= offY + margin) {
                                this.hitLine = { p: j, m: -1, t: i - 1 };
                                return true;
                            }

                            offY += margin;
                        }
                    } else {

                        if (offY <= mouseY && mouseY <= offY + margin) {

                            // если проект пуст, то индекс вехи (-1), иначе указываем на первую веху (0)

                            this.hitLine = { p: j, m: -1, t: -1 };
                            return true;
                        }

                        // линия свободных задач

                        offY += this.itemMargin;
                    }
                } else {

                    if (offY <= mouseY && mouseY <= offY + margin * 2) {

                        // если проект пуст, то индекс вехи (-1), иначе указываем на первую веху (0)

                        this.hitLine = { p: j, m: -1, t: -1 };
                        return true;
                    }

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += this.itemMargin * 2;
                }
            }

            if (offY < mouseY && pl > 0) {

                if (p.last().t.length) {
                    this.hitLine = { p: j - 1, m: -1, t: p.last().t.length - 1, bottomMilestoneInd: p[j - 1].m.length ? -1 : 0 };
                } else {
                    this.hitLine = { p: j - 1, m: -1, t: -1 };
                }

                return true;
            }

            return false;
        },
        calculateHitMilestone: function (dx, dy) {
            this.downMouse = { x: dx, y: dy };
            this.moveMouse = { x: this.downMouse.x, y: this.downMouse.y };
            this.downMouse.x -= this.timeScale.hourInPixels * this.offsetX;

            var offY = this.timeScale.height() - this.rightScroll.value() + this.itemHeight * 0.5;
            var mouseaddy = this.downMouse.y - this.itemMargin;

            var i, j, k, mi, ti, mbegin, mend, realTasksCount, margin = this.itemMargin;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    offY += margin;

                    for (mi = 0; mi < p[j].m.length; ++mi) {
                        if (0 === p[j].m[mi].t.length) {
                            offY += margin;
                        }

                        if (!this.fullscreen) {
                            if (!p[j].m[mi].collapse) {
                                offY += margin + p[j].m[mi].t.length * margin;
                            }
                        } else {
                            offY += margin + p[j].m[mi].t.length * margin;
                        }

                        if (offY >= dy) {
                            return { p: j, m: mi + 1 };
                        }
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) { offY += margin; continue; }

                    if (p[j].t.length) {

                        offY += margin;

                        for (i = p[j].t.length - 1; i >= 0; --i) {

                            // применен фильтр

                            if (p[j].t[i].filter) { continue; }

                            offY += margin;
                        }

                        if (offY >= dy) {
                            return { p: j, m: undefined };
                        }
                    } else {
                        // линия свободных задач

                        offY += margin;
                    }
                } else {

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            return { p: undefined, m: undefined };
        },
        calculateTaskHit: function () {
            if (-1 !== this.hitProject && -1 === this.hitMilestone && this.hitTask !== -1) {
                return this.storage.getTask(this.hitProject, undefined, this.hitTask);
            }

            if (-1 !== this.hitProject && -1 !== this.hitMilestone && this.hitTask !== -1) {
                return this.storage.getTask(this.hitProject, this.hitMilestone, this.hitTask);
            }

            return null;
        },
        calculateLinksHit: function (tasks, offX, offY, scrollY, worldToScreen, mousex, mousey, offWidget) {
            this.exist = false;
            this.linkDir = 0;
            // this.linkWidget.setBound({x: 0, y: 0, w: 0, h: 0});

            if (this.readMode) { return this.exist; }
            if (!tasks.length) { return false; }

            var yFrom, yTo, xFrom, xTo, taskFrom, begTime, linkId, linkType, linkDep;
            var linkFromCnY = 0, linkToCnY = 0;

            var margin = this.itemMargin;
            var itemHalfH = this.itemHeight * 0.5;

            var lineOff = this.sceneXToTime(kLineCaptureEps);
            var lineOffY = this.sceneXToTime(this.itemHeight);

            offY -= this.timeScale.marginY;

            for (var to = 0; to < tasks.length; ++to) {
                taskFrom = tasks[to];

                if (taskFrom.filter) { linkFromCnY++; }

                for (var linkInd = 0; linkInd < taskFrom.links.length; ++linkInd) {

                    var link = taskFrom.links[linkInd];
                    linkId = link.parentTaskId || link['parentTaskId'];          // minimizator fix

                    if (tasks[to]._id === linkId) { continue; }

                    linkType = link.linkType || link['linkType'];                  // minimizator fix
                    linkDep = link.dependenceTaskId || link['dependenceTaskId'];  // minimizator fix

                    linkToCnY = 0;

                    for (var cur = 0; cur < tasks.length; ++cur) {

                        if (tasks[cur].filter) { linkToCnY++; }

                        if (tasks[cur].filter && tasks[to].filter) { continue; }

                        if (tasks[cur]._id === linkId) {
                            if (kLinkBeginEnd === linkType) {

                                xFrom = tasks[cur].beginTime + tasks[cur].duration;
                                xTo = tasks[to].beginTime;

                                yFrom = offY + margin * (cur - linkToCnY);
                                yTo = offY + margin * (to - linkFromCnY);

                                if (yFrom < yTo) {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yFrom, xFrom + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom + lineOff * 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yTo, xTo, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yFrom, xFrom + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom + lineOff), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo, yTo - itemHalfH * 2, xFrom, yTo - itemHalfH * 2, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - itemHalfH * 2 - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                } else {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yTo, xFrom + lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom + lineOff / 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yTo, xTo, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xTo - lineOff, yTo, xTo - lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xTo), y: this.moveMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo - lineOff, yFrom - itemHalfH * 2, xFrom + lineOff, yFrom - itemHalfH * 2, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yFrom - itemHalfH * 2 - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].endTime) / 24), true);

                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                }
                            } else if (kLinkBeginBegin === linkType) {

                                xFrom = tasks[cur].beginTime;
                                xTo = tasks[to].beginTime;

                                yFrom = offY + margin * (cur - linkToCnY);
                                yTo = offY + margin * (to - linkFromCnY);

                                if (yFrom < yTo) {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom - lineOff, yFrom, xFrom - lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom), y: this.moveMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xFrom - lineOff, yTo, xTo, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xTo - lineOff, yFrom, xFrom, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yFrom - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo - lineOff, yFrom, xTo - lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xTo), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                } else {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom - lineOff, yTo, xFrom - lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom - lineOff), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xFrom - lineOff, yTo, xTo, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xTo - lineOff, yTo, xTo - lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xTo), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo + lineOff, yFrom, xFrom, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yFrom - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.beginTime - tasks[cur].beginTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                }
                            } else if (kLinkEndEnd === linkType) {

                                xFrom = tasks[cur].beginTime + tasks[cur].duration;
                                xTo = tasks[to].beginTime + tasks[to].duration;

                                yFrom = offY + margin * (cur - linkToCnY);
                                yTo = offY + margin * (to - linkFromCnY);

                                if (yFrom < yTo) {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yFrom, xTo + lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yFrom - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo + lineOff, yFrom, xTo + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xTo + lineOff * 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yFrom, xFrom + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom + lineOff * 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo, yTo, xFrom + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                } else {
                                    if (xFrom < xTo) {
                                        if (pointInRectangle(mousex, mousey, xFrom, yFrom, xTo + lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yFrom - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo + lineOff, yTo, xTo + lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xTo + lineOff * 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    } else {
                                        if (pointInRectangle(mousex, mousey, xFrom + lineOff, yTo, xFrom + lineOff, yFrom, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.timeToSceneX(xFrom + lineOff * 2), y: this.downMouse.y, w: 100, h: 100 },
                                                0, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                        if (pointInRectangle(mousex, mousey, xTo, yTo, xFrom + lineOff, yTo, kLineCaptureEps)) {
                                            if (this.capLink && this.capLink.dependenceTaskId === linkDep && this.capLink.parentTaskId == linkId) {
                                                this.exist = true;
                                                continue;
                                            }

                                            this.linkWidget.setBound({ x: this.moveMouse.x, y: yTo - scrollY + this.itemMargin - offWidget, w: 100, h: 100 },
                                                1, Math.floor((taskFrom.endTime - tasks[cur].endTime) / 24));
                                            this.linkWidget.setLink(link);

                                            this.hitLink = link;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return this.exist;
        },

        // расчеты

        getTheMilestoneTaskToDrag: function (e) {

            // Получаем веху куда над которой находится перемещаемая задача (пересечение двух баунд боксов, задачи и всех вех)

            this.dragTo = { t: -1, m: -1, p: -1 };

            var offY = this.timeScale.height() - this.rightScroll.value();

            var bound = { x: 0, y: 0, width: 0, height: 0, dx: 0, dy: 0 };

            this.dragTo.m = -1;
            this.dragTo.p = -1;

            bound.x = this.worldToSceneX(this.dragBound.x + this.dragBound.dx);
            bound.dx = bound.x + this.dragBound.time;

            bound.y = this.dragBound.y + this.dragBound.dy;
            bound.dy = this.dragBound.y + this.dragBound.height;

            var centerBoundY = (bound.y + bound.dy) * 0.5;

            var i, j, k, x, find = false, mind = -1;
            var tasksCount, realTasksCount;

            var margin = this.itemMargin;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                if (offY < centerBoundY && centerBoundY < offY + margin) {
                    this.dragTo.p = j;
                    return true;
                }

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    offY += margin;

                    for (i = 0; i < p[j].m.length; ++i) {

                        // применен фильтр

                        if (p[j].m[i].filter) { continue; }

                        tasksCount = p[j].m[i].t.length;

                        // подсчитываем реально видимые задачи с учетом фильтра

                        realTasksCount = tasksCount;

                        for (x = tasksCount - 1; x >= 0; --x) {
                            if (p[j].m[i].t[x].filter) { --realTasksCount; }
                        }

                        var beginY = offY + this.itemHeight;
                        var endY = beginY + margin * (realTasksCount + 0.5);

                        tasksCount = realTasksCount;

                        if (!this.fullscreen) {
                            if (!p[j].m[i].collapse) {
                                endY = beginY + margin * (tasksCount + 0.5);
                            } else {
                                endY = beginY + margin * 0.5;
                                tasksCount = 0;
                            }
                        } else {
                            endY = beginY + margin * (tasksCount + 0.5);
                        }

                        if (0 === tasksCount && this.fullscreen) { offY += margin; endY += margin; }

                        if ((beginY <= bound.y && bound.y <= endY) || (beginY <= bound.dy && bound.dy <= endY) || (beginY <= bound.y && bound.dy <= endY)) {

                            var timeMX = p[j].m[i].beginTime;
                            var timeEX = p[j].m[i].endTime;

                            // if ((timeMX <= bound.x && bound.x <= timeEX) ||
                            //     (timeMX <= bound.dx && bound.x <= timeEX) ||
                            //     (timeMX <= bound.x && bound.dx <= timeEX) ||
                            //     (timeMX >= bound.x && bound.dx >= timeEX))
                            // {
                            this.dragTo.m = i;
                            this.dragTo.p = j;

                            return true;
                            // }
                        }

                        if (!p[j].m[i].collapse) {
                            offY += margin * (tasksCount + 1);
                        } else {
                            offY += margin;
                        }
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) {

                        if (offY < centerBoundY && centerBoundY < offY + margin) {
                            this.dragTo.p = j;
                            return true;
                        }

                        offY += margin;
                        continue;
                    }

                    if (p[j].t.length) {

                        if (offY < centerBoundY && centerBoundY < offY + margin) {
                            this.dragTo.p = j;
                            return true;
                        }

                        offY += margin;

                        for (i = p[j].t.length - 1; i >= 0; --i) {

                            if (p[j].t[i].filter) { continue; }

                            if (offY < centerBoundY && centerBoundY < offY + margin) {
                                this.dragTo.p = j;
                                return true;
                            }

                            offY += margin;
                        }
                    } else {

                        if (offY < centerBoundY && centerBoundY < offY + margin) {
                            this.dragTo.p = j;
                            return true;
                        }

                        // линия свободных задач

                        offY += margin;
                    }
                } else {

                    if (offY < centerBoundY && centerBoundY < offY + margin * 2) {
                        this.dragTo.p = j;
                        return true;
                    }

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            if (offY < bound.y && pl > 0) {
                if (pl === 1)
                    this.dragTo.p = 0;
                else
                    this.dragTo.p = pl - 1;
                return true;
            }

            return false;
        },
        getContentHeight: function () {
            var offY = 0, i, j, x, mc, margin = this.itemMargin;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    // линия проектов

                    offY += margin;

                    // линии вех

                    for (i = 0; i < p[j].m.length; ++i) {

                        if (p[j].m[i].filter) { continue; }

                        offY += margin;

                        if (p[j].m[i].collapse) { continue; }

                        for (x = p[j].m[i].t.length - 1; x >= 0; --x) {
                            if (p[j].m[i].t[x].filter) { continue; }

                            offY += margin;
                        }
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) { offY += margin; continue; }

                    // линии свободных задач в проекте

                    if (p[j].t.length) {

                        for (i = p[j].t.length - 1; i >= 0; --i) {

                            if (p[j].t[i].filter) { continue; }

                            offY += margin;
                        }
                    }

                    // линия свободных задач

                    offY += margin;

                } else {

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            return offY;
        },
        getElementPosVertical: function (pc, mc, tc) {

            var margin = this.itemMargin;

            var i, j, s, x, tasksCount, realTasksCount, offY = margin * 2;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    offY += margin;

                    for (i = 0; i < p[j].m.length; ++i) {

                        // применен фильтр

                        if (p[j].m[i].filter) { continue; }

                        tasksCount = p[j].m[i].t.length;

                        // подсчитываем реально видимые задачи с учетом фильтра

                        realTasksCount = tasksCount;

                        for (x = tasksCount - 1; x >= 0; --x) {
                            if (p[j].m[i].t[x].filter) { --realTasksCount; }
                        }

                        if (pc === j && i === mc && -1 === tc) { return offY + margin * 0.5 + 2; }
                        if (p[j].m[i].collapse) { offY += margin; continue; }

                        offY += margin;

                        for (s = 0; s < p[j].m[i].t.length; ++s) {

                            // применен фильтр

                            if (p[j].m[i].t[s].filter) { continue; }

                            if (s === tc && pc === j && i === mc)
                                return offY;

                            offY += margin;
                        }
                    }

                    // свободные задачи в проекте скрыты

                    if (p[j].collapse) { offY += margin; continue; }

                    offY += margin;

                    if (-1 === mc) {

                        for (i = 0; i < p[j].t.length; ++i) {

                            // применен фильтр

                            if (p[j].t[i].filter) { continue; }

                            if (i === tc && j === pc) {
                                return offY;
                            }

                            offY += margin;
                        }

                    } else {

                        for (i = 0; i < p[j].t.length; ++i) {

                            // применен фильтр

                            if (p[j].t[i].filter) { continue; }

                            offY += margin;
                        }
                    }
                } else {

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            return offY;
        },
        getLeftMostElement: function () {

            // получаем самый крайний левый элемент слева

            var i, j, s;

            var element = { height: null, x: null };
            var pos = 0;

            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                pos += this.itemMargin;

                if (!p[j].isEmpty()) {

                    if (j === 0 && !this.fullscreen) { pos += this.itemMargin; }
                    pos += this.itemMargin;

                    for (i = 0; i < p[j].m.length; ++i) {

                        // применен фильтр

                        if (p[j].m[i].filter) { continue; }

                        pos += this.itemMargin;

                        for (s = 0; s < p[j].m[i].t.length; ++s) {

                            // применен фильтр

                            if (p[j].m[i].t[s].filter) { continue; }

                            if (!element.x || p[j].m[i].t[s].beginTime < element.x) {
                                element.x = p[j].m[i].t[s].beginTime;
                            }

                            pos += this.itemMargin;
                        }

                        if (0 === p[j].m[i].t.length) {
                            if (!element.x || p[j].m[i].beginTime < element.x) {
                                element.x = p[j].m[i].beginTime;
                            }

                            pos += this.itemMargin;
                        }
                    }

                    for (s = 0; s < p[j].t.length; ++s) {

                        // применен фильтр

                        if (p[j].t[s].filter) { continue; }

                        if (!element.x || p[j].t[s].beginTime < element.x) {
                            element.x = p[j].t[s].beginTime;
                        }

                        pos += this.itemMargin
                    }

                    pos += this.itemHeight;
                }
            }

            element.height = pos + kEps;

            return element;
        },
        getTopElementInVisibleRange: function () {

            // получаем самый первый элемент в области видимости по вертикали

            var duration = (this.zoomBar.rightDate.getTime() - this.zoomBar.leftDate.getTime()) / 3600000;      //  hours
            var from = (this.zoomBar.thumb.begin - this.zoomBar.fraction) * duration;
            var to = (this.zoomBar.thumb.end - this.zoomBar.fraction) * duration;

            var i, j, s, task, milestone, offY = 0;

            var margin = this.itemMargin;
            var p = this.storage.projects();
            var pl = p.length;

            for (j = 0; j < pl; ++j) {

                // весь контент у проекты скрыт

                if (p[j].fullCollapse) { offY += margin; continue; }

                if (!p[j].isEmpty()) {

                    offY += margin;

                    for (i = 0; i < p[j].m.length; ++i) {

                        // применен фильтр

                        if (p[j].m[i].filter) { continue; }

                        offY += margin;

                        for (s = 0; s < p[j].m[i].t.length; ++s) {

                            task = p[j].m[i].t[s];

                            // применен фильтр

                            if (task.filter) { continue; }

                            if ((task.beginTime >= from && task.beginTime <= to) ||
                                (task.endTime >= from && task.endTime <= to) ||
                                (task.beginTime >= from && task.endTime <= to) ||
                                (task.beginTime <= from && task.endTime >= to)) {

                                if (s === 0 && i === 0 && j === 0) return { height: kEps, empty: false };
                                if (s === 0) return { height: offY - margin, empty: false };

                                return { height: offY, empty: false };
                            }

                            offY += margin;
                        }

                        if (0 === p[j].m[i].t.length) {

                            if ((p[j].m[i].beginTime >= from && p[j].m[i].beginTime <= to) ||
                                (p[j].m[i].endTime >= from && p[j].m[i].endTime <= to) ||
                                (p[j].m[i].beginTime >= from && p[j].m[i].endTime <= to) ||
                                (p[j].m[i].beginTime <= from && p[j].m[i].endTime >= to)) {

                                if (s === 0 && i === 0 && j === 0) return { height: kEps, empty: false };
                                if (s === 0) return { height: offY - margin, empty: false };

                                return { height: offY, empty: false };
                            }

                            offY += margin;
                        }
                    }

                    for (s = 0; s < p[j].t.length; ++s) {

                        task = p[j].t[s];

                        // применен фильтр

                        if (task.filter) { continue; }

                        if (task.endFail) {
                            if (task.beginTime <= from || task.beginTime >= from && task.beginTime <= to) {
                                if (s === 0 && p[j].m.length === 0 && j === 0)
                                    return { height: kEps, empty: false };

                                return { height: offY, empty: false };
                            }
                        }

                        if ((task.beginTime >= from && task.beginTime <= to) ||
                            (task.endTime >= from && task.endTime <= to) ||
                            (task.beginTime >= from && task.endTime <= to) ||
                            (task.beginTime <= from && task.endTime >= to)) {

                            if (s === 0 && p[j].m.length === 0 && j === 0)
                                return { height: kEps, empty: false };

                            return { height: offY, empty: false };
                        }

                        offY += margin;
                    }

                    offY += this.itemHeight;
                } else {

                    // пустой проект имеет только две линии - сам проект и свободные задачи

                    offY += margin * 2;
                }
            }

            return { height: kEps, empty: true };
        },

        moveElement: function (e, line) {
            if (this.readMode)
                return;

            this.getTheMilestoneTaskToDrag(e);

            if (line) {
                this.dragTo = line;
            }

            var ids = null;
            var toids = null;

            var projectId = -1;
            var milestoneId = -1;
            var taskId = -1;
            var taskDrag, milestoneRef, projectRef, indexRemove, changePosition = false, elementLine = -1, elementPlace = -1, elementRemove = -1;
            var links = null;

            // TODO : перенос можем делать в рамках одного проекта

            if (this.itemToDrop.p !== this.dragTo.p) {
                this.dragTo = { t: -1, m: -1, p: -1 };
                this.needUpdate = true;

                this.editMode = kEditModeNoUse;
                return;
            }

            if (-1 !== this.dragTo.m && -1 !== this.dragTo.p) {
                if (-1 !== this.itemToDrop.t && -1 !== this.itemToDrop.p) {

                    this.calculateLineHit(e);

                    // from free zone

                    if (-1 === this.itemToDrop.m) {

                        taskDrag = this.storage.getTask(this.itemToDrop.p, undefined, this.itemToDrop.t);
                        milestoneRef = this.storage.getMilestone(this.dragTo.p, this.dragTo.m);

                        ids = this.storage.taskIds(this.itemToDrop.p, undefined, this.itemToDrop.t);
                        toids = this.storage.milestoneIds(this.dragTo.p, this.dragTo.m);

                        links = this.modelController().collectLinks(this.itemToDrop.p, undefined, ids.t);

                        if (kElementCompleted - 1 !== milestoneRef.status()) {

                            // есть связи и перемещение между разными вехами

                            if (links.length) {

                                // задача имеет связи

                                this.modelController().addTaskOperation(kHandlerBeforeMoveTaskWithLinks,
                                    {
                                        p: this.itemToDrop.p, m: undefined, t: taskDrag, index: this.itemToDrop.t,
                                        place: this.hitLine.t + 1,
                                        fromProject: this.itemToDrop.p, toProject: this.dragTo.p,
                                        fromMilestone: undefined, toMilestone: this.dragTo.m,
                                        taskId: ids.t, milestoneId: undefined, projectId: ids.p,
                                        projectToId: toids.p, milestoneToId: toids.m,
                                        linksToRemove: links, type: 'FtoM'  //  from free zone to milestone
                                    });

                            } else {

                                // если связей нету, то напрямую делаем перенос задачи

                                if (taskDrag.endFail && taskDrag.beginTime >= milestoneRef.endTime) {
                                    taskDrag.updateBegin(milestoneRef.endTime - 24);
                                }

                                this.storage.p[this.dragTo.p].m[this.dragTo.m].addTaskWithIndex(taskDrag, this.hitLine.t + 1);
                                this.storage.p[this.itemToDrop.p].removeTask(this.itemToDrop.t);
                                milestoneRef.updateTimes();

                                this.offMenus();

                                // undo

                                this._undoManager.add(kOperationMoveTask,
                                    {
                                        p: this.itemToDrop.p, m: undefined, t: taskDrag, index: this.itemToDrop.t,
                                        place: this.hitLine.t + 1,
                                        fromProject: this.itemToDrop.p, toProject: this.dragTo.p,
                                        fromMilestone: undefined, toMilestone: this.dragTo.m,
                                        taskId: ids.t, milestoneId: undefined, projectId: ids.p,
                                        projectToId: toids.p, milestoneToId: toids.m
                                    });

                                if (taskDrag.endFail && taskDrag.beginTime >= milestoneRef.endTime) {
                                    taskDrag.updateBegin(milestoneRef.endTime - 24);
                                }

                                this._undoManager.performTop();

                                if (milestoneRef.collapse) {
                                    milestoneRef.setCollapse(false);
                                }
                            }
                        }
                    }

                    // перемещаем задачу из вехи в веху

                    if (-1 !== this.itemToDrop.m && -1 !== this.dragTo.m) {

                        // переносим только в открытую веху

                        if (kElementCompleted - 1 !== this.storage.getMilestone(this.dragTo.p, this.dragTo.m).status()) {

                            indexRemove = this.itemToDrop.t;
                            elementLine = this.hitLine.t + 1;
                            elementPlace = elementLine;

                            // была попытка переместить задачу на место самой себя

                            if (this.hitLine.t + 1 === indexRemove && this.dragTo.m === this.itemToDrop.m) {
                                this.dragTo = { t: -1, m: -1, p: -1 };
                                this.needUpdate = true;

                                this.editMode = kEditModeNoUse;

                                return;
                            }

                            // переместили элемент в ту же веху

                            if (this.dragTo.m === this.itemToDrop.m) {
                                changePosition = true;

                                if (elementLine < indexRemove) {
                                    ++indexRemove;
                                } else {
                                    elementPlace++;
                                }
                            }

                            taskDrag = this.storage.getTask(this.itemToDrop.p, this.itemToDrop.m, this.itemToDrop.t);
                            milestoneRef = this.storage.getMilestone(this.dragTo.p, this.dragTo.m);

                            ids = this.storage.taskIds(this.itemToDrop.p, this.itemToDrop.m, this.itemToDrop.t);
                            toids = this.storage.milestoneIds(this.dragTo.p, this.dragTo.m);

                            links = this.modelController().collectLinks(this.itemToDrop.p, this.itemToDrop.m, ids.t);

                            // есть связи и перемещение между разными вехами

                            if (links.length && !changePosition) {

                                this.offMenus();

                                // задача имеет связи

                                this.modelController().addTaskOperation(kHandlerBeforeMoveTaskWithLinks,
                                    {
                                        p: this.itemToDrop.p, m: undefined, t: taskDrag, index: this.itemToDrop.t,
                                        place: this.hitLine + 1,
                                        fromProject: this.itemToDrop.p, toProject: this.dragTo.p,
                                        fromMilestone: this.itemToDrop.m, toMilestone: this.dragTo.m,
                                        taskId: ids.t, milestoneId: ids.m, projectId: ids.p,
                                        projectToId: toids.p, milestoneToId: toids.m,
                                        linksToRemove: links, type: 'MtoM'  //  from milestone to milestone
                                    });

                            } else {

                                // если связей нету, то напрямую делаем перенос задачи

                                milestoneRef.addTaskWithIndex(taskDrag, elementPlace);
                                this.storage.p[this.itemToDrop.p].m[this.itemToDrop.m].removeTask(indexRemove);
                                this.storage.p[this.itemToDrop.p].m[this.itemToDrop.m].updateTimes();
                                milestoneRef.updateTimes();

                                this.offMenus();

                                // undo

                                this._undoManager.add(kOperationMoveTask,
                                    {
                                        p: this.itemToDrop.p, m: undefined, t: taskDrag, index: this.itemToDrop.t,
                                        place: elementLine,
                                        fromProject: this.itemToDrop.p, toProject: this.dragTo.p,
                                        fromMilestone: this.itemToDrop.m, toMilestone: this.dragTo.m,
                                        taskId: ids.t, milestoneId: ids.m, projectId: ids.p,
                                        projectToId: toids.p, milestoneToId: toids.m
                                    });

                                this._undoManager.performTop();

                                if (milestoneRef.collapse) {
                                    milestoneRef.setCollapse(false);
                                }
                            }
                        }
                    }
                }
            } else {

                this.calculateLineHit(e);

                // task move to free project zone with scroller

                taskDrag = this.storage.getTask(this.itemToDrop.p, -1 === this.itemToDrop.m ? undefined : this.itemToDrop.m, this.itemToDrop.t);
                ids = this.storage.taskIds(this.itemToDrop.p, -1 === this.itemToDrop.m ? undefined : this.itemToDrop.m, this.itemToDrop.t);
                projectRef = this.storage.projects()[this.itemToDrop.p];

                links = this.modelController().collectLinks(this.itemToDrop.p, -1 === this.itemToDrop.m ? undefined : this.itemToDrop.m, ids.t);

                indexRemove = this.itemToDrop.t;
                elementLine = this.hitLine.t + 1;
                elementPlace = elementLine;

                // переместили элемент в ту же веху

                if (this.dragTo.m === this.itemToDrop.m) {

                    // была попытка переместить задачу на место самой себя

                    if (this.hitLine.t + 1 === indexRemove && this.dragTo.m === this.itemToDrop.m) {
                        this.dragTo = { t: -1, m: -1, p: -1 };
                        this.needUpdate = true;

                        this.editMode = kEditModeNoUse;

                        return;
                    }

                    elementLine = this.hitLine.t + 1;
                    elementPlace = elementLine;
                    elementRemove = indexRemove;

                    changePosition = true;

                    if (elementLine < indexRemove) {
                        ++elementRemove;
                    } else {
                        ++elementLine;
                    }
                }

                // есть связи и перемещение между разными вехами

                if (links.length && !changePosition) {

                    this.offMenus();

                    // задача имеет связи

                    this.modelController().addTaskOperation(kHandlerBeforeMoveTaskWithLinks,
                        {
                            p: this.itemToDrop.p, m: this.itemToDrop.m, t: taskDrag, index: this.itemToDrop.t,
                            fromProject: this.itemToDrop.p, toProject: this.itemToDrop.p,
                            fromMilestone: this.itemToDrop.m, toMilestone: undefined,
                            taskId: ids.t, milestoneId: ids.m, projectId: ids.p,
                            projectToId: projectRef._id, milestoneToId: undefined,
                            linksToRemove: links, type: 'MtoF'  //  from milestone to free zone
                        });

                } else {

                    if (changePosition) {

                        this.storage.p[this.itemToDrop.p].addTaskWithIndex(taskDrag, elementLine);
                        this.storage.p[this.itemToDrop.p].removeTask(elementRemove);

                        this.offMenus();

                        // undo

                        this._undoManager.add(kOperationMoveTask,
                            {
                                p: this.itemToDrop.p, m: this.itemToDrop.m, t: taskDrag, index: indexRemove,
                                place: elementPlace,
                                fromProject: this.itemToDrop.p, toProject: this.itemToDrop.p,
                                fromMilestone: undefined, toMilestone: undefined,
                                taskId: ids.t, milestoneId: ids.m, projectId: ids.p,
                                projectToId: projectRef._id, milestoneToId: undefined
                            });

                        this._undoManager.performTop();

                    } else {

                        // если связей нету, то напрямую делаем перенос задачи

                        // taskDrag.milestone  =   -1;
                        // var addInd          =   this.storage.p[this.itemToDrop.p].addTask(taskDrag);

                        // this.storage.p[this.itemToDrop.p].m[this.itemToDrop.m].removeTask(this.itemToDrop.t);

                        // var scrollY         =   this.getElementPosVertical (0, -1, addInd);
                        // this.animator.moveToY(Math.min(Math.max(0, scrollY - this.ctxHeight * 0.5), this.rightScroll._maxValue - this.rightScroll.viewWidth));

                        taskDrag = this.storage.getTask(this.itemToDrop.p, this.itemToDrop.m, this.itemToDrop.t);
                        this.storage.p[this.itemToDrop.p].addTaskWithIndex(taskDrag, elementPlace);
                        this.storage.p[this.itemToDrop.p].m[this.itemToDrop.m].removeTask(this.itemToDrop.t);

                        this.offMenus();

                        // undo

                        this._undoManager.add(kOperationMoveTask,
                            {
                                p: this.itemToDrop.p, m: this.itemToDrop.m, t: taskDrag, index: this.itemToDrop.t,
                                place: elementPlace, // addInd - 1,
                                fromProject: this.itemToDrop.p, toProject: this.itemToDrop.p,
                                fromMilestone: this.itemToDrop.m, toMilestone: undefined,
                                taskId: ids.t, milestoneId: ids.m, projectId: ids.p,
                                projectToId: projectRef._id, milestoneToId: undefined
                            });

                        this._undoManager.performTop();
                    }
                }
            }

            this.dragTo = { t: -1, m: -1, p: -1 };
            this.needUpdate = true;

            this.editMode = kEditModeNoUse;
        },
        editElementTitle: function (p, m, t) {
            if (this.readMode)
                return;

            var projectId = -1;
            var milestoneId = -1;
            var taskId = -1;
            var taskRef = null;

            // task in project

            if (-1 !== p && -1 === m && t !== -1) {
                projectId = this.storage.p[p]._id;
                taskId = this.storage.p[p].t[t]._id;

                taskRef = this.storage.getTask(p, undefined, t);
                if (taskRef) {
                    if (kElementCompleted == taskRef._status)
                        return;

                    taskRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.taskRef = taskRef;
                    this.editBox.p = p;
                    this.editBox.m = -1;
                    this.editBox.t = t;
                    this.editBox.text = taskRef._title;
                    this.editBox.saveText = taskRef._title;

                    this.editBox.setEnable();

                    if (this.offsetX * this.timeScale.scaleX + taskRef.beginTime < this.sceneXToTime(this.visibleLeft)) {
                        this.animator.moveCenterToX(taskRef.beginTime);
                    } else if (this.sceneToWorldX(taskRef.beginTime) + taskRef.titleWidth > this.ctxWidth) {
                        this.animator.moveCenterToX(taskRef.beginTime);
                    }

                    this._undoManager.add(kOperationChangeTitleTask,
                        {
                            p: p, m: undefined, t: taskRef, index: t,
                            taskId: taskId, milestoneId: undefined, projectId: projectId,
                            silentUpdateMode: true
                        });
                }
            }

            // task in milestone

            if (-1 !== p && -1 !== m && t !== -1) {
                projectId = this.storage.p[p]._id;
                milestoneId = this.storage.p[p].m[m]._id;
                taskId = this.storage.p[p].m[m].t[t]._id;

                taskRef = this.storage.getTask(p, m, t);
                if (taskRef) {
                    if (kElementCompleted == taskRef._status)
                        return;

                    taskRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.taskRef = taskRef;
                    this.editBox.p = p;
                    this.editBox.m = m;
                    this.editBox.t = t;
                    this.editBox.text = taskRef._title;
                    this.editBox.saveText = taskRef._title;

                    this.editBox.setEnable();

                    if (this.offsetX * this.timeScale.scaleX + taskRef.beginTime < this.sceneXToTime(this.visibleLeft)) {
                        this.animator.moveCenterToX(taskRef.beginTime);
                    } else if (this.sceneToWorldX(taskRef.beginTime) + taskRef.titleWidth > this.ctxWidth) {
                        this.animator.moveCenterToX(taskRef.beginTime);
                    }

                    this._undoManager.add(kOperationChangeTitleTask,
                        {
                            p: p, m: m, t: taskRef, index: t,
                            taskId: taskId, milestoneId: milestoneId, projectId: projectId,
                            silentUpdateMode: true
                        });
                }
            }

            // milestone

            if (-1 !== p && -1 !== m && t === -1) {
                projectId = this.storage.p[p]._id;
                milestoneId = this.storage.p[p].m[m]._id;

                var milestoneRef = this.storage.getMilestone(p, m);
                if (milestoneRef) {

                    milestoneRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.milestoneRef = milestoneRef;
                    this.editBox.p = p;
                    this.editBox.m = m;
                    this.editBox.t = -1;
                    this.editBox.text = milestoneRef._title;
                    this.editBox.saveText = milestoneRef._title;

                    this.editBox.setEnable();

                    this._undoManager.add(kOperationChangeTitleMilestone,
                        {
                            p: p, m: m, t: milestoneRef, index: undefined,
                            taskId: undefined, milestoneId: milestoneId, projectId: projectId,
                            silentUpdateMode: true
                        });

                    if (this.sceneToWorldX(milestoneRef.endTime) - milestoneRef.titleWidth < this.sceneXToTime(this.visibleLeft) ||
                        this.sceneToWorldX(milestoneRef.endTime) > this.ctxWidth) {
                        this.animator.moveCenterToX(milestoneRef.endTime);
                    }
                }
            }
        },

        updateScrollContent: function () {
            if (this.needUpdateScrollContent) {

                var t = this;

                t.needUpdateScrollContent = false;

                var endH = 0;
                var viewHeight = t.ctxHeight - t.timeScale.height() + 2;
                var scrollTo = Math.max(t.contentHeight - viewHeight, kEps);
                var regionY = Math.min(t.contentHeight, t.rightScroll.value() + viewHeight);

                if (t.taskDescWidget.isValid()) { endH = t.widgetY + t.taskDescWidget.bound.h; }
                if (t.milestoneDescWidget.isValid()) { endH = t.widgetY + t.milestoneDescWidget.bound.h; }

                if (viewHeight > t.contentHeight && viewHeight > endH && t.rightScroll.offDown === 0) {

                    // по вертикали контент оказался меньше чем видимая область, возможно нужно отскролить выпдадашку вправо

                    if (t.taskDescWidget.isValid()) {
                        if (t.widgetX + t.taskDescWidget.bound.w > t.ctxWidth)
                            t.animator.addMovementX(-(t.ctxWidth - (t.widgetX + t.taskDescWidget.bound.w) - 35) / t.timeScale.hourInPixels);
                    }

                    return;
                }

                t.offMenus();

                if (endH > regionY) {
                    var addOff = endH - regionY;
                    var contentH = regionY + addOff;
                    scrollTo = Math.max(contentH - viewHeight, kEps);

                    if (t.rightScroll.offDown === addOff && addOff === 0) {
                        t.needUpdateScrollContent = false;
                        return;
                    }

                    if (endH > t.contentHeight) {

                        t.rightScroll.setOffDown(addOff);
                        t.rightScroll.init(t.timeScale.height() + 2, viewHeight, contentH, t.ctxWidth);
                        t.offMenus();
                    } else {
                        scrollTo = floor2(scrollTo / t.itemMargin) * t.itemMargin - kEps;
                    }

                    t.animator.moveToY(scrollTo, function () {

                    });

                } else {
                    if (t.rightScroll.offDown) {
                        if (t.rightScroll.value() < scrollTo) {
                            t.rightScroll.save();
                            t.rightScroll.setOffDown(0);
                            t.rightScroll.init(t.timeScale.height() + 2, viewHeight, t.contentHeight, t.ctxWidth);
                        } else {
                            t.animator.moveToY(scrollTo, function () {
                                t.rightScroll.save();
                                t.rightScroll.setOffDown(0);
                                t.rightScroll.init(t.timeScale.height() + 2, viewHeight, t.contentHeight, t.ctxWidth);
                            });
                        }
                    }
                }

                if (t.taskDescWidget.isValid()) {
                    if (t.widgetX + t.taskDescWidget.bound.w > t.ctxWidth)
                        t.animator.addMovementX(-(t.ctxWidth - (t.widgetX + t.taskDescWidget.bound.w) - 35) / t.timeScale.hourInPixels);
                }
            }
        },

        // добавление задачи или вехи (+ctr) через клик мышью на диаграмме

        createTask: function (p, m, ind, placeholder) {
            if (this.editBox.enable || this.readMode)
                return;

            var projectId = -1;
            var milestoneId = -1;
            var taskId = -1;

            var taskRef, clickX, unitStep, beginTime, scrollY;

            // in free project zone

            if (-1 === m) {

                this.queryMoveLinks = null;

                projectId = this.storage.p[p]._id;
                taskId = this.storage.genRefId();

                this.storage.addTaskWithIndex(taskWithIds(projectId, undefined, taskId), ind);

                taskRef = this.storage.getTask(p, undefined, ind);

                clickX = this.moveMouse.x;
                unitStep = this.timeScale.scaleUnitStep();
                beginTime = (clickX - this.timeScale.hourInPixels * this.offsetX) / (this.timeScale.hourInPixels * unitStep) * unitStep * this.timeScale.scaleX;

                if (beginTime < 0) { beginTime -= unitStep; }
                if (this.sticking) { beginTime = ((beginTime / unitStep) >> 0) * unitStep; }

                taskRef.setTimes(beginTime, beginTime + 24 * 5);

                if (taskRef) {
                    taskRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.taskRef = taskRef;
                    this.editBox.p = p;
                    this.editBox.m = m;
                    this.editBox.t = ind;
                    this.editBox.createMode = true;

                    if (placeholder) {
                        this.editBox.placeHolder = placeholder;
                        // this.editBox.text   =   placeholder;
                        // taskRef._title      =   placeholder;
                    }

                    this.editBox.setEnable();

                    // undo

                    this._undoManager.add(kOperationAddTask,
                        {
                            p: p, m: undefined, t: taskRef, index: ind,
                            taskId: taskId, milestoneId: undefined, projectId: projectId,
                            silentUpdateMode: true
                        });

                    // в проекте было пусто в списке пустых задач, но добавили одну редактируемую задачку

                    scrollY = this.getElementPosVertical(p, -1, (1 === this.storage.getProject(p).t.length) ? ind : ind + 1);
                    if (this.rightScroll.value() + this.rightScroll.viewWidth < scrollY) {
                        this.animator.moveToY(Math.min(Math.max(0, scrollY - this.ctxHeight * 0.5),
                                this.rightScroll._maxValue - this.rightScroll.viewWidth) + this.itemMargin * 2);
                    }
                }
            }

            // in milestone

            if (-1 !== m) {

                this.queryMoveLinks = null;

                // в закрытую веху не добавляем элементы

                if (this.storage.p[p].m[m]._status == kElementCompleted - 1) {
                    return;
                }

                projectId = this.storage.p[p]._id;
                milestoneId = this.storage.p[p].m[m]._id;
                taskId = this.storage.genRefId();

                this.storage.addTaskWithIndex(taskWithIds(projectId, milestoneId, taskId), ind);

                taskRef = this.storage.getTask(p, m, ind);
                var milestoneRef = this.storage.getMilestone(p, m);

                clickX = this.moveMouse.x;
                unitStep = this.timeScale.scaleUnitStep();
                beginTime = (clickX - this.timeScale.hourInPixels * this.offsetX) / (this.timeScale.hourInPixels * unitStep) * unitStep * this.timeScale.scaleX;
                if (beginTime < 0) { beginTime -= unitStep; }
                if (this.sticking) { beginTime = ((beginTime / unitStep) >> 0) * unitStep; }

                taskRef.setTimes(beginTime, beginTime + 24 * 5);

                if (beginTime < milestoneRef.endTime) { taskRef.setTimes(beginTime, milestoneRef.endTime); }

                if (taskRef) {
                    taskRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.taskRef = taskRef;

                    this.editBox.p = p;
                    this.editBox.m = m;
                    this.editBox.t = ind;
                    this.editBox.createMode = true;
                    this.editBox.milestoneRefForUpdate = milestoneRef;

                    if (placeholder) {
                        this.editBox.placeHolder = placeholder;
                        // taskRef._title          =   placeholder;
                        //this.editBox.text       =   taskRef._title;
                        // this.editBox.saveText   =   taskRef._title;
                        this.editBox.addMode = true;
                    }

                    this.editBox.setEnable();

                    milestoneRef.updateTimes();

                    // undo

                    this._undoManager.add(kOperationAddTask,
                        {
                            p: p, m: m, t: taskRef, index: ind,
                            taskId: taskId, milestoneId: milestoneId, projectId: projectId,
                            silentUpdateMode: true
                        });

                    scrollY = this.getElementPosVertical(p, m, ind);// + this.itemHeight;
                    if (this.rightScroll.value() + this.rightScroll.viewWidth < scrollY) {
                        this.animator.moveToY(Math.max(0, scrollY + this.itemMargin * 2 - this.rightScroll.viewWidth));
                    }
                }
            }
        },
        createMilestone: function (p, ind) {
            if (this.editBox.enable || 0 === this.storage.projects().length || this.readMode)
                return;

            var place = { p: p, m: ind };

            if (place.p >= 0) {
                var pIds = this.storage.p[place.p]._id;
                var mIds = this.storage.genRefId();
                var milestoneRef = null;

                if (undefined === place.m || -1 === ind) {
                    place.m = this.storage.p[place.p].m.length;
                }

                this.storage.p[place.p].addMilestoneWithIndex(milestoneWithIds(pIds, mIds), place.m);
                milestoneRef = this.storage.getMilestone(place.p, place.m);

                if (milestoneRef) {

                    var placeholder = window['Gantt']['Localize_strings']['newMilestone'];
                    var clickX = this.moveMouse.x;
                    var unitStep = this.timeScale.scaleUnitStep();

                    clickX = this.moveMouse.x;
                    unitStep = this.timeScale.scaleUnitStep();
                    var beginTime = (clickX - this.timeScale.hourInPixels * this.offsetX) / (this.timeScale.hourInPixels * unitStep) * unitStep * this.timeScale.scaleX;

                    if (beginTime < 0) { beginTime -= unitStep; }
                    if (this.sticking) { beginTime = ((beginTime / unitStep) >> 0) * unitStep; }

                    milestoneRef.updateEnd(beginTime);

                    this.editBox.reset();

                    this.editBox.milestoneRef = milestoneRef;
                    this.editBox.p = place.p;
                    this.editBox.m = place.m;
                    this.editBox.t = -1;
                    this.editBox.addMode = true;
                    this.editBox.createMode = true;

                    if (placeholder) {
                        this.editBox.placeHolder = placeholder;
                    }

                    this.editBox.setEnable();

                    // undo

                    this._undoManager.add(kOperationAddMilestone,
                        {
                            p: place.p, m: place.m, t: milestoneRef, index: place.m,
                            taskId: undefined, milestoneId: mIds, projectId: pIds,
                            silentUpdateMode: true
                        });

                    // центрируем элемент по вертикали (веха может быть добавлена вне видимой области)

                    var t = this;
                    this.viewController().centeringElement(milestoneRef.id(), true, function () { t.updateContent() }, true);
                }
            }
        },

        addNewMilestone: function () {
            if (this.editBox.enable || 0 === this.storage.projects().length || this.readMode)
                return;

            var place = this.calculateHitMilestone(this.centerScreen, this.ctxHeight * 0.5);
            if (undefined === place.p) place.p = this.storage.projects().length - 1;
            if (place.p >= 0) {
                var pIds = this.storage.p[place.p]._id;
                var mIds = this.storage.genRefId();
                var milestoneRef = null;

                if (undefined === place.m) {
                    place.m = this.storage.p[place.p].m.length;
                }

                this.storage.p[place.p].addMilestoneWithIndex(milestoneWithIds(pIds, mIds), place.m);
                milestoneRef = this.storage.getMilestone(place.p, place.m);

                if (milestoneRef) {

                    var placeholder = window['Gantt']['Localize_strings']['newMilestone'];
                    var clickX = this.moveMouse.x;
                    var unitStep = this.timeScale.scaleUnitStep();

                    milestoneRef.updateEnd(kMilestoneMinDurationInHours);

                    this.editBox.reset();

                    this.editBox.milestoneRef = milestoneRef;
                    this.editBox.p = place.p;
                    this.editBox.m = place.m;
                    this.editBox.t = -1;
                    this.editBox.addMode = true;
                    this.editBox.createMode = true;

                    if (placeholder) {
                        this.editBox.placeHolder = placeholder;
                    }

                    this.editBox.setEnable();

                    // undo

                    this._undoManager.add(kOperationAddMilestone,
                        {
                            p: place.p, m: place.m, t: milestoneRef, index: place.m,
                            taskId: undefined, milestoneId: mIds, projectId: pIds,
                            silentUpdateMode: true
                        });

                    this.updateContent();

                    if (this.sceneToWorldX(milestoneRef.endTime) - milestoneRef.titleWidth < this.sceneXToTime(this.visibleLeft) ||
                        this.sceneToWorldX(milestoneRef.endTime) > this.ctxWidth) {
                        this.animator.moveCenterToX(0);
                    }

                    var scrollY = this.getElementPosVertical(place.p, place.m, -1) - this.ctxHeight * 0.5;
                    var maxVal = floor2((this.rightScroll.maxValue()) / this.itemMargin) * this.itemMargin;
                    scrollY = floor2(scrollY / this.itemMargin) * this.itemMargin;

                    this.animator.moveToY(Math.min(Math.max(0, scrollY), maxVal) + (this.itemMargin * 2 - kEps) * this.fullscreen);
                }
            }
        },
        addNewTask: function (placeholder) {
            if (this.editBox.enable || 0 === this.storage.projects().length || this.readMode)
                return;

            var endp = null;

            this.hitLine = { p: -1, m: -1, t: -1 };
            this.calculateLineHit(undefined, this.centerScreen, this.ctxHeight * 0.5);
            this.hitLine.m = -1;    //   remove if you want to add task center screen

            if (-1 === this.hitLine.p) {
                endp = this.storage.endProject();
                if (endp) {

                    this.hitLine.t = endp.t.length - 1; //   remove if you want to add task center screen

                    var endm = null;//endp.endMilestone();
                    if (!endm) {
                        this.createTask(this.storage.projects().length - 1, -1, endp.t.length, placeholder ? placeholder : window['Gantt']['Localize_strings']['newTask']);// return;
                        endp._calcTimes();
                    } else {
                        if (endp.t.length) {
                            this.createTask(this.storage.projects().length - 1, -1, endp.t.length, window['Gantt']['Localize_strings']['newTask']); //return;
                            endp._calcTimes();
                        } else {
                            this.createTask(this.storage.projects().length - 1, endp.m.length - 1, endp.m[endp.m.length - 1].t.length, placeholder ? placeholder : window['Gantt']['Localize_strings']['newTask']); //return;
                        }
                    }
                }
            } else {
                this.hitLine.t = this.storage.p[this.hitLine.p].t.length - 1;  //   remove if you want to add task center screen

                endp = this.storage.endProject();
                if (endp.collapse) {
                    endp.setCollapse(false);
                    this.updateContent(true);
                }

                this.createTask(this.hitLine.p, this.hitLine.m, this.hitLine.t + 1, window['Gantt']['Localize_strings']['newTask']);
            }

            var scrollY = this.getElementPosVertical(0, -1, this.hitLine.t + 1) - this.ctxHeight * 0.5;

            var maxVal = this.rightScroll.maxValue();
            if (!this.fullscreen) {
                maxVal = floor2((this.rightScroll.maxValue()) / this.itemMargin) * this.itemMargin;
                scrollY = floor2(scrollY / this.itemMargin) * this.itemMargin;
            }

            this.animator.moveToY(Math.min(Math.max(0, scrollY), maxVal) + (this.itemMargin * 2 - kEps) * this.fullscreen);
        },
        addTaskToMilestone: function (p, m) {
            if (this.readMode)
                return;

            var projectId = -1;
            var milestoneId = -1;
            var taskId = -1;

            if (-1 !== m) {
                projectId = this.storage.p[p]._id;
                milestoneId = this.storage.p[p].m[m]._id;
                taskId = this.storage.genRefId();

                this.storage.addTaskWithIndex(taskWithIds(projectId, milestoneId, taskId), 0);

                var taskRef = this.storage.getTask(p, m, 0);
                var milestoneRef = this.storage.getMilestone(p, m);

                var placeholder = window['Gantt']['Localize_strings']['newTask'];

                var clickX = this.menuMileStone.bound.x + this.menuMileStone.bound.w * 0.5;
                var unitStep = this.timeScale.scaleUnitStep();
                var beginTime = (clickX - this.timeScale.hourInPixels * this.offsetX) /
                    (this.timeScale.hourInPixels * unitStep) * unitStep * this.timeScale.scaleX;
                if (beginTime < 0) beginTime -= Math.max(24, unitStep);

                if (this.sticking) {
                    beginTime = ((beginTime / unitStep) >> 0) * unitStep;
                }

                taskRef.setTimes(beginTime, milestoneRef.endTime);

                if (taskRef) {
                    taskRef.isInEditMode = true;

                    this.editBox.reset();

                    this.editBox.taskRef = taskRef;
                    this.editBox.p = p;
                    this.editBox.m = m;
                    this.editBox.t = 0;
                    this.editBox.createMode = true;

                    if (placeholder) {
                        this.editBox.placeHolder = placeholder;
                    }

                    this.editBox.setEnable();

                    this._undoManager.add(kOperationAddTask,
                        {
                            p: p, m: m, t: taskRef, index: 0,
                            taskId: taskId, milestoneId: milestoneId, projectId: projectId,
                            silentUpdateMode: true
                        });
                }
            }
        },

        showPopUpResp: function (p, m, t, posX) {

            var element, coords;

            if (this.handlers[kHanderShowRespPopUpMenuWindow]) {
                if (undefined === t || -1 === t) {
                    element = this.storage.getMilestone(p, m);

                    this._modelController.statusElement = {
                        p: p, m: m, t: undefined,
                        ref: element,
                        ids: this.storage.milestoneIds(p, m),
                        isTask: false, milestone: element
                    };

                    coords = {
                        left: posX, //this.mouse.x - this.ctx.canvas.offsetLeft,
                        top: this.getElementPosVertical(p, m, -1) + this.ctx.canvas.offsetTop - this.rightScroll.value()
                    };

                } else {
                    element = this.storage.getTask(p, m, t);

                    this._modelController.statusElement = {
                        p: p, m: m, t: t,
                        ref: element,
                        ids: this.storage.taskIds(p, m, t),
                        isTask: true, task: element
                    };

                    coords = {
                        left: posX, //this.mouse.x - this.ctx.canvas.offsetLeft,
                        top: this.currentElementY + this.ctx.canvas.offsetTop
                    };
                }

                this.handlers[kHanderShowRespPopUpMenuWindow](element, coords);

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;
                this.queryMoveLinks = null;

                this.capTask = -1;
                this.capMilestone = -1;
                this.capProject = -1;
                this.capSide = -1;

                this.offMenus();
            }
        },
        showPopUpEditElement: function (p, m, t) {

            var element, coords;

            if (this.handlers[kHanderShowEditElemPopUpMenuWindow]) {
                if (undefined === t || -1 === t) {
                    element = this.storage.getMilestone(p, m);

                    this._modelController.statusElement = {
                        p: p, m: m, t: undefined,
                        ref: element,
                        ids: this.storage.milestoneIds(p, m),
                        isTask: false, milestone: element
                    };

                } else {
                    element = this.storage.getTask(p, m, t);

                    this._modelController.statusElement = {
                        p: p, m: m, t: t,
                        ref: element,
                        ids: this.storage.taskIds(p, m, t),
                        isTask: true, task: element
                    };
                }

                this.handlers[kHanderShowEditElemPopUpMenuWindow](element);

                this.menuMileStone.p = -1;
                this.menuMileStone.m = -1;
                this.queryMoveLinks = null;

                this.capTask = -1;
                this.capMilestone = -1;
                this.capProject = -1;
                this.capSide = -1;

                this.offMenus();
            }
        },

        // links

        addEditLink: function () {
            var task = null, duration;

            var p = this.hitProject;
            var m = this.hitMilestone;
            var t = this.hitTask;

            if (-1 === m && -1 === t) { return false; }

            if (-1 != p) {
                if (kOpenProject !== this.storage.getProject(p).status())
                    return false;
            }

            var side = this.hitSide;

            var posY = this.timeScale.height(), posX = 0;
            var addY = floor2(this.rightScroll.value()) + this.itemMargin * 0.5 + (!this.fullscreen) * this.itemMargin;

            if (-1 != p && -1 != t && -1 === m) {
                task = this.storage.getTask(p, undefined, t);
                posY += this.getElementPosVertical(p, m, t) - addY;

                if (this.timeToSceneX(task.beginTime) + Math.abs(this.timeToSceneX(task.endTime) - this.timeToSceneX(task.beginTime)) * 0.5 > this.moveMouse.x) {
                    side = kTaskSideLeft; posX = this.timeToSceneX(task.beginTime);
                } else {
                    side = kTaskSideRight; posX = this.timeToSceneX(task.endTime);
                }
            }

            if (-1 != this.hitProject && -1 != t && -1 != m) {
                task = this.storage.getTask(p, m, t);
                posY += this.getElementPosVertical(p, m, t) - addY;

                if (this.timeToSceneX(task.beginTime) + Math.abs(this.timeToSceneX(task.endTime) - this.timeToSceneX(task.beginTime)) * 0.5 > this.moveMouse.x) {
                    side = kTaskSideLeft; posX = this.timeToSceneX(task.beginTime);
                } else {
                    side = kTaskSideRight; posX = this.timeToSceneX(task.endTime);
                }
            }

            if (task) {
                if (kElementCompleted !== task._status) {

                    if (task.endFail) { side = kTaskSideLeft; }

                    this.linkLineEdit = { side: side, task: task, posX: posX, posY: posY };
                    this.editMode = kEditModeAddLink;

                    this.linkLineEdit.p = p;
                    this.linkLineEdit.m = m;
                    this.linkLineEdit.t = t;

                    return true;
                }
            }

            return false;
        },
        applyEditLink: function () {
            if (kEditModeAddLink === this.editMode) {
                if (this.linkLineEdit) {
                    if (this.linkLineEdit.parent) {

                        // проверяем по id
                        if (this.linkLineEdit.parent._id !== this.linkLineEdit.task._id) {

                            // стороны на валидность
                            if (kTaskSideNone !== this.linkLineEdit.side && kTaskSideNone !== this.linkLineEdit.parentSide) {

                                // добавляем связь между задачами в одном проекте
                                if (this.linkLineEdit.pp === this.linkLineEdit.p) {

                                    // добавляем связь между задачами в одной вехе или в свободной зоне
                                    if (this.linkLineEdit.pm === this.linkLineEdit.m) {

                                        if (kElementCompleted !== this.linkLineEdit.parent._status) {

                                            var i, links, lastLink, dublicate = false;

                                            var linkId0 = this.linkLineEdit.parent.id();
                                            var linkId1 = this.linkLineEdit.task.id();

                                            //                                            if (this.linkLineEdit.parent.endFail && this.linkLineEdit.task.endFail) {
                                            //                                                this.linkLineEdit.side          =   kTaskSideLeft;
                                            //                                                this.linkLineEdit.parentSide    =   kTaskSideLeft;
                                            //                                            } else if (this.linkLineEdit.parent.endFail) {
                                            //                                                this.linkLineEdit.parentSide    =   kTaskSideLeft;
                                            //                                            } else if (this.linkLineEdit.task.endFail) {
                                            //                                                this.linkLineEdit.side          =   kTaskSideLeft;
                                            //                                            }

                                            if (this.pushLink) {

                                                links = this.pushLink.task.links;
                                                lastLink = links.last();

                                                for (i = 0; i < links.length - 1; ++i) {
                                                    if (lastLink['dependenceTaskId'] === links[i]['dependenceTaskId'] &&
                                                        lastLink['linkType'] === links[i]['linkType'] &&
                                                        lastLink['parentTaskId'] === links[i]['parentTaskId'] &&
                                                        lastLink['type'] === links[i]['type']) {
                                                        dublicate = true;
                                                    }
                                                }

                                                this.pushLink.task.links.pop();

                                                if (!dublicate && (lastLink['linkType'] === kLinkEndEnd || lastLink['linkType'] === kLinkBeginBegin)) {

                                                    links = this.linkLineEdit.parent.links;

                                                    for (i = 0; i < links.length; ++i) {
                                                        if (lastLink['parentTaskId'] === links[i]['dependenceTaskId'] &&
                                                            lastLink['linkType'] === links[i]['linkType'] &&
                                                            lastLink['dependenceTaskId'] === links[i]['parentTaskId'] &&
                                                            lastLink['type'] === links[i]['type']) {
                                                            dublicate = true;
                                                        }
                                                    }
                                                }

                                                this.pushLink = undefined;
                                            }

                                            if (!dublicate) {

                                                // между двуями задачми может быть только одна связь

                                                var haveLink = false;

                                                links = this.linkLineEdit.parent.links;

                                                for (i = 0; i < links.length; ++i) {
                                                    if (linkId1 == links[i]['parentTaskId']) {
                                                        haveLink = true;
                                                        break;
                                                    }
                                                }

                                                if (!haveLink) {
                                                    links = this.linkLineEdit.task.links;
                                                    for (i = 0; i < links.length; ++i) {
                                                        if (linkId0 == links[i]['parentTaskId']) {
                                                            haveLink = true;
                                                            break;
                                                        }
                                                    }

                                                }

                                                if (!haveLink) {
                                                    // fix for minimizator

                                                    var linkObject = {};
                                                    if (kTaskSideLeft === this.linkLineEdit.side && kTaskSideRight === this.linkLineEdit.parentSide) {
                                                        linkObject['parentSide'] = this.linkLineEdit.side;
                                                        linkObject['parent'] = this.linkLineEdit.task;

                                                        linkObject['task'] = this.linkLineEdit.parent;
                                                        linkObject['side'] = this.linkLineEdit.parentSide;
                                                    } else {

                                                        linkObject['side'] = this.linkLineEdit.side;
                                                        linkObject['task'] = this.linkLineEdit.task;

                                                        linkObject['parent'] = this.linkLineEdit.parent;
                                                        linkObject['parentSide'] = this.linkLineEdit.parentSide;
                                                    }

                                                    this._modelController.addTaskOperation(kHandlerBeforeAddTaskLink, linkObject);

                                                    this.offMenus();

                                                    this.needUpdate = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (this.pushLink) {
                        this.pushLink.task.links.pop();
                        this.pushLink = undefined;
                    }

                    this.linkLineEdit = undefined;
                    this.editMode = kEditModeNoUse;

                    this._mouseInLinkZone = null;
                }
            }
        },
        isValidEditLink: function () {
            var i, links, lastLink, dublicate = false, retval = false;

            var linkId0 = this.linkLineEdit.parent.id();
            var linkId1 = this.linkLineEdit.task.id();

            if (this.linkLineEdit.parent.endFail && this.linkLineEdit.task.endFail) {
                this.linkLineEdit.side = kTaskSideLeft;
                this.linkLineEdit.parentSide = kTaskSideLeft;
            } else if (this.linkLineEdit.parent.endFail) {
                this.linkLineEdit.parentSide = kTaskSideLeft;
            } else if (this.linkLineEdit.task.endFail) {
                this.linkLineEdit.side = kTaskSideLeft;
            }

            if (this.pushLink) {

                if (kEndBeginLinksEditMode) {
                    if (kLinkBeginEnd !== this.pushLink.task.links.last()['linkType']) {

                        this.pushLink.task.links.pop();
                        this.pushLink = undefined;

                        return false;
                    }
                }

                links = this.pushLink.task.links;
                lastLink = links.last();

                for (i = 0; i < links.length - 1; ++i) {
                    if (lastLink['dependenceTaskId'] === links[i]['dependenceTaskId'] &&
                        lastLink['linkType'] === links[i]['linkType'] &&
                        lastLink['parentTaskId'] === links[i]['parentTaskId'] &&
                        lastLink['type'] === links[i]['type']) {
                        dublicate = true;
                    }
                }

                this.pushLink.task.links.pop();

                if (!dublicate && (lastLink['linkType'] === kLinkEndEnd || lastLink['linkType'] === kLinkBeginBegin)) {

                    links = this.linkLineEdit.parent.links;

                    for (i = 0; i < links.length; ++i) {
                        if (lastLink['parentTaskId'] === links[i]['dependenceTaskId'] &&
                            lastLink['linkType'] === links[i]['linkType'] &&
                            lastLink['dependenceTaskId'] === links[i]['parentTaskId'] &&
                            lastLink['type'] === links[i]['type']) {
                            dublicate = true;
                        }
                    }
                }

                this.pushLink = undefined;
            }

            if (!dublicate) {

                // между двуями задачми может быть только одна связь

                var haveLink = false;

                links = this.linkLineEdit.parent.links;

                for (i = 0; i < links.length; ++i) {
                    if (linkId1 == links[i]['parentTaskId']) {
                        haveLink = true;
                        break;
                    }
                }

                if (!haveLink) {
                    links = this.linkLineEdit.task.links;
                    for (i = 0; i < links.length; ++i) {
                        if (linkId0 == links[i]['parentTaskId']) {
                            haveLink = true;
                            break;
                        }
                    }
                }

                if (!haveLink) {
                    return true;
                }
            }

            if (this.pushLink) {
                this.pushLink.task.links.pop();
                this.pushLink = undefined;
            }

            return false;
        },

        sceneToWorldX: function (X) {
            return (this.timeScale.hourInPixels * this.offsetX + X * this.timeScale.hourInPixels / this.timeScale.scaleX);
        },
        sceneToWorldY: function (Y) {

        },
        worldToSceneX: function (X) {
            return (X - this.offsetX * this.timeScale.hourInPixels) / this.timeScale.hourInPixels * this.timeScale.scaleX;
        },
        worldToSceneY: function (Y) {
            return Y - this.timeScale.height() + this.rightScroll.value();
        },
        sceneXToTime: function (X) {
            return X * this.timeScale.scaleX / this.timeScale.hourInPixels;
        },
        timeToSceneX: function (time) {
            var worldToScreen = this.timeScale.hourInPixels / this.timeScale.scaleX;
            var offX = this.timeScale.hourInPixels * this.offsetX;

            return offX + time * worldToScreen;
        },

        windowToCanvas: function (x, y) {
            return { x: x - this.bounding.left * (this.ctxWidth / this.bounding.width), y: y - this.bounding.top * (this.ctxHeight / this.bounding.height) };
        },
        addHandler: function (evt, callback) {
            this.handlers[evt] = callback;
        },

        internalVersion: function () {
            return kInternalVersion;
        },
        isEditTitleMode: function () {
            return this._leftPanelController.getPanel().editBox.isEnable() || this.editBox.isEnable();
        },
        setEnableTouch: function (value) {
            this.enableTouch = value;
        },
        updateDefaults: function () {
            this.titlesFont = this.fontHeight + 'pt ' + kDefaultFontName;

            this.timeScale.updateDefaults();
            this.editBox.updateDefaults();
            // this._leftPanelController.getPanel().editBox.updateDefaults();
        },

        needDrawFlashBackground: function (enable) {
            this._needDrawFlashBackground = enable;
            this.needUpdate = true;
        },
        print: function () {
            var margin = this.itemMargin;

            function drawCollapseArrow(iscollapse, px, py) {
                if (py >= margin * 2) {
                    if (iscollapse) {
                        ctx.fillStyle = '#818181';
                        painter.drawArrowLeft(px, py, 13, 13);
                    } else {
                        ctx.fillStyle = '#818181';
                        painter.drawArrowBottom(px, py, 13, 13);
                    }
                }
            }

            var noneMilestones = window['Gantt']['Localize_strings']['taskWithoutMilestones'] || 'Task Without Milestones',
                highPriority = window['Gantt']['Localize_strings']['highPriority'] || 'high',
                openStatus = window['Gantt']['Localize_strings']['openStatus'] || 'open',
                closeStatus = window['Gantt']['Localize_strings']['closeStatus'] || 'close',
                addDate = window['Gantt']['Localize_strings']['addDate'] || 'add',
                noResponsible = window['Gantt']['Localize_strings']['noResponsible'],
                isTeamlabTime = ((undefined !== Teamlab) && (undefined !== Teamlab['getDisplayDate'])),
                ctx = this.ctx, child = null, rect = null, color = null, task = null, painter = new Painter(ctx),
                panel = this.leftPanelController().getPanel(),
                el = panel.el,
                topAdd = margin * 2;

            var fontH1 = '12px' + kDefaultFontName;
            var fontH2 = 'bold ' + '12px ' + kDefaultFontName;
            var fontH3 = 'bold ' + '16px ' + kDefaultFontName;

            if (el) {
                rect = el.getBoundingClientRect();

                // draw scene without scrollbars

                this.drawScene(true);

                // background

                ctx.fillStyle = '#F3F3F3';
                ctx.fillRect(0, 0, rect.width, this.ctxHeight);
                ctx.fillStyle = '#CCCCCC';
                ctx.fillRect(rect.width, 0, 1, this.ctxWidth);
                ctx.fillStyle = '#CCCCCC';
                ctx.fillRect(rect.width, 0, 1, this.ctxWidth);


                // elements

                var i, j, k, m, begin, end, text, lockProject = false,
                    offY = -this.rightScroll.value(),
                    p = this.storage.projects(),
                    pl = p.length,
                    respPosX = panel.rows[1].posX,
                    beginDateX = panel.rows[2].posX,
                    endDateX = panel.rows[3].posX,
                    statusX = panel.rows[4].posX,
                    priorityX = panel.rows[5].posX,
                    showRowResp = !panel.rows[1].isHidden,
                    showRowBeg = !panel.rows[2].isHidden,
                    showRowEnd = !panel.rows[3].isHidden,
                    showRowStat = !panel.rows[4].isHidden,
                    showRowPrior = !panel.rows[5].isHidden;

                for (j = 0; j < pl; ++j) {

                    lockProject = p[j].isLocked() || p[j].isInReadMode();

                    // строка проекта

                    if (offY >= 0) {

                        text = ellipsis(p[j]._title, 19);

                        if (lockProject) {
                            ctx.fillStyle = kLockProjectColor;
                        } else {
                            ctx.fillStyle = '#000000';
                        }

                        ctx.font = fontH3;
                        ctx.fillText(text, 18, offY + margin * 0.75 + topAdd);

                        if (showRowResp) {
                            ctx.font = fontH1;
                            ctx.fillText(ellipsis(milestoneResponsibles(p[j], noResponsible), 20), respPosX, offY + margin * 0.7 + topAdd);
                        }

                        ctx.fillStyle = '#D3D3D3';
                        ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                        ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);

                        if (p[j]._isprivate) {
                            ctx.drawImage(lockImg, 0, 36, 16, 16, 0, offY + topAdd + margin * 0.35, 16, 16);
                        }

                        drawCollapseArrow(p[j].fullCollapse, 230, offY + margin * 0.35 + topAdd);
                    }

                    // весь контент у проекты скрыт

                    if (p[j].fullCollapse) {
                        offY += margin;

                        if (offY > this.ctxHeight) {
                            break;
                        }

                        continue;
                    }

                    if (!p[j].isEmpty()) {

                        offY += this.itemMargin;

                        if (offY > this.ctxHeight) {
                            break;
                        }

                        for (m = 0; m < p[j].m.length; ++m) {

                            // применен фильтр

                            if (p[j].m[m].filter) { continue; }

                            if (offY >= 0) {

                                text = ellipsis(p[j].m[m]._title, 24);

                                if (lockProject) {
                                    ctx.fillStyle = kLockProjectColor;
                                } else {
                                    ctx.fillStyle = milestoneFill(p[j].m[m]);
                                }

                                ctx.font = fontH2;
                                ctx.fillText(text, 27, offY + margin * 0.7 + topAdd);
                                ctx.font = fontH1;
                                ctx.fillText(numberOfValidTasks(p[j].m[m].t), 195, offY + margin * 0.7 + topAdd);

                                if (showRowResp) {
                                    ctx.fillText(ellipsis(milestoneResponsibles(p[j].m[m], noResponsible), 20), respPosX, offY + margin * 0.7 + topAdd);
                                }

                                if (showRowEnd) {
                                    ctx.fillText(formatTime(p[j].m[m]._endDate, isTeamlabTime), endDateX, offY + margin * 0.7 + topAdd);
                                }

                                if (showRowStat) {
                                    if (lockProject) {
                                        ctx.fillStyle = kLockProjectColor;
                                    } else {
                                        ctx.fillStyle = '#000000';
                                    }

                                    if (kElementCompleted - 1 === p[j].m[m]._status) {
                                        ctx.fillText(closeStatus, statusX, offY + margin * 0.7 + topAdd);
                                    } else {
                                        ctx.fillText(openStatus, statusX, offY + margin * 0.7 + topAdd);
                                    }
                                }

                                ctx.fillStyle = '#D3D3D3';
                                ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                                ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);

                                if (p[j].m[m].t.length) {
                                    drawCollapseArrow(p[j].m[m].collapse, 230, offY + margin * 0.35 + topAdd);
                                }
                            }

                            offY += this.itemMargin;

                            if (offY > this.ctxHeight) {
                                break;
                            }

                            if (p[j].m[m].collapse) { continue; }

                            for (i = 0; i < p[j].m[m].t.length; ++i) {

                                // применен фильтр

                                if (p[j].m[m].t[i].filter) { continue; }

                                if (offY >= 0) {

                                    task = p[j].m[m].t[i];
                                    text = ellipsis(task._title, 28);

                                    if (lockProject) {
                                        ctx.fillStyle = kLockProjectColor;
                                    } else {
                                        ctx.fillStyle = taskFill(task);
                                    }

                                    ctx.font = fontH1;
                                    ctx.fillText(text, 36, offY + margin * 0.65 + topAdd);

                                    if (showRowResp) {
                                        ctx.fillText(ellipsis(taskResponsibles(task, noResponsible), 20), respPosX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowBeg) {
                                        ctx.fillText(task.beginFail ? addDate : formatTime(task.beginDate(), isTeamlabTime),
                                            beginDateX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowEnd) {
                                        ctx.fillText(task.endFail ? addDate : formatTime(task.endDate(), isTeamlabTime),
                                            endDateX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (task._priority && showRowPrior) {
                                        ctx.fillText(highPriority, priorityX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowStat) {
                                        if (lockProject) {
                                            ctx.fillStyle = kLockProjectColor;
                                        } else {
                                            ctx.fillStyle = '#000000';
                                        }
                                        if (kElementCompleted === task._status) {
                                            ctx.fillText(closeStatus, statusX, offY + margin * 0.7 + topAdd);
                                        } else {
                                            ctx.fillText(openStatus, statusX, offY + margin * 0.7 + topAdd);
                                        }
                                    }

                                    ctx.fillStyle = '#D3D3D3';
                                    ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                                    ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);
                                }

                                offY += margin;

                                if (offY > this.ctxHeight) {
                                    break;
                                }
                            }
                        }

                        if (offY >= 0) {

                            if (lockProject) {
                                ctx.fillStyle = kLockProjectColor;
                            } else {
                                ctx.fillStyle = '#000000';
                            }

                            ctx.font = fontH2;
                            ctx.fillText(noneMilestones, 27, offY + margin * 0.75 + topAdd);

                            ctx.font = fontH1;
                            ctx.fillText(numberOfValidTasks(p[j].t), 195, offY + margin * 0.7 + topAdd);

                            ctx.fillStyle = '#D3D3D3';
                            ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                            ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);
                        }

                        // свободные задачи в проекте скрыты

                        if (p[j].collapse) {
                            drawCollapseArrow(p[j].collapse, 230, offY + margin * 0.35 + topAdd);

                            offY += margin;

                            if (offY > this.ctxHeight) {
                                break;
                            }

                            continue;
                        }

                        if (p[j].t.length) {
                            drawCollapseArrow(p[j].collapse, 230, offY + margin * 0.35 + topAdd);

                            offY += this.itemMargin;

                            for (i = 0; i < p[j].t.length; ++i) {

                                // применен фильтр

                                if (p[j].t[i].filter) { continue; }

                                if (offY >= 0) {

                                    task = p[j].t[i];
                                    text = ellipsis(task._title, 28);

                                    if (lockProject) {
                                        ctx.fillStyle = kLockProjectColor;
                                    } else {
                                        ctx.fillStyle = taskFill(task);
                                    }

                                    ctx.font = fontH1;
                                    ctx.fillText(text, 36, offY + margin * 0.65 + topAdd);

                                    if (showRowResp) {
                                        ctx.fillText(ellipsis(taskResponsibles(task, noResponsible), 20), respPosX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowBeg) {
                                        ctx.fillText(task.beginFail ? addDate : formatTime(task.beginDate(), isTeamlabTime),
                                            beginDateX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowEnd) {
                                        ctx.fillText(task.endFail ? addDate : formatTime(task.endDate(), isTeamlabTime),
                                            endDateX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (task._priority && showRowPrior) {
                                        ctx.fillText(highPriority, priorityX, offY + margin * 0.7 + topAdd);
                                    }

                                    if (showRowStat) {
                                        if (lockProject) {
                                            ctx.fillStyle = kLockProjectColor;
                                        } else {
                                            ctx.fillStyle = '#000000';
                                        }
                                        if (kElementCompleted === task._status) {
                                            ctx.fillText(closeStatus, statusX, offY + margin * 0.7 + topAdd);
                                        } else {
                                            ctx.fillText(openStatus, statusX, offY + margin * 0.7 + topAdd);
                                        }
                                    }

                                    ctx.fillStyle = '#D3D3D3';
                                    ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                                    ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);
                                }

                                offY += margin;

                                if (offY > this.ctxHeight) {
                                    break;
                                }
                            }
                        } else {

                            // линия свободных задач

                            offY += this.itemMargin;
                        }
                    } else {

                        offY += this.itemMargin;

                        if (offY >= 0) {

                            if (lockProject) {
                                ctx.fillStyle = kLockProjectColor;
                            } else {
                                ctx.fillStyle = '#000000';
                            }

                            ctx.font = fontH2;
                            ctx.fillText(noneMilestones, 27, offY + margin * 0.75 + topAdd);

                            ctx.font = fontH1;
                            ctx.fillText(numberOfValidTasks(p[j].t), 195, offY + margin * 0.7 + topAdd);

                            ctx.fillStyle = '#D3D3D3';
                            ctx.fillRect(0, offY + topAdd, rect.width - 15, 1);
                            ctx.fillRect(0, offY + margin + topAdd, rect.width - 15, 1);
                        }

                        // пустой проект имеет только две линии - сам проект и свободные задачи

                        offY += this.itemMargin;
                    }

                    if (offY > this.ctxHeight) {
                        break;
                    }
                }

                ctx.fillStyle = '#D3D3D3';
                ctx.fillRect(0, 0, rect.width, margin * 1.5);

                for (i = 1; i < panel.rows.length; ++i) {
                    if (!panel.rows[i].isHidden) {
                        ctx.fillStyle = '#F3F3F3';
                        painter.roundRect(ctx, panel.rows[i].posX - 5, 26, panel.rows[i].minWidth, margin, 4);
                        ctx.fill();

                        ctx.fillStyle = '#666666';
                        ctx.font = fontH1;
                        ctx.fillText(panel.rows[i].translate, panel.rows[i].posX + 1, margin * 1.3);
                    }
                }
             }
        }
    };

    /**
     * API
     */

    var prot;
    var gantt = window['Gantt'] ? window['Gantt'] : (window['Gantt'] = {});

    gantt['TimeLine'] = TimeLine;

    prot = TimeLine.prototype;

    prot['render'] = prot.render;
    prot['addHandler'] = prot.addHandler;
    prot['update'] = prot.update;
    prot['viewController'] = prot.viewController;
    prot['modelController'] = prot.modelController;
    prot['undoManager'] = prot.undoManager;
    prot['userDefaults'] = prot.userDefaults;
    prot['leftPanelController'] = prot.leftPanelController;

    prot['addNewTask'] = prot.addNewTask;
    prot['addNewMilestone'] = prot.addNewMilestone;

    prot['setReadMode'] = prot.setReadMode;
    prot['print'] = prot.print;

    prot['onkeypress'] = prot.onkeypress;
    prot['onkeydown'] = prot.onkeydown;
    prot['onkeyup'] = prot.onkeyup;
    prot['ondblclick'] = prot.ondblclick;
    prot['onmousewheel'] = prot.onmousewheel;
    prot['onpaste'] = prot.onpaste;
    prot['oncopy'] = prot.oncopy;
    prot['focus'] = prot.focus;

    prot['internalVersion'] = prot.internalVersion;
    prot['isEditTitleMode'] = prot.isEditTitleMode;
    prot['setEnableTouch'] = prot.setEnableTouch;
    prot['needDrawFlashBackground'] = prot.needDrawFlashBackground;

    prot = ModelController.prototype;

    prot['addTask'] = prot.addTask;
    prot['addMilestone'] = prot.addMilestone;
    prot['addProject'] = prot.addProject;
    prot['finalize'] = prot.finalize;
    prot['clear'] = prot.clear;
    prot['applyNewAddTaskId'] = prot.applyNewAddTaskId;
    prot['applyNewAddMilestoneId'] = prot.applyNewAddMilestoneId;
    prot['addLinkWithIds'] = prot.addLinkWithIds;
    prot['finalizeStatus'] = prot.finalizeStatus;
    prot['finalizeOperation'] = prot.finalizeOperation;
    prot['collectBeginEndItems'] = prot.collectBeginEndItems;
    prot['throughIdsIndexes'] = prot.throughIdsIndexes;
    prot['buildWithThroughIndexer'] = prot.buildWithThroughIndexer;
    prot['setFilter'] = prot.setFilter;
    prot['clearFilter'] = prot.clearFilter;

    prot = ViewController.prototype;

    prot['scaleTo'] = prot.scaleTo;
    prot['strafeToDay'] = prot.strafeToDay;
    prot['strafe'] = prot.strafe;
    prot['toVisibleElementMove'] = prot.toVisibleElementMove;
    prot['buildZoomBar'] = prot.buildZoomBar;
    prot['enableSlipAnimation'] = prot.enableSlipAnimation;
    prot['fullscreen'] = prot.fullscreen;
    prot['disableUserEvents'] = prot.disableUserEvents;
    prot['scaleValue'] = prot.scaleValue;
    prot['saveViewState'] = prot.saveViewState;
    prot['loadViewState'] = prot.loadViewState;
    prot['centeringElement'] = prot.centeringElement;

    prot = ZoomBar.prototype;

    prot['canvas'] = prot.canvas;
    prot['dom'] = prot.dom;
    prot['needRepaint'] = prot.needRepaint;

    prot = LeftPanelController.prototype;
    prot['addRowsAvailable'] = prot.addRowsAvailable;
    prot['showHiddenRows'] = prot.showHiddenRows;

    prot = UndoManager.prototype;

    prot['undo'] = prot.undo;
    prot['redo'] = prot.redo;

    prot = Task.prototype;
    prot['id'] = prot.id;
    prot['title'] = prot.title;
    prot['description'] = prot.description;
    prot['owner'] = prot.owner;
    prot['beginDate'] = prot.beginDate;
    prot['endDate'] = prot.endDate;
    prot['isMilestone'] = prot.isMilestone;
    prot['status'] = prot.status;
    prot['subtasks'] = prot.subtasks;
    prot['responsibles'] = prot.responsibles;
    prot['isUndefinedEndTime'] = prot.isUndefinedEndTime;
    prot['isUndefinedBeginTime'] = prot.isUndefinedBeginTime;

    prot = Milestone.prototype;
    prot['id'] = prot.id;
    prot['title'] = prot.title;
    prot['description'] = prot.description;
    prot['owner'] = prot.owner;
    prot['tasks'] = prot.tasks;
    prot['beginDate'] = prot.beginDate;
    prot['endDate'] = prot.endDate;
    prot['isMilestone'] = prot.isMilestone;
    prot['status'] = prot.status;
    prot['responsibles'] = prot.responsibles;

    prot = Project.prototype;
    prot['id'] = prot.id;
    prot['title'] = prot.title;
    prot['description'] = prot.description;

    prot = UserDefaults.prototype;
    prot['setFontFamily'] = prot.setFontFamily;

});
