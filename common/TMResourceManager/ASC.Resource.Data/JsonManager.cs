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

                    var words = ResourceData.GetListResWords(filter, string.Empty).GroupBy(x => x.ResFile.FileID).ToList();
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