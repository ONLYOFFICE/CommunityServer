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


namespace ASC.Web.Studio.Utility.HtmlUtility.CodeFormat
{
    internal class CppFormat : CLikeFormat
    {
        protected override string Keywords
        {
            get
            {
                return "and and_eq asm auto bitand bitor bool break case catch char class compl "
                       + "const const_cast continue default delete do double dynamic_cast else enum "
                       + "explicit export extern false float for friend goto if inline int long mutable "
                       + "namespace new not not_eq operator or or_eq private protected public register "
                       + "reinterpret_cast return short signed sizeof static static_cast struct switch "
                       + "template this throw true try typedef typeid typename union unsigned using "
                       + "virtual void volatile wchar_t while xor xor_eq";
            }
        }

        protected override string Preprocessors
        {
            get
            {
                return "#define #error #if #ifdef #ifndef #else #elif #endif "
                       + "#include #line #pragma #undef";
            }
        }
    }
}