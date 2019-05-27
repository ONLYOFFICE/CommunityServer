function findAgent(agentId, item) {
    return item.id === agentId;
}
class VoipPhone {
    constructor(numberId) {
        this.agents = [];
        this.calls = [];
        this.numberId = numberId;
    }

    addOrUpdateAgent(agent) {
        const existingAgent = this.agents.find(findAgent.bind(null, agent.Id));
        if (existingAgent) {
            existingAgent.status = agent.status;
        } else {
            this.agents.push(agent);
        }
    }

    removeAgent(agentId) {
        const existingAgent = this.agents.some(findAgent.bind(null, agentId));
        if (!existingAgent) return false;

        this.agents = this.agents.filter((item) => item.id !== agentId);
        return true;
    }

    addCall(callId) {
        if (this.calls.some((item) => item === callId)) return;
        this.calls.push(callId);
    }

    removeCall(callId) {
        if (!callId || !this.calls.some((item) => item === callId)) return;
        this.calls = this.calls.filter((item) => item !== callId);
    }

    anyCalls() {
        return !!this.calls.length;
    }

    dequeueCall() {
        return this.calls.pop();
    }

    enqueue(callId, agent) {
        if (agent && this.removeAgent(agent)) {
            return agent;
        }

        const agents = this.agents.filter((item) =>item.status === 0 );
        if (agents.length) {
            return agents[0].id;
        }

        this.addCall(callId);
        return "";
    }

    onlineAgents() {
        return this.agents.filter((item) => item.status === 0).map((item) => item.id);
    }

    getAgentStatus(agentId) {
        const agent = this.agents.find(findAgent.bind(null, agentId));
        return agent ? agent.status : 2;
    }

    getAgent(contactResponsibles) {
        const isAnyNotOffline = !!this.agents.length;
        let result = this.agents.find((item) => {
            return item.status === 0 && contactResponsibles.includes(item.id);
        });

        if (!result) {
            result = this.agents.find((item) => item.status === 0);
        }

        if (result) {
            result.status = 1;
        }

        return { result, isAnyNotOffline };
    }
}

module.exports = VoipPhone;