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
using System.Linq;

namespace ASC.Notify.Model
{
    public sealed class ConstActionProvider : IActionProvider
    {
        private readonly Dictionary<string, INotifyAction> actions;

        
        public ConstActionProvider(params INotifyAction[] actions)
        {
            this.actions = actions.ToDictionary(a => a.ID);
        }

        public INotifyAction[] GetActions()
        {
            return actions.Values.ToArray();
        }

        public INotifyAction GetAction(string id)
        {
            INotifyAction action;
            actions.TryGetValue(id, out action);
            return action;
        }
    }
}
