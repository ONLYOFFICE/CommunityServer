using System.Web.UI;

namespace ASC.Web.Core
{
    public class BasePage : Page
    {
        //hack: for precompile mono
        public new virtual ValidateRequestMode ValidateRequestMode { get; set; }
    }
}
