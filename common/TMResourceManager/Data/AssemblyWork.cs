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
