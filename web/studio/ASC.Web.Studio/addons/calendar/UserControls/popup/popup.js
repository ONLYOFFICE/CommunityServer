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


(function($, window) {

	function htmlEscape(s) {
		if (typeof s !== "string") {
			return s;
		}
		return s.replace(/&(\s|[^#\w]|\w+(?:[^;\w]|$))/g, '&amp;$1')
				.replace(/</g, '&lt;')
				.replace(/>/g, '&gt;')
				.replace(/'/g, '&#039;')
				.replace(/"/g, '&quot;')
				.replace(/\n/g, '<br/>');
	}

	function isHtmlElement(e) {
		return e.nodeName !== undefined;
	}



	/*
	 * Popup base
	 */
	var popupBase = {

		// default options
		options: {
			anchor: "left,bottom",      // a point of DOM element which will be the base point of popup
			anchorToBorder: false,      // if this option is "true" then DOM element's padding and border will be taken into account for base point
			direction: "right,down",    // directions at which popup will open
			offset: "0,0",              // the offset of popup (in px)
			arrow: "up",                // arrow direction (up|down|left|right)
			arrowPosition: "-1",        // position of popup's arrow along side (in %)
			showArrow: true,            //
			cssClassName: "asc-popup"   //
		},

		_visible: false,
		_arrow: null,
		_arrowWidth: 0,
		_arrowHeight: 0,

		_arrowSize: {},

		_create: function() {
			var arrow = $('<div class="arrow" style="margin:0;"/>');

			this.element
					.hide()
					.addClass(this.options.cssClassName)
					.append(arrow)
					.appendTo("body");

			this._arrow = arrow;

			var size = this._arrowSize[this.options.cssClassName];

			if (!size) {
				size = this._arrowSize[this.options.cssClassName] = {
					width: this._arrow.outerWidth(true),
					height: this._arrow.outerHeight(true)
				};
			}

			this._arrowWidth = size.width;
			this._arrowHeight = size.height;
		},

		_makeVArrow: function(arrow, anchorX, label) {
			var top = 0;
			if (arrow === "up") {
				this.element
						.css("margin", (this._arrowHeight + 1) + "px 0 0 0");
				this._arrow
						.css("top", (top = -this._arrowHeight) + "px")
						.css("background-position", "0px 0px");
			} else {
				this.element
						.css("margin", "0 0 " + (this._arrowHeight + 1) + "px 0");
				this._arrow
						.css("top", (top = this.element.outerHeight(true) - 3 - this._arrowHeight) + "px")
						.css("background-position", (-2 * this._arrowWidth) + "px 0px");
			}
			var left = Math.floor(
					(anchorX == "left" ? 0 : this.element.outerWidth(true)) +
					(anchorX == "left" ? 1 : -1) *
					(Math.min(
							isHtmlElement(label[0]) ?
									(this.options.anchorToBorder ? label.outerWidth() : label.width()) : 0,
							this.element.outerWidth(true)
							) * 0.5) -
					(this._arrowWidth * 0.5));
			this._arrow
					.css("width", this._arrowWidth + "px")
					.css("height", this._arrowHeight + "px")
					.css("right", "")
					.css("bottom", "")
					.css("left", left + "px");
			return {x: left, y: top};
		},

		_makeHArrow: function(arrow, anchorY, label) {
			var left = 0;
			if (arrow === "left") {
				this.element
						.css("margin", "0 0 0 " + (this._arrowHeight + 1) + "px");
				this._arrow
						.css("left", (left = -this._arrowHeight) + "px")
						.css("background-position", (-3 * this._arrowWidth) + "px 0px");
			} else {
				this.element
						.css("margin", "0 " + (this._arrowHeight + 1) + "px 0 0");
				this._arrow
						.css("left", (left = this.element.outerWidth(true) - 3 - this._arrowHeight) + "px")
						.css("background-position", (-1 * this._arrowWidth) + "px 0px");
			}
			var top = Math.floor(
					(anchorY == "top" ? 0 : this.element.outerHeight(true)) +
					(anchorY == "top" ? 1 : -1) *
					(Math.min(
							isHtmlElement(label[0]) ?
									(this.options.anchorToBorder ? label.outerHeight() : label.height()) : 0,
							this.element.outerHeight(true)
							) * 0.5) -
					(this._arrowWidth * 0.5));
			this._arrow
					.css("width", this._arrowHeight + "px")
					.css("height", this._arrowWidth + "px")
					.css("right", "")
					.css("bottom", "")
					.css("top", top + "px");
			return {x: left, y: top};
		},

		_moveArrow: function(arrow, arrowOffset, arrowPos) {
			if (arrowPos < 0) {
				return {x: 0, y: 0};
			}
			if (arrow === "up" || arrow === "down") {
				var elementW = this.element.outerWidth(true);
				var left = Math.floor(elementW * arrowPos * 0.01 - this._arrowWidth * 0.5);
				left = left < 0 ? 0 : (left + this._arrowWidth > elementW ?
						elementW - this._arrowWidth : left);
				this._arrow.css("left", left + "px");
				return {x: arrowOffset.x - left, y: 0};
			} else {
				var elementH = this.element.outerHeight(true);
				var top = Math.floor(elementH * arrowPos * 0.01 - this._arrowWidth * 0.5);
				top = top < 0 ? 0 : (top + this._arrowWidth > elementH ?
						elementH - this._arrowWidth : top);
				this._arrow.css("top", top + "px");
				return {x: 0, y: arrowOffset.y - top};
			}
		},

		_parsePageX: function(_pageX) {
		    var win = $(window);
		    var studioPageContent = $('#studioPageContent');
			var pageX = (/^\s*(\d+|left|right|center)/i).exec(_pageX);
			if (!pageX) {pageX = "center";}
			var pX = parseInt(pageX[1], 10);
			if (!isNaN(pX)) {
				return pX;
			} else {
				if ("left" === pageX[1]) {
					return 0;
				} else if ("right" === pageX[1]) {
					return win.width();
			    } else {
			        var center = win.width() < studioPageContent.width() ? Math.round((studioPageContent.width() - this.element.outerWidth(true)) * 0.5) : Math.round((win.width() - this.element.outerWidth(true)) * 0.5);
			        return center;
			    }
			}
			return 0;
		},

		_parsePageY: function(_pageY) {
			var win = $(window);
			var pageY = (/^\s*(\d+|top|bottom|center)/i).exec(_pageY);
			if (!pageY) {pageY = "center";}
			var pY = parseInt(pageY[1], 10);
			if (!isNaN(pY)) {
				return pY;
			} else {
				if ("top" === pageY[1]) {
					return 0;
				} else if ("bottom" === pageY[1]) {
					return win.height();
				} else {
					return Math.round((win.height() - this.element.outerHeight(true)) * 0.5);
				}
			}
			return 0;
		},

		updatePosition: function(_elem) {
			var label = $(_elem);
			var isElement = isHtmlElement(label[0]);
			var anchor = this.options.anchor.match(/^\s*(left|right)\s*,\s*(top|bottom)/i);
			var dir = this.options.direction.match(/^\s*(left|right)\s*,\s*(up|down)/i);
			var offs = this.options.offset.match(/^\s*([+-]?\d+(\.\d+)?)\s*(px)?\s*,\s*([+-]?\d+(\.\d+)?)\s*(px)?/i);
			var arr = this.options.arrow.match(/^\s*(left|right|up|down)/i);
			var arrPos = this.options.arrowPosition.match(/^\s*([+-]?\d+(\.\d+)?)\s*(%)?/i);
			if (!anchor || !dir || !offs || !arr) {return;}

			if (!label || label.length < 1 || !isElement) {
				label.pageX = this._parsePageX(label.pageX || label[0].pageX);
				label.pageY = this._parsePageY(label.pageY || label[0].pageY);
			}

			var anchorX = anchor[1].toLowerCase();
			var anchorY = anchor[2].toLowerCase();
			var dirX = dir[1].toLowerCase();
			var dirY = dir[2].toLowerCase();
			var offset = {
				x: (offs[1] != undefined ? parseInt(offs[1]) : 0),
				y: (offs[4] != undefined ? parseInt(offs[4]) : 0)
			};
			var arrow = arr[1];
			var arrowPosition = (arrPos[1] != undefined ? parseInt(arrPos[1]) : -1);

			var x = 0;
			var y = 0;
			var arrowOffset;
			if (arrow === "up" || arrow === "down") {
				arrowOffset = this._makeVArrow(arrow, anchorX, label);
				if (arrowPosition < 0 && arrowOffset.x < 0) {
					arrowOffset.x = 0;
					this._arrow.css("left", "0");
				}
			} else {
				arrowOffset = this._makeHArrow(arrow, anchorY, label);
				if (arrowPosition < 0 && arrowOffset.y < 0) {
					arrowOffset.y = 0;
					this._arrow.css("top", "0");
				}
			}

			var frameOffset = this.options.showArrow ?
					this._moveArrow(arrow, arrowOffset, arrowPosition) : {x: 0, y: 0};

			if (this.options.showArrow) {
				this._arrow.show();
			} else {
				this._arrow.hide();
				this.element.css("margin-top", "4px");
			}

			var labelOffset = isElement ? label.offset() : undefined;
			x = (isElement ?
							labelOffset.left + (anchorX === "left" ? 0 : label.outerWidth()) :
							label.pageX) +
					(dirX === "left" && isElement ? -this.element.outerWidth(true) : 0) +
					frameOffset.x +
					offset.x;
			y = (isElement ?
							labelOffset.top + (anchorY === "top" ? 0 : label.outerHeight()) :
							label.pageY) +
					(dirY === "up" && isElement ? -this.element.outerHeight(true) : 0) +
					frameOffset.y +
					offset.y;

			this.element.css("left", x + "px").css("right", "");
			this.element.css("top", y + "px").css("bottom", "");
		},

		open: function(_elem) {
			if (this._visible) {this.close();return;}
			this.updatePosition(_elem);
			this.element.show();
			this._visible = true;
		},

        hide: function() {
            this.element.hide();
        },

        show: function() {
            this.element.show();
        },

		close: function() {
			if (this.element[0].style.display != "none") {
				this.element.hide();
			}
			this._visible = false;
		},

		isVisible: function() {
			return this._visible;
		},

		destroy: function() {
			this.element.text("");
			// call the base destroy function
			$.Widget.prototype.destroy.call(this);
		}

	};


	var modalPopupBase = {

		// default options
		options: {
			showModal: false
		},

		_uiBlocker: undefined,

		_createUIBlocker: function() {
			var id = this.element.attr("id");
			if (!id || id.length < 1) {
				id = this.element.attr("class").replace(/^\s*(\S+)/i, "$1");
			}
			if (!id || id.length < 1) {
				id = "asc-ui-blocker";
			}
			id += "-outer";
			this._uiBlocker = $('<div id="' + id + '"><div class="asc-ui-blocker"/></div>')
					.appendTo("body")
					.append(this.element)
					.find(".asc-ui-blocker")
					.hide();
		},

		_tryClose: function(event) {
			var self = event.data.thisObj;
			if (false == self._trigger("beforeClose", event)) {return false;}
			self.close(event);
			return true;
		},

		_blockScreen: function() {
			var self = this;
			if (self.options.showModal) {
				if (self._uiBlocker == undefined) {
					self._createUIBlocker();
				}
				self._uiBlocker
						.unbind("click", self._tryClose)
						.bind("click", {thisObj:self}, self._tryClose)
						.show();
			}
		},

		_unblockScreen: function() {
			if (this._uiBlocker != undefined) {
				this._uiBlocker.hide();
			}
		},

		open: function(elem) {
			this._blockScreen();
			popupBase.open.call(this, elem);
		},

		close: function(event) {
			popupBase.close.call(this);
			this._unblockScreen();
			this._trigger("close", event);
		},

		replaceUIBlocker: function(frame) {
			var blocker = $(frame);
			if (this._uiBlocker && this._uiBlocker.length > 0 &&
			    blocker && blocker.length > 0) {
				var p = this.element.parent();
				this.element.appendTo("body");
				p.remove();
			}
			this._uiBlocker = blocker;
		}

	};



	/*
	 * Popup Frame
	 */
	$.widget("asc.popupFrame", $.extend(true, {}, popupBase, modalPopupBase));



	/*
	 * Popup Menu
	 */
	var popupMenu = {

		// default options
		options: {
			cssClassName: "asc-popupmenu",
			closeTimeout: 1500,
			divider: "divider",
			items: []
		},

		_closeTimer: 0,
		_closeOnMouseleave: true,
		_label: undefined,

		_create: function() {
			popupBase._create.call(this);

			var t = this;

			t._closeOnMouseleave = t.options.closeTimeout >= 0;

			if (t._closeOnMouseleave) {
				t.element
						.mouseenter(function() {t._stopCloseTimer();})
						.mouseleave(function() {t.close();});
			} else {
				$(window.document).bind("mousedown", t, t._outerClick);
			}

            $.each(t.options.items, function(i,_item) {
				if (_item === t.options.divider || 
						_item.label === t.options.divider) {
					$("<div class='divider'/>").appendTo(t.element);
				} else {
					$("<div class='item " + (_item.cssClass !== undefined ?
								htmlEscape(_item.cssClass) : "") + "'>" +
							htmlEscape(typeof _item === "string" ? _item : _item.label) +
						"</div>")
							.click({itemIndex: i, item: _item}, function(e) {t._itemClick(e);})
							.appendTo(t.element);
				}
            });
		},

		_outerClick: function(event) {
			var t = event.data;
			if (popupBase.isVisible.call(t)) {
				if (!$.contains(t.element[0], event.target) && (t._label[0] !== event.target)) {
					t.close();
				}
			}
		},

		_itemClick: function(event) {
			this.close();
			var hasClickHandler = $.isFunction(event.data.item.click);
			if (typeof event.data.item === "string" || !hasClickHandler) {
				this._trigger("itemClick", event, event.data);
			} else {
				if (hasClickHandler) {
					event.data.item.click.call(this.element[0], event, event.data);
					if ($.isFunction(event.data.item.toggle)) {
						event.data.item.toggle.call(this.element[0], event, event.data);
						this.element
								.find(".item:eq(" + event.data.itemIndex + ")")
								.text(event.data.item.label);
					}
				}
			}
			//event.stopPropagation();
		},

		_startCloseTimer: function() {
			var t = this;
			t._closeTimer = setTimeout(function() {t.close();}, t.options.closeTimeout);
		},

		_stopCloseTimer: function() {
			clearTimeout(this._closeTimer);
		},

		open: function(_elem) {
			this._label = _elem;
			popupBase.open.call(this, _elem);
			if (this._closeOnMouseleave) {
				this[popupBase.isVisible.call(this) ? "_startCloseTimer" : "_stopCloseTimer"].call(this);
			}
		},

		close: function() {
			if (this._closeOnMouseleave) { this._stopCloseTimer(); }
			popupBase.close.call(this);
		},

		destroy: function() {
			$(window.document).unbind("mousedown", this._outerClick);
			// call the base destroy function
			popupBase.destroy.call(this);
		},

		click: function(itemNo) {
			this.element.find(".item:eq(" + itemNo + ")").click();
		}

	};

	$.widget("asc.popupMenu", $.extend(true, {}, popupBase, popupMenu));



	/*
	 * Color picker
	 */
	var colorPickerBase = $.extend(true, {}, popupBase, modalPopupBase);

	var colorPicker = {

		// default options
		options: {
			cssClassName: "asc-colorpicker",
			colors: { 0: [
				"#ff0000", "#ff2f00", "#ff5e00", "#ff8d00", "#ffbc00", "#ffec00", "#e2ff00",
				"#b3ff00", "#84ff00", "#54ff00", "#25ff00", "#00ff09", "#00ff38", "#00ff67",
				"#00ff97", "#00ffc6", "#00fff5", "#00d9ff", "#00a9ff", "#007aff", "#004bff",
				"#001cff", "#1200ff", "#4200ff", "#7100ff", "#a000ff", "#cf00ff", "#ff00fe",
				"#ffffff", "#d5d5d5", "#aaaaaa", "#808080", "#555555", "#2b2b2b", "#000000"
			]},
			colorSet: 0,
			selectedColor: ""
		},

		_table: undefined,
		_input: undefined,
		_previewIcon: undefined,

		_create: function() {
			colorPickerBase._create.call(this);

			var t = this;

			var colCount = t.options.columns && t.options.columns > 0 ?
					t.options.columns :
					Math.ceil(Math.sqrt(t.options.colors[t.options.colorSet].length)) + 1;
			var tbody = "";
			var r = -1;
			var color = t._parseColor(t.options.selectedColor), c, bc;

			for (var i = 0; i < t.options.colors[t.options.colorSet].length; ++i) {
				r = i % colCount;
				if (r == 0) {
					tbody += tbody.length < 1 ? '<tbody><tr>' : '</tr><tr>';
				}
				c = t._parseColor(t.options.colors[t.options.colorSet][i]);
				bc = ((Math.round(c.r * 0.9) << 16) | (Math.round(c.g * 0.9) << 8) |
						Math.round(c.b * 0.9)).toString(16);
				while (bc.length < 6) {bc = "0" + bc;}
				tbody +=
						'<td ' + (color && color.color == c.color ?
								'class="selected"' : '') + '>' +
							'<div class="icon" style="background-color:' +
									c.origColor + '; border: 1px solid #' + bc + '"/>' +
						'</td>';
			}
			var rem = colCount - 1 - r;
			tbody += (rem > 0 ? '<td class="remainder" colspan="' + rem + '"/>' : '') + '</tr>';

			tbody +=
					'<tr>' +
						'<td class="remainder">' +
							'<div class="icon" style="background-color:#fff; border: 1px solid #eee"/>' +
						'</td>' +
						'<td class="remainder" colspan="' + (colCount-1) + '">' +
							'<div class="iwrapper"><input type="text" value=""/></div>' +
						'</td>' +
					'</tr>';

			tbody += '</tbody>';

			t._table = 
					$('<table border="0" cellspacing="0" cellpadding="0">' + tbody + '</table>')
					.click(function(ev) {t._cellClick(ev);});

			t._previewIcon = t._table.find("tr:last .icon");

			t._table.find("td:not(.remainder)").hover(function(ev) {t._cellHover(ev);});

			t._input = t._table.find("input").keyup(function(ev) {t._keyUp(ev);});

			t.element.append(t._table);

			t.element.keyup(function(ev) {t._checkEscKey(ev);});
		},

		_parseColor: function(c) {
			var reColor =
					/\s*#([0-9a-f]{6}\s*$|[0-9a-f]{3}\s*$)|\s*rgba?\(\s*([0-9]+\s*,\s*[0-9]+\s*,\s*[0-9]+\s*(?:,\s*(?:[0-9]+(?:\.[0-9]+)?|[0-9]*\.[0-9]+))?)\s*\)/i;
			var m = reColor.exec(c);
			if (!m) {return null;}

			var reRgb =
					/^([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})$|^([0-9a-f])([0-9a-f])([0-9a-f])$|^([0-9]{1,3}),\s*([0-9]{1,3}),\s*([0-9]{1,3})(?:,\s*([0-9.]+))?$/i;
			var x = reRgb.exec(m[1] || m[2]);

			var type = (typeof x[1] === "string" || typeof x[4] === "string") ? 0 :
					(typeof x[10] === "string" ? 2 : 1);

			var r = x[1] || x[4];
			var g = x[2] || x[5];
			var b = x[3] || x[6];
			r = r ? parseInt(r.length > 1 ? r : r + r, 16) : parseInt(x[7], 10);
			g = g ? parseInt(g.length > 1 ? g : g + g, 16) : parseInt(x[8], 10);
			b = b ? parseInt(b.length > 1 ? b : b + b, 16) : parseInt(x[9], 10);

			var a = x[10];
			a = a ? (Math.round(parseFloat(a) * 100) * 0.01) : 1;

			var color2 = ((r << 16) | (g << 8) | b).toString(16);
			while (color2.length < 6) {color2 = "0" + color2;}
			color2 = "#" + color2;

			var rgb1 = r + ", " + g + ", " + b;
			var rgb2 = "rgb(" + rgb1 + ")";
			var rgba2 = "rgba(" + rgb1 + ", " + a + ")";

			return {
				type: type,
				r: r,
				g: g,
				b: b,
				a: a,
				rgb: rgb2,
				rgba: rgba2,
				color: color2,
				origColor: type === 0 ? color2 : (type === 1 ? rgb2 : rgba2)
			};
		},

		_selectCell: function() {
			var t = this;
			var c = t._parseColor(t.options.selectedColor);

			t._table.find("td.selected").removeClass("selected");

			if (c) {
				t._table.find("td:not(.remainder)").each(function() {
					var td = $(this);
					if (c.color == t._parseColor(td.find(".icon").css("background-color")).color) {
						td.addClass("selected");
						return false;
					}
					return true;
				});
			}
		},

		_cellClick: function(ev) {
			var t = this;

			var icon = $(ev.target);
			if (icon.hasClass("icon")) {
				t._table.find("td.selected").removeClass("selected");
				var td = icon.parent();
				if (!td.hasClass("remainder")) {td.addClass("selected");}
				t.options.selectedColor = icon.css("background-color");
				t._trigger("select", ev, t.options.selectedColor);
				t.close();
			}
		},

		_cellHover: function(ev) {
			var t = this;

			t._table.find("td").removeClass("highlighted");
			var cell = $(ev.target);
			cell.toggleClass("highlighted");
		},

		_updatePreviewIcon: function(colorStr) {
			var t = this;

			var c = t._parseColor(colorStr);
			if (c) {
				var bc = ((Math.round(c.r * 0.9) << 16) | (Math.round(c.g * 0.9) << 8) |
						Math.round(c.b * 0.9)).toString(16);
				while (bc.length < 6) {bc = "0" + bc;}
				t._previewIcon.css("background-color", c.color).css("border", "1px solid #" + bc);
			}
		},

		_keyUp: function(ev) {
			var t = this;

			this._updatePreviewIcon(this._input.val());

			if (ev.which == 13) {           //  'enter'
				var c = t._parseColor(t._input.val());
				if (c) {
					t.options.selectedColor = c.origColor;
					t._trigger("select", ev, t.options.selectedColor);
					t.close();
				}
			}
		},

		_checkEscKey: function(ev) {
			var t = this;

			if (ev.which == 27) {           //  'escape'
				if (t.isVisible()) {ev.stopPropagation();}
				t.close();
			}
		},

		open: function(_elem) {
			colorPickerBase.open.call(this, _elem);

			this._selectCell();

			var c = this._parseColor(this.options.selectedColor);
			this._input.val(c ? c.color : "");
			this._input.focus();

			this._updatePreviewIcon(this._input.val());
		}

	};

	$.widget("asc.colorPicker", $.extend(true, {}, colorPickerBase, colorPicker));



}(jQuery, window));
