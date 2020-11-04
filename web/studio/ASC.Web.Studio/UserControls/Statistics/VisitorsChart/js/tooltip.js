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


(function () {
  if (typeof window.ASC === 'undefined') {
    window.ASC = {};
  }
  if (typeof window.ASC.Common === 'undefined') {
    window.ASC.Common = {};
  }

  window.ASC.Common.toolTip = (function () {
    var
      wrapperId = '',
      wrapperClassName = 'tooltip-wrapper',
      wrapperHandler = null;

    var uniqueId = function (prefix) {
      return (typeof prefix != 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
    };

    var create = function () {
      if (!wrapperHandler) {
        wrapperId = uniqueId('tooltipWrapper');
        wrapperHandler = document.createElement('div');
        wrapperHandler.id = wrapperId;
        wrapperHandler.className = wrapperClassName;
        wrapperHandler.style.display = 'none';
        wrapperHandler.style.left = '0';
        wrapperHandler.style.top = '0';
        wrapperHandler.style.position = 'absolute';
        document.body.appendChild(wrapperHandler);
      }
      return wrapperHandler;
    };

    var show = function (content, handler) {
      create();
      wrapperHandler.innerHTML = content;
      wrapperHandler.style.display = 'block';

      if (typeof handler === 'function') {
        handler.call(wrapperHandler);
      }
    };

    var hide = function () {
      if (wrapperHandler && typeof wrapperHandler === 'object') {
        wrapperHandler.style.display = 'none';
      }
    };

    return {
      show  : show,
      hide  : hide
    }
  })();
})();
