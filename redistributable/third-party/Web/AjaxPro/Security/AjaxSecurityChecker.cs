using System.Reflection;

namespace AjaxPro.Security
{
    public class AjaxSecurityChecker
    {
        public delegate bool CheckMethodPermissionsDelegate(MethodInfo method);

        public event CheckMethodPermissionsDelegate CheckMethodPermissions = null;

        internal bool OnCheckMethodPermissions(MethodInfo method)
        {
            CheckMethodPermissionsDelegate handler = CheckMethodPermissions;
            if (handler != null) return handler(method);
            return false;
        }

        private AjaxSecurityChecker()
        {
            
        }

        private static AjaxSecurityChecker _instance = null;

        public static AjaxSecurityChecker Instance
        {
            get
            {
                if (_instance==null)
                {
                    _instance = new AjaxSecurityChecker();
                }
                return _instance;
            }
        }
    }
}