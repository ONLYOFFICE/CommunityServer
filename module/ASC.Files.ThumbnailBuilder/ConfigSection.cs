/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System;
using System.Configuration;

namespace ASC.Files.ThumbnailBuilder
{
    public class ConfigSection : ConfigurationSection
    {
        #region worker settings

        [ConfigurationProperty("serverRoot", DefaultValue = "http://localhost/")]
        public string ServerRoot
        {
            get { return (string)this["serverRoot"]; }
        }

        [ConfigurationProperty("launchFrequency", DefaultValue = "0:0:0.5")]
        public TimeSpan LaunchFrequency
        {
            get { return (TimeSpan)this["launchFrequency"]; }
        }

        #endregion


        #region data privider settings

        [ConfigurationProperty("connectionStringName", DefaultValue = "default")]
        public string ConnectionStringName
        {
            get { return (string)this["connectionStringName"]; }
        }

        [ConfigurationProperty("formats", DefaultValue = ".pptx|.pptm|.ppt|.ppsx|.ppsm|.pps|.potx|.potm|.pot|.odp|.fodp|.otp|.gslides|.xlsx|.xlsm|.xls|.xltx|.xltm|.xlt|.ods|.fods|.ots|.gsheet|.csv|.docx|.docxf|.oform|.docm|.doc|.dotx|.dotm|.dot|.odt|.fodt|.ott|.gdoc|.txt|.rtf|.mht|.html|.htm|.fb2|.epub|.pdf|.djvu|.xps|.oxps|.bmp|.jpeg|.jpg|.png|.gif|.tiff|.tif|.ico")]
        public string Formats
        {
            get { return (string)this["formats"]; }
        }

        private string[] formatsArray;

        public string[] FormatsArray
        {
            get
            {
                if (formatsArray != null)
                {
                    return formatsArray;
                }
                formatsArray = (Formats ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
                return formatsArray;
            }
        }

        [ConfigurationProperty("sqlMaxResults", DefaultValue = 1000)]
        public int SqlMaxResults
        {
            get { return (int)this["sqlMaxResults"]; }
        }

        #endregion


        #region thumbnails generator settings

        [ConfigurationProperty("maxDegreeOfParallelism", DefaultValue = 10)]
        public int MaxDegreeOfParallelism
        {
            get { return (int)this["maxDegreeOfParallelism"]; }
        }

        [ConfigurationProperty("availableFileSize", DefaultValue = 100L * 1024L * 1024L)]
        public long AvailableFileSize
        {
            get { return (long)this["availableFileSize"]; }
        }

        [ConfigurationProperty("attemptsLimit", DefaultValue = 3)]
        public int AttemptsLimit
        {
            get { return (int)this["attemptsLimit"]; }
        }

        [ConfigurationProperty("attemptWaitInterval", DefaultValue = 1000)]
        public int AttemptWaitInterval
        {
            get { return (int)this["attemptWaitInterval"]; }
        }

        [ConfigurationProperty("thumbnaillHeight", DefaultValue = 128)]
        public int ThumbnaillHeight
        {
            get { return (int)this["thumbnaillHeight"]; }
        }

        [ConfigurationProperty("thumbnaillWidth", DefaultValue = 192)]
        public int ThumbnaillWidth
        {
            get { return (int)this["thumbnaillWidth"]; }
        }

        [ConfigurationProperty("thumbnailAspect", DefaultValue = 2)]
        public int ThumbnailAspect
        {
            get { return (int)this["thumbnailAspect"]; }
        }

        #endregion
    }
}