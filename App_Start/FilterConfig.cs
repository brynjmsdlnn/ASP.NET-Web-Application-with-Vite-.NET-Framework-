using System.Web.Mvc;
using ASP.NET_Web_Application_with_Vite__.NET_Framework_.Filters;

namespace ASP.NET_Web_Application_with_Vite__.NET_Framework_
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ContentSecurityPolicyFilter());
        }
    }
}
