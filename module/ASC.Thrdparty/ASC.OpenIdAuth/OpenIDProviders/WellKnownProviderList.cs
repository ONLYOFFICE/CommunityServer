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

using System.Collections.Generic;

namespace OpenIdAuth.OpenIDProviders
{
    public class WellKnownProviderList:List<OpenIdProvider>
    {
        public WellKnownProviderList()
        {
            Add(new OpenIdProvider() { ProviderName = "Google", UrlFormat = "https://www.google.com/accounts/o8/id" });
            Add(new OpenIdProvider() { ProviderName = "Yahoo", UrlFormat = "http://me.yahoo.com" });
            Add(new OpenIdProvider() { ProviderName = "LiveJournal", UrlFormat = "http://{0}.livejournal.com", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "MySpace", UrlFormat = "http://myspace.com/{0}", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "WordPress", UrlFormat = "http://{0}.wordpress.com", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "Blogger", UrlFormat = "http://{0}.blogger.com", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "Steam", UrlFormat = "http://steamcommunity.com/openid/" });
            Add(new OpenIdProvider() { ProviderName = "Flickr", UrlFormat = "http://flickr.com/photos/{0}", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "AOL", UrlFormat = "http://openid.aol.com/{0}", UserNameNeeded = true });

            Add(new OpenIdProvider() { ProviderName = "Yandex", UrlFormat = "http://openid.yandex.ru/{0}", UserNameNeeded = true });
            Add(new OpenIdProvider() { ProviderName = "Vkontakte", UrlFormat = "http://VKontakteID.ru" });
            Add(new OpenIdProvider() { ProviderName = "AOL", UrlFormat = "http://openid.aol.com/{0}", UserNameNeeded = true });

        }
    }
}