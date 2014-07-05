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
// // <copyright company="Ascensio System Limited" file="StatusCode.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

namespace ASC.Xmpp.Core.protocol.x.muc
{
    /// <summary>
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        ///   <para>This Enum Member is used if we have a unknown status code. In this case you have to parse
        ///     the code on your own.</para>
        /// </summary>
        Unknown = -1,

        /// <summary>
        ///   <para>Inform user that any occupant is allowed to see the user's full JID.</para> <list type="table">
        ///                                                                                       <listheader>
        ///                                                                                         <term>Info</term>
        ///                                                                                         <description>Description</description>
        ///                                                                                       </listheader>
        ///                                                                                       <item>
        ///                                                                                         <term>stanza</term>
        ///                                                                                         <description>message</description>
        ///                                                                                       </item>
        ///                                                                                       <item>
        ///                                                                                         <term>context</term>
        ///                                                                                         <description>Entering a room</description>
        ///                                                                                       </item>
        ///                                                                                       <item>
        ///                                                                                         <term>code</term>
        ///                                                                                         <description>100</description>
        ///                                                                                       </item>
        ///                                                                                     </list>
        /// </summary>
        FullJidVisible = 100,

        /// <summary>
        ///   <para>Inform user that his or her affiliation changed while not in the room.</para> <list type="table">
        ///                                                                                         <listheader>
        ///                                                                                           <term>Info</term>
        ///                                                                                           <description>Description</description>
        ///                                                                                         </listheader>
        ///                                                                                         <item>
        ///                                                                                           <term>stanza</term>
        ///                                                                                           <description>message (out of band)</description>
        ///                                                                                         </item>
        ///                                                                                         <item>
        ///                                                                                           <term>context</term>
        ///                                                                                           <description>Affiliation change</description>
        ///                                                                                         </item>
        ///                                                                                         <item>
        ///                                                                                           <term>code</term>
        ///                                                                                           <description>101</description>
        ///                                                                                         </item>
        ///                                                                                       </list>
        /// </summary>
        AffiliationChanged = 101,

        /// <summary>
        ///   <para>Inform occupants that room now shows unavailable members.</para> <list type="table">
        ///                                                                            <listheader>
        ///                                                                              <term>Info</term>
        ///                                                                              <description>Description</description>
        ///                                                                            </listheader>
        ///                                                                            <item>
        ///                                                                              <term>stanza</term>
        ///                                                                              <description>message</description>
        ///                                                                            </item>
        ///                                                                            <item>
        ///                                                                              <term>context</term>
        ///                                                                              <description>Configuration change</description>
        ///                                                                            </item>
        ///                                                                            <item>
        ///                                                                              <term>code</term>
        ///                                                                              <description>102</description>
        ///                                                                            </item>
        ///                                                                          </list>
        /// </summary>
        ShowUnavailableMembers = 102,

        /// <summary>
        ///   <para>Inform occupants that room now does not show unavailable members</para> <list type="table">
        ///                                                                                   <listheader>
        ///                                                                                     <term>Info</term>
        ///                                                                                     <description>Description</description>
        ///                                                                                   </listheader>
        ///                                                                                   <item>
        ///                                                                                     <term>stanza</term>
        ///                                                                                     <description>message</description>
        ///                                                                                   </item>
        ///                                                                                   <item>
        ///                                                                                     <term>context</term>
        ///                                                                                     <description>Configuration change</description>
        ///                                                                                   </item>
        ///                                                                                   <item>
        ///                                                                                     <term>code</term>
        ///                                                                                     <description>103</description>
        ///                                                                                   </item>
        ///                                                                                 </list>
        /// </summary>
        HideUnavailableMembers = 103,

