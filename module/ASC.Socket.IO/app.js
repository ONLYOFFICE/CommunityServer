const express = require('express');
const logger = require('morgan');
const app = express();
const config = require('./config');
const winston = require('./app/log.js');

app.set('port', config.get('port') || 3000);

winston.stream = {
    write: (message) => winston.info(message)
};

app.use(logger("dev", { "stream": winston.stream }));

app.get('/', (req, res) => { res.send('<h1>Hello world</h1>'); });

const server = app.listen(app.get('port'), () => {
    //log.info('Express server listening on port ' + server.address().port);
});

const io = require('socket.io')(server, {perMessageDeflate : false, cookie: false});
const cookieParser = require('cookie-parser')();
const auth = require('./app/middleware/auth.js');

io
    .use((socket, next) => {
        cookieParser(socket.client.request, null, next);
    })
    .use((socket, next) => {
        auth(socket.client.request, next);
    });

const countersHub = require('./app/hubs/counters.js')(io);
const voipHub = require('./app/hubs/voip.js')(io);
const chatHub = require('./app/hubs/chat.js')(io);

app.use("/controller", require('./app/controllers')(countersHub, chatHub, voipHub));

module.exports = app;
