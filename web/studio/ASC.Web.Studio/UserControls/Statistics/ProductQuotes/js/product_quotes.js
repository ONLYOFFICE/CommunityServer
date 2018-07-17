ASC.ProductQuotes = (function () {
    var $recalculateButton,
        disableClass = "disable",
        timeOut = 10000,
        teamlab;

    function init() {
        $recalculateButton = jq('.storeUsage .button.blue');
        teamlab = Teamlab;

        initClickEvents();

        checkRecalculate();
    }

    function initClickEvents() {
        jq('.moreBox a.topTitleLink').click(function () {
            jq(jq(this).parent().prev()).find('tr').show();
            jq(this).parent().hide();
        });

        $recalculateButton.click(function () {
            if ($recalculateButton.hasClass(disableClass)) return;

            teamlab.recalculateQuota({}, { success: onRecalculate });
        });
    }

    function onRecalculate() {
        $recalculateButton.addClass(disableClass);
        setTimeout(checkRecalculate, timeOut);
    }

    function checkRecalculate() {
        teamlab.checkRecalculateQuota({}, { success: onCheckRecalculate });
    }

    function onCheckRecalculate(params, result) {
        if (!result) {
            $recalculateButton.removeClass(disableClass);
            return;
        } 
        if (!$recalculateButton.hasClass(disableClass)) {
            $recalculateButton.addClass(disableClass);
        }

        setTimeout(checkRecalculate, timeOut);
    }

    return { init: init };
})();

jq(document).ready(function () {
    ASC.ProductQuotes.init();
});