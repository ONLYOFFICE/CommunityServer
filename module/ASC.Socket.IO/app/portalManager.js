const portalInternalUrl = require('../config').get("portal.internal.url")
module.exports = (req) => {
    if(portalInternalUrl) return portalInternalUrl;

    const xRewriterUrlInternalHeader = 'x-rewriter-url-internal';
    if (req.headers && req.headers[xRewriterUrlInternalHeader]) {
        return req.headers[xRewriterUrlInternalHeader];
    }

    const xRewriterUrlHeader = 'x-rewriter-url';
    if (req.headers && req.headers[xRewriterUrlHeader]) {
        return req.headers[xRewriterUrlHeader];
    }

    return "";
};