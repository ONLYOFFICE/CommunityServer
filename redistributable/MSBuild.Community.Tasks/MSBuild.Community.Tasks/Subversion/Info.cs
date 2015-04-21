#region Copyright © 2009 MSBuild Community Task Project. All rights reserved.
/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;



namespace MSBuild.Community.Tasks.Subversion
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot("info", Namespace = "", IsNullable = false)]
    public class Info
    {
        /// <remarks/>
        public Info()
        {
            Entries = new EntryCollection();
        }

        /// <remarks/>
        [XmlElement("entry")]
        public EntryCollection Entries { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("Path: {Path}")]
    public class Entry
    {
        /// <remarks/>
        public Entry()
        {
            // create defaults to prevent null refs
            Repository = new Repository();
            WorkingCopy = new WorkingCopy();
            Commit = new LastCommit();
        }

        /// <remarks/>
        [XmlElement("url")]
        public string Url { get; set; }

        /// <remarks/>
        [XmlElement("repository")]
        public Repository Repository { get; set; }

        /// <remarks/>
        [XmlElement("wc-info")]
        public WorkingCopy WorkingCopy { get; set; }

        /// <remarks/>
        [XmlElement("commit")]
        public LastCommit Commit { get; set; }

        /// <remarks/>
        [XmlAttribute("kind")]
        public string Kind { get; set; }

        /// <remarks/>
        [XmlAttribute("path")]
        public string Path { get; set; }

        /// <remarks/>
        [XmlAttribute("revision")]
        public long Revision { get; set; }
    }

    /// <remarks/>
    public class EntryCollection : KeyedCollection<string, Entry>
    {
        /// <remarks/>
        public EntryCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        { }

        /// <remarks/>
        protected override string GetKeyForItem(Entry item)
        {
            return item.Path;
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("Root: {Root}")]
    public class Repository
    {
        /// <remarks/>
        [XmlElement("root")]
        public string Root { get; set; }

        /// <remarks/>
        [XmlElement("uuid")]
        public string Uuid { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("Schedule: {Schedule}")]
    public class WorkingCopy
    {
        /// <remarks/>
        [XmlElement("schedule")]
        public string Schedule { get; set; }

        /// <remarks/>
        [XmlElement("depth")]
        public string Depth { get; set; }

        /// <remarks/>
        [XmlElement("text-updated")]
        public DateTime TextUpdated { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool TextUpdatedSpecified { get; set; }

        /// <remarks/>
        [XmlElement("checksum")]
        public string Checksum { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("Revision: {Revision} Date: {Date}")]
    public class LastCommit
    {
        /// <remarks/>
        [XmlElement("author")]
        public string Author { get; set; }

        /// <remarks/>
        [XmlElement("date")]
        public DateTime Date { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool DateSpecified { get; set; }

        /// <remarks/>
        [XmlAttribute("revision")]
        public long Revision { get; set; }
    }
}