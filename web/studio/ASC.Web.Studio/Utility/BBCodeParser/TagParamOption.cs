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


namespace ASC.Web.Studio.Utility.BBCodeParser
{
    public class TagParamOption
    {
        public int ParamNumber { get; set; }
        public string DefaultValue { get; set; }
        public string PreValue { get; set; }
        public bool IsUseAnotherParamValue { get; set; }
        public int AnotherParamNumber { get; set; }

        public TagParamOption()
        {
            DefaultValue = "";
            ParamNumber = 0;
            PreValue = "";
            IsUseAnotherParamValue = false;
            AnotherParamNumber = 0;
        }
    }
}