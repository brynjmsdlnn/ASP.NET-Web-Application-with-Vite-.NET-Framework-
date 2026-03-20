using System;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace ASP.NET_Web_Application_with_Vite__.NET_Framework_.Filters
{
    /// <summary>
    /// Adds a per-request nonce-based Content-Security-Policy header to MVC responses.
    /// </summary>
    public class ContentSecurityPolicyFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Shared key used to store the generated nonce in <c>HttpContext.Items</c>.
        /// </summary>
        public const string NonceKey = "CSP_NONCE";

        /// <summary>
        /// Generates a cryptographically secure nonce for the current request and stores it
        /// for later retrieval by views and the result filter.
        /// </summary>
        /// <param name="filterContext">The current action execution context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nonce = GenerateNonce();
            filterContext.HttpContext.Items[NonceKey] = nonce;
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Builds and writes the CSP response header using the nonce generated for this request.
        /// </summary>
        /// <param name="filterContext">The current result execution context.</param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var nonce = filterContext.HttpContext.Items[NonceKey] as string;

            var csp = string.Join(" ",
                "default-src 'self';",
                $"script-src 'self' 'nonce-{nonce}';",
                $"style-src 'self' 'nonce-{nonce}';",
                "img-src 'self' data: https:;",
                "font-src 'self';",
                "connect-src 'self';",
                "frame-ancestors 'none';",
                "form-action 'self';"
            );

            filterContext.HttpContext.Response.Headers.Set("Content-Security-Policy", csp);

            base.OnResultExecuting(filterContext);
        }

        private static string GenerateNonce()
        {
            var bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }
    }
}
