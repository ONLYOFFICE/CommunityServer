/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


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