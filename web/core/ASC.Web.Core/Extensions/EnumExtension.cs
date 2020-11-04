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


namespace System
{
    public static class EnumExtension
    {
        public static T TryParseEnum<T>(this Type enumType, string value, T defaultValue) where T : struct
        {
            bool isDefault;
            return TryParseEnum<T>(enumType, value, defaultValue, out isDefault);            
        }

        public static T TryParseEnum<T>(this Type enumType, string value, T defaultValue, out bool isDefault) where T : struct
        {
            isDefault = false;
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                isDefault = true;
                return defaultValue;
            }
        }
    }
}
