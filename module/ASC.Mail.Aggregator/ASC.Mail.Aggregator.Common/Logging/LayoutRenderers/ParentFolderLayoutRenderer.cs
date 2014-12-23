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

using System.IO;
using System.Reflection;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace ASC.Mail.Aggregator.Common.Logging.LayoutRenderers
{
    /// <summary>
    /// The parent application directory.
    /// </summary>
    [LayoutRenderer("parentdir")]
    [AppDomainFixedOutput]
    public class ParentFolderLayoutRenderer : LayoutRenderer
    {
        private string logDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentFolderLayoutRenderer" /> class.
        /// </summary>
        public ParentFolderLayoutRenderer()
        {
            this.logDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name + Path.DirectorySeparatorChar;
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
        /// Renders the parent directory and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var outputPath = this.logDir;

            if (this.Dir != null)
            {
                outputPath = Path.Combine(outputPath, this.Dir);
            }

            if (this.File != null)
            {
                outputPath = Path.Combine(outputPath, this.File);
            }

            builder.Append(outputPath);
        }
    }
}
