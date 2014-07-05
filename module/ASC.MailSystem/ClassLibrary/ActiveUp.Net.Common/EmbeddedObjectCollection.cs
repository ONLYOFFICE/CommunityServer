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

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Represents a collection of EmbeddedObject objects.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class EmbeddedObjectCollection : MimePartCollection
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EmbeddedObjectCollection()
        {

        }
        /// <summary>
        /// Add a MimePart to the embedded objects collection.
        /// </summary>
        /// <param name="part"></param>
        public new void Add(MimePart part)
        {
            part.ContentDisposition.Disposition = "inline";
            this.List.Add(part);
        }
        /// <summary>
        /// Generate and add a MimePart to the embedded objects collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="generateContentId">If true, a Content-ID Header field will be generated for the part.</param>
        public new string Add(string path, bool generateContentId)
        {
            MimePart part = new MimePart(path, generateContentId);
            part.ContentDisposition.Disposition = "inline";
            this.List.Add(part);
            return part.EmbeddedObjectContentId;
        }
        /// <summary>
        /// Generate and add a MimePart to the embedded objects collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="contentId">The Content-ID Header field will be used for the part.</param>
        public virtual string Add(string path, string contentId)
        {
            MimePart part = new MimePart(path, contentId);
            part.ContentDisposition.Disposition = "inline";
            this.List.Add(part);
            return part.EmbeddedObjectContentId;
        }
        /// <summary>
        /// Generate and add a MimePart to the embedded objects collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="contentId">The Content-ID Header field will be used for the part.</param>
        /// <param name="charset">The charset of the text contained in the file.</param>
        public virtual string Add(string path, string contentId, string charset)
        {
            MimePart part = new MimePart(path, contentId, charset);
            part.ContentDisposition.Disposition = "inline";
            this.List.Add(part);
            return part.EmbeddedObjectContentId;
        }
        /// <summary>
        /// Generate and add a MimePart to the embedded objects collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="generateContentId">If true, a Content-ID Header field will be generated for the part.</param>
        /// <param name="charset">The charset of the text contained in the file.</param>
        /// <remarks>This method is to be used with text files to ensure data integrity using the correct charset.</remarks>
        public new string Add(string path, bool generateContentId, string charset)
        {
            MimePart part = new MimePart(path, generateContentId, charset);
            part.ContentDisposition.Disposition = "inline";
            this.List.Add(part);
            return part.ContentId;
        }
    }
}