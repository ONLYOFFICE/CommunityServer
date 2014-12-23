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

using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class AppenderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.APPEND, IsRequired = true)]
        public string Append
        {
            get { return (string) this[Schema.APPEND]; }
            set { this[Schema.APPEND] = value; }
        }

        [ConfigurationProperty(Schema.APPENDSECURE)]
        public string AppendSecure
        {
            get { return (string) this[Schema.APPENDSECURE]; }
            set { this[Schema.APPENDSECURE] = value; }
        }

        [ConfigurationProperty(Schema.ACCEPT_ENCODING)]
        public string AcceptEncoding
        {
            get { return (string) this[Schema.ACCEPT_ENCODING]; }
            set { this[Schema.ACCEPT_ENCODING] = value; }
        }

        [ConfigurationProperty(Schema.EXTs)]
        public string Extensions
        {
            get { return (string)this[Schema.EXTs]; }
            set { this[Schema.EXTs] = value; }
        }
    }
}