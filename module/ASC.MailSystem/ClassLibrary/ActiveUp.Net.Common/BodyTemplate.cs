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
using System.IO;
using System.Collections;
using System.Xml;
using System.Data;
using System.Collections.Specialized;
using System.ComponentModel;
#if !PocketPC
using System.Web.UI;
#endif

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// A message body template.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class BodyTemplate
    {
        private string _content;
        private BodyFormat _format;
        private object _dataSource;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public BodyTemplate()
        {
            _content = string.Empty;
            _format = BodyFormat.Text;
        }

        /// <summary>
        /// Creates the body template based on the specified content. The default format is Text.
        /// </summary>
        /// <param name="content">The content.</param>
        public BodyTemplate(string content)
        {
            _content = content;
            _format = BodyFormat.Text;
        }

        /// <summary>
        /// Creates the body template based on the specified content and format.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="format">The format to use.</param>
        public BodyTemplate(string content, BodyFormat format)
        {
            _content = content;
            _format = format;
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
        /// Gets or sets the message body format.
        /// </summary>
        public BodyFormat Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
            }
        }

        /// <summary>
        /// Gets or sets the message body content.
        /// </summary>
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
    }
}
