/*
 * Copyright (c) Arnaud Buathier <arnaud@arnapou.net>
 * https://github.com/arnapou/jqcron
 *
 * Licensed under MIT
 */

var jqCronDefaultSettings = {
	texts: {},
	monthdays: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31],
	hours: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23],
	hour_labels: ["00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"],
	minutes: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59],
	lang: 'en',
	enabled_minute: false,
	enabled_hour: true,
	enabled_day: true,
	enabled_week: true,
	enabled_month: true,
	enabled_year: true,
	multiple_dom: false,
	multiple_month: false,
	multiple_mins: false,
	multiple_dow: false,
	multiple_time_hours: false,
	multiple_time_minutes: false,
	numeric_zero_pad: false,
	default_period: 'day',
	default_value: '',
	no_reset_button: true,
	disabled: false,
	bind_to: null,
	bind_method: {
		set: function($element, value) {
			$element.is(':input') ? $element.val(value) : $element.data('jqCronValue', value);
		},
		get: function($element) {
			return $element.is(':input') ? $element.val() : $element.data('jqCronValue');
		}
	}
};

/**
 * Custom extend of json for jqCron settings.
 * We don't use jQuery.extend because simple extend does not fit our needs, and deep extend has a bad
 * feature for us : it replaces keys of "Arrays" instead of replacing the full array.
 */
(function($){
	var extend = function(dst, src) {
		for(var i in src) {
			if($.isPlainObject(src[i])) {
				dst[i] = extend(dst[i] && $.isPlainObject(dst[i]) ? dst[i] : {}, src[i]);
			}
			else if(Array.isArray(src[i])) {
				dst[i] = src[i].slice(0);
			}
			else if(src[i] !== undefined) {
				dst[i] = src[i];
			}
		}
		return dst;
	};
	this.jqCronMergeSettings = function(obj) {
		return extend(extend({}, jqCronDefaultSettings), obj || {});
	};
}).call(this, jQuery);

/**
 * Shortcut to get the instance of jqCron instance from one jquery object
 */
(function($){
	$.fn.jqCronGetInstance = function() {
		return this.data('jqCron');
	};
}).call(this, jQuery);

/**
 * Main plugin
 */
(function($){
	$.fn.jqCron = function(settings) {
		var saved_settings = settings;
		return this.each(function() {
			var cron, saved;
			var $this = $(this);
			var settings = jqCronMergeSettings(saved_settings); // clone settings
			var translations = settings.texts[settings.lang];

			if (typeof(translations) !== 'object' || $.isEmptyObject(translations)) {
				console && console.error(
					'Missing translations for language "' + settings.lang + '". ' +
					'Please include jqCron.' + settings.lang + '.js or manually provide ' +
					'the necessary translations when calling $.fn.jqCron().'
				);
				return;
			}

			if(!settings.jquery_container) {
				if($this.is(':container')) {
					settings.jquery_element = $this.uniqueId('jqCron');
				}
				else if($this.is(':autoclose')) {
					// delete already generated dom if exists
					if($this.next('.jqCron').length == 1) {
						$this.next('.jqCron').remove();
					}
					// generate new
					settings.jquery_element = $('<span class="jqCron"></span>').uniqueId('jqCron').insertAfter($this);
				}
				else {
					console && console.error(settings.texts[settings.lang].error1.replace('%s', this.tagName));
					return;
				}
			}

			// autoset bind_to if it is an input
			if($this.is(':input')) {
				settings.bind_to = settings.bind_to || $this;
			}

			// init cron object
			if(settings.bind_to){
				if(settings.bind_to.is(':input')) {
					// auto bind from input to object if an input, textarea ...
					settings.bind_to.blur(function(){
						var value = settings.bind_method.get(settings.bind_to);
						$this.jqCronGetInstance().setCron(value);
					});
				}
				saved = settings.bind_method.get(settings.bind_to);
				cron = new jqCron(settings);
				cron.setCron(saved);
			}
			else {
				cron = new jqCron(settings);
			}
			$(this).data('jqCron', cron);
		});
	};
}).call(this, jQuery);

