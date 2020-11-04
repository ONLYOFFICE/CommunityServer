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

namespace ASC.Web.Core.Helpers
{
    public class GrammaticalHelper
    {
        public static string ChooseNumeralCase(int number, string nominative, string genitiveSingular, string genitivePlural)
        {
            if (
                String.Compare(
                    System.Threading.Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName,
                    "rus", true) == 0)
            {
                int[] formsTable = { 2, 0, 1, 1, 1, 2, 2, 2, 2, 2 };

                number = Math.Abs(number);
                int res = formsTable[((((number % 100) / 10) != 1) ? 1 : 0) * (number % 10)];
                switch (res)
                {
                    case 0:
                        return nominative;
                    case 1:
                        return genitiveSingular;
                    default:
                        return genitivePlural;
                }
            }
            else
                return number == 1 ? nominative : genitivePlural;
        }
    }
}
