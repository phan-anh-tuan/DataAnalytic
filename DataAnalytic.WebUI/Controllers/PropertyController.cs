using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.IO;

using DataAnalytic.Domain.Entities;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Concrete;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.App_Start;


using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace DataAnalytic.WebUI.Controllers
{
    public class PropertySearchCriteria
    {
        [DataType(DataType.Date)]
        public DateTime? FromDate { set; get; }
        [DataType(DataType.Date)]
        public DateTime? ToDate { set; get; }
        public string[] Suburb { set; get; }
        public string[] Type { set; get; }
        public int? MinNoOfBedroom { set; get; }
        public int? MaxNoOfBedroom { set; get; }
        public decimal? MinPrice { set; get; }
        public decimal? MaxPrice { set; get; }
        public string City { set; get; }
    }

    public class PropertyController : Controller
    {
        private IObjectRepository<AuctionResult> repository;
        private IUnityContainer container = UnityConfig.GetConfiguredContainer();
        private LogWriter logWriter = LoggingUtility.LogWriter;

        public PropertyController(IObjectRepository<AuctionResult> auctionResultRepository)
        {
            repository = auctionResultRepository;
        }
        //
        // GET: /Property/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Users="quyennguyen,tuanphan")]
        public ActionResult Search(PropertySearchCriteria search)
        {

            var Suburbs = new List<string>();

            var suburbQry = from result in repository.GetDataSet
                            group result by result.Suburb into g
                            select g.Key;

            List<string> lstSuburbs = suburbQry.ToList<string>();
            lstSuburbs.Insert(0, "All");
            ViewBag.Suburb = new SelectList(lstSuburbs);

            ViewBag.Type = new SelectList(new string[] { "house", "unit", "town house" });

            var cities = new List<string>();

            var cityQry = from result in repository.GetDataSet
                          group result by result.City into g
                          select g.Key;
            ViewBag.City = new SelectList(cityQry);
            return View(Find(search));
        }

        public ActionResult ImportData()
        {
            DirectoryInfo[] dirs = new DirectoryInfo(Path.Combine(DataAnalytic.WebUI.Utility.File.FileUtility.BaseFilePath, "HomePriceGuide")).GetDirectories();
            IEnumerable<string> query = from dir in dirs
                                        select dir.Name;
            return View(query.ToArray<string>());
        }

        [HttpPost]
        public bool ImportData(string containerFolder)
        {
            try
            {
                DataAnalytic.WebUI.Business.Concrete.REAuctionResultDownloader processor = new DataAnalytic.WebUI.Business.Concrete.REAuctionResultDownloader();

                string[] datepart = containerFolder.Split('-');
                if (datepart.Length == 3)
                {
                    processor.Persisting(new DateTime(int.Parse(datepart[0]), int.Parse(datepart[1]), int.Parse(datepart[2])));
                    if (!Request.IsLocal)
                    {
                        DirectoryInfo folder = new DirectoryInfo(Path.Combine(DataAnalytic.WebUI.Utility.File.FileUtility.BaseFilePath, "HomePriceGuide", containerFolder));
                        if (folder != null)
                        {
                            folder.Delete(true);
                        }
                    }
                }
                else
                {
                    throw new Exception("invalid container path");
                }

                return true;
            }
            catch (Exception ex)
            {
                logWriter.Write(string.Format("Error {0}", ex.Message));
                return false;
            }
        }

        #region private functions
        private List<AuctionResult> Find(PropertySearchCriteria search)
        {
            IQueryable queryableData = repository.GetDataSet;

            ParameterExpression pe = Expression.Parameter(typeof(AuctionResult), "auctionResult");
            Expression predicateBody = Expression.Equal(Expression.Constant("1"), Expression.Constant("1"));
            bool isFilter = false;

            Expression left;
            Expression right;
            Expression fromDateE;
            Expression toDateE;
            Expression suburbE;
            Expression typeE;
            Expression minNoBedroomE;
            Expression maxNoBedroomE;
            Expression minPriceE;
            Expression maxPriceE;
            Expression cityE;

            if (search.FromDate != null)
            {
                left = Expression.Property(pe, "TransactionDate");
                right = Expression.Constant(search.FromDate);
                fromDateE = Expression.GreaterThanOrEqual(left, right);
                predicateBody = Expression.And(predicateBody, fromDateE);
                isFilter = true;
            }

            if (search.ToDate != null)
            {
                left = Expression.Property(pe, "TransactionDate");
                right = Expression.Constant(search.ToDate);
                toDateE = Expression.LessThanOrEqual(left, right);
                predicateBody = Expression.And(predicateBody, toDateE);
                isFilter = true;
            }

            if (search.Suburb != null && search.Suburb.Length > 0 && !search.Suburb.Contains<string>("All"))
            {
                Expression tmp = null;
                foreach (var s in search.Suburb)
                {
                    left = Expression.Property(pe, "Suburb");
                    right = Expression.Constant(s);
                    if (tmp == null) {
                        tmp = Expression.Equal(left, right);
                    } else{
                        suburbE = Expression.Equal(left, right);
                        tmp = Expression.OrElse(tmp,suburbE);
                    }
                }
                predicateBody = Expression.And(predicateBody, tmp);
                isFilter = true;
            }

            if (search.Type != null && search.Type.Length > 0)
            {
                Expression tmp = null;
                foreach (var t in search.Type)
                {
                    left = Expression.Property(pe, "Type");
                    right = Expression.Constant(t);
                    if (tmp == null)
                    {
                        tmp = Expression.Equal(left, right);
                    }
                    else
                    {
                        typeE = Expression.Equal(left, right);
                        tmp = Expression.OrElse(tmp, typeE);
                    }
                }
                predicateBody = Expression.And(predicateBody, tmp);
                isFilter = true;
            }

            if (search.MinNoOfBedroom > 0)
            {
                left = Expression.Property(pe, "NoOfBedroom");
                right = Expression.Constant(search.MinNoOfBedroom);
                minNoBedroomE = Expression.GreaterThanOrEqual(left, right);
                predicateBody = Expression.And(predicateBody, minNoBedroomE);
                isFilter = true;
            }

            if (search.MaxNoOfBedroom > 0)
            {
                left = Expression.Property(pe, "NoOfBedroom");
                right = Expression.Constant(search.MaxNoOfBedroom);
                maxNoBedroomE = Expression.LessThanOrEqual(left, right);
                predicateBody = Expression.And(predicateBody, maxNoBedroomE);
                isFilter = true;
            }

            if (search.MinPrice > 0)
            {
                left = Expression.Property(pe, "Price");
                right = Expression.Constant(search.MinPrice);
                minPriceE = Expression.GreaterThanOrEqual(left, Expression.Convert(right, left.Type));
                predicateBody = Expression.And(predicateBody, minPriceE);
                isFilter = true;
            }

            if (search.MaxPrice > 0)
            {
                left = Expression.Property(pe, "Price");
                right = Expression.Constant(search.MaxPrice);
                maxPriceE = Expression.LessThanOrEqual(left, Expression.Convert(right, left.Type));
                predicateBody = Expression.And(predicateBody, maxPriceE);
                isFilter = true;
            }

            if (!string.IsNullOrEmpty(search.City))
            {
                left = Expression.Property(pe, "City");
                right = Expression.Constant(search.City);
                cityE = Expression.Equal(left, right);
                predicateBody = Expression.And(predicateBody, cityE);
                isFilter = true;
            }


            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<AuctionResult, bool>>(predicateBody, new ParameterExpression[] { pe }));

            // Create an executable query from the expression tree.
            IQueryable<AuctionResult> queryVariable = queryableData.Provider.CreateQuery<AuctionResult>(whereCallExpression).OrderBy(p => p.Price);
            
            if (isFilter)
            {
                return queryVariable.ToList<AuctionResult>();
            }
            else
            {
                return new List<AuctionResult>();
            }
        }
        #endregion
    }
}
