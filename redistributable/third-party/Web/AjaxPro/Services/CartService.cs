/*
 * CartService.cs
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
	[AjaxNamespace("AjaxPro.Services.Cart")]
	public abstract class ICartService
	{
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="cartName">Name of the cart.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
		[AjaxMethod]
		public abstract bool AddItem(string cartName, object item);

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="cartName">Name of the cart.</param>
        /// <returns></returns>
		[AjaxMethod]
		public abstract object[] GetItems(string cartName);
	}
}
#endif