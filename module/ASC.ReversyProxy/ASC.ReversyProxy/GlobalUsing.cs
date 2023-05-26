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


global using MySql.Data.MySqlClient;
global using Yarp.ReverseProxy.Configuration;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Primitives;
global using Yarp.ReverseProxy.Model;
global using ASC.ReversyProxy;
global using Microsoft.Extensions.Hosting.WindowsServices;
global using NLog;
global using NLog.Web;


