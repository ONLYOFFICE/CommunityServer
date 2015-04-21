/*
 * jQuery autoResize (textarea auto-resizer)
 * @copyright James Padolsey http://james.padolsey.com
 * @version 1.04
 */

(function($) {

    $.fn.autoResize = function(options) {

        // Just some abstracted details,
        // to make plugin users happy:
        var settings = $.extend({
            onResize: function() {
            },
            animate: true,
            animateDuration: 150,
            animateCallback: function() {
            },
            limit: 1000,
            cleanInput: false
        }, options);

        // Only textarea's auto-resize:
        this.filter('textarea').each(function() {

            // Get rid of scrollbars and disable WebKit resizing:
            var textarea = $(this).css({ resize: 'none', 'overflow-y': 'hidden' }),
                // Cache original height, for use later:
                origHeight = textarea.height(),
                // Need clone of textarea, hidden off screen:
                clone = (function() {

                    // Properties which may effect space taken up by chracters:
                    var props = ['height', 'lineHeight', 'textDecoration', 'letterSpacing'],
                        propOb = {};

                    // Create object of styles to apply:
                    $.each(props, function(i, prop) {
                        propOb[prop] = textarea.css(prop);
                    });

                    // Clone the actual textarea removing unique properties
                    // and insert before original textarea:
                    return textarea.clone().removeAttr('id').removeAttr('name').css({
                        position: 'absolute',
                        top: 0,
                        left: -9999
                    }).css(propOb).attr('tabIndex', '-1').insertBefore(textarea);

                })(),
                lastScrollTop = null,
                updateSize = function() {
                    // Prepare the clone:
                    clone.width($(this).width());
                    clone.height(0).val($(this).val()).scrollTop(10000);

                    // Find the height of text:
                    var scrollTop = Math.max(clone.scrollTop(), origHeight),
                        toChange = $(this).add(clone);

                    // Don't do anything if scrollTip hasen't changed:
                    if (lastScrollTop === scrollTop) {
                        return;
                    }
                    lastScrollTop = scrollTop;

                    // Check for limit:
                    if (scrollTop >= settings.limit) {
                        toChange.height(settings.limit);
                        $(this).css('overflow-y', '');
                        return;
                    }
                    // Fire off callback:
                    settings.onResize.call(this);

                    // Either animate or directly apply height:
                    settings.animate && textarea.css('display') === 'block' ?
                        toChange.stop().animate({ height: scrollTop }, settings.animateDuration, settings.animateCallback)
                        : toChange.height(scrollTop);
                },
                cleanKeydown = function(e) {
                    // 32 - space code
                    // 13 - enter code
                    if ("" == $(this).val() && (32 == e.which || 13 == e.which)) {
                        return false;
                    }
                },
                cleanPaste = function() {
                    var $this = $(this);
                    if ("" == $(this).val()) {
                        setTimeout(function() {
                            $this.val($this.val().replace(/^\s+/g, "").replace(/\s+$/g, ""));
                        }, 0);
                    }
                    ;
                };

            // Bind namespaced handlers to appropriate events:
            textarea
                .unbind('.dynSiz');

            if (settings.cleanInput) {
                textarea
                    .bind('paste.dynSiz', cleanPaste)
                    .bind('keydown.dynSiz', cleanKeydown);
            }

            textarea
                .bind('keyup.dynSiz', updateSize)
                .bind('keydown.dynSiz', updateSize)
                .bind('input.dynSiz', updateSize);

        });

        // Chain:
        return this;

    };


})(jQuery);