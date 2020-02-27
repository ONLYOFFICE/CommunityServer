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


namespace ASC.Xmpp.Server.storage
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Interface;

    public class XMLUserStore:IUserStore
    {
        private readonly string path;
        private Dictionary<string, DataSet> userDatas = new Dictionary<string, DataSet>();

        public XMLUserStore(string path)
        {
            this.path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private DataSet GetUserSet(string username)
        {
            if (!userDatas.ContainsKey(username))
            {
                DataSet userSet = new DataSet("userdata");
                //try load
                if (File.Exists(Path.Combine(path,username+".xml")))
                {
                    //Load
                    try
                    {
                        userSet.ReadXml(Path.Combine(path, username + ".xml"), XmlReadMode.ReadSchema);
                    }
                    catch
                    {
                    }
                }
                userDatas.Add(username, userSet);
            }
            return userDatas[username];
        }

        private void SaveUserSet(string username)
        {
            DataSet userSet = GetUserSet(username);
            try
            {
                userSet.WriteXml(Path.Combine(path, username + ".xml"), XmlWriteMode.WriteSchema);
            }
            catch {}
        }

        private DataTable GetUserTable(string userName, UserStorageSections section)
        {
            DataSet userSet = GetUserSet(userName);
            if (!userSet.Tables.Contains(section.ToString()))
            {
                userSet.Tables.Add(section.ToString());
            }
            return userSet.Tables[section.ToString()];
        }

        public void SetUserItem(string userName, UserStorageSections section, object data)
        {
            try
            {
                DataTable userTable = GetUserTable(userName, section);
                if (!userTable.Columns.Contains("userdata"))
                {
                    userTable.Columns.Add("userdata");
                    userTable.Columns.Add("datatype");
                }
                DataRow row;
                if (userTable.Rows.Count == 0)
                {
                    row = userTable.NewRow();
                    userTable.Rows.Add(row);
                }
                else
                {
                    row = userTable.Rows[0];
                }
                Serialize(row, data);
                SaveUserSet(userName);
            }
            catch (Exception)
            {
            }
        }

        private void Serialize(DataRow row, object data) 
        {
            row["datatype"] = data.GetType();
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, data);
                row["userdata"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(writer.ToString()));
            }
            
        }

        private object Deserialize(DataRow row)
        {
            Type type = Type.GetType(row["datatype"].ToString());
            XmlSerializer serializer = new XmlSerializer(type);
            using (StringReader reader = new StringReader(Encoding.UTF8.GetString(Convert.FromBase64String(row["userdata"].ToString()))))
            {
                return serializer.Deserialize(reader);
            }
        }


        public object GetUserItem(string userName, UserStorageSections section)
        {
            try
            {
                DataTable userTable = GetUserTable(userName, section);
                if (!userTable.Columns.Contains("userdata"))
                {
                    userTable.Columns.Add("userdata");
                    userTable.Columns.Add("datatype");
                }
                DataRow row;
                if (userTable.Rows.Count == 0)
                {
                    return null;
                }
                row = userTable.Rows[0];
                return Deserialize(row);
            }
            catch
            {
                return null;
            }
        }

    }
}