        /// <summary>
        ///   <para>Inform occupants that some room configuration has changed.</para> <list type="table">
        ///                                                                             <listheader>
        ///                                                                               <term>Info</term>
        ///                                                                               <description>Description</description>
        ///                                                                             </listheader>
        ///                                                                             <item>
        ///                                                                               <term>stanza</term>
        ///                                                                               <description>message</description>
        ///                                                                             </item>
        ///                                                                             <item>
        ///                                                                               <term>context</term>
        ///                                                                               <description>Configuration change</description>
        ///                                                                             </item>
        ///                                                                             <item>
        ///                                                                               <term>code</term>
        ///                                                                               <description>104</description>
        ///                                                                             </item>
        ///                                                                           </list>
        /// </summary>
        ConfigurationChanged = 104,

        /// <summary>
        ///   <para>Inform occupant that room UI should not allow cut-copy-and-paste operations.</para> <list type="table">
        ///                                                                                               <listheader>
        ///                                                                                                 <term>Info</term>
        ///                                                                                                 <description>Description</description>
        ///                                                                                               </listheader>
        ///                                                                                               <item>
        ///                                                                                                 <term>stanza</term>
        ///                                                                                                 <description>message</description>
        ///                                                                                               </item>
        ///                                                                                               <item>
        ///                                                                                                 <term>context</term>
        ///                                                                                                 <description>Entering a room; configuration change</description>
        ///                                                                                               </item>
        ///                                                                                               <item>
        ///                                                                                                 <term>code</term>
        ///                                                                                                 <description>171</description>
        ///                                                                                               </item>
        ///                                                                                             </list>
        /// </summary>
        RoomUiProhibitCutCopyPasteOperations = 171,

        /// <summary>
        ///   <para>Inform occupant that room UI should allow cut-copy-and-paste operations.</para> <list type="table">
        ///                                                                                           <listheader>
        ///                                                                                             <term>Info</term>
        ///                                                                                             <description>Description</description>
        ///                                                                                           </listheader>
        ///                                                                                           <item>
        ///                                                                                             <term>stanza</term>
        ///                                                                                             <description>message</description>
        ///                                                                                           </item>
        ///                                                                                           <item>
        ///                                                                                             <term>context</term>
        ///                                                                                             <description>Entering a room; configuration change</description>
        ///                                                                                           </item>
        ///                                                                                           <item>
        ///                                                                                             <term>code</term>
        ///                                                                                             <description>172</description>
        ///                                                                                           </item>
        ///                                                                                         </list>
        /// </summary>
        RoomUiAllowCutCopyPasteOperations = 172,

        /// <summary>
        ///   <para>Inform user that a new room has been created</para> <list type="table">
        ///                                                               <listheader>
        ///                                                                 <term>Info</term>
        ///                                                                 <description>Description</description>
        ///                                                               </listheader>
        ///                                                               <item>
        ///                                                                 <term>stanza</term>
        ///                                                                 <description>presence</description>
        ///                                                               </item>
        ///                                                               <item>
        ///                                                                 <term>context</term>
        ///                                                                 <description>Entering a room</description>
        ///                                                               </item>
        ///                                                               <item>
        ///                                                                 <term>code</term>
        ///                                                                 <description>201</description>
        ///                                                               </item>
        ///                                                             </list>
        /// </summary>
        RoomCreated = 201,

        /// <summary>
        ///   <para>Inform user that he or she has been banned from the room</para> <list type="table">
        ///                                                                           <listheader>
        ///                                                                             <term>Info</term>
        ///                                                                             <description>Description</description>
        ///                                                                           </listheader>
        ///                                                                           <item>
        ///                                                                             <term>stanza</term>
        ///                                                                             <description>presence</description>
        ///                                                                           </item>
        ///                                                                           <item>
        ///                                                                             <term>context</term>
        ///                                                                             <description>Removal from room</description>
        ///                                                                           </item>
        ///                                                                           <item>
        ///                                                                             <term>code</term>
        ///                                                                             <description>301</description>
        ///                                                                           </item>
        ///                                                                         </list>
        /// </summary>
        Banned = 301,

