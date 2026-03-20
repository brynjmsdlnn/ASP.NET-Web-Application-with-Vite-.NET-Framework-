using ASP.NET_Web_Application_with_Vite__.NET_Framework_.Middleware;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace ASP.NET_Web_Application_with_Vite__.NET_Framework_.Helpers
{
    /// <summary>
    /// Razor HTML helper that emits Vite asset tags from either the dev server or
    /// the production manifest, mirroring Laravel's <c>@vite()</c> directive.
    /// </summary>
    public static class ViteHelper
    {
        // ─── Constants ────────────────────────────────────────────────────────────

        private static readonly string[] CssExtensions = { ".css", ".scss", ".sass", ".less", ".styl" };

        // ─── Manifest cache (process-wide) ────────────────────────────────────────

        private static readonly object ManifestLock = new object();
        private static Dictionary<string, ManifestEntry> _manifest;

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Emits <c>&lt;script&gt;</c> and <c>&lt;link&gt;</c> tags for the supplied entry paths.
        /// In development, tags point to the Vite dev server.
        /// In production, tags are resolved from <c>manifest.json</c>.
        /// </summary>
        /// <example>
        /// <code>@Html.Vite("Styles/app.css", "Scripts/main.js")</code>
        /// </example>
        public static IHtmlString Vite(this HtmlHelper html, params string[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return new HtmlString(string.Empty);
            }

            return ViteMiddleware.IsEnabled(HttpContext.Current)
                ? RenderDevTags(entries)
                : RenderProdTags(entries);
        }

        // ─── Dev rendering ────────────────────────────────────────────────────────

        private static IHtmlString RenderDevTags(string[] entries)
        {
            string origin = ViteMiddleware.GetViteOrigin();
            StringBuilder sb = new StringBuilder();

            // Vite HMR client must always be first
            sb.AppendLine(ScriptTag(origin + "/@vite/client"));

            foreach (string entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                string path = "/" + NormalizePath(entry);

                if (IsCssEntry(entry))
                    sb.AppendLine(LinkTag(origin + path));   // <link> prevents FOUC
                else
                    sb.AppendLine(ScriptTag(origin + path));
            }

            return new HtmlString(sb.ToString());
        }

        // ─── Production rendering ─────────────────────────────────────────────────

        private static IHtmlString RenderProdTags(string[] entries)
        {
            Dictionary<string, ManifestEntry> manifest = GetManifest();
            StringBuilder sb = new StringBuilder();
            HashSet<string> emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                ManifestEntry asset;
                if (!TryGetManifestEntry(manifest, entry, out asset))
                    continue;

                // Emit any associated CSS chunks first
                if (asset.Css != null)
                {
                    foreach (string css in asset.Css)
                    {
                        if (!string.IsNullOrWhiteSpace(css) && emitted.Add(NormalizePath(css)))
                            sb.AppendLine(LinkTag(AssetUrl(css)));
                    }
                }

                // Emit the entry asset itself
                if (IsCssEntry(entry) || IsCssEntry(asset.File))
                {
                    if (emitted.Add(NormalizePath(asset.File)))
                        sb.AppendLine(LinkTag(AssetUrl(asset.File)));
                }
                else
                {
                    if (emitted.Add(NormalizePath(asset.File)))
                        sb.AppendLine(ScriptTag(AssetUrl(asset.File)));
                }
            }

            return new HtmlString(sb.ToString());
        }

        // ─── Manifest ─────────────────────────────────────────────────────────────

        private static Dictionary<string, ManifestEntry> GetManifest()
        {
            if (_manifest != null) return _manifest;

            lock (ManifestLock)
            {
                if (_manifest != null) return _manifest;

                if (HttpContext.Current?.Server == null)
                    throw new InvalidOperationException(
                        "Cannot resolve the Vite manifest without an active HTTP context.");

                string manifestPath = HttpContext.Current.Server.MapPath(ManifestVirtualPath());

                if (!File.Exists(manifestPath))
                    throw new FileNotFoundException(
                        "Vite manifest not found. Run 'npm run build' to generate it.", manifestPath);

                string json = File.ReadAllText(manifestPath);
                Dictionary<string, ManifestEntry> raw =
                    JsonConvert.DeserializeObject<Dictionary<string, ManifestEntry>>(json)
                    ?? new Dictionary<string, ManifestEntry>();

                _manifest = new Dictionary<string, ManifestEntry>(raw, StringComparer.OrdinalIgnoreCase);
            }

            return _manifest;
        }

        /// <summary>
        /// Resolves a manifest entry using tolerant path matching:
        /// exact key → stripped prefix → leading-slash variants → filename fallback → src fallback.
        /// </summary>
        private static bool TryGetManifestEntry(
            Dictionary<string, ManifestEntry> manifest,
            string entry,
            out ManifestEntry asset)
        {
            string normalized = NormalizePath(entry);
            string fileName = Path.GetFileName(normalized);

            // 1. Exact match (with and without leading slash)
            if (manifest.TryGetValue(normalized, out asset) ||
                manifest.TryGetValue("/" + normalized, out asset))
                return true;

            // 2. Fuzzy scan — filename or src match
            foreach (KeyValuePair<string, ManifestEntry> kvp in manifest)
            {
                string key = NormalizePath(kvp.Key);
                string keyFileName = Path.GetFileName(key);

                bool keyMatch = string.Equals(key, normalized, StringComparison.OrdinalIgnoreCase)
                             || (!string.IsNullOrEmpty(fileName) &&
                                 string.Equals(keyFileName, fileName, StringComparison.OrdinalIgnoreCase));

                if (keyMatch) { asset = kvp.Value; return true; }

                if (kvp.Value?.Src != null)
                {
                    string src = NormalizePath(kvp.Value.Src);
                    string srcFileName = Path.GetFileName(src);

                    bool srcMatch = string.Equals(src, normalized, StringComparison.OrdinalIgnoreCase)
                                 || (!string.IsNullOrEmpty(fileName) &&
                                     string.Equals(srcFileName, fileName, StringComparison.OrdinalIgnoreCase));

                    if (srcMatch) { asset = kvp.Value; return true; }
                }
            }

            asset = null;
            return false;
        }

        // ─── URL helpers ──────────────────────────────────────────────────────────

        private static string AssetUrl(string relativeAsset)
        {
            string distPath = DistPath().Trim('/');
            string assetPath = NormalizePath(relativeAsset).TrimStart('/');
            string appBase = AppBasePath();

            return string.IsNullOrEmpty(appBase)
                ? $"/{distPath}/{assetPath}"
                : $"{appBase}/{distPath}/{assetPath}";
        }

        private static string AppBasePath()
        {
            string appPath = HttpContext.Current?.Request?.ApplicationPath;
            if (string.IsNullOrEmpty(appPath) || appPath == "/") return string.Empty;
            return appPath.TrimEnd('/');
        }

        private static string DistPath()
        {
            string path = ConfigurationManager.AppSettings["ViteDistPath"];
            return string.IsNullOrWhiteSpace(path) ? "wwwroot/dist" : path.Trim().Trim('~', '/');
        }

        private static string ManifestVirtualPath()
        {
            return "~/" + DistPath().Replace("\\", "/").Trim('/') + "/.vite/manifest.json";
        }

        // ─── Tag builders ─────────────────────────────────────────────────────────

        private static string LinkTag(string href)
            => $"<link rel=\"stylesheet\" href=\"{href}\" />";

        private static string ScriptTag(string src)
            => $"<script type=\"module\" src=\"{src}\"></script>";

        // ─── Utility ──────────────────────────────────────────────────────────────

        private static bool IsCssEntry(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return CssExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            string normalized = path.Replace("\\", "/").Trim();
            normalized = normalized.TrimStart('~', '/');

            if (normalized.StartsWith("./", StringComparison.Ordinal))
                normalized = normalized.Substring(2);

            return normalized;
        }

        // ─── Manifest model ───────────────────────────────────────────────────────

        private class ManifestEntry
        {
            [JsonProperty("file")] public string File { get; set; }
            [JsonProperty("src")] public string Src { get; set; }
            [JsonProperty("isEntry")] public bool IsEntry { get; set; }
            [JsonProperty("css")] public List<string> Css { get; set; }
            [JsonProperty("imports")] public List<string> Imports { get; set; }
        }
    }
}