/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Xmpp.Core.protocol.iq.disco;
using XmppData = ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Services.Muc2.Room.Member;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Settings
{
    /// <summary>
    /// </summary>
    public class MucRoomSettings
    {
        private const string RoomTitle = "Room title";

        private const string RoomPublic = "Make room public searchable";

        private const string RoomPasswordProtected = "Make room password protected";

        private const string RoomPassword = "Password";

        private const string AllowChangeSubject = "Allow users to change subject";

        private const string MakeRoomPersistent = "Make room persistent";

        #region Properties

        internal MucRoom Room { get; set; }
        /// <summary>
        /// </summary>
        public bool Anonymous { get; set; }

        /// <summary>
        /// </summary>
        public bool CanChangeSubject { get; set; }

        /// <summary>
        /// </summary>
        public bool CanInvite { get; set; }

        /// <summary>
        /// </summary>
        public Role CanSeeMemberList { get; set; }

        /// <summary>
        /// </summary>
        public int HistoryCountOnEnter { get; set; }

        /// <summary>
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// </summary>
        public bool Logging { get; set; }

        /// <summary>
        /// </summary>
        public int MaxOccupant { get; set; }

        /// <summary>
        /// </summary>
        public bool MembersOnly { get; set; }

        /// <summary>
        /// </summary>
        public bool Moderated { get; set; }

        /// <summary>
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// </summary>
        public bool PasswordProtected { get; set; }

        /// <summary>
        /// </summary>
        public bool Persistent { get; set; }

        /// <summary>
        /// </summary>
        public Role PresenceBroadcastedFrom { get; set; }

        /// <summary>
        /// </summary>
        public string Subject { get; set; }

        private string title;

        /// <summary>
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                if (Room != null)
                {
                    Room.TitleChange(title);
                }
            }
        }

        /// <summary>
        /// </summary>
        public bool UserNamesOnly { get; set; }

        private bool visible;

        /// <summary>
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                if (Room != null)
                {
                    Room.VisibilityChange(value);
                }
            }
        }

        public bool IsNew { get; set; }

        public List<MucRoomMemberInfo> Members { get; set; }

        internal static List<MucRoomMemberInfo> ParseMemberList(string data)
        {
            List<MucRoomMemberInfo> infos = new List<MucRoomMemberInfo>();
            if (!string.IsNullOrEmpty(data))
            {
                string[] fields = data.Trim(';', ' ').Split(';');
                foreach (string field in fields)
                {
                    try
                    {
                        infos.Add(new MucRoomMemberInfo(field));
                    }
                    catch (Exception) { }

                }
            }
            return infos;
        }

        internal string GetMemberList()
        {
            var builder = new StringBuilder();
            lock (Members)
            {
                if (Members == null || Members.Count == 0) return null;
                foreach (var member in Members)
                {
                    builder.AppendFormat("{0};", member.ToString());
                }
            }
            return builder.ToString();
        }
        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal static MucRoomSettings CreateDefault(MucRoom room)
        {
            return new MucRoomSettings
            {
                Title = room != null ? room.Name : null,
                Logging = true,
                CanChangeSubject = true,
                CanInvite = true,
                HistoryCountOnEnter = 10,
                PresenceBroadcastedFrom = Role.visitor | Role.moderator | Role.participant,
                CanSeeMemberList = Role.moderator | Role.participant,
                UserNamesOnly = false,
                Visible = true,
                Anonymous = false,
                Room = room,
                IsNew = true,
                Members = new List<MucRoomMemberInfo>(),
                Persistent = false
            };
        }

        public DiscoFeature[] GetFeatures()
        {
            List<DiscoFeature> features = new List<DiscoFeature>();

            if (PasswordProtected)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_PASSWORDPROTECTED));
            }
            if (Visible)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_PUBLIC));
            }
            else
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_HIDDEN));
            }

            if (Persistent)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_PERSISTANT));
            }
            else
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_TEMPORARY));
            }

            if (Moderated)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_MODERATED));
            }
            else
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_UNMODERATED));
            }

            if (Anonymous)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_ANONYMOUS));
            }
            else
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_NONANONYMOUS));
            }

            if (MembersOnly)
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_MEMBERSONLY));
            }
            else
            {
                features.Add(new DiscoFeature(Features.FEAT_MUC_OPEN));
            }

            return features.ToArray();
        }

        public XmppData.Data GetDataForm(Jid creator)
        {
            XmppData.Data data = new XmppData.Data(XmppData.XDataFormType.form);
            data.Title = string.Format("Configuration of {0} room", Room.Jid);
            data.Instructions = string.Format("Room {0} was created. Please fill configuration", Room.Name);

            data.AddChild(new XmppData.Field(XmppData.FieldType.Hidden) { Var = "FORM_TYPE", FieldValue = Features.FEAT_MUC_ROOMCONFIG });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Text_Single) { Var = "muc#roomconfig_roomtitle", Label = RoomTitle, FieldValue = Title });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Boolean) { Var = "muc#roomconfig_publicroom", FieldValue = Visible ? "1" : "0", Label = RoomPublic });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Boolean) { Var = "muc#roomconfig_passwordprotectedroom", FieldValue = PasswordProtected ? "1" : "0", Label = RoomPasswordProtected });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Text_Private) { Var = "muc#roomconfig_roomsecret", Label = RoomPassword, FieldValue = Password });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Boolean) { Var = "muc#roomconfig_changesubject", FieldValue = CanChangeSubject ? "1" : "0", Label = AllowChangeSubject });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Boolean) { Var = "muc#roomconfig_enablelogging", FieldValue = Logging ? "1" : "0", Label = "Enable Public Logging?" });
            data.AddChild(new XmppData.Field(XmppData.FieldType.Boolean) { Var = "muc#roomconfig_persistentroom", FieldValue = Persistent ? "1" : "0", Label = MakeRoomPersistent });
            //data.AddChild(new Field(FieldType.Boolean) { Var = "muc#roomconfig_usernamesonly", FieldValue = UserNamesOnly ? "1" : "0", Label = "Allow only real User Names?" });
            return data;
        }

        public XmppData.Data GetResultForm()
        {
            XmppData.Data data = new XmppData.Data(XmppData.XDataFormType.result);

            data.AddChild(new XmppData.Field(XmppData.FieldType.Hidden) { Var = "FORM_TYPE", FieldValue = Features.FEAT_MUC_ROOMINFO });
            data.AddChild(new XmppData.Field() { Var = "muc#roominfo_description", Label = RoomTitle, FieldValue = Title });
            data.AddChild(new XmppData.Field() { Var = "muc#roomconfig_publicroom", FieldValue = Visible ? "1" : "0", Label = RoomPublic });
            data.AddChild(new XmppData.Field() { Var = "muc#roomconfig_passwordprotectedroom", FieldValue = PasswordProtected ? "1" : "0", Label = RoomPasswordProtected });
            data.AddChild(new XmppData.Field() { Var = "muc#roomconfig_persistentroom", FieldValue = Persistent ? "1" : "0", Label = MakeRoomPersistent });
            //data.AddChild(new Field() { Var = "muc#roominfo_subject", FieldValue = Subject, Label = "Real user names only" });
            //data.AddChild(new Field() { Var = "muc#roomconfig_enablelogging", FieldValue = Logging ? "1" : "0", Label = "Logging enabled" });
            return data;
        }

        #endregion

        public void SubmitForm(XmppData.Data submit)
        {
            ElementList fields = submit.SelectElements(typeof(XmppData.Field));
            foreach (var field in fields)
            {
                if (field is XmppData.Field)
                {
                    XmppData.Field fld = (XmppData.Field)field;
                    //Set conf back
                    switch (fld.Var)
                    {
                        case "muc#roomconfig_usernamesonly":
                            UserNamesOnly = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_roomtitle":
                            Title = fld.GetValue();
                            break;
                        case "muc#roomconfig_enablelogging":
                            Logging = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_changesubject":
                            CanChangeSubject = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_allowinvites":
                            CanInvite = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_publicroom":
                            Visible = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_persistentroom":
                            Persistent = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_membersonly":
                            MembersOnly = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_passwordprotectedroom":
                            PasswordProtected = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_moderated":
                            Moderated = fld.GetValueBool();
                            break;
                        case "muc#roomconfig_roomsecret":
                            Password = fld.GetValue();
                            break;
                    }
                }
            }
        }

        internal MucRoomMemberInfo GetMemberInfo(Jid jid)
        {
            lock (Members)
            {
                foreach (MucRoomMemberInfo memberInfo in Members)
                {
                    if (Equals(memberInfo.Jid.Bare, jid.Bare))
                    {
                        return memberInfo;
                    }
                }
            }
            return null;
        }

        public bool IsMember(Jid jid)
        {
            if (MembersOnly)
            {
                return GetMemberInfo(jid) != null;
            }
            return true;
        }

        internal void UpdateMemberInfo(Jid jid, Affiliation affiliation, Role role)
        {
            MucRoomMemberInfo info = GetMemberInfo(jid);
            if (info == null)
            {
                info = new MucRoomMemberInfo();
                lock (Members)
                {
                    Members.Add(info);
                }
            }
            info.Jid = new Jid(jid.Bare);
            info.Affiliation = affiliation;
            info.Role = role;
            Room.SettingsSave();
        }


        public Affiliation GetMemeberAffilation(Jid memberJid)
        {
            MucRoomMemberInfo info = GetMemberInfo(memberJid);
            if (info != null)
            {
                return info.Affiliation;
            }
            return MembersOnly ? Affiliation.none : Affiliation.member;
        }

        public Role GetMemeberRole(Affiliation affiliation)
        {
            Role proposedRole = Role.visitor;
            if (Moderated)
            {
                if (affiliation == Affiliation.owner || affiliation == Affiliation.admin)
                {
                    proposedRole = Role.moderator;
                }
                else if (affiliation == Affiliation.member)
                {
                    proposedRole = Role.participant;
                }
                else if (affiliation != Affiliation.outcast)
                {
                    proposedRole = Role.visitor;
                }
                else
                {
                    proposedRole = Role.none;
                }
            }
            else
            {
                if (affiliation == Affiliation.owner || affiliation == Affiliation.admin)
                {
                    proposedRole = Role.moderator;
                }
                else
                {
                    proposedRole = Role.participant;
                }

            }
            return proposedRole;
        }

        public bool CanChangeAffilation(Affiliation affiliationCurrent, Affiliation affiliationSetting, Affiliation affiliationFrom)
        {
            if (affiliationFrom == Affiliation.owner)
            {
                //can change
                if (affiliationCurrent != Affiliation.owner)
                {
                    //Can change definitly
                    return true;
                }
            }
            else if (affiliationFrom == Affiliation.admin)
            {
                //can change
                if (affiliationCurrent != Affiliation.owner || affiliationCurrent != Affiliation.admin)
                {
                    //Can change definitly
                    return true;
                }
            }
            return false;
        }

        public bool CanChangeRole(Role roleCurrent, Role roleSetting, Role roleFrom, Affiliation affiliationFrom)
        {
            if (roleFrom == Role.moderator || affiliationFrom == Affiliation.admin || affiliationFrom == Affiliation.owner)
            {
                //can change
                if (roleCurrent != Role.moderator)
                {
                    //Can change definitly
                    return true;
                }
            }
            return false;
        }

        public int[] GetEnterStatusCodes()
        {
            List<int> codes = new List<int>();
            if (IsNew)
            {
                codes.Add(201);// New room
                IsNew = false;
            }
            if (Logging)
            {
                codes.Add(170);// Logging
            }
            return codes.ToArray();
        }
    }
}