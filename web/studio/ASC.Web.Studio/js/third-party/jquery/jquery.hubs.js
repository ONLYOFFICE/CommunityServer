/*!
 * ASP.NET SignalR JavaScript Library v2.0.3
 * http://signalr.net/
 *
 * Copyright Microsoft Open Technologies, Inc. All rights reserved.
 * Licensed under the Apache 2.0
 * https://github.com/SignalR/SignalR/blob/master/LICENSE.md
 *
 */

/// <reference path="..\..\SignalR.Client.JS\Scripts\jquery-1.6.4.js" />
/// <reference path="jquery.signalR.js" />
(function ($, window, undefined) {
    /// <param name="$" type="jQuery" />
    "use strict";

    if (typeof ($.signalR) !== "function") {
        throw new Error("SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.");
    }

    var signalR = $.signalR;

    function makeProxyCallback(hub, callback) {
        return function () {
            // Call the client hub method
            callback.apply(hub, $.makeArray(arguments));
        };
    }

    function registerHubProxies(instance, shouldSubscribe) {
        var key, hub, memberKey, memberValue, subscriptionMethod;

        for (key in instance) {
            if (instance.hasOwnProperty(key)) {
                hub = instance[key];

                if (!(hub.hubName)) {
                    // Not a client hub
                    continue;
                }

                if (shouldSubscribe) {
                    // We want to subscribe to the hub events
                    subscriptionMethod = hub.on;
                } else {
                    // We want to unsubscribe from the hub events
                    subscriptionMethod = hub.off;
                }

                // Loop through all members on the hub and find client hub functions to subscribe/unsubscribe
                for (memberKey in hub.client) {
                    if (hub.client.hasOwnProperty(memberKey)) {
                        memberValue = hub.client[memberKey];

                        if (!$.isFunction(memberValue)) {
                            // Not a client hub function
                            continue;
                        }

                        subscriptionMethod.call(hub, memberKey, makeProxyCallback(hub, memberValue));
                    }
                }
            }
        }
    }

    $.hubConnection.prototype.createHubProxies = function () {
        var proxies = {};
        this.starting(function () {
            // Register the hub proxies as subscribed
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, true);

            this._registerSubscribedHubs();
        }).disconnected(function () {
            // Unsubscribe all hub proxies when we "disconnect".  This is to ensure that we do not re-add functional call backs.
            // (instance, shouldSubscribe)
            registerHubProxies(proxies, false);
        });

        proxies.c = this.createHubProxy('c'); 
        proxies.c.client = { };
        proxies.c.server = {
            gci: function (userName) {
            /// <summary>Calls the gci method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"userName\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["gci"], $.makeArray(arguments)));
             },

            gid: function () {
            /// <summary>Calls the gid method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies.c.invoke.apply(proxies.c, $.merge(["gid"], $.makeArray(arguments)));
             },

            grm: function (calleeUserName, id) {
            /// <summary>Calls the grm method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"calleeUserName\" type=\"String\">Server side type is System.String</param>
            /// <param name=\"id\" type=\"Number\">Server side type is System.Int32</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["grm"], $.makeArray(arguments)));
             },

            gs: function () {
            /// <summary>Calls the gs method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies.c.invoke.apply(proxies.c, $.merge(["gs"], $.makeArray(arguments)));
             },

            gui: function (userName) {
            /// <summary>Calls the gui method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"userName\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["gui"], $.makeArray(arguments)));
             },

            p: function () {
            /// <summary>Calls the p method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies.c.invoke.apply(proxies.c, $.merge(["p"], $.makeArray(arguments)));
             },

            s: function (calleeUserName, messageText) {
            /// <summary>Calls the s method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"calleeUserName\" type=\"String\">Server side type is System.String</param>
            /// <param name=\"messageText\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["s"], $.makeArray(arguments)));
             },

            sstt: function (state) {
            /// <summary>Calls the sstt method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"state\" type=\"Number\">Server side type is System.Byte</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["sstt"], $.makeArray(arguments)));
             },

            st: function (calleeUserName) {
            /// <summary>Calls the st method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"calleeUserName\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["st"], $.makeArray(arguments)));
             },

            cu: function () {
            /// <summary>Calls the st method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"calleeUserName\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["cu"], $.makeArray(arguments)));
             },

            dcu: function () {
            /// <summary>Calls the st method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
            /// <param name=\"calleeUserName\" type=\"String\">Server side type is System.String</param>
                return proxies.c.invoke.apply(proxies.c, $.merge(["dcu"], $.makeArray(arguments)));
             }
        };
        
        proxies.voip = this.createHubProxy('voip');
        proxies.voip.client = {
            miss: function(call) {
                console.log(call);
            },
            mail: function(call) {
                console.log(call);
            },
            onlineAgents : function(agents) {
                console.log(agents);
            },
            start: function() {
                console.log("start");
            },
            end: function() {
                console.log("end");
            }
        };
        proxies.voip.server = {
            status: function (status) {
                /// <summary>Calls the st method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                /// <param name=\"status\" type=\"String\">Server side type is System.String</param>
                return proxies.voip.invoke.apply(proxies.voip, $.merge(["status"], $.makeArray(arguments)));
            },            
            dequeue: function () {
                /// <summary>Calls the dequeue method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies.voip.invoke.apply(proxies.voip, $.merge(["dequeue"], $.makeArray(arguments)));
            },            
            enqueue: function (call) {
                /// <summary>Calls the enqueue method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                /// <param name=\"call\" type=\"String\">Server side type is System.String</param>
                return proxies.voip.invoke.apply(proxies.voip, $.merge(["enqueue"], $.makeArray(arguments)));
            },            
            onlineAgents: function () {
                /// <summary>Calls the onlineAgents method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                return proxies.voip.invoke.apply(proxies.voip, $.merge(["onlineAgents"], $.makeArray(arguments)));
            }
        };
        
        proxies.users = this.createHubProxy('users');
        proxies.users.client = {
            renderOnlineUsers: function() {
                
            },
            renderOnlineUser: function() {
                
            },
            renderOfflineUser: function() {
                
            }
        };
        
        proxies.users.server = {
            addToObserversOnlineUsers: function() {
                /// <summary>Calls the addToObserversOnlineUsers method on the server-side c hub.&#10;Returns a jQuery.Deferred() promise.</summary>
                /// <param name=\"status\" type=\"String\">Server side type is System.String</param>
                return proxies.users.invoke.apply(proxies.users, $.merge(["addToObserversOnlineUsers"], $.makeArray(arguments)));
            }
        };
        
        return proxies;
    };

    signalR.hub = $.hubConnection("/signalr", { useDefaultPath: false });
    $.extend(signalR, signalR.hub.createHubProxies());

}(window.jQuery, window));

