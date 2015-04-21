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


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.IO;
using System;

namespace TMResourceData
{
    public class AssemblyWork
    {
        static readonly List<Assembly> ListAssembly = new List<Assembly>();


        public static void PatchResourceManager(Type resourceManagerType)
        {
            if (resourceManagerType.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
            {
                try
                {
                    var resManager = resourceManagerType.InvokeMember("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, resourceManagerType, new object[] { });
                    var fileName = resourceManagerType.Name + ".resx";

                    var databaseResourceManager = new DBResourceManager(fileName, resManager as ResourceManager);
                    resourceManagerType.InvokeMember("resourceMan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.SetField, null, resourceManagerType, new object[] { databaseResourceManager });
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException is FileNotFoundException && ((FileNotFoundException)e.InnerException).FileName == "App_GlobalResources")
                    {
                        // ignore
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static void UploadResourceData(Assembly[] assemblies)
        {
            var callingAssembly = Assembly.GetCallingAssembly();

            if (!ListAssembly.Contains<Assembly>(callingAssembly))
            {
                ListAssembly.Add(callingAssembly);
                RemoveResManager(callingAssembly);
            }

            foreach (var assembly in assemblies.Except(ListAssembly).Where(assembly => ListAssembly.IndexOf(assembly) < 0 && (assembly.GetName().Name.IndexOf("ASC") >= 0 || assembly.GetName().Name.IndexOf("App_GlobalResources") >= 0)))
            {
                if (assembly.GetName().Name == "ASC.Common" || assembly.GetName().Name == "ASC.Core.Common")
                {
                    continue;
                }
                ListAssembly.Add(assembly);
                RemoveResManager(assembly);
            }
        }

        static void RemoveResManager(Assembly assembly)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    PatchResourceManager(type);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // ignore
            }
        }
    }
}
