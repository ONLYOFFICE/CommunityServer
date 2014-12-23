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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.compression
{
    /*
     * 
     * Note: If the initiating entity did not understand any of the advertised compression methods, 
     * it SHOULD ignore the compression option and proceed as if no compression methods were advertised.
     * 
     * If the initiating entity requests a stream compression method that is not supported by the 
     * receiving entity, the receiving entity MUST return an <unsupported-method/> error:
     * 
     * Example 3. Receiving Entity Reports That Method is Unsupported
     * <failure xmlns='http://jabber.org/protocol/compress'>
     *  <unsupported-method/>
     * </failure>
     * 
     * If the receiving entity cannot establish compression using the requested method for any 
     * other reason, it MUST return a <setup-failed/> error:
     * 
     * Example 4. Receiving Entity Reports That Compression Setup Failed
     * <failure xmlns='http://jabber.org/protocol/compress'>
     *  <setup-failed/>
     * </failure>
     */

    public class Failure : Element
    {
        public Failure()
        {
            TagName = "failure";
            Namespace = Uri.COMPRESS;
        }
    }
}