        /// <summary>
        ///   <para>Inform all occupants of new room nickname</para> <list type="table">
        ///                                                            <listheader>
        ///                                                              <term>Info</term>
        ///                                                              <description>Description</description>
        ///                                                            </listheader>
        ///                                                            <item>
        ///                                                              <term>stanza</term>
        ///                                                              <description>presence</description>
        ///                                                            </item>
        ///                                                            <item>
        ///                                                              <term>context</term>
        ///                                                              <description>Exiting a room</description>
        ///                                                            </item>
        ///                                                            <item>
        ///                                                              <term>code</term>
        ///                                                              <description>303</description>
        ///                                                            </item>
        ///                                                          </list>
        /// </summary>
        NewNickname = 303,

        /// <summary>
        ///   <para>Inform user that he or she has been kicked from the room</para> <list type="table">
        ///                                                                           <listheader>
        ///                                                                             <term>Info</term>
        ///                                                                             <description>Description</description>
        ///                                                                           </listheader>
        ///                                                                           <item>
        ///                                                                             <term>stanza</term>
        ///                                                                             <description>presence</description>
        ///                                                                           </item>
        ///                                                                           <item>
        ///                                                                             <term>context</term>
        ///                                                                             <description>Removal from room</description>
        ///                                                                           </item>
        ///                                                                           <item>
        ///                                                                             <term>code</term>
        ///                                                                             <description>307</description>
        ///                                                                           </item>
        ///                                                                         </list>
        /// </summary>
        Kicked = 307,

        /// <summary>
        ///   <para>Inform user that he or she is being removed from the room because of an affiliation change</para> <list
        ///    type="table">
        ///                                                                                                             <listheader>
        ///                                                                                                               <term>Info</term>
        ///                                                                                                               <description>Description</description>
        ///                                                                                                             </listheader>
        ///                                                                                                             <item>
        ///                                                                                                               <term>stanza</term>
        ///                                                                                                               <description>presence</description>
        ///                                                                                                             </item>
        ///                                                                                                             <item>
        ///                                                                                                               <term>context</term>
        ///                                                                                                               <description>Removal from room</description>
        ///                                                                                                             </item>
        ///                                                                                                             <item>
        ///                                                                                                               <term>code</term>
        ///                                                                                                               <description>321</description>
        ///                                                                                                             </item>
        ///                                                                                                           </list>
        /// </summary>
        AffiliationChange = 321,

        /// <summary>
        ///   <para>Inform user that he or she is being removed from the room because the room has been changed
        ///     to members-only and the user is not a member</para> <list type="table">
        ///                                                           <listheader>
        ///                                                             <term>Info</term>
        ///                                                             <description>Description</description>
        ///                                                           </listheader>
        ///                                                           <item>
        ///                                                             <term>stanza</term>
        ///                                                             <description>presence</description>
        ///                                                           </item>
        ///                                                           <item>
        ///                                                             <term>context</term>
        ///                                                             <description>Removal from room</description>
        ///                                                           </item>
        ///                                                           <item>
        ///                                                             <term>code</term>
        ///                                                             <description>322</description>
        ///                                                           </item>
        ///                                                         </list>
        /// </summary>
        MembersOnly = 322,

        /// <summary>
        ///   <para>Inform user that he or she is being removed from the room because of a system shutdown</para> <list type="table">
        ///                                                                                                         <listheader>
        ///                                                                                                           <term>Info</term>
        ///                                                                                                           <description>Description</description>
        ///                                                                                                         </listheader>
        ///                                                                                                         <item>
        ///                                                                                                           <term>stanza</term>
        ///                                                                                                           <description>presence</description>
        ///                                                                                                         </item>
        ///                                                                                                         <item>
        ///                                                                                                           <term>context</term>
        ///                                                                                                           <description>Removal from room</description>
        ///                                                                                                         </item>
        ///                                                                                                         <item>
        ///                                                                                                           <term>code</term>
        ///                                                                                                           <description>332</description>
        ///                                                                                                         </item>
        ///                                                                                                       </list>
        /// </summary>
        Shutdown = 332
    }
}