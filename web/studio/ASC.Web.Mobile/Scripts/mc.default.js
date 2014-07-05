/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


;(function ($) {
  var platform = (function () {
    var 
        ua = navigator.userAgent,
        android = /android/i.test(ua),
        ver = (ua.match(/.+(?:rv|it|ra|ie|ox|me|id|on|os)[\/:\s]([\d._]+)/i) || [0, '0'])[1].replace('_', '');

    if (android) {
        ver = (ua.match(/.+(?:rv|ra|ie|ox|me|id|os)[\/:\s]([\d._]+)/i) || [0, '0'])[1].replace('_', '');
    }

    return {
        version     : isFinite(parseFloat(ver)) ? parseFloat(ver) : ver,
        operamobile : /opera/i.test(ua) && /mobi/i.test(ua),
        android     : /android/i.test(ua),
        ios         : /iphone|ipad/i.test(ua),
        ipod        : /ipod/i.test(ua),
        phonegap    : !!window.PHONEGAP
    }
  })();

  $.extend($, {platform : platform});

  var supportFixed = (!('ontouchend' in document) || ($.platform.android && $.platform.version > 2.2 && !$.platform.operamobile)) && true;
  var unsupportAnamatedImg = ('ontouchend' in document) && $.platform.android && $.platform.version < 2.2;

  $.extend(
    $.support,
    {
      webapp            : window.innerHeight === window.screen.availHeight,
      platform          : platform,
      orientation       : 'orientation' in window,
      touch             : 'ontouchend' in document,
      csstransitions    : 'WebKitTransitionEvent' in window,
      pushState         : !!history.pushState,
      cssPositionFixed  : !!supportFixed,
      iscroll           : ('ontouchend' in document) && !supportFixed && !!$.platform.ios && true,
      fscroll           : ('ontouchend' in document) && !supportFixed && !!$.platform.android && true,
      svg               : !($.browser.msie && $.browser.version < 9) && !$.platform.operamobile && !$.platform.android && true,
      imgsvg            : !$.browser.mozilla && !($.browser.msie && $.browser.version < 9) && true,
      imganimated       : !unsupportAnamatedImg,
      dataimage         : !($.browser.msie && $.browser.version < 9) && true,
      fileupload        : !!window.File && !$.platform.ios && !$.platform.operamobile && !($.platform.android && $.platform.version < 2.3) && true,
      filereader        : !!window.FileReader
    }
  );
})(jQuery);

