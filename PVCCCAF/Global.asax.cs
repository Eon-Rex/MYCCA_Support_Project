using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using PVCCCAF.Models;

namespace CCAF
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            GlobalFilters.Filters.Add(new SessionExpireAttribute());
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");           //Remove Server Header   
            Response.Headers.Remove("X-AspNet-Version"); //Remove X-AspNet-Version Header
        }

        public class SessionExpireAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                HttpContext context = HttpContext.Current;
                // check  sessions here
                if (context.Request.RawUrl.ToUpper() == "/AUTHENTICATION/LOGIN" || context.Request.RawUrl == "/"
                    || context.Request.RawUrl.ToUpper() == "/?APPROVE=1" || context.Request.RawUrl.ToUpper() == "/AUTHENTICATION/LOGIN?APPROVE=1") //&& context.Request.RawUrl != "/?Approve=1" && context.Request.RawUrl != "/Authentication/Login?Approve=1"
                {
                    base.OnActionExecuting(filterContext);
                    return;
                    //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Authentication" },
                    //                    { "Action", "Login" } });
                    //return;
                }
                if (HttpContext.Current.Session["USERCODE"] == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Authentication" },
                                        { "Action", "Login" } });
                    return;
                }
                string actionName = filterContext.ActionDescriptor.ActionName;
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                if (controllerName + actionName != "")
                {
                    HttpContext.Current.Session["ROLECODE"].ToString();
                    DataTable data = (DataTable)HttpContext.Current.Session["MENULIST"];
                    DataRow[] dataPerDay = (from myRow in data.AsEnumerable()
                                            where myRow.Field<string>("ROLECODE") == HttpContext.Current.Session["ROLECODE"].ToString()
                                            && myRow.Field<string>("CONTROLER") == controllerName
                                            && myRow.Field<string>("ACTION") == actionName
                                            select myRow).ToArray();
                    if (dataPerDay.Count() == 0)
                    {
                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.HttpContext.Response.StatusCode = 401;
                            filterContext.Result = new JsonResult
                            {
                                Data = new
                                {
                                    Error = "NotAuthorized",
                                    LogOnUrl = "/"
                                },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                            filterContext.HttpContext.Response.End();
                        }
                        else
                        {
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Authentication" },
                                        { "Action", "Login" } });
                        }

                        return;
                    }
                    //HttpContext.Current.Session["ROLECODE"].ToString();
                    //var data = (List<WCCONNECT.Models.RoleD>)HttpContext.Current.Session["CartProducts"];
                    //var result = from i in data
                    //             where i.Role == HttpContext.Current.Session["ROLECODE"].ToString()
                    //             && i.Action == actionName && i.Controller == controllerName
                    //             select i;
                    //if (!result.Any())
                    //{
                    //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{ "Controller", "Authentication" },
                    //                    { "Action", "Dashboard" } });
                    //    return;
                    //}
                }
                AuditTB objaudit = new AuditTB();
                //Getting Action Name
                //string actionName = filterContext.ActionDescriptor.ActionName;
                //Getting Controller Name
                //string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var request = filterContext.HttpContext.Request;
                if (HttpContext.Current.Session["USERCODE"] == null) // For Checking User is Logged in or Not 
                {
                    objaudit.UserID = "0";
                }
                else
                {
                    objaudit.UserID = HttpContext.Current.Session["USERCODE"].ToString();
                }
                objaudit.UsersAuditID = DateTime.Now;
                objaudit.SessionID = HttpContext.Current.Session.SessionID; // Application SessionID
                                                                            // User IPAddress
                objaudit.IPAddress =
                request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;
                objaudit.PageAccessed = request.RawUrl;  // URL User Requested
                objaudit.LoggedInAt = DateTime.Now;      // Time User Logged In || And time User Request Method
                if (actionName == "LOGIN")
                {
                    objaudit.LoggedOutAt = DateTime.Now; // Time User Logged OUT
                }
                objaudit.LoginStatus = "A";
                objaudit.ControllerName = controllerName; // ControllerName
                objaudit.ActionName = actionName;         // ActionName
                try
                {
                    PVCCCAF.DataAccessLayer.DataAccessLayer.SaveUserAuditDataToDB(objaudit); // Saving in database using Stored Procedures
                }
                catch
                {

                }
                //appcontext.AuditTBs.Add(objaudit);
                //appcontext.SaveChanges();                    // Saving in database using Entity Framework

                base.OnActionExecuting(filterContext);
            }
        }
        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            if (arg.Equals("User", StringComparison.InvariantCultureIgnoreCase))
            {
                //CUST/00011
                var user = context.User.Identity.Name; // TODO here you have to pick an unique identifier from the current user identity
                return string.Format("{0}@{1}", HttpContext.Current.Session["USERID"].ToString(), HttpContext.Current.Session["USERID"].ToString());
            }

            return base.GetVaryByCustomString(context, arg);
        }

        /// <summary>
        /// Extend AuthorizeAttribute to correctly handle AJAX authorization
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class MyAuthorizeAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            Error = "NotAuthorized",
                            LogOnUrl = "/"
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    // this is a standard request, let parent filter to handle it
                    base.HandleUnauthorizedRequest(filterContext);
                }

                //            getData = function(request) {
                //$.ajax({
                //                    url: “/ home / GetData”,
                //    type: “POST”,
                //    data: request,
                //    contentType: “application / json;”,
                //    dataType: “json”,
                //    success: function(repJSON) {
                //                    },
                //    error: function(xhr) {
                //                        if (xhr.status === 401)
                //                        {
                //                            window.location.href = xhr.Data.LogOnUrl;
                //                            return;
                //                        }
                //                    }
                //                });
            }
        }
    }
}
