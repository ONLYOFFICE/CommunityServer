/*
 * ShortURL (https://github.com/delight-im/ShortURL)
 * Copyright (c) delight.im (https://www.delight.im/)
 * Licensed under the MIT License (https://opensource.org/licenses/MIT)
 */

/**
 * ShortURL: Bijective conversion between natural numbers (IDs) and short strings
 *
 * ShortURL.encode() takes an ID and turns it into a short string
 * ShortURL.decode() takes a short string and turns it into an ID
 *
 * Features:
 * + large alphabet (51 chars) and thus very short resulting strings
 * + proof against offensive words (removed 'a', 'e', 'i', 'o' and 'u')
 * + unambiguous (removed 'I', 'l', '1', 'O' and '0')
 *
 * Example output:
 * 123456789 <=> pgK8p
 */

 //'23456789bcdfghjkmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ-_'

var _alphabet = "5XzpDt6wZRdsTrJkSY_cgPyxN4j-fnb9WKBF8vh3GH72QqmLVCM",
	_base = _alphabet.length,
	_initial = _base * _base;

module.exports = {

	encode: function(num) {
		num += _initial;
		var str = '';
		while (num > 0) {
			str = _alphabet.charAt(num % _base) + str;
			num = Math.floor(num / _base);
		}
		return str;
	},

	decode: function(str) {
		var num = 0;
		for (var i = 0; i < str.length; i++) {
            let index = _alphabet.indexOf(str.charAt(i));
            if (index < 0) return null;
			num = num * _base + index;
		}
		return num - _initial;
    }

};