;window.DefaultMobile = (function ($) {
    var
        screenMetrics = {width: null, height: null},
        $window = $(window),
        $document = $(document),
        $body = $(document.body),
        $mainform = $('#bodyWrapper'),
        //touchClick = $.support.touch ? 'touchend' : 'click',
        touchClick = 'click',
        touchStartEvent = $.support.touch ? 'touchstart' : 'mousedown',
        touchStopEvent = $.support.touch ? 'touchend' : 'mouseup',
        touchMoveEvent = $.support.touch ? 'touchmove' : 'mousemove';

    var converText = function (str, toText) {
        str = typeof str === 'string' ? str : '';
        if (!str) {
            return '';
        }

        if (toText === true) {
            var
                symbols = [
                    ['&lt;',  '<'],
                    ['&gt;',  '>'],
                    ['&and;', '\\^'],
                    ['&sim;', '~'],
                    ['&amp;', '&']
                ];

            var symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
            }
            return str;
        }

        var o = document.createElement('textarea');
        o.innerHTML = str;
        return o.value;
    };

    var setScreenMetrics = function () {
        if (screenMetrics.width === null && Math.abs(window.orientation) === 90) {
            screenMetrics.width = window.innerHeight;
        }
        if (screenMetrics.height === null && Math.abs(window.orientation) !== 90) {
            screenMetrics.height = window.innerHeight;
        }
    };

    var scrollToTop = (function () {
        if ($.platform.android) {
            return function () {
                window.scrollTo(0, 1);
            };
        }
        return function () {
          //  window.scrollTo(0, 0);
        };
    })();

    var scrollToItem = (function () {
        if ($.support.iscroll) {
            return function ($page, $item) {
                var scroller = $page.find('div.ui-scroller:first').data('iScroll');
                if (scroller) {
                    var offsettop = $item.length > 0 ? $item[0].offsetTop - 4 : -1;
                    if (offsettop !== -1) {
                        scroller.scrollTop(0, -offsettop, 0);
                    }
                }
            }
        };
        return function ($page, $item) {
            window.scrollTo(0, Math.round($item.offset().top));
        };
    })();

    var defaultResizeBody = (function () {
        if ($.support.touch) {
            return function () {
                var windowmetric = {
                    height: $.support.webapp ? window.innerHeight - 4 : window.screen.availHeight,
                    width: $.support.webapp ? window.innerHeight + 4 : window.screen.availWidth
                };

                var screenheight = Math.abs(window.orientation) === 90 ? windowmetric.width : windowmetric.height;

                $('#bodyWrapper').height(screenheight);
            };
        }
        return function () {
            //
        };
    })();

    var resizeBody = (function () {
        if ($.support.touch) {
            return function ($wrapper) {
                $wrapper = $wrapper && typeof $wrapper === 'object' ? $wrapper : $('#bodyWrapper');
                var windowmetric = {
                    height: $.support.webapp ? window.innerHeight - 4 : window.screen.availHeight - $body.find('div.ui-header:first').height() + 4,
                    width: $.support.webapp ? window.innerHeight + 4 : window.screen.availWidth - $body.find('div.ui-header:first').height() + 4
                };

                var screenheight = Math.abs(window.orientation) === 90 ? windowmetric.width - 4 : windowmetric.height + 4;
                screenheight = (Math.abs(window.orientation) === 90 ? screenMetrics.width : screenMetrics.height) || screenheight;

                //$wrapper.find('div.ui-page-active:first').css('top', -window.scrollY + 'px');
                $wrapper.find('div.ui-page-active:first').css('top', -document.documentElement.scrollTop + 'px');
                $wrapper.height(screenheight);
            };
        }
        return function ($wrapper) {
            //
        };
    })();

    var resizeDialog = (function () {
        return function ($dialog) {
            $dialog = $dialog && typeof $dialog === 'object' ? $dialog : $('div.ui-dialog.ui-dialog-active:first');
            var $page = $('div.ui-page.ui-page-active:first');
            var screenheight = $page.height();

            $dialog.height(screenheight);
        }
    })();

    var resizePage = (function () {
        if ($.support.touch) {
            return function ($page) {
                $page = $page && typeof $page === 'object' ? $page : $('div.ui-page.ui-change-page:first');
                var windowmetric = {
                    height: $.support.webapp ? window.innerHeight - 4 : window.screen.availHeight - $page.find('div.ui-header:first').height() + 4,
                    width: $.support.webapp ? window.innerHeight + 4 : window.screen.availWidth - $page.find('div.ui-header:first').height() + 4
                };

                var screenheight = Math.abs(window.orientation) === 90 ? windowmetric.width - 4 : windowmetric.height + 4;
                screenheight = (Math.abs(window.orientation) === 90 ? screenMetrics.width : screenMetrics.height) || screenheight;

                //if ($.platform.android) {
                //  screenheight += window.screenTop * 2;
                //}

                if ($page.removeClass('ui-resized-page').height('auto').height() <= screenheight) {
                    $page.height(screenheight).addClass('ui-resized-page');
                }
            };
        }
        return function ($page) {
            //
        };
    })(); 
  
    function onInputKeyPress(evt) {        
        $($(evt.target).parent()).after($(evt.target).parent().clone(true));
        $(evt.target).after('<button class="delete-field"></button>');
        jQuery(document).trigger('updatepage');
        $(evt.target).unbind();        
    } 
    
    var keyDownOnWkeInput = function ($page) {
        $page.find('input.wke').bind("keypress", function(e){
                onInputKeyPress(e);
        });
    };

     
    var renderAnimatedImages = (function () {
      if ($.support.imganimated) {
        return function ($page) {
        };
      }
      return function ($page) {
        var src = '', img = null, $images = $page.find('div.ui-item-content:first img[src$=".gif"]'), imagesInd = $images.length;
        while (imagesInd--) {
          img = $images[imagesInd];
          src = img.src;
          if (src.indexOf('/images/smiley/') !== -1) {
            img.src = src.replace('.gif', '.png');
          }
        }
      };
    })();

    var renderCustomSelects = function ($page) {
      $page.find('select').each(function () {
        var
          $select = $(this),
          $customSelect = $select.parent('div.custom-select:first');

        var
          selectValue = $select.val(),
          selectText = selectValue,
          $options = $select.find('option'),
          $optionsInd = $options.length;
        while ($optionsInd--) {
          if ($options[$optionsInd].value == selectValue) {
            selectText = $options[$optionsInd].innerHTML;
            break;
          }
        }

        if ($customSelect.length === 0) {
          var
            customSelect = document.createElement('div'),
            selectHelper = document.createElement('div'),
            selectTitle = document.createElement('span');

          selectTitle.className = 'inner-text';
          selectTitle.appendChild(document.createTextNode(selectText));
          selectHelper.className = 'helper-container';
          selectHelper.appendChild(selectTitle);
          customSelect.className = 'custom-select';
          customSelect.appendChild(selectHelper);
          $customSelect = $(customSelect);

          $select
            .before(customSelect)
            .appendTo(customSelect)
            .focus(function () {$(this).parent().addClass('focus')})
            .blur(function () {$(this).parent().removeClass('focus')})
            .change(function () {
              var
                $select = $(this),
                selectValue = $select.val(),
                selectText = selectValue,
                $options = $select.find('option'),
                $optionsInd = $options.length;

              while ($optionsInd--) {
                if ($options[$optionsInd].value == selectValue) {
                  selectText = $options[$optionsInd].innerHTML;
                  break;
                }
              }

              $select.parent().find('span.inner-text:first').text(converText(selectText));
            });
        }
        $customSelect.attr('class', 'custom-select custom-select-' + $select.attr('class'));
        $select.parent().find('span.inner-text:first').text(converText(selectText));
      });
    };

    var renderVectorImages = (function () {
        if ($.support.svg && $.support.imgsvg) {
            return function ($page) {
                //
            };
        }
        if ($.support.svg && !$.support.imgsvg) {
            return function ($page) {
                var img = null, $images = $page.find('img[src$=".svg"]'), imagesInd = $images.length;
                var o = null;
                while (imagesInd--) {
                    img = $images[imagesInd];
                    o = document.createElement('object');
                    o.setAttribute('type', 'image/svg+xml');
                    o.setAttribute('data', img.getAttribute('src'));
                    o.className = img.className;
                    img.parentNode.insertBefore(o, img);
                    img.parentNode.removeChild(img);
                }
            };
        }
        return function ($page) {
            var img = null, $images = $page.find('img[src$=".svg"]'), imagesInd = $images.length;
            while (imagesInd--) {
                img = $images[imagesInd];
                img.src = img.src.replace('.svg', '.png');
            }
        };
    })();

    var renderCheckbox = (function () {
        if ($.support.dataimage) {
            return function ($page) {
                var o = null, checkbox = null, $checkboxes = $page.find('input.input-checkbox'), checkboxesInd = $checkboxes.length;
                while (checkboxesInd--) {
                    checkbox = $checkboxes[checkboxesInd];
                    if (checkbox.className.indexOf('hidden-input') !== -1) {
                        continue;
                    }
                    o = document.createElement('label');
                    if (!checkbox.id) {
                        checkbox.id = 'checkbox-' + Math.floor(Math.random() * 1000000);
                    }
                    o.className = 'custom-checkbox ' + checkbox.className + (checkbox.checked ? ' checked' : '');
                    o.setAttribute('for', checkbox.id);
                    checkbox.className += ' hidden-input';
                    checkbox.parentNode.insertBefore(o, checkbox);
                }
            };
        }
        return function ($page) {
            //
        };
    })();

    var resizeViewer = (function () {
        if ($.support.touch) {
            return function ($page) {

            };
        }
        return function ($page) {
            var 
                $viewer = $page.find('iframe.file-container:first'),
                $header = $page.find('div.ui-header:first'),
                $footer = $page.find('div.ui-footer:first');

            var screenheight = document.documentElement.clientHeight > 0 ? document.documentElement.clientHeight : document.body.clientHeight;

            $viewer.height(screenheight - $header.outerHeight() - $footer.outerHeight() - 4);
        };
    })();

    var hideLoadingImg = function ($page) {
      var $fileContainer = $page.find('img.file-container:first');
      if ($fileContainer.length > 0) {
        $page.addClass('docitemfile-loading');
        $fileContainer.load(function () {
          jQuery(this).parents('div.ui-page:first').removeClass('docitemfile-loading');
        });
      }
    };

    var resizeScroller = (function () {
        if ($.support.iscroll) {
            return function () {
                DefaultMobile.setScreenMetrics();

                var $page = $('div.ui-page-active:first'), $scroller = $page.find('div.ui-scroller:first');

                if ($scroller.length > 0) {
                    setTimeout(DefaultMobile.scrollToTop, 1);

                    var 
                        $content = $page.find('div.ui-content:first'),
                        $header = $page.find('div.ui-header:first'),
                        $footer = $page.find('div.ui-footer:first');

                    var windowmetric = {
                        height: $.support.webapp ? window.innerHeight - 4 : window.screen.availHeight - $body.find('div.ui-header:first').height() + 4,
                        width: $.support.webapp ? window.innerHeight + 4 : window.screen.availWidth - $body.find('div.ui-header:first').height() + 4
                    };

                    var screenheight = Math.abs(window.orientation) === 90 ? windowmetric.width - 4 : windowmetric.height + 4;
                    screenheight = (Math.abs(window.orientation) === 90 ? screenMetrics.width : screenMetrics.height) || screenheight;

                    $page.addClass('ui-scroller').height(screenheight);
                    $content.height(screenheight - $header.outerHeight() - $footer.outerHeight() + 4);

                    var scroller = $scroller.data('iScroll');
                    if (scroller) {
                        scroller.refresh();
                    }
                }
            };
        }
        return function () {
            DefaultMobile.setScreenMetrics();
        };
    })();

    var resizeFilterItems = function ($navbar) {
        var $items = $navbar.find('span.people-filter-item');
        var fontsize = Math.floor($navbar.height() / $items.length) * 0.08;
        if (fontsize) {
            $items.css('font-size', fontsize + 'em');
            $navbar.height('auto').css('margin-top', -($navbar.height() >> 1) + 4 + 'px');
        }
    }

    var processTemplate = function (templid, data) {
        //var timestamp = new Date().getTime();
        var $item = $('#' + templid).tmpl(data);

        //try {console._messages.push('template: ' + templid + ' : ' + (new Date().getTime() - timestamp))} catch (err) {}
        return $item;
    }

    var renderDialogByTemplate = function (templid, dialogid, title, data) {
        var $dialog = $(), $mainform = $('#bodyWrapper');
        if (dialogid) {
            $dialog = $mainform.children('div.ui-dialog').filter('[data-dialogid="' + dialogid + '"]:first');
        }
        if ($dialog.length > 0) {
            switch (templid) {
                case 'template-dialog-people':
                    return $dialog;
            }
            $dialog.remove();
        }

        if (!data.hasOwnProperty('title')) {
            data.title = title;
        }
        if (!data.hasOwnProperty('dialogtitle')) {
            data.dialogtitle = title;
        }
        data.title = data.title || ASC.Resources.PageTitle || document.title;
        data.dialogtitle = data.pagetitle || ASC.Resources.PageTitle || document.title;
        $dialog = processTemplate(templid, data).appendTo($mainform);

        if (dialogid) {
            $dialog.attr('data-dialogid', dialogid);
        }

        if (data.hasOwnProperty('item') && data.item.hasOwnProperty('id')) {
            $dialog.attr('data-itemid', data.item.id);
        }

        if (data.hasOwnProperty('id')) {
            $dialog.attr('data-itemid', data.id);
        }

        return $dialog;
    }

    var renderPageByTemplate = function (templid, pageid, title, data) {
        var $page = $(), $mainform = $('#bodyWrapper');
        if (pageid) {
            $page = $mainform.children('div.ui-page').filter('[data-pageid="' + pageid + '"]:first');
            //console.log($page);
        }
        if ($page.length > 0) {
            switch (templid) {
                case 'template-page-people':
                    return $page;
            }

            $page.remove();
        }

        if (!data.hasOwnProperty('title')) {
            data.title = title;
        }
        if (!data.hasOwnProperty('pagetitle')) {
            data.pagetitle = title;
        }
        data.title = data.title || ASC.Resources.PageTitle || document.title;
        data.pagetitle = data.pagetitle || ASC.Resources.PageTitle || document.title;
        $page = processTemplate(templid, data).appendTo($mainform);

        if (pageid) {
            $page.attr('data-pageid', pageid);
        }

        if (data.hasOwnProperty('item') && data.item && data.item.hasOwnProperty('id')) {
            $page.attr('data-itemid', data.item.id);
        }

        if (data.hasOwnProperty('id')) {
            $page.attr('data-itemid', data.id);
        }

        return $page;
    }

    var renderDialog = function (evtname, dgtype, dgid, dgtitle, data) {
        var dgtmpl = '', pgname = '', args = [], fn = null;

        switch (dgtype.toLowerCase()) {
            case 'dialog-documents-additem': dgtmpl = TeamlabMobile.templates.dgdocsadditem; dgid = 'pgdcs-additm-' + dgid; break;
            case 'dialog-crm-additem': dgtmpl = TeamlabMobile.templates.dgcrmadditem; dgid = 'pgcrm-additm-' + dgid; break;
            case 'dialog-crm-navigate': dgtmpl = TeamlabMobile.templates.dgcrmnavigate; dgid = 'pgcrm-navigate-' + dgid; break;
            case 'dialog-crm-addtocontact': dgtmpl = TeamlabMobile.templates.dgcrmaddtocontact; dgid = 'pgcrm-addtocontact-' + dgid; break;
            case 'dialog-crm-additem-file': dgtmpl = TeamlabMobile.templates.dgcrmaddfiletocontact; dgid = 'pgcrm-addtocontact-' + dgid; break;
            default:
                return $();
        }

        $document.trigger('startrenderdialog');
        var $dialog = renderDialogByTemplate(dgtmpl, dgid, dgtitle, data || {});
        $dialog.addClass('ui-dialog-active');
        $document.trigger('endrenderdialog', [$dialog, evtname]);

        return $dialog;
    }

    var renderPage = function (evtname, pgtype, pgid, pgtitle, data) {
        var pgtmpl = '', pgname = '', args = [], fn = null;
        switch (pgtype.toLowerCase()) {
            case 'page-auth': pgtmpl = TeamlabMobile.templates.pgauth; pgid = 'pgauth-' + pgid; break;
            case 'page-search': pgtmpl = TeamlabMobile.templates.pgsearch; pgid = 'pgsch-' + pgid; break;
            case 'page-default': pgtmpl = TeamlabMobile.templates.pgdefault; pgid = 'pgdef-' + pgid; break;
            case 'page-rewrite': pgtmpl = TeamlabMobile.templates.pgrewrite; pgid = 'pgrwr-' + pgid; break;
            case 'page-exception': pgtmpl = TeamlabMobile.templates.pgexception; pgid = 'pgexc-' + pgid; break;
            case 'page-community': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-' + pgid; break;
            case 'page-community-blog': pgtmpl = TeamlabMobile.templates.pgcommblog; pgid = 'pgcom-blg-' + pgid; break;
            case 'page-community-poll': pgtmpl = TeamlabMobile.templates.pgcommpoll; pgid = 'pgcom-pll-' + pgid; break;
            case 'page-community-forum': pgtmpl = TeamlabMobile.templates.pgcommforum; pgid = 'pgcom-frm-' + pgid; break;
            case 'page-community-event': pgtmpl = TeamlabMobile.templates.pgcommevent; pgid = 'pgcom-evt-' + pgid; break;
            case 'page-community-bookmark': pgtmpl = TeamlabMobile.templates.pgcommbookmark; pgid = 'pgcom-bkm-' + pgid; break;
            case 'page-community-addblog': pgtmpl = TeamlabMobile.templates.pgcommaddblog; pgid = 'pgcom-addblg-' + pgid; break;
            case 'page-community-addpoll': pgtmpl = TeamlabMobile.templates.pgcommaddpoll; pgid = 'pgcom-addblg-' + pgid; break;
            case 'page-community-addforum': pgtmpl = TeamlabMobile.templates.pgcommaddforum; pgid = 'pgcom-addblg-' + pgid; break;
            case 'page-community-addevent': pgtmpl = TeamlabMobile.templates.pgcommaddevent; pgid = 'pgcom-addblg-' + pgid; break;
            case 'page-community-addbookmark': pgtmpl = TeamlabMobile.templates.pgcommaddbookmark; pgid = 'pgcom-addblg-' + pgid; break;
            case 'page-community-blogs': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-blgs-' + pgid; break;
            case 'page-community-polls': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-plls-' + pgid; break;
            case 'page-community-forums': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-frms-' + pgid; break;
            case 'page-community-events': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-evts-' + pgid; break;
            case 'page-community-bookmarks': pgtmpl = TeamlabMobile.templates.pgcommunity; pgid = 'pgcom-bkms-' + pgid; break;
            case 'page-community-addcomment': pgtmpl = TeamlabMobile.templates.pgaddcomment; pgid = 'pgcom-addcm-' + pgid; break;
            case 'page-people': pgtmpl = TeamlabMobile.templates.pgpeople; pgid = 'pgppl-' + pgid; break;
            case 'page-people-profile': pgtmpl = TeamlabMobile.templates.pgpeopitem; pgid = 'pgppl-itm-' + pgid; break;
            case 'page-projects': pgtmpl = TeamlabMobile.templates.pgprojects; pgid = 'pgprj-' + pgid; break;
            case 'page-projects-tasks': pgtmpl = TeamlabMobile.templates.pgprojitems; pgid = 'pgprj-tsks-' + pgid; break;
            case 'page-projects-milestones': pgtmpl = TeamlabMobile.templates.pgprojitems; pgid = 'pgprj-mlts-' + pgid; break;
            case 'page-projects-project': pgtmpl = TeamlabMobile.templates.pgprojproject; pgid = 'pgprj-prj-' + pgid; break;
            case 'page-projects-project-team': pgtmpl = TeamlabMobile.templates.pgprojprojectteam; pgid = 'pgprj-ptm-' + pgid; break;
            case 'page-projects-project-files': pgtmpl = TeamlabMobile.templates.pgprojprojectfiles; pgid = 'pgprj-pfl-' + pgid; break;
            case 'page-projects-project-tasks': pgtmpl = TeamlabMobile.templates.pgprojprojecttasks; pgid = 'pgprj-pts-' + pgid; break;
            case 'page-projects-project-milestones': pgtmpl = TeamlabMobile.templates.pgprojprojectmilestones; pgid = 'pgprj-pms-' + pgid; break;
            case 'page-projects-project-discussions': pgtmpl = TeamlabMobile.templates.pgprojprojectdiscussions; pgid = 'pgprj-pds-' + pgid; break;
            case 'page-projects-milestone-tasks': pgtmpl = TeamlabMobile.templates.pgprojmilestonetasks; pgid = 'pgprj-mts-' + pgid; break;
            case 'page-projects-task': pgtmpl = TeamlabMobile.templates.pgprojtask; pgid = 'pgprj-tsk-' + pgid; break;
            case 'page-projects-discussion': pgtmpl = TeamlabMobile.templates.pgprojdiscussion; pgid = 'pgprj-dsc-' + pgid; break;
            case 'page-projects-addtask': pgtmpl = TeamlabMobile.templates.pgprojaddtask; pgid = 'pgprj-addtsk-' + pgid; break;
            case 'page-projects-addmilestone': pgtmpl = TeamlabMobile.templates.pgprojaddmilestone; pgid = 'pgprj-addmlt-' + pgid; break;
            case 'page-projects-adddiscussion': pgtmpl = TeamlabMobile.templates.pgprojadddiscussion; pgid = 'pgprj-adddsc-' + pgid; break;
            case 'page-projects-addcomment': pgtmpl = TeamlabMobile.templates.pgaddcomment; pgid = 'pgprj-addcm-' + pgid; break;
            case 'page-documents': pgtmpl = TeamlabMobile.templates.pgdocuments; pgid = 'pgdcs-' + pgid; break;
            case 'page-documents-file': pgtmpl = TeamlabMobile.templates.pgdocsfile; pgid = 'pgdcs-file-' + pgid; break;
            case 'page-documents-additem': pgtmpl = TeamlabMobile.templates.pgdocsadditem; pgid = 'pgdcs-additm-' + pgid; break;
            case 'page-documents-addfile': pgtmpl = TeamlabMobile.templates.pgdocsaddfile; pgid = 'pgdcs-addfle-' + pgid; break;
            case 'page-documents-addfolder': pgtmpl = TeamlabMobile.templates.pgdocsaddfolder; pgid = 'pgdcs-addfld-' + pgid; break;
            case 'page-documents-adddocument': pgtmpl = TeamlabMobile.templates.pgdocsadddocument; pgid = 'pgdcs-adddcm-' + pgid; break;
            case 'page-documents-editdocument': pgtmpl = TeamlabMobile.templates.pgdocseditdocument; pgid = 'pgdcs-edtdoc-' + pgid; break;
            case 'page-crm': pgtmpl = TeamlabMobile.templates.pgcrm; pgid = 'pgcrm-' + pgid; break;
            case 'page-crm-tasks': pgtmpl = TeamlabMobile.templates.pgcrmtasks; pgid = 'pgcrm-tasks' + pgid; break;
            //case 'timeline-crm-tasks': pgtmpl = TeamlabMobile.templates.lbcrmtaskstimeline; pgid = 'pgcrm-taskstimeline' + pgid; break;            
            case 'page-crm-task': pgtmpl = TeamlabMobile.templates.pgcrmtask; pgid = 'pgcrm-task' + pgid; break;
            case 'page-crm-addtask': pgtmpl = TeamlabMobile.templates.pgcrmaddtask; pgid = 'pgcrm-addtask' + pgid; break;
            case 'page-crm-addcompany': pgtmpl = TeamlabMobile.templates.pgcrmaddcompany; pgid = 'pgcrm-addcompany' + pgid; break;
            case 'page-crm-addpersone': pgtmpl = TeamlabMobile.templates.pgcrmaddpersone; pgid = 'pgcrm-addpersone' + pgid; break;
            case 'page-crm-addhistoryevent': pgtmpl = TeamlabMobile.templates.pgcrmaddhistoryevent; pgid = 'pgcrm-addhistoryevent' + pgid; break;
            case 'page-crm-addnote': pgtmpl = TeamlabMobile.templates.pgcrmaddnote; pgid = 'pgcrm-addnote' + pgid; break;
            case 'page-crm-contacthistory': pgtmpl = TeamlabMobile.templates.pgcrmcontacthistory; pgid = 'pgcrm-contacthistory' + pgid; break;
            case 'page-crm-contactfiles': pgtmpl = TeamlabMobile.templates.pgcrmcontactfiles; pgid = 'pgcrm-contactfiles' + pgid; break;
            case 'page-crm-contactasks': pgtmpl = TeamlabMobile.templates.pgcrmcontacttasks; pgid = 'pgcrm-contacttasks' + pgid; break;
            case 'page-crm-contacpersones': pgtmpl = TeamlabMobile.templates.pgcrmcontactpersones; pgid = 'pgcrm-contactpersones' + pgid; break;
            case 'page-crm-person': pgtmpl = TeamlabMobile.templates.pgcrmperson; pgid = 'pgcrm-prn' + pgid; break;
            case 'page-crm-company': pgtmpl = TeamlabMobile.templates.pgcrmcompany; pgid = 'pgcrm-cpn' + pgid; break;    
            default:
                return $();
        }

        $document.trigger('startrenderpage');
        var $page = null;
        try {
          $page = renderPageByTemplate(pgtmpl, pgid, pgtitle, data || {});
        } catch (err) {
          $page = null;
          console.log('renderPage error: ', err);
          $body.removeClass('ui-mobile-rendering-page').addClass('ui-mobile-rendered-page');
        }

        if ($page) {
          $mainform.children('div.ui-page.ui-page-active').not($page).removeClass('ui-page-active').addClass('last-active-page');
          $page.addClass('ui-page-active');
          $document.trigger('endrenderpage', [$page, evtname]);
        }

        return $page || $();
    }

    var move_me = function (evt, $page, $this) {
        var href = $this.attr('data-href');
        if (href) {
            location.href = href;
        }
    }

    return {
        constants : {
            screenMetrics : screenMetrics,

            touchClick      : touchClick,
            touchStartEvent : touchStartEvent,
            touchStopEvent  : touchStopEvent,
            touchMoveEvent  : touchMoveEvent
        },

        setScreenMetrics  : setScreenMetrics,

        scrollToTop   : scrollToTop,
        scrollToItem  : scrollToItem,

        defaultResizeBody   : defaultResizeBody,
        resizeBody          : resizeBody,
        resizeDialog        : resizeDialog,
        resizePage          : resizePage,
        resizeViewer        : resizeViewer,
        resizeScroller      : resizeScroller,
        resizeFilterItems   : resizeFilterItems,
        keyDownOnWkeInput   : keyDownOnWkeInput,

        hideLoadingImg  : hideLoadingImg,

        renderAnimatedImages  : renderAnimatedImages,
        renderCustomSelects   : renderCustomSelects,
        renderVectorImages    : renderVectorImages,
        renderCheckbox        : renderCheckbox,
        renderDialog          : renderDialog,
        renderPage            : renderPage,

        processTemplate     : processTemplate,

        move_me : move_me
    };
})(jQuery);

