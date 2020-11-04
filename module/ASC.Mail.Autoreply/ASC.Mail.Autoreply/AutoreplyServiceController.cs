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


using ASC.Common.Module;
using ASC.Mail.Autoreply.AddressParsers;
using log4net.Config;

[assembly: XmlConfigurator]

namespace ASC.Mail.Autoreply
{

    public class AutoreplyServiceController : IServiceController
    {
        private AutoreplyService _autoreplyService;

        public void Start()
        {
            if (_autoreplyService == null)
            {
                _autoreplyService = new AutoreplyService();
                _autoreplyService.RegisterAddressParser(new CommentAddressParser());
                _autoreplyService.RegisterAddressParser(new CommunityAddressParser());
                _autoreplyService.RegisterAddressParser(new FileAddressParser());
                _autoreplyService.RegisterAddressParser(new ProjectAddressParser());
                _autoreplyService.Start();
            }
        }

        public void Stop()
        {
            if (_autoreplyService != null)
            {
                _autoreplyService.Stop();
                _autoreplyService.Dispose();
            }
        }
    }
}
