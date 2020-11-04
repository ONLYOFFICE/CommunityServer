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


using System.Collections.Generic;

namespace TMResourceData.Model
{
    public class ResModule
    {
        public string Name { get; set; }
        public bool IsLock { get; set; }
        public List<ResWord> ListWords { get; set; }
        public Dictionary<WordStatusEnum, int> Counts { get; set; }

        public ResModule()
        {
            ListWords = new List<ResWord>();
            Counts = new Dictionary<WordStatusEnum, int>
                         {
                             {WordStatusEnum.Translated, 0},
                             {WordStatusEnum.Changed, 0},
                             {WordStatusEnum.All, 0},
                             {WordStatusEnum.Untranslated, 0}
                         };
        }
    }
}
