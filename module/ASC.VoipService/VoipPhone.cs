/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;

namespace ASC.VoipService
{
    public class VoipPhone
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Alias { get; set; }
        public VoipSettings Settings { get; set; }
        public Agent Caller
        {
            get { return Settings.Caller; }
        }

        public VoipPhone()
        {
            Settings = new VoipSettings();
        }

        public virtual VoipCall Call(string to, string contactId = null)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall LocalCall(string to)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall RedirectCall(string callId, string to)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall HoldUp(string callId)
        {
            throw new NotImplementedException();
        }

        public virtual void AnswerQueueCall(string callId)
        {
            throw new NotImplementedException();
        }

        public virtual void RejectQueueCall(string callId)
        {
            throw new NotImplementedException();
        }
    }

    public class VoipRecord
    {
        public string Id { get; set; }

        public string Uri { get; set; }

        public int Duration { get; set; }

        public decimal Price { get; set; }
    }
}
