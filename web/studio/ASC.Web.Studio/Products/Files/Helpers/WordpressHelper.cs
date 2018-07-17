using System;
using ASC.FederatedLogin;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Files.Classes;
using ASC.Web.Files.ThirdPartyApp;
using log4net;
namespace ASC.Web.Files.Helpers
{
    public class WordpressToken
    {
        public static ILog Log = Global.Logger;
        public const string AppAttr = "wordpress";

        public static OAuth20Token GetToken()
        {
            return Token.GetToken(AppAttr);
        }

        public static void SaveToken(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");
            Token.SaveToken(new Token(token, AppAttr));
        }
        public static void DeleteToken(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");
            Token.DeleteToken(AppAttr);

        }
    }
    public class WordpressHelper
    {
        public static ILog Log = Global.Logger;
        public enum WordpressStatus
        {
            draft = 0,
            publish = 1
        }

        public static string GetWordpressMeInfo(string token)
        {
            try
            {
                return WordpressLoginProvider.GetWordpressMeInfo(token);
            }
            catch (Exception ex)
            {
                Log.Error("Get Wordpress info about me ", ex);
                return "";
            }
           
        }

        public static bool CreateWordpressPost(string title, string content, int status, string blogId, OAuth20Token token)
        {
            try
            {
                var wpStatus = ((WordpressStatus)status).ToString();
                WordpressLoginProvider.CreateWordpressPost(title, content, wpStatus, blogId, token);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Create Wordpress post ", ex);
                return false;
            }
        }
    }
}