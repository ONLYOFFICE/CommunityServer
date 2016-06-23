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


using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace ASC.Mail.Aggregator.Common.Logging.LayoutRenderers
{
    /// <summary>
    /// The current application logs directory.
    /// </summary>
    [LayoutRenderer("syslogdir")]
    [AppDomainFixedOutput]
    public class SystemLogFolderLayoutRenderer : LayoutRenderer
    {
        private readonly string _logDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemLogFolderLayoutRenderer" /> class.
        /// </summary>
        public SystemLogFolderLayoutRenderer()
        {
            _logDir = Path.DirectorySeparatorChar == '/' ? "/var/log/" : Path.GetTempPath();
        }

        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with with the base directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with with the base directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string Dir { get; set; }

        /// <summary>
        /// Renders the application base directory and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var outputPath = _logDir;

            if (Dir != null)
            {
                outputPath = Path.Combine(outputPath, Dir);
            }

            if (File != null)
            {
                outputPath = Path.Combine(outputPath, File);
            }

            builder.Append(outputPath);
        }
    }
}
