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


using System;

namespace ASC.Web.Core.WebZones
{
    [Flags]
    public enum WebZoneType
    {
        Nowhere = 1,
        StartProductList = 2,
        TopNavigationProductList = 4,
        CustomProductList = 8,

        All = Nowhere | StartProductList | TopNavigationProductList | CustomProductList
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WebZoneAttribute : Attribute
    {
        public WebZoneType Type { get; private set; }

        public WebZoneAttribute(WebZoneType type)
        {
            Type = type;
        }
    }

    public interface IRenderWebItem
    {
    }
}