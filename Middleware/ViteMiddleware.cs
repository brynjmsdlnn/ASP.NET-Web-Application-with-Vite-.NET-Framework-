using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace ASP.NET_Web_Application_with_Vite__.NET_Framework_.Middleware
{
    /// <summary>
    /// Detects whether the local Vite dev server is reachable and stores the result
    /// in <see cref="HttpContext.Items"/> so Razor views can decide which asset source to load.
    /// </summary>
    public class ViteMiddleware : IHttpModule
    {
        // ─── Constants ────────────────────────────────────────────────────────────

        private const string ContextItemKey = "UseViteDevServer";
        private const string ViteServerProbePath = "/@vite/client";
        private const string DefaultViteOrigin = "http://localhost:5173";
        private const int ProbeTimeoutMilliseconds = 250;

        // ─── Probe cache (process-wide) ───────────────────────────────────────────

        private static readonly object CacheLock = new object();
        private static readonly TimeSpan CacheWindow = TimeSpan.FromSeconds(2);
        private static DateTime _lastProbeUtc = DateTime.MinValue;
        private static bool _lastProbeResult;

        // ─── IHttpModule ──────────────────────────────────────────────────────────

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
        }

        /// <inheritdoc />
        public void Dispose() { }

        // ─── Request handler ──────────────────────────────────────────────────────

        private static void OnBeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app == null) return;

            app.Context.Items[ContextItemKey] = ResolveDevServerEnabled(app.Context);
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the Vite dev server flag is enabled in
        /// <c>Web.config</c> and the dev server is currently reachable.
        /// Reads the cached result from <see cref="HttpContext.Items"/> when available.
        /// </summary>
        public static bool IsEnabled(HttpContext context)
        {
            if (!DevServerAllowed()) return false;

            if (context != null)
            {
                bool? cached = context.Items[ContextItemKey] as bool?;
                if (cached.HasValue) return cached.Value;
            }

            return ProbeDevServer();
        }

        /// <summary>
        /// Returns the Vite dev server origin defined in <c>Web.config</c>,
        /// falling back to <c>http://localhost:5173</c>.
        /// </summary>
        public static string GetViteOrigin()
        {
            string origin = ConfigurationManager.AppSettings["ViteDevServerOrigin"];
            return string.IsNullOrWhiteSpace(origin) ? DefaultViteOrigin : origin.TrimEnd('/');
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private static bool ResolveDevServerEnabled(HttpContext context)
        {
            if (!DevServerAllowed()) return false;
            return ProbeDevServer();
        }

        private static bool DevServerAllowed()
        {
            bool allowed;
            bool.TryParse(ConfigurationManager.AppSettings["UseViteDevServer"], out allowed);
            return allowed;
        }

        /// <summary>
        /// Sends a HEAD request to the Vite client endpoint.
        /// Results are cached for <see cref="CacheWindow"/> to avoid probing on every request.
        /// </summary>
        private static bool ProbeDevServer()
        {
            DateTime now = DateTime.UtcNow;

            // Fast path — return cached result without locking
            if ((now - _lastProbeUtc) < CacheWindow) return _lastProbeResult;

            lock (CacheLock)
            {
                // Re-check inside lock (double-checked locking)
                now = DateTime.UtcNow;
                if ((now - _lastProbeUtc) < CacheWindow) return _lastProbeResult;

                _lastProbeResult = TryReachViteServer();
                _lastProbeUtc = now;
            }

            return _lastProbeResult;
        }

        private static bool TryReachViteServer()
        {
            try
            {
                string url = GetViteOrigin() + ViteServerProbePath;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = ProbeTimeoutMilliseconds;
                request.ReadWriteTimeout = ProbeTimeoutMilliseconds;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    int status = (int)response.StatusCode;
                    return status >= 200 && status < 400;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}