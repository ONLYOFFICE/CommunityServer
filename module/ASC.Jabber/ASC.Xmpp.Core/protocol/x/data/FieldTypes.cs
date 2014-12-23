/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Xmpp.Core.protocol.x.data
{
    /// <summary>
    ///   Field Types
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        ///   a unknown fieldtype
        /// </summary>
        Unknown,

        /// <summary>
        ///   The field enables an entity to gather or provide an either-or choice between two options. The allowable values are 1 for yes/true/assent and 0 for no/false/decline. The default value is 0.
        /// </summary>
        Boolean,

        /// <summary>
        ///   The field is intended for data description (e.g., human-readable text such as "section" headers) rather than data gathering or provision. The <value /> child SHOULD NOT contain newlines (the \n and \r characters); instead an application SHOULD generate multiple fixed fields, each with one <value /> child.
        /// </summary>
        Fixed,

        ///<summary>
        ///  The field is not shown to the entity providing information, but instead is returned with the form.
        ///</summary>
        Hidden,

        /// <summary>
        ///   The field enables an entity to gather or provide multiple Jabber IDs.
        /// </summary>
        Jid_Multi,

        /// <summary>
        ///   The field enables an entity to gather or provide a single Jabber ID.
        /// </summary>
        Jid_Single,

        /// <summary>
        ///   The field enables an entity to gather or provide one or more options from among many.
        /// </summary>
        List_Multi,

        /// <summary>
        ///   The field enables an entity to gather or provide one option from among many.
        /// </summary>
        List_Single,

        /// <summary>
        ///   The field enables an entity to gather or provide multiple lines of text.
        /// </summary>
        Text_Multi,

        /// <summary>
        ///   password style textbox. The field enables an entity to gather or provide a single line or word of text, which shall be obscured in an interface (e.g., *****).
        /// </summary>
        Text_Private,

        /// <summary>
        ///   The field enables an entity to gather or provide a single line or word of text, which may be shown in an interface. This field type is the default and MUST be assumed if an entity receives a field type it does not understand.
        /// </summary>
        Text_Single
    }
}