jq(document).ready(function () {
    if (!ASC.Resources.Master.Hub.Url) {
        return;
    }
    var transpots = ["webSockets", "longPolling", "foreverFrame", "serverSentEvents"],
        RECONNECT_TIMEOUT = 1500,
        LIMIT_ATTEMPTS = 3,
        attempt = 0;
    jq.connection.hub.qs = {
        token: ASC.Resources.Master.Hub.Token
    };
    
    if (ASC.Resources.Master.numberId && ASC.Resources.Master.Hub.VoipEnabled == "true") {
        jq.connection.hub.qs.numberId = ASC.Resources.Master.numberId;
    } else {
        delete jq.connection.hub.proxies.voip;
    }
    if (ASC.Resources.Master.Hub.WebChat != "true") {
        delete jq.connection.hub.proxies.c;
    }
    jq.connection.hub.url = ASC.Resources.Master.Hub.Url;
    jq.connection.hub.logging = ASC.Resources.Master.Hub.Logging == "true";
    jq.connection.onStart = jq.connection.hub.start({
        transport: transpots
    });
    jq.connection.hub.disconnected(function () {
        var timeoutId = setTimeout(function () {
            if (attempt < LIMIT_ATTEMPTS) {
                jq.connection.hub.start({
                    transport: transpots
                }).done(function () {
                    clearTimeout(timeoutId);
                    timeoutId = null;
                    attempt = 0;
                });
                attempt++;
            } else {
                clearTimeout(timeoutId);
                timeoutId = null;
                attempt = 0;
            }
        }, RECONNECT_TIMEOUT);
    });
});