const VoipPhone = require('./voipPhone.js');

class ListVoipPhone {
    constructor() {
        this.phones = [];
    }

    addPhone(numberId) {
        const existingPhone = this.phones.find((item) => item.numberId === numberId);
        if (!existingPhone) {
            this.phones.push(new VoipPhone(numberId));
        }
    }

    getPhone(numberId) {
        return this.phones.find((item) => item.numberId === numberId);
    }

    addOrUpdateAgent(numberId, agent) {
        const phone = this.getPhone(numberId);
        if (!phone) return;
        phone.addOrUpdateAgent(agent);
    }

    removeAgent(numberId, agentId) {
        const phone = this.getPhone(numberId);
        if (!phone) return;
        phone.removeAgent(agentId);
    }

    anyCalls(numberId) {
        const phone = this.getPhone(numberId);
        if (!phone) return false;
        return phone.anyCalls();
    }

    dequeueCall(numberId) {
        const phone = this.getPhone(numberId);
        if (!phone) return "";
        return phone.dequeueCall();
    }

    removeCall(numberId, callId) {
        const phone = this.getPhone(numberId);
        if (!phone) return;
        phone.removeCall(callId);
    }

    enqueue(numberId, callId, agent) {
        const phone = this.getPhone(numberId);
        if (!phone) return "";
        return phone.enqueue(callId, agent);
    }

    onlineAgents(numberId) {
        const phone = this.getPhone(numberId);
        if (!phone) return [];
        return phone.onlineAgents();
    }

    getStatus(numberId, agentId) {
        const phone = this.getPhone(numberId);
        if (!phone) return 2;
        return phone.getAgentStatus(agentId);
    }

    getAgent(numberId, contactResponsibles) {
        const phone = this.getPhone(numberId);
        if (!phone) return null;
        return phone.getAgent(contactResponsibles);
    }
}

module.exports = ListVoipPhone;