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

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Represents the formatting options of a message template field.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class FieldFormat
    {
        private string _name, _format;
        private PaddingDirection _paddingDir;
        private char _paddingChar;
        private int _totalWidth;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public FieldFormat()
        {
            this.Name = string.Empty;
            this.Format = string.Empty;
            this.PaddingDir = PaddingDirection.Left;
            this.TotalWidth = 0;
            this.PaddingChar = ' ';
        }

        /// <summary>
        /// Creates the field format based on it's name and format string.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="format">The format string of the field format.</param>
        public FieldFormat(string name, string format)
        {
            this.Name = name;
            this.Format = format;
            this.PaddingDir = PaddingDirection.Left;
            this.TotalWidth = 0;
            this.PaddingChar = ' ';
        }

        /// <summary>
        /// Creates the field format based on the field name, format string, padding direction, total width and padding char.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="format">The format string.</param>
        /// <param name="paddingDir">The padding direction.</param>
        /// <param name="totalWidth">The total width.</param>
        /// <param name="paddingChar">The padding char.</param>
        public FieldFormat(string name, string format, PaddingDirection paddingDir, int totalWidth, char paddingChar)
        {
            this.Name = name;
            this.Format = format;
            this.PaddingDir = paddingDir;
            this.TotalWidth = totalWidth;
            this.PaddingChar = paddingChar;
        }

        /// <summary>
        /// The padding char.
        /// </summary>
        public char PaddingChar
        {
            get
            {
                return _paddingChar;
            }
            set
            {
                _paddingChar = value;
            }
        }

        /// <summary>
        /// The total width.
        /// </summary>
        public int TotalWidth
        {
            get
            {
                return _totalWidth;
            }
            set
            {
                _totalWidth = value;
            }
        }

        /// <summary>
        /// The padding direction.
        /// </summary>
        public PaddingDirection PaddingDir
        {
            get
            {
                return _paddingDir;
            }
            set
            {
                _paddingDir = value;
            }
        }

        /// <summary>
        /// The field name.
        /// </summary>
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

        /// <summary>
        /// The format string.
        /// </summary>
        public string Format
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
    }
}
