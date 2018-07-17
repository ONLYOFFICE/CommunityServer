module.exports = function (socket, next) {
    const apiRequestManager = require('../apiRequestManager.js');
    const req = socket.client.request;
    const authService = require('./authService.js')();
    const co = require('co');
    const session = socket.handshake.session;

    if (req.user) {
        next();
        return;
    }

    if (!req.cookies || (!req.cookies['asc_auth_key'] && !req.cookies['authorization'])) {
        socket.disconnect('unauthorized');
        next(new Error('Authentication error'));
        return;
    }

    if(session && session.user && session.portal) {
        req.user = session.user;
        req.portal = session.portal;
        next();
        return;
    }

    if(req.cookies['authorization']){
        if(!authService(req)){
            next(new Error('Authentication error'));
        } else{
            next();
        }
        return;
    }

    co(function*(){
        var batchRequest = apiRequestManager.batchFactory()
            .get("people/@self.json?fields=id,userName,displayName,isVisitor")
            .get("portal.json?fields=tenantId,tenantDomain");

        [session.user, session.portal] = [req.user, req.portal] = yield apiRequestManager.batch(batchRequest, req);
        session.save();
        next();
    }).catch((err) => {
        socket.disconnect('unauthorized');
        next(new Error('Authentication error'));
    });
}