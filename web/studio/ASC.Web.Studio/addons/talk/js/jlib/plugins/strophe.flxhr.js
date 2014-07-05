/* flXHR plugin
**
** This plugin implements cross-domain XmlHttpRequests via an invisible
** Flash plugin.
**
** In order for this to work, the BOSH service *must* serve a
** crossdomain.xml file that allows the client access.
**
** flXHR.js should be loaded before this plugin.
*/

Strophe.addConnectionPlugin('flxhr', {
    init: function () {
        // replace Strophe.Request._newXHR with new flXHR version
        // if flXHR is detected
        if (flensed && flensed.flXHR) {
            Strophe.Request.prototype._newXHR = function () {
                var xhr = new flensed.flXHR({
                    autoUpdatePlayer: true,
                    instancePooling: true,
                    noCacheHeader: false});
                xhr.onreadystatechange = this.func.prependArg(this);

                return xhr;
            };
        } else {
            Strophe.error("flXHR plugin loaded, but flXHR not found." +
                          "  Falling back to native XHR implementation.");
        }
    }
});
