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
using System.Runtime.Serialization;

namespace ASC.Core.Tenants
{
    [Serializable]
    public class TenantTooShortException : Exception
    {
        public int MinLength = 0;
        public int MaxLength = 0;

        public TenantTooShortException(string message)
            : base(message)
        {
        }

        public TenantTooShortException(string message, int minLength, int maxLength)
            : base(message)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        protected TenantTooShortException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class TenantIncorrectCharsException : Exception
    {
        public TenantIncorrectCharsException(string message)
            : base(message)
        {
        }

        protected TenantIncorrectCharsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class TenantAlreadyExistsException : Exception
    {
        public IEnumerable<string> ExistsTenants
        {
            get;
            private set;
        }

        public TenantAlreadyExistsException(string message, IEnumerable<string> existsTenants)
            : base(message)
        {
            ExistsTenants = existsTenants ?? Enumerable.Empty<string>();
        }

        protected TenantAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}