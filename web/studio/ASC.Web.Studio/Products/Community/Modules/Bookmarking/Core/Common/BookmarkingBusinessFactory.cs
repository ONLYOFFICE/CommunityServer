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


using System.Web;

namespace ASC.Bookmarking.Common
{

	public static class BookmarkingBusinessFactory
	{
		public static T GetObjectFromSession<T>() where T : class, new()
		{
		    T obj;
			var key = typeof(T).ToString();
            if (HttpContext.Current.Session != null)
            {
                obj = (T) HttpContext.Current.Session[key];
                if (obj == null)
                {
                    obj = new T();
                    HttpContext.Current.Session[key] = obj;
                }
            }
            else
            {
                obj = (T)HttpContext.Current.Items[key];
                if (obj == null)
                {
                    obj = new T();
                    HttpContext.Current.Items[key] = obj;
                }
            }
		    return obj;
		}

        public static void UpdateObjectInSession<T>(T obj) where T : class, new()
        {
            var key = typeof(T).ToString();
            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[key] = obj;
            }
            else
            {
                HttpContext.Current.Items[key] = obj;
            }
        }

	    public static void UpdateDisplayMode(BookmarkDisplayMode mode)
	    {
	        var key = typeof (BookmarkDisplayMode).Name;

	        if (HttpContext.Current != null && HttpContext.Current.Session != null)
	            HttpContext.Current.Session.Add(key, mode);
	    }

	    public static BookmarkDisplayMode GetDisplayMode()
	    {
	        var key = typeof (BookmarkDisplayMode).Name;

	        if (HttpContext.Current != null && HttpContext.Current.Session != null)
	            return (BookmarkDisplayMode) HttpContext.Current.Session[key];

	        return BookmarkDisplayMode.AllBookmarks;
	    }
	}
}
