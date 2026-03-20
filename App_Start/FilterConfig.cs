using System.Web.Mvc;
using $safeprojectname$.Filters;

namespace $safeprojectname$
{
    /// <summary>
    /// Registers global MVC filters for the application.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Adds global filters used by every request.
        /// </summary>
        /// <param name="filters">The MVC global filter collection.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ContentSecurityPolicyFilter());
        }
    }
}