/**
 * jqCron class
 */
(function($){
	var jqCronInstances = [];

	function jqCron(settings) {
		var _initialized  = false;
		var _self         = this;
		var _$elt         = this;
		var _$obj         = $('<span class="jqCron-container"></span>');
		var _$blocks      = $('<span class="jqCron-blocks"></span>');
		var _$blockPERIOD = $('<span class="jqCron-period"></span>');
		var _$blockDOM    = $('<span class="jqCron-dom"></span>');
		var _$blockMONTH  = $('<span class="jqCron-month"></span>');
		var _$blockMINS   = $('<span class="jqCron-mins"></span>');
		var _$blockFL     = $('<span class="jqCron-fl"></span>');
		var _$blockDOW    = $('<span class="jqCron-dow"></span>');
		var _$blockTIME   = $('<span class="jqCron-time"></span>');
		var _$cross       = $('<span class="jqCron-cross">&#10008;</span>');
		var _selectors    = [];
		var _selectorPeriod, _selectorMins, _selectorTimeH, _selectorTimeM, _selectorFL, _selectorDow, _selectorDom, _selectorMonth;

		var default_values = {
			hour: '0 0 * ? * *',
			day: '0 0 0 ? * *',
			week: '0 0 0 ? * 1',
			month: '0 0 0 ? * 1#1'
		};

		var dowText, $dowText;

		// instanciate a new selector
		function newSelector($block, multiple, type){
			var selector = new jqCronSelector(_self, $block, multiple, type);
			selector.$.bind('selector:open', function(){
				// we close all opened selectors of all other jqCron
				for(var n = jqCronInstances.length; n--; ){
					if(jqCronInstances[n] != _self) {
						jqCronInstances[n].closeSelectors();
					}
					else {
						// we close all other opened selectors of this jqCron
						for(var o = _selectors.length; o--; ){
							if(_selectors[o] != selector) {
								_selectors[o].close();
							}
						}
					}
				}
			});
			selector.$.bind('selector:change', function(){
				var boundChanged = false;
				// don't propagate if not initialized
				if(!_initialized) return;
				// bind data between two minute selectors (only if they have the same multiple settings)
				if(settings.multiple_mins == settings.multiple_time_minutes) {
					if(selector == _selectorMins) {
						boundChanged = _selectorTimeM.setValue(_selectorMins.getValue());
					}
					else if(selector == _selectorTimeM) {
						boundChanged = _selectorMins.setValue(_selectorTimeM.getValue());
					}
				}
				// we propagate the change event to the main object
				boundChanged || _$obj.trigger('cron:change', _self.getCron());
			});
			_selectors.push(selector);
			return selector;
		}

		// disable the selector
		this.disable = function(){
			_$obj.addClass('disable');
			settings.disable = true;
			_self.closeSelectors();
		};

		// return if the selector is disabled
		this.isDisabled = function() {
			return settings.disable == true;
		};

		// enable the selector
		this.enable = function(){
			_$obj.removeClass('disable');
			settings.disable = false;
		};

		// get cron value
		this.getCron = function(){
			var period = _selectorPeriod.getValue();
			var items = ['0', '*', '*', '?', '*', '*'];
			if(period == 'hour') {
				items[1] = _selectorMins.getCronValue();
			}
			if(period == 'day' || period == 'week' || period == 'month' || period == 'year') {
				items[1] = _selectorTimeM.getCronValue();
				items[2] = _selectorTimeH.getCronValue();
			}
			if(period == 'month') {
				items[5] = _selectorDow.getCronValue();
				if (items[5] !== "*") {
					var val = _selectorFL.getCronValue();
					items[5] += val === "*" ? "" : val;
				}
			}
			if(period == 'year') {
				items[3] = _selectorDom.getCronValue();
				items[4] = _selectorMonth.getCronValue();
				items[5] = "?";
			}
			if(period == 'week') {
				items[5] = _selectorDow.getCronValue();
			}
			return items.join(' ');
		};

		// set cron (string like * * * * *)
		this.setCron = function(str) {
			if(!str) return;
			try {
				str = str.slice(2);
				str = str.replace(/\s+/g, ' ').replace(/^ +/, '').replace(/ +$/, ''); // sanitize
				var mask = str.replace("?", "*").replace(/[^\* ]/g, '-').replace(/-+/g, '-').replace(/ +/g, '');
				var items = str.split(' ');
				// if (items.length != 5) _self.error(_self.getText('error2'));
				if(mask == '*****') {						// 1 possibility
					_selectorPeriod.setValue('minute');
				}
				else if(mask == '-****') {					// 1 possibility
					_selectorPeriod.setValue('hour');
					_selectorMins.setCronValue(items[0]);
					_selectorTimeM.setCronValue(items[0]);
				}
				else if(mask.substring(2, mask.length) == '***') {			// 4 possibilities
					_selectorPeriod.setValue('day');
					_selectorMins.setCronValue(items[0]);
					_selectorTimeM.setCronValue(items[0]);
					_selectorTimeH.setCronValue(items[1]);
				}
				else if(mask.substring(2, mask.length) == '-**') {			// 4 possibilities
					_selectorPeriod.setValue('month');
					_selectorMins.setCronValue(items[0]);
					_selectorTimeM.setCronValue(items[0]);
					_selectorTimeH.setCronValue(items[1]);
					_selectorDom.setCronValue(items[2]);
				}
				else if(mask.substring(2, mask.length) == '**-') {			// 4 possibilities
					_selectorMins.setCronValue(items[0]);
					_selectorTimeM.setCronValue(items[0]);
					_selectorTimeH.setCronValue(items[1]);
					if (items[4].length === 1) {
						_selectorPeriod.setValue('week');
						_selectorDow.setCronValue(items[4]);
					} else {
						_selectorPeriod.setValue('month');
						_selectorDow.setCronValue(items[4][0]);
						_selectorFL.setCronValue(items[4][1]);
					}
				}
				else if (mask.substring(3, mask.length) == '-*') {			// 8 possibilities
					_selectorPeriod.setValue('year');
					_selectorMins.setCronValue(items[0]);
					_selectorTimeM.setCronValue(items[0]);
					_selectorTimeH.setCronValue(items[1]);
					_selectorDom.setCronValue(items[2]);
					_selectorMonth.setCronValue(items[3]);
				}
				else {
					_self.error(_self.getText('error4'));
				}
				_self.clearError();
			} catch(e) {}
		};

		// close all child selectors
		this.closeSelectors = function(){
			for(var n = _selectors.length; n--; ){
				_selectors[n].close();
			}
		};

		// get the main element id
		this.getId = function(){
			return _$elt.attr('id');
		}

		// get the translated text
		this.getText = function(key) {
			var text = settings.texts[settings.lang][key] || null;
			if(typeof(text) == "string" && text.match('<b')){
				text = text.replace(/(<b *\/>)/gi, '</span><b /><span class="jqCron-text">');
				text = '<span class="jqCron-text">' + text + '</span>';
			}
			return text;
		};

		// get the human readable text
		this.getHumanText = function() {
				var texts=[];
				_$obj
				.find('> span > span:visible')
				.find('.jqCron-text, .jqCron-selector > span')
				.each(function() {
						var text = $(this).text().replace(/\s+$/g, '').replace(/^\s+/g, '');
						text && texts.push(text);
				});
				return texts.join(' ').replace(/\s:\s/g, ':');
		}

		// get settings
		this.getSettings = function(){
			return settings;
		};

		// display an error
		this.error = function(msg) {
			console && console.error('[jqCron Error] ' + msg);
			_$obj.addClass('jqCron-error').attr('title', msg);
			throw msg;
		};

		// clear error
		this.clearError = function(){
			_$obj.attr('title', '').removeClass('jqCron-error');
		};

		// clear
		this.clear = function() {
			_selectorDom.setValue([]);
			_selectorFL.setValue([]);
			_selectorDow.setValue([]);
			_selectorMins.setValue([]);
			_selectorMonth.setValue([]);
			_selectorTimeH.setValue([]);
			_selectorTimeM.setValue([]);
			_self.triggerChange();
		};

		// init (called in constructor)
		this.init = function(){
			var n,i,list;
			if(_initialized) return;

			settings = jqCronMergeSettings(settings);
			settings.jquery_element || _self.error(_self.getText('error3'));
			_$elt = settings.jquery_element;
			_$elt.append(_$obj);
			_$obj.data('id', settings.id);
			_$obj.data('jqCron', _self);
			_$obj.append(_$blocks);
			settings.no_reset_button || _$obj.append(_$cross);
			(!settings.disable) || _$obj.addClass('disable');
			_$blocks.append(_$blockPERIOD);

			if ( /^(ko)$/i.test(settings.lang) )
			{
				_$blocks.append(_$blockMONTH, _$blockDOM);
			}
			else
			{
				_$blocks.append(_$blockDOM, _$blockMONTH);
			}

			_$blocks.append(_$blockMINS);
			_$blocks.append(_$blockFL);
			_$blocks.append(_$blockDOW);
			_$blocks.append(_$blockTIME);

			// various binding
			_$cross.on("click", function(){
				_self.isDisabled() || _self.clear();
			});

			// binding from cron to target
			_$obj.bind('cron:change', function(evt, value){
				if(!settings.bind_to) return;
				settings.bind_method.set && settings.bind_method.set(settings.bind_to, value);
				_self.clearError();
			});

			// PERIOD
			_$blockPERIOD.append(_self.getText('text_period'));
			_selectorPeriod = newSelector(_$blockPERIOD, false, 'period');
			settings.enabled_minute && _selectorPeriod.add('minute', _self.getText('name_minute'));
			settings.enabled_hour   && _selectorPeriod.add('hour',   _self.getText('name_hour'));
			settings.enabled_day    && _selectorPeriod.add('day',    _self.getText('name_day'));
			settings.enabled_week   && _selectorPeriod.add('week',   _self.getText('name_week'));
			settings.enabled_month  && _selectorPeriod.add('month',  _self.getText('name_month'));
			settings.enabled_year   && _selectorPeriod.add('year',   _self.getText('name_year'));
			_selectorPeriod.$.bind('selector:change', function(e, value){
				_$blockDOM.hide();
				_$blockMONTH.hide();
				_$blockMINS.hide();
				_$blockFL.hide();
				_$blockDOW.hide();
				_$blockTIME.hide();
				if(value == 'hour') {
					_$blockMINS.show();
				}
				else if(value == 'day') {
					_$blockTIME.show();
				}
				else if(value == 'week') {
					$dowText.text(dowText);
					_$blockDOW.show();
					_$blockTIME.show();
				}
				else if(value == 'month') {
					_$blockFL.show();
					$dowText.text(" ");
					_$blockDOW.show();
					//_$blockDOM.show();
					_$blockTIME.show();
				}
				else if(value == 'year') {
					_$blockDOM.show();
					_$blockMONTH.show();
					_$blockTIME.show();
				}
				_self.setCron(default_values[value]);
			});
			_selectorPeriod.setValue(settings.default_period);

			// MINS  (minutes)
			_$blockMINS.append(_self.getText('text_mins'));
			_selectorMins = newSelector(_$blockMINS, settings.multiple_mins, 'minutes');
			for(i=0, list=settings.minutes; i<list.length; i++){
				_selectorMins.add(list[i], list[i] < 10 ? "0" + list[i] : list[i]);
			}

			// TIME  (hour:min)
			_$blockTIME.append(_self.getText('text_time'));
			_selectorTimeH = newSelector(_$blockTIME, settings.multiple_time_hours, 'time_hours');
			for(i=0, list=settings.hours, labelsList=settings.hour_labels; i<list.length; i++){
				_selectorTimeH.add(list[i], labelsList[i]);
			}
			_selectorTimeM = newSelector(_$blockTIME, settings.multiple_time_minutes, 'time_minutes');
			for(i=0, list=settings.minutes; i<list.length; i++){
				_selectorTimeM.add(list[i], list[i] < 10 ? "0" + list[i] : list[i]);
			}

			// FL  (first or last)
			_$blockFL.append(_self.getText('text_dow'));
			_selectorFL = newSelector(_$blockFL, false, 'fl');
			var flValues = ["#1", "L"];
			for(i=0, list=_self.getText('first_last'); i<list.length; i++){
				_selectorFL.add(flValues[i], list[i]);
			}

			// DOW  (day of week)
			_$blockDOW.append(_self.getText('text_dow'));
			_selectorDow = newSelector(_$blockDOW, settings.multiple_dow, 'day_of_week');
			for(i=0, list=_self.getText('weekdays'); i<list.length; i++){
				_selectorDow.add(i+1, list[i]);
			}

			$dowText = _$blockDOW.children().first(); 
			dowText = $dowText.text();

			// DOM  (day of month)
			_$blockDOM.append(_self.getText('text_dom'));
			_selectorDom = newSelector(_$blockDOM, settings.multiple_dom, 'day_of_month');
			for(i=0, list=settings.monthdays; i<list.length; i++){
				_selectorDom.add(list[i], list[i]);
			}

			// MONTH  (day of week)
			_$blockMONTH.append(_self.getText('text_month'));
			_selectorMonth = newSelector(_$blockMONTH, settings.multiple_month, 'month');
			for(i=0, list=_self.getText('months'); i<list.length; i++){
				_selectorMonth.add(i+1, list[i]);
			}

			// close all selectors when we click in body
			$('body').on("click", function(){
				var i, n = _selectors.length;
				for(i = 0; i < n; i++){
					_selectors[i].close();
				}
			});
			_initialized = true;

			// default value
			if(settings.default_value) {
				_self.setCron(settings.default_value);
			}
		};

		// trigger a change event
		this.triggerChange = function(){
			_$obj.trigger('cron:change', _self.getCron());
		};

		// store instance in array
		jqCronInstances.push(this);

		// expose main jquery object
		this.$ = _$obj;

		// init
		//try {
			this.init();
			_self.triggerChange();
		//} catch(e){}
	}
	this.jqCron = jqCron;
}).call(this, jQuery);


