const express = require('express');
const logger = require('morgan');
const cookieParser = require('cookie-parser');
const expressSession = require("express-session");
const sharedsession = require("express-socket.io-session");
const redis = require('redis');
const RedisStore = require('connect-redis')(expressSession);
const MemoryStore = require('memorystore')(expressSession);
const config = require('./config');
const winston = require('./app/log.js');
const app = express();

const secret = config.get("core.machinekey") + (new Date()).getTime();
const secretCookieParser = cookieParser(secret);
const baseCookieParser = cookieParser();

winston.stream = {
    write: (message) => winston.info(message)
};

const redisOpt = {
    host: config.get("redis:host"),
    port: config.get("redis:port"),
    ttl: 3600
}

let store;
if(redisOpt.host && redisOpt.port){
    let redisClient = redis.createClient(redisOpt);
    store =new RedisStore({ client: redisClient });
} else {
    store = new MemoryStore();
}

const session = expressSession({
    store: store,
    secret: secret,
    resave: true,
    saveUninitialized: true,
    cookie: { 
        path: '/', 
        httpOnly: true, 
        secure: false,
        maxAge: null
    },
    cookieParser: secretCookieParser,
    name: "socketio.sid"
});

app.set('port', config.get('port') || 3000);
app.use(logger("dev", { "stream": winston.stream }));
app.use(session);
app.get('/', (req, res) => { res.send('<h1>Hello world</h1>'); });

const server = app.listen(app.get('port'), () => {
    //log.info('Express server listening on port ' + server.address().port);
});

const io = require('socket.io')(server, {
    perMessageDeflate : false,
    cookie: false,
    handlePreflightRequest: function (req, res) {
        session(req, res, ()=>{});        
        res.writeHead(200, {'Content-Type': 'text/html'});
        res.end();
      },
    allowRequest : function(req, fn){
        var cookies = baseCookieParser(req, null, ()=>{});
        if(!req.cookies || (!req.cookies['asc_auth_key'] && !req.cookies['authorization'])){
            return fn('auth', false);
        }

        return io.checkRequest(req, fn);
    }
});
const auth = require('./app/middleware/auth.js');

io
    .use(sharedsession(session, secretCookieParser, {autoSave: true}))
    .use((socket, next) => {
        baseCookieParser(socket.client.request, null, next);
    })
    .use((socket, next) => {
        auth(socket, next);
    });

const countersHub = require('./app/hubs/counters.js')(io);
const voipHub = require('./app/hubs/voip.js')(io);
const chatHub = require('./app/hubs/chat.js')(io);
const filesHub = require('./app/hubs/files.js')(io);

app.use("/controller", require('./app/controllers')(countersHub, chatHub, voipHub, filesHub));

module.exports = app;
