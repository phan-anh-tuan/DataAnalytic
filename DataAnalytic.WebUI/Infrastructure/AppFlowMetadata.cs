using System;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.Util.Store;

using DataAnalytic.WebUI.Utility;

namespace DataAnalytic.WebUI.Infrastructure
{
    public class AppFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    /**************************************
                     * for production environment
                     * ***********************************/
                    //ClientId = "786675496571-soeimjp5ku3lrbsbr9bhqi6ur78u2klv.apps.googleusercontent.com",
                    //ClientSecret = "5RoG95A8O2DAykZHagRJOfOw"
                    /**************************************
                     * for development environment
                     * ***********************************/
                    ClientId = AppConfiguration.GoogleClientId,
                    ClientSecret = AppConfiguration.GoogleClientSecret
                },
                Scopes = new[] { YouTubeService.Scope.Youtube, YouTubeService.Scope.YoutubeUpload },
                DataStore = new FileDataStore(System.IO.Path.Combine(DataAnalytic.WebUI.Utility.File.FileUtility.BaseFilePath,"YouTubeService.Api.Auth.Store"))
            });

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }
            return user.ToString();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }
    }
}