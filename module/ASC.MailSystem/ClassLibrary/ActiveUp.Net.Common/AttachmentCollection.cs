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
    /// Represents a collection of Attachment objects.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class AttachmentCollection : MimePartCollection
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AttachmentCollection()
        {

        }
        /// <summary>
        /// Add a MimePart to the attachment collection.
        /// </summary>
        /// <param name="part"></param>
        public new void Add(MimePart part)
        {
            part.ContentDisposition.Disposition = "attachment";
            this.List.Add(part);
        }
        /// <summary>
        /// Generate and add a MimePart to the attachment collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="generateContentId">If true, a Content-ID Header field will be generated for the part.</param>
        public new void Add(string path, bool generateContentId)
        {
            MimePart part = new MimePart(path,generateContentId);
            part.ContentDisposition.Disposition = "attachment";
            this.List.Add(part);
        }
        /// <summary>
        /// Generate and add a MimePart to the attachment collection, using the specified file.
        /// </summary>
        /// <param name="path">The file containing the MimePart's content.</param>
        /// <param name="generateContentId">If true, a Content-ID Header field will be generated for the part.</param>
        /// <param name="charset">The charset of the text contained in the file.</param>
        /// <remarks>This method is to be used with text files to ensure data integrity using the correct charset.</remarks>
        public new void Add(string path, bool generateContentId, string charset)
        {
            MimePart part = new MimePart(path,generateContentId,charset);
            part.ContentDisposition.Disposition = "attachment";
            this.List.Add(part);
        }

        /// <summary>
        /// Adds the specified attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <param name="filename">The filename.</param>
        public void Add(byte[] attachment, string filename)
        {
            MimePart part = new MimePart(attachment, filename);
            part.ContentDisposition.Disposition = "attachment";
            this.List.Add(part);
        }

        /// <summary>
        /// Stores all the attachments to a folder.
        /// </summary>
        /// <param name="path">The destination folder.</param>
        public void StoreToFolder(string path)
        {
            foreach (MimePart mimePart in this)
            {
                //mimePart.StoreToFile(path.TrimEnd('\\') + "\\" + (!string.IsNullOrEmpty(mimePart.Filename) ? mimePart.Filename : mimePart.ContentName));
                mimePart.StoreToFile(string.Format("{0}\\{1}", path.TrimEnd('\\'), mimePart.Filename));
            }
        }
    }
}