/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


#if !CF
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using ASC.Xmpp.Core.utils.Xml.Dom;

#if !SL
#endif

namespace ASC.Xmpp.Core.protocol.iq.vcard
{
    /// <summary>
    ///   Vcard Photo When you dont want System.Drawing in the Lib just remove the photo stuff
    /// </summary>
    public class Photo : Element
    {
        // <!-- Photograph property. Value is either a BASE64 encoded
        // binary value or a URI to the external content. -->
        // <!ELEMENT PHOTO ((TYPE, BINVAL) | EXTVAL)>	

        #region << Constructors >>

        public Photo()
        {
            TagName = "PHOTO";
            Namespace = Uri.VCARD;
        }

#if !SL
        public Photo(Image image, ImageFormat format)
            : this()
        {
            SetImage(image, format);
        }
#endif

        public Photo(string url)
            : this()
        {
            SetImage(url);
        }

        #endregion

        /// <summary>
        ///   The Media Type, Only available when BINVAL
        /// </summary>
        public string Type
        {
            //<TYPE>image</TYPE>
            get { return GetTag("TYPE"); }
            set { SetTag("TYPE", value); }
        }

        /// <summary>
        ///   Sets the URL of an external image
        /// </summary>
        /// <param name="url"> </param>
        public void SetImage(string url)
        {
            SetTag("EXTVAL", url);
        }

#if !SL
        public void SetImage(Image image, ImageFormat format)
        {
            // if we have no FOrmatprovider then we save the image as PNG
            if (format == null) format = ImageFormat.Png;

            string sType = "image";
            if (format == ImageFormat.Jpeg) sType = "image/jpeg";
            else if (format == ImageFormat.Png) sType = "image/png";
            else if (format == ImageFormat.Gif) sType = "image/gif";
#if!CF_2
            else if (format == ImageFormat.Tiff) sType = "image/tiff";
#endif
            SetTag("TYPE", sType);

            var ms = new MemoryStream();
            image.Save(ms, format);
            byte[] buf = ms.GetBuffer();
            SetTagBase64("BINVAL", buf);
        }

        /// <summary>
        ///   returns the image format or null for unknown formats or TYPES
        /// </summary>
        public ImageFormat ImageFormat
        {
            get
            {
                string sType = GetTag("TYPE");

                if (sType == "image/jpeg")
                    return ImageFormat.Jpeg;
                else if (sType == "image/png")
                    return ImageFormat.Png;
                else if (sType == "image/gif")
                    return ImageFormat.Gif;
#if!CF_2
                else if (sType == "image/tiff")
                    return ImageFormat.Tiff;
#endif
                else
                    return null;
            }
        }

        /// <summary>
        ///   gets or sets the from internal (binary) or external source When external then it trys to get the image with a Webrequest
        /// </summary>
        public Image Image
        {
            get
            {
                try
                {
                    if (HasTag("BINVAL"))
                    {
                        byte[] pic = Convert.FromBase64String(GetTag("BINVAL"));
                        var ms = new MemoryStream(pic, 0, pic.Length);
                        return new Bitmap(ms);
                    }
                    else if (HasTag("EXTVAL"))
                    {
                        WebRequest req = WebRequest.Create(GetTag("EXTVAL"));
                        WebResponse response = req.GetResponse();
                        return new Bitmap(response.GetResponseStream());
                    }
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
            /*
            set
			{
				SetTag("TYPE", "image");
				MemoryStream ms = new MemoryStream();
				// Save the Image as PNG to the Memorystream
				value.Save(ms, ImageFormat.Png);
				byte[] buf = ms.GetBuffer();				
				SetTagBase64("BINVAL", buf);
			}
            */
        }
#endif
    }
}

#endif