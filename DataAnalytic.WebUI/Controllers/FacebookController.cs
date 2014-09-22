using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using Facebook;

namespace DataAnalytic.WebUI.Controllers
{
    public class FacebookController : Controller
    {
        //
        // GET: /Facebook/

        private const string CLIENT_ID = "614186898701622";
        private const string APP_SECRET = "469ef82bf952630dda9e2f422bceec7d";
        private const string KidPN_PAGE_ID = "1477271959197572";
        public ActionResult Index()
        {
            if (HttpRuntime.Cache["access_token"] == null)
            {
                string clientId = CLIENT_ID;

                string redirectUrl = "http://localhost:60819/Facebook/AuthCallback";

                string scope = "user_friends,read_friendlists,read_stream,manage_pages,publish_actions";
                //,manage_pages,,publish_actions
                string url = string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}", clientId, redirectUrl, scope);

                return new RedirectResult(url);
            }
            else
            {
                //string page_access_token = GetPageAccessToken();
                var client = new FacebookClient();

                client.AccessToken = GetPageAccessToken();
                
                string url = string.Format("/v2.1/{0}/feed", KidPN_PAGE_ID);
                dynamic result = client.Post(url, new { link = "https://www.youtube.com/watch?v=PqIDXEGnq-U", name = "The wheel on the bus", description = "Enjoy the movie and subcribe to my channel to view new movie everyday" });

                return View("abc");

            }
        }

        public ActionResult AuthCallback(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                GetAccessToken();
            }

            return RedirectToAction("Index");
        }

        #region private functions
        //private string GetAccessToken()
        //{
        //    if (HttpRuntime.Cache["access_token"] != null)
        //    {
        //        return HttpRuntime.Cache["access_token"].ToString();
        //    }
        //    return null;
        //}

        private string GetAccessToken()
        {

            if (HttpRuntime.Cache["access_token"] == null)
            {

                Dictionary<string, string> args = GetOauthTokens(Request.Params["code"]);

                //HttpRuntime.Cache.Insert("access_token", args["access_token"], null, DateTime.Now.AddMinutes(Convert.ToDouble(args["expires"])), TimeSpan.Zero);
                HttpRuntime.Cache.Insert("access_token", args["access_token"]);
            }
            return HttpRuntime.Cache["access_token"].ToString();
        }

        private string GetPageAccessToken()
        {
                string access_token = GetAccessToken();
                if (access_token != null)
                {
                    var client = new FacebookClient();

                    client.AccessToken = access_token;

                    string url = "/v2.1/me/accounts";
                    dynamic result = client.Get(url);
                    return result.data[0].access_token;
            
                }

                

            return null;
        }

        private Dictionary<string, string> GetOauthTokens(string accessCode)
        {

            Dictionary<string, string> tokens = new Dictionary<string, string>();

            string clientId = CLIENT_ID;

            string redirectUrl = "http://localhost:60819/Facebook/AuthCallback";

            string clientSecret = APP_SECRET;

            FacebookClient client = new FacebookClient();
            dynamic result = client.Get("oauth/access_token", new
            {
                client_id = clientId,
                redirect_uri = redirectUrl,
                client_secret = clientSecret,
                code = accessCode
            });

            tokens.Add("access_token", result["access_token"]);
            //tokens.Add("expires", result["expires"].ToString());

            return tokens;


        }
        #endregion
    }
}
