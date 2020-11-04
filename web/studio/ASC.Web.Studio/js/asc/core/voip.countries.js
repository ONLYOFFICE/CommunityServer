/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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