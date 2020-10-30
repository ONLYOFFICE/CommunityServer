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


namespace ASC.ActiveDirectory.Base.Expressions
{
    /// <summary>
    /// Operations
    /// </summary>
    /// <remarks>
    /// [1 - negation][1 - binary][number]
    /// </remarks>
    public enum Op
    {
        //------------  UNARY -------------
        /// <summary>Attribute exists</summary>
        Exists = 0x000001,
        /// <summary>Attribute does not exist</summary>
        NotExists = 0x010002,

        //------------  BINARY -------------
        /// <summary>Equal</summary>
        Equal = 0x000103,
        /// <summary>Not equal</summary>
        NotEqual        = 0x010104,
        /// <summary>Strong less</summary>
        Less = 0x000105,
        /// <summary>Less or equal</summary>
        LessOrEqual = 0x000106,
        /// <summary>Strong greater</summary>
        Greater = 0x000107,
        /// <summary>Greater or equal</summary>
        GreaterOrEqual = 0x000108
    }
}
