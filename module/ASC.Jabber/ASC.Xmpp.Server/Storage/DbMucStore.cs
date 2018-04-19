/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Server.Services.Muc2.Room.Settings;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ASC.Xmpp.Server.Storage
{
    public class DbMucStore : DbStoreBase, IMucStore
    {
        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_room", true)
                .AddColumn(new SqlCreate.Column("jid", DbType.String, 255).NotNull(true).PrimaryKey(true))
                .AddColumn("title", DbType.String, 255)
                .AddColumn("description", DbType.String, UInt16.MaxValue)
                .AddColumn("subject", DbType.String, 255)
                .AddColumn("instructions", DbType.String, 255)
                .AddColumn("pwd", DbType.String, 255)
                .AddColumn("pwdprotect", DbType.Int32)
                .AddColumn("visible", DbType.Int32)
                .AddColumn("members", DbType.String, UInt16.MaxValue)
                .AddColumn("maxoccupant", DbType.Int32)
                .AddColumn("historycountonenter", DbType.Int32)
                .AddColumn("anonymous", DbType.Int32)
                .AddColumn("logging", DbType.Int32)
                .AddColumn("membersonly", DbType.Int32)
                .AddColumn("usernamesonly", DbType.Int32)
                .AddColumn("moderated", DbType.Int32)
                .AddColumn("persistent", DbType.Int32)
                .AddColumn("presencebroadcastedfrom", DbType.Int32)
                .AddColumn("canchangesubject", DbType.Int32)
                .AddColumn("caninvite", DbType.Int32)
                .AddColumn("canseememberlist", DbType.Int32);

            var t2 = new SqlCreate.Table("jabber_room_history", true)
                .AddColumn(new SqlCreate.Column("id", DbType.Int32).NotNull(true).Autoincrement(true).PrimaryKey(true))
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("stamp", DbType.DateTime, true)
                .AddColumn("message", DbType.String, MESSAGE_COLUMN_LEN, true)
                .AddIndex("jabber_room_history_jid", "jid");

            return new[] { t1, t2 };
        }


        #region IMucStore Members

        public List<MucRoomInfo> GetMucs(string server)
        {
            return ExecuteList(new SqlQuery("jabber_room").Select("jid").Where(Exp.Like("jid", "@" + server, SqlLike.EndWith)))
                .ConvertAll(r => new MucRoomInfo(new Jid((string)r[0])));
        }

        public MucRoomInfo GetMuc(Jid mucName)
        {
            if (Jid.IsNullOrEmpty(mucName)) throw new ArgumentNullException("mucName");

            return ExecuteList(new SqlQuery("jabber_room").Select("jid").Where("jid", mucName.Bare))
                .ConvertAll(r => new MucRoomInfo(new Jid((string)r[0])))
                .SingleOrDefault();
        }

        public void SaveMuc(MucRoomInfo muc)
        {
            if (muc == null) throw new ArgumentNullException("muc");

            SetMucRoomSettings(muc.Jid, MucRoomSettings.CreateDefault(null));
        }

        public void RemoveMuc(Jid mucName)
        {
            if (Jid.IsNullOrEmpty(mucName)) throw new ArgumentNullException("mucName");

            ExecuteNonQuery(new SqlDelete("jabber_room").Where("jid", mucName.Bare));
            RemoveMucMessages(mucName);
        }

        public List<Message> GetMucMessages(Jid mucName, int count, int startindex = 0)
        {
            if (Jid.IsNullOrEmpty(mucName)) throw new ArgumentNullException("mucName");

            var q = new SqlQuery("jabber_room_history").Select("message").Where("jid", mucName.Bare).OrderBy("id", false);
            if (startindex < int.MaxValue){q.SetFirstResult(startindex);}
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            var messages = ExecuteList(q).ConvertAll(m => ElementSerializer.DeSerializeElement<Message>((string)m[0]));
            messages.Reverse();
            return messages;
        }

        public List<Message> GetMucMessages(Jid mucName, DateTime after)
        {
            if (Jid.IsNullOrEmpty(mucName)) throw new ArgumentNullException("mucName");

            var q = new SqlQuery("jabber_room_history").Select("message").Where("jid", mucName.Bare).Where(Exp.Ge("stamp", after)).OrderBy("id", false);
            var messages = ExecuteList(q).ConvertAll(m => ElementSerializer.DeSerializeElement<Message>((string)m[0]));
            messages.Reverse();
            return messages;
        }

        public void AddMucMessages(Jid mucName, params Message[] messages)
        {
            if (mucName == null || messages == null || messages.Length == 0) return;

            var batch = new List<ISqlInstruction>(messages.Length);
            foreach (var m in messages)
            {
                if (m != null)
                {
                    if (m.XDelay == null) m.XDelay = new Delay();
                    m.XDelay.Stamp = DateTime.UtcNow;

                    batch.Add(
                        new SqlInsert("jabber_room_history")
                        .InColumnValue("jid", mucName.Bare)
                        .InColumnValue("message", ElementSerializer.SerializeElement(m))
                        .InColumnValue("stamp", m.XDelay.Stamp));
                }
            }
            ExecuteBatch(batch);
        }

        public void RemoveMucMessages(Jid mucName)
        {
            if (Jid.IsNullOrEmpty(mucName)) throw new ArgumentNullException("mucName");

            ExecuteNonQuery(new SqlDelete("jabber_room_history").Where("jid", mucName.Bare));
        }

        public MucRoomSettings GetMucRoomSettings(Jid roomName)
        {
            if (Jid.IsNullOrEmpty(roomName)) throw new ArgumentNullException("roomName");

            return ExecuteList(new SqlQuery("jabber_room").Where("jid", roomName.Bare)
                    .Select("anonymous")
                    .Select("canchangesubject")
                    .Select("caninvite")
                    .Select("canseememberlist")
                    .Select("historycountonenter")
                    .Select("instructions")
                    .Select("logging")
                    .Select("maxoccupant")
                    .Select("membersonly")
                    .Select("moderated")
                    .Select("pwd")
                    .Select("pwdprotect")
                    .Select("persistent")
                    .Select("presencebroadcastedfrom")
                    .Select("subject")
                    .Select("title")
                    .Select("usernamesonly")
                    .Select("visible")
                    .Select("members"))
                .ConvertAll(o => new MucRoomSettings
                    {
                        Anonymous = Convert.ToBoolean(o[0]),
                        CanChangeSubject = Convert.ToBoolean(o[1]),
                        CanInvite = Convert.ToBoolean(o[2]),
                        CanSeeMemberList = (Role)Convert.ToInt32(o[3]),
                        HistoryCountOnEnter = Convert.ToInt32(o[4]),
                        Instructions = o[5] as string,
                        Logging = Convert.ToBoolean(o[6]),
                        MaxOccupant = Convert.ToInt32(o[7]),
                        MembersOnly = Convert.ToBoolean(o[8]),
                        Moderated = Convert.ToBoolean(o[9]),
                        Password = o[10] as string,
                        PasswordProtected = Convert.ToBoolean(o[11]),
                        Persistent = Convert.ToBoolean(o[12]),
                        PresenceBroadcastedFrom = (Role)Convert.ToInt32(o[13]),
                        Subject = o[14] as string,
                        Title = o[15] as string,
                        UserNamesOnly = Convert.ToBoolean(o[16]),
                        Visible = Convert.ToBoolean(o[17]),
                        Members = MucRoomSettings.ParseMemberList(o[18] as string)
                    })
                .SingleOrDefault();
        }

        public void SetMucRoomSettings(Jid roomName, MucRoomSettings settings)
        {
            if (Jid.IsNullOrEmpty(roomName)) throw new ArgumentNullException("roomName");
            if (settings == null) throw new ArgumentNullException("settings");

            ExecuteNonQuery(
                new SqlInsert("jabber_room", true)
                .InColumnValue("jid", roomName.Bare)
                .InColumnValue("title", settings.Title ?? roomName.User)
                .InColumnValue("subject", settings.Subject)
                .InColumnValue("instructions", settings.Instructions)
                .InColumnValue("pwd", settings.Password)
                .InColumnValue("pwdprotect", settings.PasswordProtected)
                .InColumnValue("visible", settings.Visible)
                .InColumnValue("members", settings.GetMemberList())
                .InColumnValue("maxoccupant", settings.MaxOccupant)
                .InColumnValue("historycountonenter", settings.HistoryCountOnEnter)
                .InColumnValue("anonymous", settings.Anonymous)
                .InColumnValue("logging", settings.Logging)
                .InColumnValue("membersonly", settings.MembersOnly)
                .InColumnValue("usernamesonly", settings.UserNamesOnly)
                .InColumnValue("moderated", settings.Moderated)
                .InColumnValue("persistent", settings.Persistent)
                .InColumnValue("presencebroadcastedfrom", settings.PresenceBroadcastedFrom)
                .InColumnValue("canchangesubject", settings.CanChangeSubject)
                .InColumnValue("caninvite", settings.CanInvite)
                .InColumnValue("canseememberlist", settings.CanSeeMemberList));
        }

        #endregion
    }
}