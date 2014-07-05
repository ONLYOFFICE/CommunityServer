/*
 * ChatService.cs
 * 
 * Copyright © 2007 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#if(NET20)
/*
 * MS	06-04-16	initial version
 * 
 * 
 * 
 * 
 * 
 */
using System;
using System.Text;

namespace AjaxPro.Services
{
	[AjaxNamespace("AjaxPro.Services.Chat")]
	public abstract class IChatService
	{
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
		[AjaxMethod]
		public abstract bool SendMessage(string room, string message);

        /// <summary>
        /// Retrieves the new.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="lastRetreived">The last retreived.</param>
        /// <returns></returns>
		[AjaxMethod]
		public abstract object[] RetrieveNew(string room, DateTime lastRetreived);

        /// <summary>
        /// Retrieves the last.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
		[AjaxMethod]
		public abstract object[] RetrieveLast(string room, int count);
	}
}
#endif