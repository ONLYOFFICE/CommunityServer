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
// // <copyright company="Ascensio System Limited" file="VcardUpdate.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#region using

using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.protocol.x.vcard_update
{

    #region usings

    #endregion

    /*
        <presence>
          <x xmlns='vcard-temp:x:update'>
            <photo/>
          </x>
        </presence>
    */

    /// <summary>
    /// </summary>
    public class VcardUpdate : Element
    {
        #region Constructor

        /// <summary>
        ///   Initializes a new instance of the <see cref="VcardUpdate" /> class.
        /// </summary>
        public VcardUpdate()
        {
            TagName = "x";
            Namespace = Uri.VCARD_UPDATE;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="VcardUpdate" /> class.
        /// </summary>
        /// <param name="photo"> The photo. </param>
        public VcardUpdate(string photo) : this()
        {
            Photo = photo;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   SHA1 hash of the avatar image data <para>if no image/avatar should be advertised, or other clients should be forced
        ///                                        to remove the image set it to a empty string value ("")</para> <para>if this protocol is supported but you ae not ready o advertise a imaeg yet
        ///                                                                                                         set teh value to null.</para> <para>Otherwise teh value must the SHA1 hash of the image data.</para>
        /// </summary>
        public string Photo
        {
            get { return GetTag("photo"); }

            set
            {
                if (value == null)
                {
                    RemoveTag("photo");
                }
                else
                {
                    SetTag("photo", value);
                }
            }
        }

        #endregion
    }
}