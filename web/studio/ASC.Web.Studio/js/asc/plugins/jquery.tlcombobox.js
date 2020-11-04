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


// tl combobox
(function ($, win, doc, body) {
  function converText (str) {
    var o = document.createElement('textarea');
    o.innerHTML = str;
    return o.value;
  }

  function onBodyClick (evt) {
    $(document.body).unbind('click', onBodyClick);
    jQuery('span.tl-combobox').addClass('un-showed').removeClass('showed-options').find('div.combobox-container:first').hide();
    setTimeout(function () {
      jQuery('span.tl-combobox').removeClass('un-showed');
    }, 0);
  }

  function onHelperFocus (evt, needTitleClick) {
    if (needTitleClick !== false) {
      onComboboxTitleClick(evt);
    }
  }

  function onHelperKeyup (evt) {
    return false;
  }

  function onHelperKeypress (evt) {
    switch (evt.keyCode) {
      case 9  :   //tab
        onBodyClick();
        return undefined;
      case 13 :   //enter
        onBodyClick();
        setFocusedOption(this);
        break;
      case 38 :   //up
      case 40 :   //down
        changeFocusedOption(this, evt.keyCode);
        break;
    }
    return false;
  }

  function onSelectFocus (evt, needTitleClick) {
    jQuery(this).parents('span.tl-combobox:first').find('input:first').trigger('focus', [needTitleClick]);
  }

  function onSelectChange (evt) {
    var
        $this = jQuery(this),
        $combobox = $this.parents('span.tl-combobox:first'),
        $select = $combobox.find('select:first'),
        $helper = $combobox.find('input:first'),
        $option = null,
        value = $this.val();

      if (!value && !this.value){
            return;
        }

      if (value == null && typeof this.value != "undefined"){
            value = this.value;
        }
      var
        title = $this.find('option[value="' + value + '"]').text(),
        classname = '',
        classes = $combobox.attr('class').split(/\s+/),
        classesInd = 0;
        classesInd = classes ? classes.length : 0;
    while (classesInd--) {
      classname = classes[classesInd];
      if (classname.indexOf('select-value-') === 0) {
        $combobox.removeClass(classname);
      }
    }

    $combobox.find('li.in-focus').removeClass('in-focus');
    $combobox.find('div.combobox-title-inner-text:first').text(title || ' ');
    $combobox.find('div.combobox-title-inner-text:first').attr('title' , title || ' ');
    $combobox.attr('data-value', value).addClass('select-value-' + value)
      .find('li.option-item[data-value="' + value + '"]:first').addClass('selected-item').siblings().removeClass('selected-item');

  //  $helper.trigger('focus', [false]);
  }

  function onComboboxTitleClick(evt) {
    var
      $combobox = jQuery(evt.target).parents('span.tl-combobox:first');

    if ($combobox.hasClass('disabled') || $combobox.hasClass('un-showed')) {
      return undefined;
    }

    if ($combobox.hasClass('showed-options')) {
      return undefined;
    }

    onBodyClick();
    $combobox.addClass('showed-options').find('div.combobox-container:first').show();

    var
      $select = $combobox.find('select:first'),
      $helper = $combobox.find('input:first'),
      $options = $combobox.find('li.option-item'),
      $selected = $options.filter('.selected-item:first');
    
    $options.removeClass('in-focus');
    if ($selected.length === 0) {
      $selected = $options.not('.hidden').filter(':first');
    }
    if ($selected.length > 0) {
      var selected = $selected[0];
      selected.parentNode.scrollTop = selected.offsetTop;
    }
  //  $helper.trigger('focus', [false]);

    setTimeout(bindBodyEvents, 1);
  }

  function onComboboxOptionHover (evt) {
    jQuery(this).addClass('in-focus').siblings().removeClass('in-focus');
  }

  function onComboboxOptionClick (evt) {
    var
      $combobox = jQuery(this).parents('span.tl-combobox:first');

    if ($combobox.hasClass('disabled')) {
      return undefined;
    }

    var
      $select = $combobox.find('select:first'),
      $target = jQuery(evt.target),
      value = $target.attr('data-value');

    $select.val(value).change();
  }

  function setFocusedOption (helper) {
    var
      $combobox = jQuery(helper).parents('span.tl-combobox:first');

    if ($combobox.hasClass('disabled')) {
      return undefined;
    }

    var
      $select = $combobox.find('select:first'),
      $target = $combobox.find('li.option-item.in-focus'),
      value = $target.attr('data-value');

    if (value) {
      $select.val(value).change();
    }
  }

  function changeFocusedOption (helper, keycode) {
    var
      $combobox = jQuery(helper).parents('span.tl-combobox:first');

    if ($combobox.hasClass('disabled')) {
      return undefined;
    }

    var
      $options = $combobox.find('li.option-item').not('.hidden'),
      $infocus = $options.filter('.in-focus');

    if ($infocus.length === 0) {
      $infocus = $options.filter('.selected-item:first');
    }

    switch (keycode) {
      case 38 :   //up
        while(($infocus = $infocus.prev()).length > 0) {
          if (!$infocus.hasClass('hidden')) {
            break;
          }
        }
        //$infocus = $infocus.prev();
        if ($infocus.length === 0) {
          $infocus = $options.filter(':first');
        }
        break;
      case 40 :   //down
        while(($infocus = $infocus.next()).length > 0) {
          if (!$infocus.hasClass('hidden')) {
            break;
          }
        }
        //$infocus = $infocus.next();
        if ($infocus.length === 0) {
          $infocus = $options.filter(':last');
        }
        break;
    }
    if ($infocus.length > 0) {
      var
        infocus = $infocus[0],
        parent = infocus.parentNode;
      if (infocus.offsetTop < parent.scrollTop) {
        parent.scrollTop = infocus.offsetTop;
      } else if (infocus.offsetTop + infocus.offsetHeight > parent.scrollTop + parent.offsetHeight) {
        parent.scrollTop = infocus.offsetTop - parent.offsetHeight + infocus.offsetHeight;
      }
    }
    $infocus.addClass('in-focus').siblings().removeClass('in-focus');
  }

  function renderOption (option, datavalue) {
    return [
        '<li',
          ' class="option-item',
            option.classname ? ' ' + option.classname : '',
            option.selected === true ? ' selected-item' : '',
            option.disabled === true ? ' display-none' : '',
          '"',
          ' data-value="' + datavalue + '"',
          ' title="' + (Encoder.htmlEncode(jQuery.trim(option.title))  || '&nbsp;') + '"',
        '>',
        option.title || '&nbsp;',
        '</li>'
    ];
  }

  function renderCombobox(select, options) {
    var
      html = [],
      option = null,
      optionsvalue = [],
      selectclassname = select.className,
      selectoption = options.length ? options[0] : { title: "", value: "" };

    for (var i = 0, n = options.length; i < n; i++) {
      option = options[i];
      if (option.selected === true) {
        selectoption = option;
      }
      optionsvalue.push(option.value);
      html = html.concat(renderOption(option, optionsvalue.length - 1));
    }

    html = [
      '<div class="combobox-title">',
        '<div class="inner-text combobox-title-inner-text" title="' + (Encoder.htmlEncode(jQuery.trim(selectoption.title)) || '&nbsp;') + '">',
          selectoption.title || '&nbsp;',
        '</div>',
      '</div>',
      '<div class="combobox-wrapper">',
        '<div class="combobox-container">',
          '<ul class="combobox-options">',
            html.join(''),
          '</ul>',
        '</div>',
      '</div>'
    ];

    var o = doc.createElement('span');
    o.className = 'tl-combobox' + ' tl-combobox-container' + ' select-value-' + selectoption.value + (selectclassname ? ' ' + selectclassname : '');
    //if (typeof select.getAttribute('disabled') === 'string') {
    //  o.className += ' disabled';
    //}
    o.innerHTML = html.join('');

    var
      optionsvalueLen = optionsvalue.length,
      value = null,
      node = null,
      nodes = o.getElementsByTagName('li'),
      nodesInd = 0;
    nodesInd = nodes ? nodes.length : 0;
    while (nodesInd--) {
      node = nodes[nodesInd];
      value = node.getAttribute('data-value');
      value = isFinite(+value) ? +value : -1;
      if (value > -1 && value < optionsvalueLen) {
        node.setAttribute('data-value', optionsvalue[value]);
      }
    }
    return o;
  }

  function reRenderCombobox(o, select, options) {
    var
      html = [],
      option = null,
      optionsvalue = [],
      selectclassname = select.className,
      selectoption = options.length ? options[0] : null;

    for (var i = 0, n = options.length; i < n; i++) {
      option = options[i];
      if (option.selected === true) {
        selectoption = option;
      }
      optionsvalue.push(option.value);
      html = html.concat(renderOption(option, optionsvalue.length - 1));
    }

    try {
      var ul = select.previousSibling.firstChild.lastChild;
      ul.innerHTML = html.join('');
    } catch (err) {}

    if (selectoption != null) {
      try {
        var title = o.firstChild.firstChild;
        title.innerHTML = selectoption.title || '&nbsp;';
        title.setAttribute('title', converText(selectoption.title || '&nbsp;'));
        o.className = 'tl-combobox' + ' tl-combobox-container' + ' select-value-' + selectoption.value + (selectclassname ? ' ' + selectclassname : '');
      } catch (err) {}
    }

    var
      optionsvalueLen = optionsvalue.length,
      value = null,
      node = null,
      nodes = o.getElementsByTagName('li'),
      nodesInd = 0;
    nodesInd = nodes ? nodes.length : 0;
    while (nodesInd--) {
      node = nodes[nodesInd];
      value = node.getAttribute('data-value');
      value = isFinite(+value) ? +value : -1;
      if (value > -1 && value < optionsvalueLen) {
        node.setAttribute('data-value', optionsvalue[value]);
      }
    }

    return o;
  }

  function updateSelect (select) {
    var
      o = null,
      selectvalue = select.value,
      opts = [],
      optionvalue = null,
      options = null,
      optionsInd = 0,
      option = null;

    onBodyClick();

    options = select.getElementsByTagName('option');
    optionsInd = options ? options.length : 0;

    if (select.selectedIndex == -1 && options.length) {
        select.selectedIndex = 0;
        selectvalue = select.value;
    }

    while (optionsInd--) {
      option = options[optionsInd];
      optionvalue = option.getAttribute('value');
      opts.unshift({ classname: option.className, value: optionvalue, title: option.innerHTML, selected: optionvalue == selectvalue, disabled: option.disabled });
    }

    if (select.className.indexOf('tl-combobox') === -1) {
      o = renderCombobox(select, opts);
      if (o) {
        select.parentNode.insertBefore(o, select);
        o.appendChild(select);
        if (select.className.indexOf('none-helper') === -1) {
          var helper = document.createElement('input');
          o.appendChild(helper);
        }
        o.setAttribute('data-value', selectvalue);
        return o;
      }
    } else {
      o = select.parentNode;
      if (o) {
        reRenderCombobox(o, select, opts);
        o.setAttribute('data-value', selectvalue);
        return o;
      }
    }

    return null;
  }

  function bindBodyEvents () {
    jQuery(document.body).one('click', onBodyClick);
  }

  function bindComboboxEvents ($select, $combobox) {
    //setTimeout((function ($select) {
    //  return function () {
    //    $select.unbind('focus', onSelectFocus).bind('focus', onSelectFocus);
    //  };
    //})($select), 500);
    $select
      //.blur(onSelectBlur)
      //.focus(onSelectFocus)
      .unbind('change', onSelectChange).bind('change', onSelectChange);
    $combobox.find('input')
      .unbind('focus', onHelperFocus).bind('focus', onHelperFocus)
      .unbind('keydown', onHelperKeypress).bind('keydown', onHelperKeypress)
      .unbind('keyup', onHelperKeyup).bind('keyup', onHelperKeyup);
    $combobox.find('ul.combobox-options:first')
      .unbind('click', onComboboxOptionClick).bind('click', onComboboxOptionClick)
      .find('li.option-item')
        .unbind('hover', onComboboxOptionHover).bind('hover', onComboboxOptionHover);
    $combobox.find('div.combobox-title:first')
      .unbind('click', onComboboxTitleClick).bind('click', onComboboxTitleClick);
  }

  $.fn.tlCombobox = $.fn.tlcombobox = function (params) {
    var
      //wasupdated = false,
      select = null,
      combobox = null,
      $selects = $(this),
      selectsInd = 0,
      $select = null;

    if (typeof params === 'boolean') {
      selectsInd = $selects.length;
      while (selectsInd--) {
        if (params === true) {
          $($selects[selectsInd]).attr('disabled', false).parents('span.tl-combobox:first').removeClass('disabled');
        } else {
          $($selects[selectsInd]).attr('disabled', true).parents('span.tl-combobox:first').addClass('disabled');
        }
      }
      return $selects;
    }

    if (typeof params === 'string') {
      switch (params) {
        case 'show' :
          $($selects[selectsInd]).show().parents('span.tl-combobox:first').show();
          return $selects;
        case 'hide' :
          $($selects[selectsInd]).hide().parents('span.tl-combobox:first').hide();
          return $selects;
      }
    }

    selectsInd = $selects.length;
    while (selectsInd--) {
      select = $selects[selectsInd];
      $select = $(select);

      combobox = updateSelect(select);
      bindComboboxEvents($select, $(combobox));
      //setTimeout((function ($select, $combobox, callback) {
      //  return function () {
      //    callback($select, $combobox);
      //  };
      //})($(select), $(combobox), bindComboboxEvents), 500);
      $select.addClass('tl-combobox');

      if (params && params.hasOwnProperty('align')) {
          if (params.align == 'left') {
              $select.parents('span.tl-combobox:first').addClass('left-align');
          } else {
              $select.parents('span.tl-combobox:first').removeClass('left-align');
          }
      }

      //wasupdated = true;
    }

    //if (wasupdated) {
    //  var
    //    zindex = 0,
    //    $comboboxes = $('span.tl-combobox'),
    //    comboboxesInd = $comboboxes.length;
    //  while (comboboxesInd--) {
    //    $($comboboxes[comboboxesInd]).css('zIndex', ++zindex);
    //  }
    //}
    return $selects;
  };
})(jQuery, window, document, document.body);