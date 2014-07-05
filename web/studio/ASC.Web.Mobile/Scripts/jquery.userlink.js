
(function ($) {
  $.fn.userlink = function () {
    var userid = null, item = null, itemsInd = 0;

    itemsInd = this.length;
    while (itemsInd--) {
      item = this[itemsInd];
      if (!!item.getAttribute('data-userlink')) {
        continue;
      }
      userid = item.getAttribute('__ascuser');
      if (userid) {
        item.innerHTML = '<a href="#people/' + userid + '">' + item.innerHTML + '</a>';
      }
      item.setAttribute('data-userlink', '1');
    }
  };
})(jQuery);
