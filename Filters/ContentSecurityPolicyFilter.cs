using System.Web.Mvc;

namespace ASP.NET_Web_Application_with_Vite__.NET_Framework_.Filters
{
    /// <summary>
    /// Adds a Content-Security-Policy header to every MVC response.
    /// </summary>
    public class ContentSecurityPolicyFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            var csp = string.Join(" ",
                "default-src 'self';",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval';",
                "style-src 'self' 'unsafe-inline';",
                "img-src 'self' data: https:;",
                "font-src 'self';",
                "connect-src 'self';",
                "frame-ancestors 'none';",
                "form-action 'self';"
            );

            response.Headers.Set("Content-Security-Policy", csp);

            base.OnResultExecuting(filterContext);
        }
    }
}
