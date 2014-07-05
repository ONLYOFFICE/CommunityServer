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
#region Attachment Object
    /// <summary>
    /// Reprensents a file attachment (a MimePart with an attachment Content-Disposition).
    /// Attachments are displayed only upon request of the receiving user, while EmbeddedObjects are displayed upon display of the message's content.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Attachment : ActiveUp.Net.Mail.MimePart
    {
        //bool _isLoaded = false;
        //string _originalPath;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Attachment()
        {

        }
        /*/// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path of the file to be treated as an attachment.</param>
        /// <param name="loadNow">If true, file content is loaded into the object. If false, file content is left on the disk and can be loaded by using the <see cref="Load"/> method. File content will be automatically loaded when message is sent.</param>
        public Attachment(string filePath, bool loadNow)
        {
            _Initialization(filePath,ActiveUp.Net.Mail.ContentTransferEncoding.Base64,loadNow);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path of the file to be treated as an attachment.</param>
        /// <param name="encoding">Transfer encoding to be used for the attachment's content.</param>
        /// <param name="loadNow">If true, file content is loaded into the object. If false, file content is left on the disk and can be loaded by using the <see cref="Load"/> method. File content will be automatically loaded when message is sent.</param>
        public Attachment(string filePath, ActiveUp.Net.Mail.ContentTransferEncoding encoding, bool loadNow)
        {
            _Initialization(filePath,encoding,loadNow);
        }

        /// <summary>
        /// Initialize the Attachment object.
        /// </summary>
        /// <param name="filePath">Path of the file to be treated as an attachment.</param>
        /// <param name="encoding">Transfer encoding to be used for the attachment's content.</param>
        /// <param name="loadNow">If true, file content is loaded into the object. If false, file content is left on the disk and can be loaded by using the <see cref="Load"/> method. File content will be automatically loaded when message is sent.</param>
        private void _Initialization(string filePath, ActiveUp.Net.Mail.ContentTransferEncoding encoding, bool loadNow)
        {
            string extension = System.IO.Path.GetExtension(filePath);
            string filename = System.IO.Path.GetFileName(filePath);
            this.OriginalPath = filePath;
            this.ContentDisposition = ActiveUp.Net.Mail.ContentDisposition.Attachment;
            this.ContentType = ActiveUp.Net.Mail.MimeTypesHelper.GetMimeqType(extension);
            this.ContentTransferEncoding = encoding;
            this.ContentId = "<AUA"+System.DateTime.Now.ToString("yyMMddhhmmss")+System.DateTime.Now.Millisecond.ToString()+"@"+System.Net.Dns.GetHostName()+">";
            this.ContentName = filename;
            if(loadNow)
            {
                System.IO.FileStream fs = new System.IO.FileStream(filePath,System.IO.FileMode.Open,System.IO.FileAccess.Read);
                fs.Lock(0,System.Convert.ToInt32(fs.Length));
                this.BinaryContent = new byte[System.Convert.ToInt32(fs.Length)];
                fs.Read(this.BinaryContent,0,System.Convert.ToInt32(fs.Length));
                fs.Unlock(0,System.Convert.ToInt32(fs.Length));
                fs.Close();
                this.IsLoaded = true;
            }
        }

        /// <summary>
        /// Indicates whether the attachment's content is loaded or not.
        /// The content can be loaded by using the <see cref="Load"/> method. File content will be automatically loaded when message is sent.</param>
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
            set
            {
                this._isLoaded = value;
            }
        }
        internal string OriginalPath
        {
            get
            {
                return this._originalPath;
            }
            set
            {
                this._originalPath = value;
            }
        }
        /// <summary>
        /// Loads the file content into the object.
        /// </summary>
        public void Load()
        {
            if(!this.IsLoaded && this.OriginalPath!=null)
            {
                System.IO.FileStream fs = new System.IO.FileStream(this.OriginalPath,System.IO.FileMode.Open,System.IO.FileAccess.Read);
                fs.Lock(0,System.Convert.ToInt32(fs.Length));
                this.BinaryContent = new byte[System.Convert.ToInt32(fs.Length)];
                fs.Read(this.BinaryContent,0,System.Convert.ToInt32(fs.Length));
                fs.Unlock(0,System.Convert.ToInt32(fs.Length));
                fs.Close();
                this.IsLoaded = true;
            }
        }*/
    }
    #endregion
}