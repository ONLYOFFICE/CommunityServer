using System;
using System.Collections.Generic;
using System.Linq;
using ASC.VoipService;
using log4net;

namespace ASC.SignalR.Base.Hubs.Voip
{
    class SignalRVoipPhone
    {
        public string Phone { get; private set; }
        private List<Agent> Agents { get; set; }
        private List<string> Calls { get; set; }

        public SignalRVoipPhone(string numberId)
        {
            Phone = numberId;
            Agents = new List<Agent>();
            Calls = new List<string>();
        }

        public void AddOrUpdateAgent(Agent agent)
        {
            var existingAgent = Agents.FirstOrDefault(r => r.Id == agent.Id);
            if (existingAgent != null)
            {
                existingAgent.Status = agent.Status;
            }
            else
            {
                Agents.Add(agent);
            }
        }

        public bool RemoveAgent(Guid agentId)
        {
            if (Agents.All(r => r.Id != agentId)) return false;

            Agents.RemoveAll(r => r.Id == agentId);
            return true;
        }

        public void AddCall(string callId)
        {
            if (Calls.Contains(callId)) return;
            Calls.Add(callId);
        }

        public void RemoveCall(string callId)
        {
            if (string.IsNullOrEmpty(callId) || !Calls.Contains(callId)) return;
            Calls.Remove(callId);
        }

        public bool AnyCalls()
        {
            return Calls.Any();
        }

        public string DequeueCall()
        {
            var result = Calls.FirstOrDefault();
            RemoveCall(result);
            return result;
        }

        public string Enqueue(string callId, string agent)
        {
            var agents = Agents.Where(r => r.Status == AgentStatus.Online).ToList();

            if (!string.IsNullOrEmpty(agent) && RemoveAgent(new Guid(agent)))
            {
                return agent;
            }

            if (agents.Any())
            {
                return agents.First().Id.ToString();
            }

            AddCall(callId);
            return string.Empty;
        }

        public List<string> OnlineAgents()
        {
            return Agents.Where(r => r.Status == AgentStatus.Online).Select(r=> r.Id.ToString()).ToList();
        }

        public AgentStatus GetAgentStatus(string agentId)
        {
            var agent = Agents.FirstOrDefault(r => r.Id.ToString() == agentId);

            return agent == null ? AgentStatus.Offline : agent.Status;
        }

        public Tuple<Agent, bool> GetAgent(List<Guid> contactResponsibles)
        {
            var result = Agents.FirstOrDefault(r => r.Status == AgentStatus.Online && contactResponsibles.Contains(r.Id)) ??
                         Agents.FirstOrDefault(r => r.Status == AgentStatus.Online);
            var isAnyNotOffline = Agents.Any();

            if (result != null)
            {
                result.Status = AgentStatus.Paused;
            }

            return new Tuple<Agent, bool>(result, isAnyNotOffline);
        }
    }

    class ListPhones
    {
        private static readonly object LockObj = new object();

        private readonly List<SignalRVoipPhone> phones;
        private readonly ILog log;

        public ListPhones(ILog log)
        {
            phones = new List<SignalRVoipPhone>();
            this.log = log;
        }

        public void AddPhone(string numberId)
        {
            lock (LockObj)
            {
                if (phones.All(r => r.Phone != numberId))
                {
                    phones.Add(new SignalRVoipPhone(numberId));

                    log.InfoFormat("AddPhone:{0}", numberId);
                }
            }
        }

        public SignalRVoipPhone GetPhone(string numberId)
        {
            lock (LockObj)
            {
                return phones.FirstOrDefault(r => r.Phone == numberId);
            }
        }

        public void AddOrUpdateAgent(string numberId, Agent agent)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);

                if (phone == null) return;

                phone.AddOrUpdateAgent(agent);

                log.InfoFormat("AddOrUpdateAgent number:{0}, agent:{1}", numberId, agent.Id);
            }
        }

        public void RemoveAgent(string numberId, Guid agentId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);

                if (phone == null) return;

                phone.RemoveAgent(agentId);

                log.InfoFormat("RemoveAgent number:{0}, agent:{1}", numberId, agentId);
            }
        }

        public bool AnyCalls(string numberId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);

                return phone != null && phone.AnyCalls();
            }
        }

        public string DequeueCall(string numberId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return "";
                var result = phone.DequeueCall();

                log.InfoFormat("DequeueCall number:{0}, callId:{1}", numberId, result);

                return result;
            }
        }

        public void RemoveCall(string numberId, string callId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return;

                phone.RemoveCall(callId);

                log.InfoFormat("RemoveCall number:{0}, callId:{1}", numberId, callId);
            }
        }

        public string Enqueue(string numberId, string callId, string agent)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return "";

                var result = phone.Enqueue(callId, agent);

                log.InfoFormat("Enqueue number:{0}, agent:{1}", numberId, agent);

                return result;
            }
        }

        public List<string> OnlineAgents(string numberId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return new List<string>();

                return phone.OnlineAgents();
            }
        }

        public AgentStatus GetStatus(string numberId, string agentId)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return AgentStatus.Offline;

                var status = phone.GetAgentStatus(agentId);

                log.InfoFormat("GetAgentStatus number:{0}, agent:{1}, status:{2}", numberId, agentId, status);

                return status;
            }
        }

        public Tuple<Agent, bool> GetAgent(string numberId, List<Guid> contactResponsibles)
        {
            lock (LockObj)
            {
                var phone = GetPhone(numberId);
                if (phone == null) return null;

                return phone.GetAgent(contactResponsibles);
            }
        }
    }
}