;(function ($) {
    var fld = '', engns = null, brwsrs = null, $body = $(document.body), ua = navigator.userAgent;

    $body.addClass($.support.touch ? 'plt-mobile' : 'plt-desktop');

    engns = {'engn-ios': /iphone|ipad/i.test(ua), 'engn-android': /android/i.test(ua), 'engn-gecko': /gecko/i.test(ua) && !/webkit/i.test(ua), 'engn-webkit': /webkit/i.test(ua)};
    brwsrs = {'brwr-msie': '\v' == 'v' || /msie/i.test(ua), 'brwr-opera': !!window.opera, 'brwr-chrome': !!window.chrome, 'brwr-safari': /safari/i.test(ua) && !window.chrome, 'brwr-firefox': /firefox/i.test(ua)};

    for (fld in engns) {
        if (engns.hasOwnProperty(fld)) {
            if (engns[fld]) {
                $body.addClass(fld);
            }
        }
    }

    for (fld in brwsrs) {
        if (brwsrs.hasOwnProperty(fld)) {
            if (brwsrs[fld]) {
                $body.addClass(fld);
                $body.addClass(fld + '-' + $.platform.version);
            }
        }
    }

    var
        $window = $(window),
        $document = $(document),
        $body = $(document.body),
        $mainform = $('#bodyWrapper'),
        $pageheader = $(),
        $pagefooter = $(),
        wasMoved = 0,
        eventTimestamp = 299,
        startEventTimestamp = 0,
        startClickedObject = null,
        handlerRerenderFooter = 0;

    if (!document.getElementsByClassName) {
        document.getElementsByClassName = function() { return [] };
    }

    if ($.support.cssPositionFixed) {
        $(document.body).addClass('support-position-fixed');
    }

    if (!$.support.iscroll && !$.support.cssPositionFixed) {
        $(document.body).addClass('unsupport-iscroll');
    }

    if ($.support.orientation) {
        document.body.className += ' ' + Math.abs(window.orientation) === 90 ? ' ui-landscape' : ' ui-portrait';

        if (document.addEventListener) {
            window.addEventListener('orientationchange', function(evt) {
                var removeclass = '', addclass = '', classes = document.body.className;
                if (classes) {
                    if (Math.abs(window.orientation) === 90) {
                        removeclass = 'ui-portrait'; addclass = 'ui-landscape';
                    } else {
                        removeclass = 'ui-landscape'; addclass = 'ui-portrait';
                    }
                    classes = classes.split(/\s+/);
                    var classesInd = classes.length;
                    while (classesInd--) {
                        if (classes[classesInd] === removeclass) {
                            classes.splice(classesInd, 1);
                        }
                    }
                    classes.push(addclass);
                    document.body.className = classes.join(' ');
                }
            });
        }
    }

    if ($.support.touch) {
        //if ($.platform.android) {
        //  if (document.addEventListener) {
        //    document.addEventListener('touchmove', function (evt) {evt.preventDefault()}, false);
        //  }
        //}
    }

    function jqScrollerFormat(format) {
        var 
      result = format,
      terms = [
        [/dddd/g, 'dd'],
        [/ddd/g, 'dd'],
        [/MMMM/g, 'mm'],
        [/MMM/g, 'mm'],
        [/M/g, 'm'],
        [/yyyy/g, 'yy'],
        [/yyy/g, 'yy']
      ];

        for (var i = 0, n = terms.length; i < n; i++) {
            result = result.replace(terms[i][0], terms[i][1]);
        }
        return result;
        return format;
    }

    var moveFloatFooter = (function() {
        if ($.platform.operamobile) {
            return function() {
                clearTimeout(handlerRerenderFooter);
            };
        }
        return function() {
            clearTimeout(handlerRerenderFooter);
            //for (var fld in window.screen) {
            //  if (typeof window.screen[fld] === 'number' && window.screen[fld] > 0) {
            //    alert(fld + ': ' + window.screen[fld]);
            //  }
            //}
            //var msg = [
            //  'window.pageYOffset: ' + window.pageYOffset,
            //  'window.screenY: ' + window.screenY,
            //  'window.scrollY: ' + window.scrollY,
            //  'availHeight: ' + window.screen.availHeight,
            //  'innerHeight: ' + window.innerHeight,
            //  'availWidth: ' + window.screen.availWidth,
            //  'innerWidth: ' + window.innerWidth
            //].join('\n');
            //alert(msg);
            var screenheight = Math.min(window.innerHeight, window.screen.availHeight);
            $pagefooter.css('display', 'block').css('bottom', 'auto').css('top', Math.max(window.pageYOffset, window.scrollY) + screenheight - ($pagefooter[0].offsetHeight) + 'px');
        };
    })();

    var rerenderFooter = (function () {
        if ($.support.fscroll) {
            return function () {
                $pagefooter.css('display', 'none');
                clearTimeout(handlerRerenderFooter);
                handlerRerenderFooter = setTimeout(moveFloatFooter, 500);
            };
        }
        return function () {
        };
    })();

    var setRerenderFooterCallbacks = function ($scroller) {
        $window.unbind('scroll', rerenderFooter).unbind(DefaultMobile.constants.touchMoveEvent, rerenderFooter).unbind(DefaultMobile.constants.touchMoveEvent, rerenderFooter);
        if ($scroller.length > 0) {
            $window.bind('scroll', rerenderFooter).bind(DefaultMobile.constants.touchMoveEvent, rerenderFooter).bind(DefaultMobile.constants.touchMoveEvent, rerenderFooter);
        }
    };

    function onGetException(fname, type, message, comment, params) {
        //ASC.Controls.messages.show('error', message);        
        var errId = 0, errText = null, needsafeback = false;
        try {
            switch (fname.toLowerCase()) {
                case 'ongetblog'        : errText = ASC.Resources.ErrGetBlogException; break;
                case 'ongetpoll'        : errText = ASC.Resources.ErrGetPollException; break;
                case 'ongetforum'       : errText = ASC.Resources.ErrGetForumException; break;
                case 'ongetevent'       : errText = ASC.Resources.ErrGetEventException; break;
                case 'ongetbookmark'    : errText = ASC.Resources.ErrGetBookmarkException; break;
                case 'onaddtask'        : errText = ASC.Resources.ErrAddTaskException; break;
                case 'onaddcrmtask'     : errText = ASC.Resources.ErrAddTaskException; break;
                case 'ongettask'        : errText = ASC.Resources.ErrGetTaskException; break;
                case 'onaddmilestone'   : errText = ASC.Resources.ErrAddMilestoneException; break;
                case 'onadddiscussion'  : errText = ASC.Resources.ErrAddDiscussionException; break;
                default                 : errText = message; break;
            }

            switch (type) {
                case '403'  : errText = ASC.Resources.ErrAccessDeniedException; break;
                case '404'  : errText = ASC.Resources.ErrInvalidRequestException; break;
                case '500'  : errText = ASC.Resources.ErrUnknownException; break;
                case '503'  : errText = ASC.Resources.ErrUnknownException; break;
            }
        } catch (err) {
            errText = ASC.Resources.ErrUnknownException;
        }

        if (errText) {
          if (navigator.onLine === false) {
            errText = ASC.Resources.ErrNoInternetConnection || ASC.Resources.ErrInvalidRequestException;
            ASC.Controls.messages.show('noconnection', 'error', errText);
          } else {
            ASC.Controls.messages.show('error', errText);
          }
          $window.scrollTop(0);
        }

        $body.removeClass('ui-mobile-rendering-page').addClass('ui-mobile-rendered-page');
        var $page = $('div.ui-page-active:first');
        $page
          .removeClass('loading-items')
          .removeClass('update-status')
          .removeClass('docitemfile-loading')
          .removeClass('projectteam-loading')
          .removeClass('projectmilestones-loading');

        $page.find('button').removeClass('disabled');
        $page.find('li.active-item').removeClass('active-item');

        ASC.Controls.AnchorController.lazymove(null);

        //try {
        //    needsafeback = params && params.hasOwnProperty('__changeanch') ? params.__changeanch === true : needsafeback;
        //} catch (err) {
        //    //
        //}

        //if (needsafeback && !ASC.Controls.AnchorController.safeback()) {
        //    var $page = DefaultMobile.renderPage('exception-page', 'page-exception', 'exception', 'TeamLab', {});
        //}
    }

    function onAuthPage(data) {
        var $page = DefaultMobile.renderPage('auth-page', 'page-auth', 'auth', 'TeamLab', data);
    }

    function onSearchPage(data, query, params) {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        data = { pagetitle: ASC.Resources.LblSearchTitle, type: 'search-page', query: query, anchor: anchor, items: data };

        var $page = DefaultMobile.renderPage('search-page', 'page-search', 'search' + Math.floor(Math.random() * 1000000), 'TeamLab', data);
    }

    function onIndexPage(data) {
        data.isapp = window.PHONEGAP;
        var $page = DefaultMobile.renderPage('index-page', 'page-default', 'default', 'TeamLab', data);
    }

    function onRewritePage(data) {
        var $page = DefaultMobile.renderPage('rewrite-page', 'page-rewrite', 'rewrite', 'TeamLab', data);
    }

    // blah-blah-blah

    function onAddComment(comment, params) {
        var $page = $('div.ui-page-active:first').removeClass('loading-comments');

        $('textarea.ui-text-area.comment-content').val('');
        $('button.create-comment').removeClass('disabled');

        DefaultMobile.resizePage($page);

        var $comment = $page.find('li.item-comment[data-commentid="' + comment.id + '"]:first');
        if ($comment.length > 0) {
            setTimeout((function(top) { return function() { window.scrollTo(0, top) } })(Math.round($comment.offset().top)), 100);
        }
    }

    // blah-blah-blah

    function onResizeTextArea(evt) {
        var defaultHeight = 60;
        this.style.height = defaultHeight + 'px';
        var height = defaultHeight, scrollHeight = this.scrollHeight - 10; // 5 * 2 - padding
        if (scrollHeight > height) {
            while (scrollHeight > height) {
                height += 16;
            }
            this.style.height = height + 'px';
        }
    }

    function onFormSubmitByCtrlEnter(evt) {
        switch (evt.keyCode) {
            case 13:
                if (evt.ctrlKey === true) {
                    $(this).parents('div.ui-page:first').find('button.create-item:first').trigger('click');
                }
                break;
        }
    }

    function onFormSubmitByEnter(evt) {
        switch (evt.keyCode) {
            case 13:
                $(this).parents('div.ui-page:first').find('button.create-item:first').trigger('click');
                break;
        }
    }

    function onIndexSearch(evt) {
        var 
            $page = null,
            $form = $(this),
            searchvalue = '';

        searchvalue = $form.length > 0 ? $form.find('input.top-search-field:first').val() : searchvalue;

        if (typeof searchvalue !== 'string' || (searchvalue = (searchvalue.replace(/^\s+|\s+$/g, '')).toLowerCase()).length === 0) {
            return undefined;
        }

        $page = $('div.ui-page-active:first');

        if (searchvalue) {
            $page.find('form.search-form:first').addClass('active');
        } else {
            $page.find('form.search-form:first').removeClass('active');
        }

        TeamlabMobile.search(searchvalue);
    }

    function onIndexSearchKeyUp (evt) {
        var 
            $page = null,
            searchvalue = evt.target.value;

        $page = $('div.ui-page-active:first');

        if (searchvalue) {
          $page.find('form.search-form:first').addClass('active');
        } else {
          $page.find('form.search-form:first').removeClass('active');
        }
    }

    // blah-blah-blah

    //

    //console = console || {};
    //console._messages = [];
    //console._log = function (msg) {console._messages.push(msg)};
    //console._print = function () {alert(console._messages.join('\n'));console._messages = []};

    function callMethodByClassname(classname, thisArg, argsArray) {
        var
            cls = '',
            classesInd = 0,
            classes = typeof classname === 'string' ? classname.split(/\s+/) : [],
        classesInd = classes ? classes.length : 0;
        while (classesInd--) {
            cls = classes[classesInd].replace(/-/g, '_');
            if (typeof DefaultMobile[cls] === 'function') {
                DefaultMobile[cls].apply(thisArg, argsArray);
            }
        }
    } 

    function onH1Click(evt) {
        try {console._print()} catch (err) {}
    }

    function onImgClick(evt) {
        $(this).toggleClass('image-fullsize');
    }

    function onLabelClick(evt) {
        var $page = $('div.ui-page-active:first'), $this = $(this);

        if ($page.length === 0 || $this.hasClass('disabled')) {
            return false;
        }

        callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);

        if ($this.hasClass('disable-search-label')) {
            var $page = $('div.ui-page-active:first');

            $page.find('li.item-index').removeClass('uncorrect-item');
            $page.find('li.item-persone').removeClass('uncorrect-item');
            $page.find('form.search-form:first').removeClass('active').find('input.top-search-field:first').val('');
            window.focus();
        }
    }

    function onSelectChange(evt) {
        var $page = $('div.ui-page-active:first'), $this = $(this);

        if ($page.length === 0 || $this.hasClass('disabled')) {
            return false;
        }

        callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);
    }

    function onInputClick(evt) {
        var $page = $('div.ui-page-active:first'), $this = $(this);

        if ($this.is(':checkbox')) {
            if ($page.length === 0 || $this.hasClass('disabled')) {
                return false;
            }

            callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);

            var itemid = $this.attr('data-itemid');
            itemid = isFinite(+itemid) ? +itemid : null;
            if (itemid !== null) {
                var itemtype = null;
                //if ($page.hasClass('page-projects-item')) {
                //    itemtype = ($page.hasClass('page-projects-task') && 'task') || null;
                //}
                //if ($page.hasClass('page-projects')) {
                //    itemtype = ($this.parents('li.item').hasClass('task') && 'task') || null;
                //}
                if ($page.hasClass('page-crm-tasks') || $page.hasClass('page-crm-contacttasks') || $page.hasClass('page-crm-task')){
                    itemtype = (($page.hasClass('page-crm-contacttasks') || $page.hasClass('page-crm-tasks') || $page.hasClass('page-crm-task')) && 'crm-tasks') || null;
                }
                var
                  fn = null,
                  params = {};
                switch (itemtype) {
                    //case 'task':
                    //    fn = TeamlabMobile.updateTaskStatus;
                    //    break;
                    case 'crm-tasks':
                        if($page.attr('data-itemid') != null || $page.attr('data-itemid') != ''){
                            params.contactId = $page.attr('data-itemid');
                        }                      
                        fn = TeamlabMobile.updateCrmTaskStatus;
                        break;
                }
                if (fn !== null) {
                    $page.addClass('update-status');
                    $this.parents('li.item.:first').addClass('update-status');
                    fn(params, itemid, $this.is(':checked'));
                    return false;
                }
            }

            return false;
        }
    }

    function onButtonClick(evt) {
        var $page = $('div.ui-page-active:first'), $this = $(this), fn = null, flexec = false;

        if ($page.length === 0 || $this.hasClass('disabled')) {
            return false;
        }

        callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);

        return false;
    }

    function onBtnLinkClick(evt) {
        var $page = $('div.ui-page-active:first'), $this = $(this);

        if ($page.length === 0 || $this.hasClass('disabled')) {
            return false;
        }

        callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);
    }

    function onBodyMoveEvent(evt) {
        wasMoved++;
    }

    function onHrefStartEvent(evt) {
        $(this).addClass('active');
        wasMoved = 0;
        startClickedObject = this;
        startEventTimestamp = new Date().getTime();
    }

    function onHrefStopEvent (evt) {
        var $this = $(this);
        $this.removeClass('active');
        $(startClickedObject).removeClass('active');
        //if (wasMoved === false && startClickedObject === this) {
        if (startClickedObject === this && (((wasMoved < 2) && (new Date().getTime() - startEventTimestamp < eventTimestamp)) || ($this.hasClass('ui-btn') || $this.hasClass('nav-menu-item')))) {
          return onHrefClick.apply(this, [evt]);
        }
    }

    function onNoneHrefClick(evt) {
        if (typeof evt.button === 'number' && evt.button !== 0) {
            evt.preventDefault();
            return false;
        }

        var $this = $(this);

        if ($this.hasClass('target-blank')) {
            var href = this.getAttribute('href') || '';
            href = href.charAt(0) === '#' ? href.substring(1) : href;

            if (href) {
                window.open(href);
            }
            evt.preventDefault();
            return false;
        }

        evt.preventDefault();
        return false;
    }

    function onHrefClick(evt) {
    
        var $page = $('div.ui-page-active:first'), $this = $(this);        
        callMethodByClassname($this.attr('class'), this, [evt, $page, $this]);
        
        if (typeof evt.button === 'number' && evt.button !== 0) {
            evt.preventDefault();
            return false;
        }

        var back = this.getAttribute('data-back'), $this = $(this);

        back = back || null;
        if ($this.hasClass('screenzoom')) {
            return false;
        }

        if ($this.hasClass('target-none')) {
            evt.preventDefault();
            return false;
        }

        if ($this.hasClass('ui-item-link')) {
            $this.parent().addClass('active-item');
        }

        if ($this.hasClass('target-update')) {
            evt.preventDefault();
            return false;
        }

        if ($this.hasClass('target-portals')) {
            history.go(-(history.length - 1));
            return undefined;
        }

        if ($this.hasClass('target-standart')) {
            $.cookie('asc_nomobile', '1', {path : '/'});
            var href = this.getAttribute('href') || '';
            if (href) {
                location.href = href;
            }
            return undefined;
        }

        if ($this.hasClass('target-logout')) {
            var href = this.getAttribute('href') || '';
            if (href) {
                location.href = href;
            }
            return undefined;
        }

        if ($this.hasClass('target-back')) {
            var scrollTop = 0;
            scrollTop = window.scrollY;
            TeamlabMobile.goPrevHistoryStep($this.hasClass('none-shift-back'));
            window.scrollY = scrollTop;
            //document.documentElement.scrollTop = scrollTop;
            evt.preventDefault();
            return false;
        }

        if ($this.hasClass('target-blank')) {
            //var href = this.getAttribute('href') || '';
            //href = href.charAt(0) === '#' ? href.substring(1) : href;

            //if (href) {
            //  window.open(href);
            //}
            //evt.preventDefault();
            return undefined;
        }

        if ($this.hasClass('target-top')) {
            var href = this.getAttribute('href') || '';
            href = href.charAt(0) === '#' ? href.substring(1) : href;
            if (href) {
                if (!$this.hasClass('change-page-none')) {
                    jQuery(document).trigger('changepage');
                }
                setTimeout((function (href) {
                    return function () {
                        location.href = href;
                    };
                })(href), 100);
            }
            evt.preventDefault();
            return false;
        }

        if ($this.hasClass('target-dialog')) {
            TeamlabMobile.showDialog(this.getAttribute('href'));

            evt.preventDefault();
            return false;
        }

        scrollTop = window.scrollY;
        //scrollTop = document.documentElement.scrollTop;

        var href = this.getAttribute('href') || '';
        ASC.Controls.AnchorController.lazymove(back ? { __back: back} : null, href.length === 0 || href === '/' ? '#' : href);

        //var href = this.getAttribute('href') || '';
        //setTimeout(function () {
        //  ASC.Controls.AnchorController.lazymove(back ? {__back : back} : null, href.length === 0 || href === '/' ? '#' : href);
        //  window.scrollY = scrollTop;
        //}, 50); 
        window.scrollY = scrollTop;
        //document.documentElement.scrollTop = scrollTop;
        evt.preventDefault();
        return false;
    }

    function onMenuItemClick (evt) {
        var $menuitem = null;
        if (!($menuitem = $(this).parents('.ui-menu-item:first')).hasClass('active-item')) {
            $menuitem.addClass('active-item').siblings().removeClass('active-item');
        }
    }

    function onDialogClick (evt) {
        setTimeout(function () {
          jQuery('div.ui-dialog-active').removeClass('ui-dialog-active');
        }, 300);
    }

    function delegateItem ($item) {
        $(document.body).unbind(DefaultMobile.constants.touchMoveEvent, onBodyMoveEvent).bind(DefaultMobile.constants.touchMoveEvent, onBodyMoveEvent);
        if ($item.hasClass('ui-page')) {
            //$item.find('h1').unbind(DefaultMobile.constants.touchClick, onH1Click).bind(DefaultMobile.constants.touchClick, onH1Click);
            $item.find('img').unbind(DefaultMobile.constants.touchClick, onImgClick).bind(DefaultMobile.constants.touchClick, onImgClick);
            $item.find('label').unbind(DefaultMobile.constants.touchClick, onLabelClick).bind(DefaultMobile.constants.touchClick, onLabelClick);
            $item.find('input').unbind(DefaultMobile.constants.touchClick, onInputClick).bind(DefaultMobile.constants.touchClick, onInputClick);
            $item.find('button').unbind(DefaultMobile.constants.touchClick, onButtonClick).bind(DefaultMobile.constants.touchClick, onButtonClick);
            $item.find('select').unbind('change', onSelectChange).bind('change', onSelectChange);
            $item.find('span.ui-btn').unbind(DefaultMobile.constants.touchClick, onBtnLinkClick).bind(DefaultMobile.constants.touchClick, onBtnLinkClick);

            //$item.find('a').unbind(DefaultMobile.constants.touchClick, onHrefClick).bind(DefaultMobile.constants.touchClick, onHrefClick);
            $item.find('a')
                .unbind(DefaultMobile.constants.touchClick, onNoneHrefClick).bind(DefaultMobile.constants.touchClick, onNoneHrefClick)
                .unbind(DefaultMobile.constants.touchStartEvent, onHrefStartEvent).bind(DefaultMobile.constants.touchStartEvent, onHrefStartEvent)
                .unbind(DefaultMobile.constants.touchStopEvent, onHrefStopEvent).bind(DefaultMobile.constants.touchStopEvent, onHrefStopEvent);

            $item.find('.ui-menu-accordion .ui-menu-item .ui-menu-item-title').unbind(DefaultMobile.constants.touchClick, onMenuItemClick).bind(DefaultMobile.constants.touchClick, onMenuItemClick);
        } else if ($item.hasClass('ui-dialog')) {
            $item.unbind(DefaultMobile.constants.touchStopEvent, onDialogClick).bind(DefaultMobile.constants.touchStopEvent, onDialogClick);

            //$item.find('a').unbind(DefaultMobile.constants.touchClick, onHrefClick).bind(DefaultMobile.constants.touchClick, onHrefClick);
            $item.find('a')
                .unbind(DefaultMobile.constants.touchClick, onNoneHrefClick).bind(DefaultMobile.constants.touchClick, onNoneHrefClick)
                .unbind(DefaultMobile.constants.touchStartEvent, onHrefStartEvent).bind(DefaultMobile.constants.touchStartEvent, onHrefStartEvent)
                .unbind(DefaultMobile.constants.touchStopEvent, onHrefStopEvent).bind(DefaultMobile.constants.touchStopEvent, onHrefStopEvent);
        }
    }    

    TeamlabMobile.bind(TeamlabMobile.events.getException, onGetException);

    TeamlabMobile.bind(TeamlabMobile.events.changePage, function() {jQuery(document).trigger('changepage')});

    TeamlabMobile.bind(TeamlabMobile.events.authPage, onAuthPage);
    TeamlabMobile.bind(TeamlabMobile.events.indexPage, onIndexPage);
    TeamlabMobile.bind(TeamlabMobile.events.rewritePage, onRewritePage);
    TeamlabMobile.bind(TeamlabMobile.events.searchPage, onSearchPage);
    TeamlabMobile.bind(TeamlabMobile.events.addComment, onAddComment);

    function resetLastActivePage() {
        $('div.ui-dialog-active').removeClass('ui-dialog-active');
        $('div.last-active-page').removeClass('last-active-page');
        $body.removeClass('ui-mobile-rendering-page').addClass('ui-mobile-rendered-page');
    }

    $document
        .bind(DefaultMobile.constants.touchStartEvent, function(evt) { })
        .bind(DefaultMobile.constants.touchClick, function(evt) { })
        //.bind('click', function (evt) {})
        .bind('startrenderdialog', function(evt) {
            // TODO
        })
        .bind('endrenderdialog', function(evt, $dialog, type) {
            delegateItem($dialog);

            DefaultMobile.resizeDialog($dialog);

            //switch (type) {
            //  case 'documents-additem-dialog' :
            //    
            //    break;
            //}
        })
        .bind('changepage', function(evt) {
            $('input[type="datepick"]').scroller('hide');
            ASC.Controls.messages.hideAll();
            //DefaultMobile.resizeBody($mainform);
            //DefaultMobile.resizePage();
            $body.removeClass('ui-mobile-rendered-page').addClass('ui-mobile-rendering-page');
        })
        .bind('updatepage', function(evt) {
            $body.removeClass('ui-mobile-rendering-page').addClass('ui-mobile-rendered-page');
            var $page = $('div.ui-page-active:first');
            delegateItem($page);

            DefaultMobile.renderCustomSelects($page);
            DefaultMobile.renderCheckbox($page);
            var scroller = $page.find('div.ui-scroller:first').data('iScroll');
            if (scroller) {
                scroller.refresh();
            }
        })
        .bind('startrenderpage', function(evt) {
            //
        })
        .bind('endrenderpage', function(evt, $page, type) {            
            ASC.Controls.AnchorController.lazymove();
            delegateItem($page);

            setTimeout(DefaultMobile.scrollToTop, 1);
            DefaultMobile.renderVectorImages($page);
            DefaultMobile.hideLoadingImg($page);
            DefaultMobile.resizePage($page);
            DefaultMobile.keyDownOnWkeInput($page);
            DefaultMobile.renderCustomSelects($page);
            DefaultMobile.renderCheckbox($page);

            $pageheader = $page.find('div.ui-header:first');
            $pagefooter = $page.find('div.ui-footer:first');
            var $scroller = $page.find('div.ui-scroller:first');

            setTimeout(resetLastActivePage, 300);

            if ($.support.touch && $.support.iscroll && $scroller.length > 0 && !$scroller.hasClass('none-iscroll')) {
                var 
                    $content = $page.find('div.ui-content:first'),
                    $header = $page.find('div.ui-header:first'),
                    $footer = $page.find('div.ui-footer:first');

                var windowmetric = {
                    height: $.support.webapp ? window.innerHeight - 4 : window.screen.availHeight - $body.find('div.ui-header:first').height() + 4,
                    width: $.support.webapp ? window.innerHeight + 4 : window.screen.availWidth - $body.find('div.ui-header:first').height() + 4
                };

                var screenheight = Math.abs(window.orientation) === 90 ? windowmetric.width - 4 : windowmetric.height + 4;
                screenheight = (Math.abs(window.orientation) === 90 ? DefaultMobile.constants.screenMetrics.width : DefaultMobile.constants.screenMetrics.height) || screenheight;

                $page.addClass('ui-scroller').height(screenheight);
                $content.height(screenheight - $header.outerHeight() - $footer.outerHeight() + 4);

                if ($scroller.data('isScrolled') !== true) {
                    $scroller.data('isScrolled', true);
                    var scroller = new iScroll($content[0]);
                    $scroller.data('iScroll', scroller);
                }
            }

            if ($.support.touch && $.support.fscroll) {
                if ($page.find('div.ui-scroller:first').length > 0) {
                    rerenderFooter();
                }
            }

            if ($.support.touch && $.support.fscroll) {
                setRerenderFooterCallbacks($scroller);
            }

            $page.find('div.fckAscUser').userlink();

            var scrollerTheme = '';
            scrollerTheme = $.platform.ios ? 'ios' : scrollerTheme;
            //scrollerTheme = $.platform.android ? 'android' : scrollerTheme;
            $page.find('input[type="datepick"]').scroller({
                theme: scrollerTheme,
                setText: ASC.Resources.BtnSet,
                cancelText: ASC.Resources.BtnCancel,
                dayText: ASC.Resources.LblDay,
                monthText: ASC.Resources.LblMonth,
                yearText: ASC.Resources.LblYear,
                timeFormat: jqScrollerFormat(TeamlabMobile.dateFormats.time),
                dateFormat: jqScrollerFormat(TeamlabMobile.dateFormats.date),
                dayNamesShort: TeamlabMobile.nameCollections.shortdays,
                dayNames: TeamlabMobile.nameCollections.days,
                monthNamesShort: TeamlabMobile.nameCollections.shortmonths,
                monthNames: TeamlabMobile.nameCollections.months
            });

            //save history step
            //console.log(type);
            switch (type) {
              case 'community-addblog-page' :
              case 'community-addforum-page' :
              case 'community-addbookmark-page' :
              case 'projects-addtask-page' :
              case 'projects-addmilestone-page' :
              case 'projects-adddiscussion-page' :
              case 'crm-addnote-page' :
                break;
              default :
                TeamlabMobile.saveHistoryPage();
                break;
            }

            switch (type) {
                case 'index-page':
                    $page.find('input.top-search-field:first').val('').unbind('keyup').bind('keyup', onIndexSearchKeyUp);
                    $page.find('input.top-search-field:first').val('').parents('form.search-form:first').unbind('submit').bind('submit', onIndexSearch);
                    break;
                case 'search-page':
                    $page.find('input.top-search-field:first').val('').unbind('keyup').bind('keyup', onIndexSearchKeyUp);
                    $page.find('input.top-search-field:first').parents('form.search-form:first').unbind('submit').bind('submit', onIndexSearch);
                    break;
                case 'people-page':
                    $page.find('li.active-item').removeClass('active-item');
                    $page.find('li.item-index').removeClass('uncorrect-item');
                    $page.find('li.item-persone').removeClass('uncorrect-item');
                    $page.find('input.top-search-field:first').val('').unbind('keyup').bind('keyup', DefaultMobile.filter_people_contacts);
                    DefaultMobile.resizeFilterItems($page.find('div.ui-navbar-people:first'));
                    break;
                case 'crm-page':
                    $page.find('input.top-search-field:first').parents('form.search-form:first').unbind('submit').bind('submit', DefaultMobile.search_crm_contacts);
                    //$page.find('li.active-item').removeClass('active-item');
                    //$page.find('li.item.person').removeClass('uncorrect-item');
                    //$page.find('input.top-search-field:first').val('').unbind('keyup').bind('keyup', DefaultMobile.filter_crm_contacts);
                    break;
                case 'people-item-page':
                case 'community-blog-page':
                case 'community-poll-page':
                case 'community-forum-page':
                case 'community-event-page':
                case 'community-bookmark-page':
                case 'projects-discussion-page':
                case 'page-projects-milestone-tasks':
                    $page.find('textarea.comment-content:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    $page.find('textarea.comment-content:first').unbind('keypress').bind('keypress', onFormSubmitByCtrlEnter);
                    break;
                case 'community-addblog-page':
                    $page.find('textarea.blog-description:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'community-addforum-page':
                    $page.find('textarea.forum-description:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'community-addbookmark-page':
                    $page.find('textarea.bookmark-description:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'projects-addtask-page':
                    $page.find('textarea.task-description:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'documents-adddocument-page':
                    $page.find('textarea.document-content:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'projects-page':
                case 'projects-tasks-page':
                case 'projects-task-page':              
                case 'crm-task-page':
                case 'crm-contacttasks-page':
                    $page.find('textarea.comment-content:first').unbind('keyup').bind('keyup', onResizeTextArea);
                    break;
                case 'crm-tasks-page':
                    $page.find('input.top-search-field:first').parents('form.search-form:first').unbind('submit').bind('submit', DefaultMobile.search_crm_tasks);
                    break;
                case 'community-addcomment-page':
                    $page.find('textarea:first').focus();
                    break;
                case 'documents-page':
                    $page.find('li.active-item').removeClass('active-item');
                    break;
                case 'documents-item-page':
                    DefaultMobile.resizeViewer($page);
                    break;
                case 'documents-addfolder-page':
                    $page.find('input.folder-title:first').unbind('keyup').bind('keyup', onFormSubmitByEnter);
                    break;                
            }
        });

    $window
        .bind('orientationchange', function(evt) {
            setTimeout(DefaultMobile.scrollToTop, 1);
            setTimeout(DefaultMobile.setScreenMetrics, 10);

            $('div.ui-dialog-active').removeClass('ui-dialog-active');

            if ($.support.touch) {
                var $page = $('div.ui-page-active:first');

                DefaultMobile.resizeBody($mainform);
                DefaultMobile.resizePage($page);
                DefaultMobile.resizePage();

                var needTimeout = true;
                if (Math.abs(window.orientation) === 90 && DefaultMobile.constants.screenMetrics.width !== null || Math.abs(window.orientation) !== 90 && DefaultMobile.constants.screenMetrics.height !== null) {
                    needTimeout = false;
                }

                if (needTimeout) {
                    setTimeout(DefaultMobile.resizeScroller, 100);
                } else {
                    DefaultMobile.resizeScroller();
                }

                if ($.support.fscroll) {
                    if ($page.find('div.ui-scroller:first').length > 0) {
                        rerenderFooter();
                    }
                }
            }
        });

    $(function() {
        if (window.DISABLEDCOOKIES === true) {
            return undefined;
        }
        if (window.INVALIDCLIENT === true) {
            return undefined;
        }


        DefaultMobile.defaultResizeBody();
        setTimeout(DefaultMobile.scrollToTop, 1000);
        setTimeout(DefaultMobile.setScreenMetrics, 1100);

        setTimeout(
      function() {
          DefaultMobile.resizeBody();
          DefaultMobile.resizePage();

          if ($('div.ui-page-active:first').hasClass('page-auth')) {
              $(".target-standart").unbind(DefaultMobile.constants.touchClick, onHrefClick).bind(DefaultMobile.constants.touchClick, onHrefClick);
              return undefined;
          }        

          jQuery(document.body).removeClass('ui-mobile-rendered-page').addClass('ui-mobile-rendering-page');
          var p = { counter: 0, handler: null };
          p.handler = setInterval(
          (function(p) {
              return function() {
                  if (!window.applicationCache || ++p.counter > 100 || window.applicationCache.status === 0 || window.applicationCache.status === 1 || window.applicationCache.status === 4) {
                      clearInterval(p.handler);
                      //TeamlabMobile.renderScripts('text/x-jquery-tmpl');
                      TeamlabMobile.preInit();
                      return undefined;
                  }
              };
          })(p),
          100
        );
      },
      1500
    );
    });
})(jQuery);
