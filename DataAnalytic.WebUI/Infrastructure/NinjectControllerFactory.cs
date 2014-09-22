using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnalytic.Domain.Concrete;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;
using Ninject;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace DataAnalytic.WebUI.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        IKernel ninjectKernel;
        public NinjectControllerFactory() {
            ninjectKernel = new StandardKernel();
            AddBindings();
        }
         protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            //return base.GetControllerInstance(requestContext, controllerType);
            return controllerType == null ? null : (IController)ninjectKernel.Get(controllerType);
        }
        public void AddBindings()
        {
             //ninjectKernel.Bind<IObjectRepository<DataSource>>().To<EFDataSourceRepository>();
        }
    }
}