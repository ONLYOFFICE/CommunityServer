if (typeof ASC === "undefined")
    ASC = {};

ASC.SocketIO = function (hub) {
    var master = ASC.Resources.Master;
    if (master.Hub && master.Hub.Url) {
        var masterHub = master.Hub;
        var opts = {
            path: masterHub.Url,
            reconnectionAttempts: 5,
            perMessageDeflate: "false"
        }
        if (master.numberId && masterHub.VoipEnabled) {
            opts.query = { numberId: master.numberId };
        }
        this.socket = io(hub, opts);
    }
};

ASC.SocketIO.prototype.emit = function() {
    if (this.socket) {
        this.socket.emit.apply(this.socket, arguments);
    }
    return this;
}

ASC.SocketIO.prototype.on = function () {
    if (this.socket) {
        if (arguments[0] === "connect" && typeof arguments[1] === "function" && this.socket.connected) {
            arguments[1]();
        } else {
            this.socket.on.apply(this.socket, arguments);
        }
    }
    return this;
}

ASC.SocketIO.prototype.connect = function () {
    return this.on.call(this, "connect", arguments[0]);
}

ASC.SocketIO.prototype.reconnect_failed = function () {
    return this.on.call(this, "reconnect_failed", arguments[0]);
}

ASC.SocketIO.prototype.connected = function () {
    return this.socket && this.socket.connected;
}

ASC.SocketIO.disabled = function () {
    return !ASC.Resources.Master.Hub || !ASC.Resources.Master.Hub.Url;
}

ASC.SocketIO.Factory = (function () {
    var chat, voip, counters;

    return {
        get chat() {
            if (!chat) {
                chat = new ASC.SocketIO("/chat");
            }
            return chat;
        },
        get voip() {
            if (!voip) {
                voip = new ASC.SocketIO("/voip");
            }
            return voip;
        },
        get counters() {
            if (!counters) {
                counters = new ASC.SocketIO("/counters");
            }
            return counters;
        }
    };
})();

jq(document).ready(function () {
    ASC.SocketIO.Factory.counters
    .on('getNewMessagesCount', function (counts) {
        if (ASC.Controls.MailReader) {
            ASC.Controls.MailReader.setUnreadMailMessagesCount(counts.ma);
        }
    })
    .on('sendMailNotification', function (state) {
        ASC.Mail.Utility._showSignalRMailNotification(state);
    })
    .on('updateFolders', function (counts, shouldUpdateMailBox) {
        if (window.serviceManager) {
            window.serviceManager.updateFolders();
            if (shouldUpdateMailBox) {
                window.serviceManager.getAccounts();
            }
        } else if (ASC.Controls.MailReader) {
            ASC.Controls.MailReader.setUnreadMailMessagesCount(counts);
        }
    });
});