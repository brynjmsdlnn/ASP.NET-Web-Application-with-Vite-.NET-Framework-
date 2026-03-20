using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace $safeprojectname$.Filters
{
    /// <summary>
    /// Adds a per-request nonce-based Content-Security-Policy header to MVC responses.
    /// </summary>
    public class ContentSecurityPolicyFilter : ActionFilterAttribute
    {
        public const string NonceKey = "CSP_NONCE";
        public const string ExtraScriptSourceKey = "CSP_EXTRA_SCRIPT_SRC";
        public const string ExtraStyleSourceKey = "CSP_EXTRA_STYLE_SRC";
        public const string ExtraConnectSourceKey = "CSP_EXTRA_CONNECT_SRC";

        #region Generate nonce early so views can read it

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.HttpContext;
            ctx.Items[NonceKey] = GenerateNonce();
            RegisterViteCspSources(ctx);
            base.OnActionExecuting(filterContext);
        }

        #endregion

        #region Build and set CSP header before response flushes

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var ctx = filterContext.HttpContext;
            var nonce = ctx.Items[NonceKey] as string;

            var extraScript = ctx.Items[ExtraScriptSourceKey] as string;
            var extraStyle = ctx.Items[ExtraStyleSourceKey] as string;
            var extraConnect = ctx.Items[ExtraConnectSourceKey] as string;

            var csp = string.Join(" ",
                "default-src 'self';",
                BuildDirective("script-src", nonce, extraScript),
                BuildDirective("script-src-elem", nonce, extraScript),
                BuildDirective("style-src", nonce, extraStyle),
                BuildDirective("style-src-elem", nonce, extraStyle),
                "img-src 'self' data: https:;",
                "font-src 'self';",
                BuildConnectDirective(extraConnect),
                "frame-ancestors 'none';",
                "form-action 'self';"
            );

            ctx.Response.Headers.Set("Content-Security-Policy", csp);

            base.OnResultExecuting(filterContext);
        }

        #endregion

        #region Vite CSP registration

        private static void RegisterViteCspSources(HttpContextBase ctx)
        {
            if (ctx == null) return;

            var useDevServer = string.Equals(
                ConfigurationManager.AppSettings["UseViteDevServer"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (!useDevServer) return;

            var origin = ConfigurationManager.AppSettings["ViteDevServerOrigin"];
            if (string.IsNullOrWhiteSpace(origin)) return;

            var wsOrigin = ToWebSocketOrigin(origin);
            var connectSrc = string.IsNullOrWhiteSpace(wsOrigin)
                ? origin
                : $"{origin} {wsOrigin}";

            ctx.Items[ExtraScriptSourceKey] = origin;
            ctx.Items[ExtraStyleSourceKey] = origin;
            ctx.Items[ExtraConnectSourceKey] = connectSrc;
        }

        private static string ToWebSocketOrigin(string origin)
        {
            if (origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return "wss://" + origin.Substring("https://".Length);

            if (origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return "ws://" + origin.Substring("http://".Length);

            return string.Empty;
        }

        #endregion

        #region Directive builders

        private static string BuildDirective(string directive, string nonce, string extraSources)
        {
            var sources = new List<string> { "'self'" };

            if (!string.IsNullOrWhiteSpace(nonce))
            {
                sources.Add($"'nonce-{nonce}'");
            }

            AppendExtraSources(sources, extraSources);

            return $"{directive} {string.Join(" ", sources)};";
        }

        private static string BuildConnectDirective(string extraSources)
        {
            var sources = new List<string> { "'self'" };
            AppendExtraSources(sources, extraSources);
            return $"connect-src {string.Join(" ", sources)};";
        }

        private static void AppendExtraSources(List<string> sources, string extraSources)
        {
            if (string.IsNullOrWhiteSpace(extraSources))
            {
                return;
            }

            foreach (var source in extraSources.Split(
                new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                sources.Add(source);
            }
        }

        #endregion

        #region Nonce generation

        private static string GenerateNonce()
        {
            var bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        #endregion
    }
}
