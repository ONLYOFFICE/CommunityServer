jq(document).ready(function () {
    jq('#switcherSubscriptionButton').one('click', function () {
        if (!jq('#subscriptionBlockContainer').hasClass('subsLoaded') &&
            typeof (window.CommonSubscriptionManager) != 'undefined' &&
            typeof (window.CommonSubscriptionManager.LoadSubscriptions) === 'function') {
            window.CommonSubscriptionManager.LoadSubscriptions();
            jq('#subscriptionBlockContainer').addClass('subsLoaded');
        }
    });
});