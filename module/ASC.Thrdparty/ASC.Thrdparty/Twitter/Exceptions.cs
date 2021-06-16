/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


namespace ASC.Thrdparty.Twitter
{
    public class TwitterException : SocialMediaException
    {
        public TwitterException(string message)
            : base(message)
        {
        }
    }

    public class ConnectionFailureException : TwitterException
    {
        public ConnectionFailureException(string message)
            : base(message)
        {
        }
    }

    public class RateLimitException : TwitterException
    {
        public RateLimitException(string message)
            : base(message)
        {
        }
    }

    public class ResourceNotFoundException : TwitterException
    {
        public ResourceNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class UnauthorizedException : TwitterException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}
