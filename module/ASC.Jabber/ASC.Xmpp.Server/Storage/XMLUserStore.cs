/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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