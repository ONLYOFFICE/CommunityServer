/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Photo.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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