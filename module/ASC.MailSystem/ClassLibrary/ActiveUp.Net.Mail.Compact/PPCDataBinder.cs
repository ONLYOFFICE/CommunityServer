using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Psuedo Class to Support DataBinder. Since it's missing, we imply Value as current value
    /// </summary>
#if PocketPC
    public class DataBinder
    {
        /// <summary>
        /// Gets Default Property value of the Object. Only available in PPC, returns the object itself.
        /// </summary>
        /// <param name="source">item for retrieving Data</param>
        /// <returns></returns>
        public static object GetPropertyValue(object source)
        {
            return source.ToString();
        }

        public static object GetPropertyValue(object source, string field)
        {
            return source.ToString();
        }


        public static object GetPropertyValue(object source,string field,string format)
        {
            return source.ToString();
        }

        public static object Eval(object source, string Expression)
        {
            return source.ToString();
        }

        public static object Eval(object source, string Expression,string format)
        {
            return source.ToString();
        }

    }
#endif
}
