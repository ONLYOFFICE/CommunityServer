/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Voip === "undefined")
    ASC.Voip = {};

ASC.Voip.Countries = (function () {
    function country(iso, title, code) {
        return { iso: iso, title: title, code: code };
    };

    return [
        country('US', 'United States', 1),
        country('AT', 'Austria', 43),
        country('AU', 'Australia', 61),
        country('BE', 'Belgium', 32),
        country('BG', 'Bulgaria', 359),
        country('CA', 'Canada', 1),
        country('CH', 'Switzerland', 41),
        country('CZ', 'Czech Republic', 420),
        country('DK', 'Denmark', 45),
        country('ES', 'Spain', 34),
        country('FI', 'Finland', 358),
        country('FR', 'France', 33),
        country('GB', 'United Kingdom', 44),
        country('GR', 'Greece', 30),
        country('HK', 'Hong Kong', 852),
        country('IE', 'Ireland', 353),
        country('IL', 'Israel', 972),
        country('IT', 'Italy', 39),
        country('JP', 'Japan', 81),
        country('LV', 'Latvia', 371),
        country('MX', 'Mexico', 52),
        country('NL', 'The Netherlands', 31),
        country('NZ', 'New Zealand', 64),
        country('PL', 'Poland', 48),
        country('PR', 'Puerto Rico', 1787),
        country('PT', 'Portugal', 351),
        country('RO', 'Romania', 40),
        country('SE', 'Sweden', 46),
        country('SK', 'Slovakia', 421)
    ];
})();