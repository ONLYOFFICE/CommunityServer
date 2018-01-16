module.exports = function (req, next) {
    const apiRequestManager = require('../apiRequestManager.js');
    const authService = require('./authService.js')();
    const co = require('co');

    if (req.user) {
        next();
        return;
    }
    if (!req.cookies || (!req.cookies['asc_auth_key'] && !req.cookies['authorization'])) {
        next(new Error('Authentication error'));
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
            .get("people/@self.json")
            .get("portal.json");

        [req.user, req.portal] = yield apiRequestManager.batch(batchRequest, req);

        next();
    }).catch((err) => {
        next();
    });
}