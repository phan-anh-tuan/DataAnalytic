using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnalytic.WebUI.Infrastructure;

namespace DataAnalytic.WebUI.Controllers
{
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(); }
        }
    }
}