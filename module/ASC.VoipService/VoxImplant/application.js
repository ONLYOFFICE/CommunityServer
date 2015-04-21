/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


var operatorCalls = {},
    nOperatorCalls = 0,
    activeOperatorCall,
    TIMEOUT = 5000,
    operatorTimer,
    extensionLength = 3;
var callTimer;

VoxEngine.addEventListener(AppEvents.CallAlerting, function (e) {
    var oper;
    
    Logger.write("e.callerid:" + e.callerid);
    
    for (var i = 0; i < voxImplantSettings.operators.length; i++) {
        if (e.callerid.indexOf(voxImplantSettings.operators[i].data) > -1) {
            oper = voxImplantSettings.operators[i];
        }
    }
    
    typeof oper == "undefined" ? inboundCall(e) : outboundCall(e, oper);
});

function outboundCall(e, oper) {
    Logger.write("outboundCall");
    VoxEngine.forwardCallToPSTN(null, function (c1, c2) {
        c1.addEventListener(CallEvents.Connected, function(e) {
            if (oper.Record) {
                e.call.record();
            }
        });
    });
}

function inboundCall(e) {
    var incomingCall = e.call;
    Logger.write("inboundCall");
    incomingCall.answer();
    
    if (voxImplantSettings.workingHours) {
        Logger.write("WorkingHours");
        var date = new Date();
        var workingFrom = voxImplantSettings.workingHours.from.split(':');
        var workingTo = voxImplantSettings.workingHours.to.split(':');
        Logger.write("workingFrom: " + parseInt(workingFrom[0], 10));
        Logger.write("workingTo: " + parseInt(workingTo[0], 10));
        if (!(parseInt(workingFrom[0], 10) <= date.getUTCHours() && parseInt(workingTo[0], 10) > date.getUTCHours())) {
            Logger.write("WorkingHours:voiceMail");
            incomingCall.addEventListener(CallEvents.Connected, function (e) {
                voiceMail(e.call);
            });
            return;
        }
    }


    incomingCall.handleTones(true);

    incomingCall.addEventListener(CallEvents.Connected, function (e) {
        Logger.write("Connected:" + e.call.incoming());
        voxImplantSettings.greetingAudio != null && voxImplantSettings.greetingAudio.enabled
            ? e.call.startPlayback(voxImplantSettings.greetingAudio.url, false)
            : callUser(e);

    });
    
    var input = '';
    
    incomingCall.addEventListener(CallEvents.ToneReceived, function (e) {
        Logger.write("ToneReceived");
        input += e.tone;
        if (input.length == extensionLength) {
            e.call.stopPlayback();
            forwardCallToExtension(e.call, input);
        }
    });

    incomingCall.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);
    incomingCall.addEventListener(CallEvents.PlaybackFinished, callUser);
}

function callUser(e) {
    e.call.removeEventListener(CallEvents.PlaybackFinished, callUser);
    operatorTimer = setTimeout(function () {
        forwardCallToOperator(e.call);
    }, TIMEOUT);
}

function forwardCallToExtension(call, ext) {
    Logger.write("forwardCallToExtension");
    clearTimeout(operatorTimer);
    call.handleTones(false);
    call.playProgressTone("RU");
    
    var oper = voxImplantSettings.operators[0];
    
    for (var i = 0; i < voxImplantSettings.operators.length; i++) {
        if (voxImplantSettings.operators[i].data == ext) {
            oper = voxImplantSettings.operators[i];
        }
    }

    var call2 = Dial(oper, true);
    
    call2.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);
    
    call2.addEventListener(CallEvents.Connected, function (e) {
        clearTimeout(callTimer);
        VoxEngine.sendMediaBetween(call, e.call);
        if (oper.Record) {
            e.call.record();
        }
    });
}

function forwardCallToOperator(call) {
    Logger.write("forwardCallToOperator");
    call.handleTones(false);
    clearTimeout(operatorTimer);
    nOperatorCalls = 0;
    call.playProgressTone("RU");
    
    if (!voxImplantSettings.operators.some(function (item) { return !item.Status; })) {
        call.removeEventListener(CallEvents.PlaybackFinished, callUser);
        voiceMail(call);
        return;
    }

    for (var i in voxImplantSettings.operators) {
        var j = voxImplantSettings.operators[i];
        if (j.Status) continue;
        
        nOperatorCalls++;
        
        operatorCalls[j] = Dial(j);
        
        operatorCalls[j].addEventListener(CallEvents.Failed, function (e) {
            if (typeof activeOperatorCall == "undefined") {
                delete operatorCalls[e.call.number()];
                nOperatorCalls--;
                if (nOperatorCalls == 0) {
                    Logger.write("Failed");
                    voiceMail(call);
                }
            }
        });

        operatorCalls[j].addEventListener(CallEvents.Connected, function (e) {
            clearTimeout(callTimer);
            delete operatorCalls[e.call.number()];
            activeOperatorCall = e.call;
            VoxEngine.sendMediaBetween(call, activeOperatorCall);
            if (j.Record) {
                call.record();
            }
            activeOperatorCall.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);
            for (var i in operatorCalls) {
                operatorCalls[i].hangup();
            }
            operatorCalls = {};
        });
    }
}

function Dial(oper, voice) {
    var call;
    switch (oper.Answer) {
        case 0:
            call = VoxEngine.callPSTN(oper.Data);
            break;
        case 1:
            call = VoxEngine.callSIP(oper.Data);
            break;
        case 2:
            call = VoxEngine.callUser(voxImplantSettings.name + oper.Data);
            break;
    }
    if (voice) {
        call.addEventListener(CallEvents.Failed, voiceMail);
    }

    return call;
}

function voiceMail(call) {
    call = call.call || call;
    call.removeEventListener(CallEvents.PlaybackFinished, voiceMail);
    if (voxImplantSettings.voiceMail == null) {
        VoxEngine.terminate();
    }
    Logger.write("voiceMail:EnterOperator");
    Logger.write("voiceMail:TimeOut" + voxImplantSettings.voiceMail.timeOut);
    call.startPlayback(voxImplantSettings.voiceMail.leaveMessage, false);
    call.addEventListener(CallEvents.PlaybackFinished, function(e) {
        e.call.record();
        setTimeout(function () {
            Logger.write("voiceMail:TimeOut");
            VoxEngine.terminate();
        }, voxImplantSettings.voiceMail.timeOut * 1000);
    });

}

VoxEngine.addEventListener(AppEvents.HttpRequest, function (e) {
    var destination;
    e.content.split('&').map(function (item) {
        var data = item.split('=');

        if (data[0] == 'destination') {
            destination = data[1];
        }
        
        return { name: data[0], value: data[1] };
    });
    
    Logger.write("destination:" + destination);
    var oper = voxImplantSettings.caller || voxImplantSettings.operators[0];
    Logger.write("data:" + oper.Data + ",answer:" + oper.Answer);
    
    if (destination.length > 0) {
        var call1 = Dial(oper, true);
        
        call1.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);

        call1.addEventListener(CallEvents.Connected, function (e) {
            clearTimeout(callTimer);
            if (oper.Record) {
                e.call.record();
            }
        });

        var call2 = VoxEngine.callPSTN(destination, voxImplantSettings.name + oper.Data);
        VoxEngine.sendMediaBetween(call1, call2);
        VoxEngine.easyProcess(call1, call2, function (call3, call4) {
            call3.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);
            call4.addEventListener(CallEvents.Disconnected, VoxEngine.terminate);
        });
        return call1.id();
    }
    return '';
});