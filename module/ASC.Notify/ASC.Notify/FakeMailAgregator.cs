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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.Notify.Messages;
using System.Threading;

namespace ASC.Notify
{
    //Всё равно уберу, пусть пока статик
    static class FakeMailAgregator
    {
        private static readonly Random rnd = new Random();

        internal static bool SendMail(NotifyMessage m)
        {
            Thread.Sleep(rnd.Next(1000, 3000));
            return (rnd.Next(100)>10);
        }
    }
}
