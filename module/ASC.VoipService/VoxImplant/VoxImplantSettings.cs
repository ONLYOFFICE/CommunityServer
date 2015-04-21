/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.IO;
using System.Text;

namespace ASC.VoipService.VoxImplant
{
    public class VoxImplantSettings : VoipSettings
    {
        public RuleInfoType Rule { get; set; }

        public VoxImplantSettings()
        {
            
        }

        public VoxImplantSettings(Uri uri)
        {
            var voiceUrl = uri.AbsoluteUri;
            var settings = voiceUrl.Substring(0, voiceUrl.IndexOf(";", System.StringComparison.Ordinal));

            JsonSettings = settings.Substring(settings.IndexOf("{", System.StringComparison.Ordinal));
        }

        public VoxImplantSettings(string settings) : base(settings)
        {
        }

        public override string Connect(bool user = true, string contactId = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("var voxImplantSettings={0};", JsonSettings);

            using (var stream = GetType().Assembly.GetManifestResourceStream("ASC.VoipService.VoxImplant.application.js"))
            {
                using (var sr = new StreamReader(stream))
                {
                    stringBuilder.Append(sr.ReadToEnd());
                }
            }

            return stringBuilder.ToString();
        }
    }
}
