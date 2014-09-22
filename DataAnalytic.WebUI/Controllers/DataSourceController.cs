using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

using DataAnalytic.Domain.Entities;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.WebUI.Business.Abstract;
using DataAnalytic.WebUI.Business.Concrete;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.Utility.File;
using DataAnalytic.WebUI.App_Start;

using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using HtmlAgilityPack;



namespace DataAnalytic.WebUI.Controllers
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
    
    public class DataSourceController : Controller
    {
        private IObjectRepository<DataSource> repository;
        private IUnityContainer container = UnityConfig.GetConfiguredContainer();
        private LogWriter logWriter = LoggingUtility.LogWriter;

        // GET: /DataSource/

        public DataSourceController(IObjectRepository<DataSource> dataSourceRepository)
        {
            repository = dataSourceRepository;


            //string filepath = "C:\\Temp\\www.abc.net.au\\2014-August-05\\show.htm";

            //HtmlDocument doc = new HtmlDocument();
            //doc.Load(filepath);

            //foreach (HtmlNode scriptNodes in doc.DocumentNode.SelectNodes("//script"))
            //{
            //    //logWriter.Write(string.Format("ABC4Kids script\n {0}", scriptNodes.InnerText));
            //    string s = scriptNodes.InnerText;
            //    //string sPattern = @"var\s*videoClip\s*=\s*\{(\w\W)*\}";
            //    string sPattern = @"http:[\w\W]*.mp4";
            //    foreach (Match match in Regex.Matches(s, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            //    {
            //        logWriter.Write(string.Format("Video URL = {0}", match.Value));
            //    }
            //}
        }

        public ActionResult Index()
        {

            Company[] companies = { new Company {Name = "Consolidated Messenger"}, 
                                    new Company {Name = "Alpine Ski House"}, 
                                    new Company {Name = "Southridge Video"}, 
                                    new Company {Name = "City Power & Light"},
                               new Company {Name = "Coho Winery"}, 
                               new Company {Name = "Wide World Importers"}, 
                               new Company {Name = "Graphic Design Institute"}, 
                               new Company {Name = "Adventure Works"},
                               new Company {Name = "Humongous Insurance"}, 
                               new Company {Name = "Woodgrove Bank"}, 
                               new Company {Name = "Margie's Travel"}, 
                               new Company {Name = "Northwind Traders"},
                               new Company {Name = "Blue Yonder Airlines"}, 
                               new Company {Name = "Trey Research"}, 
                               new Company {Name = "The Phone Company"},
                               new Company {Name = "Wingtip Toys"}, 
                               new Company {Name = "Lucerne Publishing"}, 
                               new Company {Name = "Fourth Coffee"}};



            // The IQueryable data to query.
            IQueryable<Company> queryableData = companies.AsQueryable<Company>();

            //IQueryable<Company> anotherQueryVariable = queryableData.Where(company => ((company.get_Name().ToLower() == "coho winery") || (company.get_Name().Length > 16))).OrderBy(company => company.get_Name());

            //foreach (Company c in anotherQueryVariable)
            //    Console.WriteLine(c.Name);

            // Compose the expression tree that represents the parameter to the predicate.
            ParameterExpression pe = Expression.Parameter(typeof(Company), "company");

            // ***** Where(company => (company.ToLower() == "coho winery" || company.Length > 16)) *****
            // Create an expression tree that represents the expression 'company.ToLower() == "coho winery"'.
            //System.Reflection.MethodInfo[] methods = typeof(Company).GetMethods();
            Expression tmp = Expression.Property(pe,"Name");
            Expression left = Expression.Call(tmp, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            Expression right = Expression.Constant("coho winery");
            Expression e1 = Expression.Equal(left, right);

            // Create an expression tree that represents the expression 'company.Length > 16'.
            left = Expression.Property(tmp, typeof(string).GetProperty("Length"));
            right = Expression.Constant(16, typeof(int));
            Expression e2 = Expression.GreaterThan(left, right);

            // Combine the expression trees to create an expression tree that represents the 
            // expression '(company.ToLower() == "coho winery" || company.Length > 16)'.
            Expression predicateBody = Expression.OrElse(e1, e2);

            // Create an expression tree that represents the expression 
            // 'queryableData.Where(company => (company.ToLower() == "coho winery" || company.Length > 16))'
            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<Company, bool>>(predicateBody, new ParameterExpression[] { pe }));
            // ***** End Where ***** 


            // ***** OrderBy(company => company) ***** 
            // Create an expression tree that represents the expression 
            // 'whereCallExpression.OrderBy(company => company)'
            //Queryable.OrderBy
            MethodCallExpression orderByCallExpression = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new Type[] { queryableData.ElementType, typeof(string) },
                whereCallExpression,
                Expression.Lambda<Func<Company, string>>(Expression.Property(pe, "Name"), new ParameterExpression[] { pe }));
            // ***** End OrderBy ***** 

            // Create an executable query from the expression tree.
            IQueryable<Company> results = queryableData.Provider.CreateQuery<Company>(orderByCallExpression);

            // Enumerate the results. 
            foreach (Company company in results)
                Console.WriteLine(company.Name);

            //string path = FileUtility.BaseFilePath;
           return View(repository.GetDataSet);
        }

        public PartialViewResult GetUpdateStatus(string URL)
        {
            try
            {
                if (Uri.IsWellFormedUriString(URL, System.UriKind.RelativeOrAbsolute))
                {
                    Uri uri = new Uri(URL);

                    //Get the Processor specific to the URL if one exists otherwise get the default processor
                    IURLProcessor fileProcessor;
                    if (container.IsRegistered<IURLProcessor>(uri.Host))
                    {
                        fileProcessor = container.Resolve<IURLProcessor>(uri.Host);
                    }
                    else
                    {
                        fileProcessor = container.Resolve<IURLProcessor>("default");
                    }

                    fileProcessor.Process(URL);
                    repository.UpdateDate(URL);

                    return PartialView(new string[] { "Completed" });

                }
                else
                {
                    return PartialView(new string[] { "Invalid URL !!!" });
                }
            }
            catch (Exception ex)
            {

                logWriter.Write(ex.Message);
                return PartialView(new string[] { "Error !!!" });
            }
        }
    }
}
