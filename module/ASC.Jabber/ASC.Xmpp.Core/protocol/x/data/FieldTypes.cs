/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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