/**
 * jqCronSelector class
 */
(function($){
	function jqCronSelector(_cron, _$block, _multiple, _type){
		var _self      = this;
		var _$list     = $('<ul class="jqCron-selector-list"></ul>');
		var _$title    = $('<span class="jqCron-selector-title"></span>');
		var _$selector = $('<span class="jqCron-selector"></span>');
		var _values    = {};
		var _value     = [];
		var _hasNumericTexts = true;
		var _numeric_zero_pad = _cron.getSettings().numeric_zero_pad;

		// return an array without doublon
		function array_unique(l){
			var i=0,n=l.length,k={},a=[];
			while(i<n) {
				k[l[i]] || (k[l[i]] = 1 && a.push(l[i]));
				i++;
			}
			return a;
		}

		// get the value (an array if multiple, else a single value)
		this.getValue = function(){
			return _multiple ? _value : _value[0];
		};

		// get a correct string for cron
		this.getCronValue = function(){
			if(_value.length == 0) return '*';
			var cron = [_value[0]], i, s = _value[0], c = _value[0], n = _value.length;
			for(i=1; i<n; i++) {
				if(_value[i] == c+1) {
					c = _value[i];
					cron[cron.length-1] = s+'-'+c;
				}
				else {
					s = c = _value[i];
					cron.push(c);
				}
			}
			return cron.join(',');
		};

		// set the cron value
		this.setCronValue = function(str) {
			var values = [], m ,i, n;
			if(str !== '*') {
				while(str != '') {
					// test "*/n" expression
					m = str.match(/^\*\/([0-9]+),?/);
					if(m && m.length == 2) {
						for(i=0; i<=59; i+=(m[1]|0)) {
							values.push(i);
						}
						str = str.replace(m[0], '');
						continue;
					}
					// test "a-b/n" expression
					m = str.match(/^([0-9]+)-([0-9]+)\/([0-9]+),?/);
					if(m && m.length == 4) {
						for(i=(m[1]|0); i<=(m[2]|0); i+=(m[3]|0)) {
							values.push(i);
						}
						str = str.replace(m[0], '');
						continue;
					}
					// test "a-b" expression
					m = str.match(/^([0-9]+)-([0-9]+),?/);
					if(m && m.length == 3) {
						for(i=(m[1]|0); i<=(m[2]|0); i++) {
							values.push(i);
						}
						str = str.replace(m[0], '');
						continue;
					}
					// test "c" expression
					m = str.match(/^([0-9]+),?/);
					if(m && m.length == 2) {
						values.push(m[1]|0);
						str = str.replace(m[0], '');
						continue;
					}

					// test last or first dow
					if (str === "L") {
						values.push("L");
						str = "";
						continue;
					}
					if (str === "#") {
						values.push("#1");
						str = "";
						continue;
					}
					// something goes wrong in the expression
					return ;
				}
			}
			_self.setValue(values);
		};

		// close the selector
		this.close = function(){
			_$selector.trigger('selector:close');
		};

		// open the selector
		this.open = function(){
			_$selector.trigger('selector:open');
		};

		// whether the selector is open
		this.isOpened = function() {
			return _$list.is(':visible');
		};

		// add a selected value to the list
		this.addValue = function(key) {
			var values = _multiple ? _value.slice(0) : []; // clone array
			values.push(key);
			_self.setValue(values);
		};

		// remove a selected value from the list
		this.removeValue = function(key) {
			if(_multiple) {
				var i, newValue = [];
				for(i=0; i<_value.length; i++){
					if(key != [_value[i]]) {
						newValue.push(_value[i]);
					}
				}
				_self.setValue(newValue);
			}
			else {
				_self.clear();
			}
		};

		// set the selected value(s) of the list
		this.setValue = function(keys){
			var i, newKeys = [], saved = _value.join(' ');
			if(!Array.isArray(keys)) keys = [keys];
			_$list.find('li').removeClass('selected');
			keys = array_unique(keys);
			keys.sort(function(a, b){
				var ta = typeof(a);
				var tb = typeof(b);
				if(ta==tb && ta=="number") return a-b;
				else return String(a) == String(b) ? 0 : (String(a) < String(b) ? -1 : 1);
			});
			if(_multiple) {
				for(i=0; i<keys.length; i++){
					if(keys[i] in _values) {
						_values[keys[i]].addClass('selected');
						newKeys.push(keys[i]);
					}
				}
			}
			else {
				if(keys[0] in _values) {
					_values[keys[0]].addClass('selected');
					newKeys.push(keys[0]);
				}
			}
			// remove unallowed values
			_value = newKeys;
			if(saved != _value.join(' ')) {
				_$selector.trigger('selector:change', _multiple ? keys : keys[0]);
				return true;
			}
			return false;
		};

		// get the title text
		this.getTitleText = function(){
			var getValueText = function(key) {
				return (key in _values) ? _values[key].text() : key;
			};

			if(_value.length == 0) {
				return _cron.getText('empty_' + _type) || _cron.getText('empty');
			}
			var cron = [getValueText(_value[0])], i, s = _value[0], c = _value[0], n = _value.length;
			for(i=1; i<n; i++) {
				if(_value[i] == c+1) {
					c = _value[i];
					cron[cron.length-1] = getValueText(s)+'-'+getValueText(c);
				}
				else {
					s = c = _value[i];
					cron.push(getValueText(c));
				}
			}
			return cron.join(',');
		};

		// clear list
		this.clear = function() {
			_values = {};
			_self.setValue([]);
			_$list.empty();
		};

		// add a (key, value) pair
		this.add = function(key, value) {
			if(!(value+'').match(/^[0-9]+$/)) _hasNumericTexts = false;
			if(_numeric_zero_pad && _hasNumericTexts && value < 10) {
				value = '0'+value;
			}
			var $item = $('<li>' + value + '</li>');
			_$list.append($item);
			_values[key] = $item;
			$item.on("click", function(){
				if(_multiple && $(this).hasClass('selected')) {
					_self.removeValue(key);
				}
				else {
					_self.addValue(key);
					if(!_multiple) _self.close();
				}
			});
		};

		// expose main jquery object
		this.$ = _$selector;

		// constructor
		_$block.find('b:eq(0)').after(_$selector).remove();
		_$selector
		.addClass('jqCron-selector-' + _$block.find('.jqCron-selector').length)
		.append(_$title)
		.append(_$list)
		.bind('selector:open', function(){
			if(_hasNumericTexts) {
				var nbcols = 1, n = _$list.find('li').length;
				if(n > 5 && n <= 16) nbcols = 2;
				else if(n > 16 && n <= 23) nbcols = 3;
				else if(n > 23 && n <= 40) nbcols = 4;
				else if(n > 40) nbcols = 5;
				_$list.addClass('cols'+nbcols);
			}
			_$list.show();
		})
		.bind('selector:close', function(){
			_$list.hide();
		})
		.bind('selector:change', function(){
			_$title.html(_self.getTitleText());
		})
		.on("click", function(e){
			e.stopPropagation();
		})
		.trigger('selector:change')
		;
		$.fn.disableSelection && _$selector.disableSelection(); // only work with jQuery UI
		_$title.on("click", function(e){
			(_self.isOpened() || _cron.isDisabled()) ? _self.close() : _self.open();
		});
		_self.close();
		_self.clear();
	}
	this.jqCronSelector = jqCronSelector;
}).call(this, jQuery);

