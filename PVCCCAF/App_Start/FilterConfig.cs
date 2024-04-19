using System.Web;
using System.Web.Mvc;
using static CCAF.MvcApplication;

namespace CCAF
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MyAuthorizeAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new SessionExpireAttribute());
        }
    }
}
