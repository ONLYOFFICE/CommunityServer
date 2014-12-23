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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using TMResourceData.Model;

namespace TMResourceData
{
    public static class JsonManager
    {
        public static void UploadJson(string fileName, Stream fileStream, string projectName, string moduleName)
        {
            var culture = GetCultureFromFileName(fileName);

            string jsonString;
            using (var reader = new StreamReader(fileStream))
            {
                jsonString = reader.ReadToEnd();
            }
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

            var fileID = ResourceData.AddFile(fileName, projectName, moduleName);
            const string resourceType = "text";
            foreach (var key in jsonObj.Keys)
            {
                var word = new ResWord
                    {
                        Title = key,
                        ValueFrom = jsonObj[key],
                        ResFile = new ResFile {FileID = fileID}
                    };
                ResourceData.AddResource(culture, resourceType, DateTime.UtcNow, word, true, "Console");
            }
        }

        public static string ExportJson(string project, string module, List<string> languages, string exportPath)
        {
            using (var fastZip = new ZipFile())
            {
                var filter = new ResCurrent
                    {
                        Project = new ResProject {Name = project},
                        Module = new ResModule {Name = module}
                    };

                var zipDirectory = Directory.CreateDirectory(exportPath + module);
                foreach (var language in languages)
                {
                    filter.Language = new ResCulture {Title = language};

                    var words = GetResource.GetListResWords(filter, string.Empty).GroupBy(x => x.ResFile.FileID).ToList();
                    if (!words.Any())
                    {
                        Console.WriteLine("Error!!! Can't find appropriate project and module. Possibly wrong names!");
                        return null;
                    }

                    foreach (var fileWords in words)
                    {
                        var wordsDictionary = new Dictionary<string, string>();
                        foreach (var word in fileWords.OrderBy(x=>x.Title).Where(word => !wordsDictionary.ContainsKey(word.Title)))
                        {
                            wordsDictionary[word.Title] = word.ValueTo ?? word.ValueFrom;
                        }

                        var firstWord = fileWords.FirstOrDefault();
                        var fileName = firstWord == null ? module : Path.GetFileNameWithoutExtension(firstWord.ResFile.FileName);

                        var zipFileName = zipDirectory.FullName + "\\" + fileName
                                          + (language == "Neutral" ? string.Empty : "." + language) + ".json";
                        using (TextWriter writer = new StreamWriter(zipFileName))
                        {
                            var obj = JsonConvert.SerializeObject(wordsDictionary, Formatting.Indented);
                            writer.Write(obj);
                        }
                    }
                }

                var zipPath = exportPath + "\\" + module + ".zip";
                fastZip.AddDirectory(zipDirectory.FullName);
                fastZip.Save(zipPath);

                zipDirectory.Delete(true);
                return zipPath;
            }
        }

        private static string GetCultureFromFileName(string fileName)
        {
            var culture = "Neutral";
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (nameWithoutExtension != null && nameWithoutExtension.Split('.').Length > 1)
            {
                culture = nameWithoutExtension.Split('.')[1];
            }

            return culture;
        }
    }
}