/**
 * Generate unique id for each element.
 * Skip elements which have already an id.
 */
(function($){
	var jqUID = 0;
	var jqGetUID = function(prefix){
		var id;
		while(1) {
			jqUID++;
			id = ((prefix || 'JQUID')+'') + jqUID;
			if(!document.getElementById(id)) return id;
		}
	};
	$.fn.uniqueId =  function(prefix) {
		return this.each(function(){
			if($(this).attr('id')) return;
			var id = jqGetUID(prefix);
			$(this).attr('id', id);
		});
	};
}).call(this, jQuery);


/**
 * Extends jQuery selectors with new block selector
 */
(function($){
	$.extend($.expr.pseudos, {
		container: function(a) {
			return (a.tagName+'').toLowerCase() in {
				a:1,
				abbr:1,
				acronym:1,
				address:1,
				b:1,
				big:1,
				blockquote:1,
				button:1,
				cite:1,
				code:1,
				dd: 1,
				del:1,
				dfn:1,
				div:1,
				dt:1,
				em:1,
				fieldset:1,
				form:1,
				h1:1,
				h2:1,
				h3:1,
				h4:1,
				h5:1,
				h6: 1,
				i:1,
				ins:1,
				kbd:1,
				label:1,
				li:1,
				p:1,
				pre:1,
				q:1,
				samp:1,
				small:1,
				span:1,
				strong:1,
				sub: 1,
				sup:1,
				td:1,
				tt:1
			};
		},
		autoclose: function(a) {
			return (a.tagName+'').toLowerCase() in {
				area:1,
				base:1,
				basefont:1,
				br:1,
				col:1,
				frame:1,
				hr:1,
				img:1,
				input:1,
				link:1,
				meta:1,
				param:1
			};
		}
	});
}).call(this, jQuery);
