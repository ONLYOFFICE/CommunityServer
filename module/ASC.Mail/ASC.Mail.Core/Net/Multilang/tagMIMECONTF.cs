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

namespace MultiLanguage
{
    using System;
    using System.Security;

    public enum tagMIMECONTF
    {
        MIMECONTF_BROWSER = 2,
        MIMECONTF_EXPORT = 0x400,
        MIMECONTF_IMPORT = 8,
        MIMECONTF_MAILNEWS = 1,
        MIMECONTF_MIME_IE4 = 0x10000000,
        MIMECONTF_MIME_LATEST = 0x20000000,
        MIMECONTF_MIME_REGISTRY = 0x40000000,
        MIMECONTF_MINIMAL = 4,
        MIMECONTF_PRIVCONVERTER = 0x10000,
        MIMECONTF_SAVABLE_BROWSER = 0x200,
        MIMECONTF_SAVABLE_MAILNEWS = 0x100,
        MIMECONTF_VALID = 0x20000,
        MIMECONTF_VALID_NLS = 0x40000
    }
}
