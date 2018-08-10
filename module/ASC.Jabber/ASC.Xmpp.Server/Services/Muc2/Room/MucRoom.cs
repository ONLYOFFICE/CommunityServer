/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.protocol.iq.vcard;
using XmppData = ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.protocol.x.muc.iq.admin;
using ASC.Xmpp.Core.protocol.x.muc.iq.owner;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services.Muc2.Helpers;
using ASC.Xmpp.Server.Services.Muc2.Room.Member;
using ASC.Xmpp.Server.Services.Muc2.Room.Settings;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using System;
using Error = ASC.Xmpp.Core.protocol.client.Error;
using Item = ASC.Xmpp.Core.protocol.x.muc.Item;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    internal delegate void MemberActionDelegate(MucRoomMember member);
    internal delegate void MemberAddressChangeDelegate(MucRoomMember member, Jid newAddress);
    internal delegate void MemberActionBroadcastDelegate(MucRoomMember member, Presence presence);

    internal class MucRoom : XmppServiceBase
    {
        private readonly MucService mucService;
        private readonly IServiceProvider context;
        public MucRoomMemberCollection members;

        private bool visible = true;

        public IXmppSender Sender
        {
            get
            {
                return ((IXmppSender)context.GetService(typeof(IXmppSender)));
            }
        }

        public XmppSessionManager SessionManager
        {
            get
            {
                return ((XmppSessionManager)context.GetService(typeof(XmppSessionManager)));
            }
        }

        public override DiscoItem DiscoItem
        {
            get { return visible ? new DiscoItem() { Name = RoomSettings.Title, Jid = Jid } : null; }
        }

        public MucRoom(Jid jid, string name, MucService mucService, IServiceProvider context)
        {
            if (jid == null)
            {
                throw new ArgumentNullException("jid");
            }
            if (mucService == null)
            {
                throw new ArgumentNullException("mucService");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.mucService = mucService;
            this.context = context;
            Jid = jid;
            Name = name;
            members = new MucRoomMemberCollection(mucService.ServiceManager);
            members.MemberAdded += BroadcastPresencesToMember;
            members.MemberRemoved += members_MemberRemoved;

            //Create handler
            lock (Handlers)
            {
                Handlers.Add(new MucRoomStanzaHandler(this));
                Handlers.Add(new ServiceDiscoHandler(Jid));
            }
        }

        void members_MemberRemoved(MucRoomMember member)
        {
            member.PresenceType = PresenceType.unavailable;
            if (members.Count == 0 && !RoomSettings.Persistent)
            {
                //Say goodbuy to our room
                DestroyRoom(new Destroy("temporary room is empty"));
            }
        }

        internal void MemberReenter(MucRoomMember member)
        {
            MemberEnter(member);
            BroadcastPresencesToMember(member);
        }

        void BroadcastPresencesToMember(MucRoomMember member)
        {
            //Send all ocupant presence to new
            foreach (MucRoomMember existingMember in members)
            {
                if (!ReferenceEquals(member, existingMember))
                {
                    member.Send(existingMember.Presence);
                }
            }

        }

        protected override void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {
            base.OnRegisterCore(handlerManager, serviceManager, serviceProvider);
            LoadRoomSettings();
        }

        private void LoadRoomSettings()
        {
            RoomSettings = mucService.MucStorage.GetMucRoomSettings(Jid);
            if (RoomSettings == null)
            {
                RoomSettings = MucRoomSettings.CreateDefault(this);
                mucService.MucStorage.SetMucRoomSettings(Jid, RoomSettings);
            }
            else
            {
                RoomSettings.Room = this;
            }
            UpdateDiscoInfo();
            visible = RoomSettings.Visible;
        }

        internal MucRoomSettings RoomSettings { get; set; }

        private void UpdateDiscoInfo()
        {
            DiscoInfo.RemoveAllChildNodes();
            DiscoInfo.AddIdentity(new DiscoIdentity("text", RoomSettings.Title, "conference"));
            DiscoInfo.AddFeature(new DiscoFeature(Uri.MUC));
            DiscoInfo.AddFeature(new DiscoFeature(Features.FEAT_MUC_ROOMINFO));
            DiscoInfo.AddFeature(new DiscoFeature(Features.FEAT_MUC_ROOMCONFIG));
            foreach (var feature in RoomSettings.GetFeatures())
            {
                DiscoInfo.AddFeature(feature);
            }
        }

        public MucRoomMember GetRealMember(Jid from)
        {
            return members.FindByRealJid(from);
        }

        public void TryEnterRoom(MucRoomMember member, Presence presence)
        {
            if (MucHelpers.IsJoinRequest(presence))
            {
                if (CanEnterRoom(member, presence))
                {
                    if (RoomSettings.IsNew)
                    {
                        member.Affiliation = Affiliation.owner;
                        member.Role = Role.moderator;
                    }
                    //enter
                    members.Add(member);
                    RoomSettings.UpdateMemberInfo(member.RealJid, member.Affiliation, member.Role);//Update settings on enter
                    //subscribe to events
                    SetMemberEvents(member);
                    MemberEnter(member);
                }
            }
            else
            {
                ErrorPresence(presence, ErrorCondition.BadRequest);
                member.Send(presence);
            }
        }

        private void MemberEnter(MucRoomMember member)
        {
            member.EnterRoom(RoomSettings.GetEnterStatusCodes());

            //Send history
            if (RoomSettings.Logging)
            {
                foreach (var msg in mucService.MucStorage.GetMucMessages(Jid, 20, 0))
                {
                    member.Send(msg);
                }
            }
            //Send subject
            member.Send(new Message() { From = Jid, Subject = RoomSettings.Subject });
        }

        private void SetMemberEvents(MucRoomMember member)
        {
            member.PresenceBroadcasted += MemberPresenceBroadcasted;
            member.PresenceChanged += MemberPresenceChanged;
            member.AddressChanged += MemberAddressChanged;
            member.Unavailible += MemberUnavailible;
            member.RoleChanged += MemberRoleChanged;
            member.AffilationChanged += MemberAffilationChanged;
        }

        void MemberAffilationChanged(MucRoomMember member)
        {
            if (member.Affiliation == Affiliation.none || member.Affiliation == Affiliation.outcast)
            {
                //remove
                members.Remove(member);
            }
            else
            {
                BroadcastPresence(member, true);
            }
        }

        void MemberRoleChanged(MucRoomMember member)
        {
            //Broadcast presence
            if (member.Role == Role.none)
            {
                //remove
                members.Remove(member);
            }
            else
            {
                BroadcastPresence(member, true);
            }
        }

        void MemberUnavailible(MucRoomMember member)
        {
            members.Remove(member);
        }

        void MemberAddressChanged(MucRoomMember member, Jid newAddress)
        {
            members.RebindAddress(member, newAddress);
        }

        private void MemberPresenceBroadcasted(MucRoomMember member)
        {
            MemberPresenceBroadcasted(member, member.Presence);
        }

        void MemberPresenceBroadcasted(MucRoomMember member, Presence presence)
        {
            Broadcast(member, true, presence);
        }

        void MemberPresenceChanged(MucRoomMember member)
        {
            //Send new ocupant presence to all
            BroadcastPresence(member);
        }

        private void BroadcastPresence(MucRoomMember member)
        {
            Broadcast(member, true, member.Presence);
        }

        private void BroadcastPresence(MucRoomMember member, bool includeSender)
        {
            Broadcast(member, includeSender, member.Presence);
        }

        private void Broadcast(MucRoomMember member, bool includeSender, Stanza stanza)
        {
            foreach (MucRoomMember existingMember in members)
            {
                if (!ReferenceEquals(member, existingMember) || includeSender)
                {
                    existingMember.Send(stanza);
                }
            }
            //send to self if was removed already
            if (!members.Contains(member) && includeSender)
            {
                member.Send(stanza);
            }
        }

        private bool CanEnterRoom(MucRoomMember member, Presence presence)
        {
            if (RoomSettings.PasswordProtected)
            {
                string password = MucHelpers.GetPassword(presence);
                if (!RoomSettings.Password.Equals(password, StringComparison.Ordinal))
                {
                    // Return error
                    ErrorPresence(presence, ErrorCondition.NotAuthorized, 401);
                    member.Send(presence);
                    return false;
                }
            }

            if (RoomSettings.UserNamesOnly)
            {
                if (!presence.From.User.Equals(presence.To.Resource))
                {
                    // username tries to enter with not his username
                    ErrorPresence(presence,
                                  ErrorCondition.Conflict, 406);
                    member.Send(presence);
                    return false;
                }
            }

            // Check member
            if (RoomSettings.IsMember(presence.From))
            {
                // Add new
                member.Affiliation = RoomSettings.GetMemeberAffilation(presence.From);
                member.Role = RoomSettings.GetMemeberRole(member.Affiliation);

                if (member.Affiliation == Affiliation.outcast)
                {
                    ErrorPresence(presence, ErrorCondition.Conflict, 403);
                    member.Send(presence);
                    return false;
                }
                if (member.Role == Role.none)
                {
                    ErrorPresence(presence,
                              ErrorCondition.Conflict, 403);
                    member.Send(presence);
                    return false;
                }
            }
            else
            {
                // Return error
                ErrorPresence(presence, ErrorCondition.RegistrationRequired, 407);
                member.Send(presence);
                return false;
            }
            return true;
        }


        private void ErrorPresence(Presence presence, ErrorCondition condition)
        {
            ErrorPresence(presence, condition, -1);
        }

        private void ErrorPresence(Presence presence, ErrorCondition condition, int code)
        {
            presence.Type = PresenceType.error;
            presence.RemoveAllChildNodes();
            presence.Error = new Error(condition);
            if (code != -1)
            {
                presence.Error.Code = (ErrorCode)code;
            }
            presence.SwitchDirection();
            presence.From = Jid;
        }

        public bool TryNickChange(Presence presence)
        {
            MucRoomMember member = members.FindByRealJid(presence.From);
            if (!presence.To.Resource.Equals(member.Nick))
            {
                if (!RoomSettings.UserNamesOnly)
                {
                    //Nick change
                    member.Nick = presence.To.Resource;
                    return true;
                }
            }
            return false;
        }

        public void BroadcastMessage(Message msg, MucRoomMember member)
        {
            msg.From = member.Jid;
            //store
            if (RoomSettings.Logging)
            {
                mucService.MucStorage.AddMucMessages(Jid, msg);
            }
            Broadcast(member, true, msg);
        }

        public void DeclinedUser(Message msg, User user, XmppStream stream)
        {
            Message declineMsg = new Message(user.Decline.To, Jid, MessageType.normal, null);
            User userElement = new User();
            userElement.Decline = new Decline();
            userElement.Decline.From = msg.From;
            userElement.Decline.Reason = user.Decline.Reason;
            declineMsg.RemoveAllChildNodes();
            declineMsg.AddChild(userElement);
            Send(declineMsg);
        }

        private bool Send(Stanza stanza)
        {
            if (stanza.To == null)
            {
                XmppStanzaError.ToForbidden(stanza);
            }
            XmppSession session = SessionManager.GetSession(stanza.To);
            if (session != null)
            {
                Sender.SendTo(session.Stream, stanza);
            }
            return session != null;
        }

        public void InviteUser(Message msg, User user, XmppStream stream)
        {
            if (RoomSettings.CanInvite)
            {
                Message inviteMsg = new Message(user.Invite.To, Jid, MessageType.normal, null);
                User userElement = new User();
                userElement.Invite = new Invite();

                MucRoomMember member = GetRealMember(msg.From);
                userElement.Invite.From = member == null ? msg.From : member.Jid;
                userElement.Invite.Reason = user.Invite.Reason;
                inviteMsg.RemoveAllChildNodes();
                inviteMsg.AddChild(userElement);

                if (!Send(inviteMsg))
                {
                    // Return error
                    msg.SwitchDirection();
                    msg.Type = MessageType.error;
                    msg.Error = new Error(ErrorType.cancel, ErrorCondition.ItemNotFound);
                    Sender.SendTo(stream, msg);
                }
            }
            else
            {
                msg.SwitchDirection();
                msg.Type = MessageType.error;
                msg.Error = new Error(ErrorType.cancel, ErrorCondition.NotAllowed);
                Sender.SendTo(stream, msg);
            }
        }

        public void ChangeSubject(MucRoomMember member, string subject)
        {
            if (RoomSettings.CanChangeSubject)
            {
                RoomSettings.Subject = subject;
                Message msg = new Message();
                msg.From = member.Jid;
                msg.Type = MessageType.groupchat;
                msg.Subject = subject;
                Broadcast(member, true, msg);
            }
            else
            {
                Message msg = new Message();
                msg.From = Jid;
                msg.Type = MessageType.error;
                msg.Error = new Error(ErrorCondition.NotAllowed);
                member.Send(msg);
            }
        }

        public void AdminCommand(IQ iq, MucRoomMember member)
        {
            Admin admin = iq.Query as Admin;
            Admin returnAdmin = new Admin();
            if (admin != null)
            {
                if (iq.Type == IqType.get)
                {
                    foreach (Core.protocol.x.muc.iq.admin.Item item in admin.GetItems())
                    {
                        if (item.Actor == null && item.Affiliation != Affiliation.none)
                        {
                            lock (RoomSettings.Members)
                            {
                                foreach (MucRoomMemberInfo mucRoomMember in RoomSettings.Members)
                                {
                                    if (mucRoomMember.Affiliation == item.Affiliation)
                                    {
                                        returnAdmin.AddItem(
                                            new Core.protocol.x.muc.iq.admin.Item(mucRoomMember.Affiliation,
                                                                             mucRoomMember.Role,
                                                                             mucRoomMember.Jid));
                                    }

                                }
                            }
                        }
                    }
                    iq.Query = returnAdmin;
                    iq.Type = IqType.result;
                }
                else if (iq.Type == IqType.set)
                {
                    // Change affilation
                    foreach (Core.protocol.x.muc.iq.admin.Item item in admin.GetItems())
                    {
                        if (item.Actor == null)
                        {
                            Actor actor = new Actor() { Jid = iq.From };
                            MucRoomMemberInfo memberToModify = FindMemberInfo(item);
                            if (memberToModify != null)
                            {
                                //Get member
                                if ((int)item.Affiliation != -1)
                                {
                                    if (RoomSettings.CanChangeAffilation(memberToModify.Affiliation,
                                                                         item.Affiliation,
                                                                         member.Affiliation))
                                    {
                                        memberToModify.Affiliation = item.Affiliation;
                                        //Try notify online 
                                        MucRoomMember onlineMember = members.FindByBareJid(memberToModify.Jid);
                                        if (onlineMember != null)
                                        {
                                            onlineMember.Affiliation = item.Affiliation;
                                        }
                                    }
                                    else
                                    {
                                        // Error!
                                        XmppStanzaError.ToErrorStanza(iq, new Error(ErrorCondition.NotAllowed));
                                        return;
                                    }
                                }

                                if ((int)item.Role != -1)
                                {
                                    if (RoomSettings.CanChangeRole(memberToModify.Role,
                                                                   item.Role,
                                                                   member.Role,
                                                                   member.Affiliation))
                                    {
                                        memberToModify.Role = item.Role;
                                        MucRoomMember onlineMember = members.FindByBareJid(memberToModify.Jid);
                                        if (onlineMember != null)
                                        {
                                            if (item.Role != Role.none)
                                            {
                                                onlineMember.Role = item.Role;
                                            }
                                            else
                                            {
                                                //Role == none it's a kick
                                                onlineMember.Kick(actor, item.Reason);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Error!
                                        XmppStanzaError.ToErrorStanza(iq, new Error(ErrorCondition.NotAllowed));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    SettingsSave();
                    iq.RemoveAllChildNodes();
                    iq.Type = IqType.result;
                }
            }


        }

        private MucRoomMemberInfo FindMemberInfo(Core.protocol.x.muc.iq.admin.Item item)
        {
            MucRoomMemberInfo memberToModify = null;
            if (item.Jid != null)
            {
                memberToModify = GetMemberinfo(item.Jid);
            }
            else if (item.Nickname != null)
            {
                MucRoomMember roomMember = members.FindByNick(item.Nickname);
                if (roomMember != null && roomMember.RealJid != null)
                {
                    memberToModify = GetMemberinfo(roomMember.RealJid);
                }
            }
            return memberToModify;
        }

        private MucRoomMemberInfo GetMemberinfo(Jid jid)
        {
            MucRoomMemberInfo memberToModify = RoomSettings.GetMemberInfo(jid);
            if (memberToModify == null)
            {
                //Add
                memberToModify = new MucRoomMemberInfo();
                memberToModify.Jid = new Jid(jid.Bare);
                memberToModify.Affiliation = Affiliation.member;
                memberToModify.Role = Role.participant;
                lock (RoomSettings.Members)
                {
                    RoomSettings.Members.Add(memberToModify);
                }
            }
            return memberToModify;
        }

        public void OwnerCommand(IQ iq, MucRoomMember member)
        {
            Owner owner = iq.Query as Owner;

            // look for destroy
            if (owner != null)
            {
                Destroy destroy = owner.SelectSingleElement(typeof(Destroy)) as Destroy;
                if (destroy != null)
                {
                    DestroyRoom(destroy);
                    return;
                }

                if (!owner.HasChildElements)
                {
                    // Return config
                    owner.AddChild(RoomSettings.GetDataForm(member.Jid));
                }

                XmppData.Data dataSubmit = (XmppData.Data)owner.SelectSingleElement(typeof(XmppData.Data));

                // form config
                if (dataSubmit != null && dataSubmit.Type == XmppData.XDataFormType.submit)
                {
                    RoomSettings.SubmitForm(dataSubmit);
                    SettingsSave();
                    iq.Query.RemoveAllChildNodes();
                }
            }

            iq.Type = IqType.result;
            iq.SwitchDirection();
        }

        private void DestroyRoom(Destroy destroy)
        {
            Presence offline = new Presence();
            offline.Type = PresenceType.unavailable;
            User user = new User();
            user.Item = new Item(Affiliation.none, Role.none);
            offline.AddChild(user);
            user.AddChild(destroy);
            destroy.Namespace = null;

            foreach (MucRoomMember member in members)
            {
                offline.From = member.Jid;
                member.Send(offline);
            }
            members.Clear();
            mucService.RemoveRoom(this);
        }

        public void TitleChange(string title)
        {
            UpdateDiscoInfo();
        }

        public void VisibilityChange(bool visible)
        {
            this.visible = visible;
        }

        public void SettingsSave()
        {
            mucService.MucStorage.SetMucRoomSettings(Jid, RoomSettings);
        }

        public Vcard GetMemberVcard(MucRoomMember member)
        {
            return mucService.VcardStorage.GetVCard(member.RealJid);
        }
    }
}