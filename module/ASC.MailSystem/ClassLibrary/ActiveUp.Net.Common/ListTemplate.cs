// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Data;
using System.Collections;
using System.ComponentModel;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Summary description for ListTemplate.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class ListTemplate
    {
        string _name, _content, _regionID, _nulltext;
        int _count;
        bool _hascount;
        object _dataSource;

        public ListTemplate()
        {
            _name = string.Empty;
            _regionID = string.Empty;
            _content = string.Empty;
            _nulltext = string.Empty;
        }

        public ListTemplate(string name, string content)
        {
            _name = name;
            _content = content;
            _regionID = string.Empty;
            _nulltext = string.Empty;
        }

        public ListTemplate(string name, string content, string regionid)
        {
            _name = name;
            _content = content;
            _regionID = regionid;
            _nulltext = string.Empty;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string RegionID
        {
            get
            {
                return _regionID;
            }
            set
            {
                _regionID = value;
            }
        }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
            }
        }

        public string NullText
        {
            get
            {
                return _nulltext;
            }
            set
            {
                _nulltext = value;
            }
        }


        /// <summary>
        /// Get or set the main data source of the body template.
        /// </summary>
        public object DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                /*if (value != null && !(value is IEnumerable) && !(value is IListSource))
                {
                    throw new ArgumentException("Invalid DataSource Type. The DataSource must implement IEnumerable or IListSource interfaces.");
                }*/
                //DebugTrace("DataSource", "Type is " + value.GetType());
                if (value.GetType().ToString() == "System.Data.DataSet")
                {
                    _dataSource = ((System.Data.DataSet)value).Tables[0];
                }
                else if (value.GetType().ToString() == "System.Data.DataRow")
                {
                    _dataSource = ((DataRow)value).Table.Clone();
                    ((DataTable)_dataSource).Rows.Add(((DataRow)value).ItemArray);
                }
                else
                {
                    _dataSource = value;
                }
            }
        }
        
        /// <summary>
        /// Number of records in the datasource.
        /// </summary>
        public int Count
        {
            get
            {
                if (!_hascount)
                {
                    if (DataSource == null)
                    {
                        _count= 0;
                    }
                    else
                    {
                        IEnumerator items = GetEnumerator(DataSource);
                        if (items != null && items.MoveNext())
                        {
                            _count= 1;
                        }
                        else 
                        {
                            _count= 0;
                        }
                    }
                    _hascount=true;
                }
                return _count;
            }
        }
        /// <summary>
        /// Get the enumerator from the specified data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The enumerator.</returns>
        private IEnumerator GetEnumerator(object dataSource)
        {
            //if (dataSource != null)

            // Set the IEnumerator object
            IEnumerator items = null;
            
            // IEnumerable & IListSource specific methods
            if (dataSource is IEnumerable)
            {
                items = ((IEnumerable)dataSource).GetEnumerator();
            }

            if (dataSource is IListSource)
            {
                items = ((IListSource)dataSource).GetList().GetEnumerator();
            } 
    
            if (dataSource is System.Data.DataRow)
            {
                
                DataView dataView = new DataView(((System.Data.DataRow)dataSource).Table);
                ArrayList sourceList = new ArrayList();

                foreach (DataRowView drv in dataView)
                {
                    if (drv.Row == dataSource)
                    {
                        sourceList.Add(drv);
                        break;
                    }
                }
                //sourceList.Add(dataView[0]);
                items = sourceList.GetEnumerator();
            }

            // Return the IEnumerator
            return items;
        }
    }
}
