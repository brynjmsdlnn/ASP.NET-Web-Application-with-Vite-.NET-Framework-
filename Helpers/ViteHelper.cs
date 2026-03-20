using $safeprojectname$.Middleware;
using $safeprojectname$.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace $safeprojectname$.Helpers
{
    /// <summary>
    /// Razor HTML helper that emits Vite asset tags from either the dev server or
    /// the production manifest, mirroring Laravel's <c>@vite()</c> directive.
    /// </summary>
    public static class ViteHelper
    {
        #region Constants

        private static readonly string[] CssExtensions = { ".css", ".scss", ".sass", ".less", ".styl" };

        #endregion

        #region Manifest cache (process-wide)

        private static readonly object ManifestLock = new object();
        private static Dictionary<string, ManifestEntry> _manifest;

        #endregion

        #region Public API

        /// <summary>
        /// Emits script and link tags for the supplied entry paths.
        /// In development, tags point to the Vite dev server.
        /// In production, tags are resolved from <c>manifest.json</c>.
        /// </summary>
        /// <example>
        /// <code>@Html.Vite("Styles/app.css", "Scripts/app.js")</code>
        /// </example>
        public static IHtmlString Vite(this HtmlHelper html, params string[] entries)
        {
            if (entries == null || entries.Length == 0)
                return new HtmlString(string.Empty);

            if (ViteMiddleware.IsEnabled(HttpContext.Current))
            {
                return RenderDevTags(entries);
            }

            return RenderProdTags(entries);
        }

        #endregion

        #region Dev rendering

        private static IHtmlString RenderDevTags(string[] entries)
        {
            var origin = ViteMiddleware.GetViteOrigin();
            var sb = new StringBuilder();

            // Vite HMR client must always be first
            sb.AppendLine(ScriptTag($"{origin}/@vite/client"));

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                var path = "/" + NormalizePath(entry);

                sb.AppendLine(IsCssEntry(entry)
                    ? LinkTag(origin + path)    // <link> prevents FOUC
                    : ScriptTag(origin + path));
            }

            return new HtmlString(sb.ToString());
        }

        #endregion

        #region Production rendering

        private static IHtmlString RenderProdTags(string[] entries)
        {
            var manifest = GetManifest();
            var sb = new StringBuilder();
            var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;
                if (!TryGetManifestEntry(manifest, entry, out var asset)) continue;

                // Emit associated CSS chunks first
                if (asset.Css != null)
                {
                    foreach (var css in asset.Css)
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

        #endregion

        #region Manifest

        private static Dictionary<string, ManifestEntry> GetManifest()
        {
            if (_manifest != null) return _manifest;

            lock (ManifestLock)
            {
                if (_manifest != null) return _manifest;

                if (HttpContext.Current?.Server == null)
                    throw new InvalidOperationException(
                        "Cannot resolve the Vite manifest without an active HTTP context.");

                var manifestPath = HttpContext.Current.Server.MapPath(ManifestVirtualPath());

                if (!File.Exists(manifestPath))
                    throw new FileNotFoundException(
                        "Vite manifest not found. Run 'npm run build' to generate it.", manifestPath);

                var raw = JsonConvert.DeserializeObject<Dictionary<string, ManifestEntry>>(
                              File.ReadAllText(manifestPath))
                          ?? new Dictionary<string, ManifestEntry>();

                _manifest = new Dictionary<string, ManifestEntry>(raw, StringComparer.OrdinalIgnoreCase);
            }

            return _manifest;
        }

        private static bool TryGetManifestEntry(
            Dictionary<string, ManifestEntry> manifest,
            string entry,
            out ManifestEntry asset)
        {
            var normalized = NormalizePath(entry);
            var fileName = Path.GetFileName(normalized);

            // 1. Exact match
            if (manifest.TryGetValue(normalized, out asset) ||
                manifest.TryGetValue("/" + normalized, out asset))
                return true;

            // 2. Fuzzy scan — filename or src match
            foreach (var kvp in manifest)
            {
                var key = NormalizePath(kvp.Key);
                var keyFileName = Path.GetFileName(key);

                if (string.Equals(key, normalized, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(fileName) &&
                     string.Equals(keyFileName, fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    asset = kvp.Value;
                    return true;
                }

                if (kvp.Value?.Src != null)
                {
                    var src = NormalizePath(kvp.Value.Src);
                    var srcFileName = Path.GetFileName(src);

                    if (string.Equals(src, normalized, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(fileName) &&
                         string.Equals(srcFileName, fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        asset = kvp.Value;
                        return true;
                    }
                }
            }

            asset = null;
            return false;
        }

        #endregion

        #region Tag builders

        private static string ScriptTag(string src)
        {
            var nonce = GetNonce();
            var nonceAttr = string.IsNullOrEmpty(nonce) ? "" : $" nonce=\"{nonce}\"";
            return $"<script type=\"module\" src=\"{src}\"{nonceAttr}></script>";
        }

        private static string LinkTag(string href)
        {
            var nonce = GetNonce();
            var nonceAttr = string.IsNullOrEmpty(nonce) ? "" : $" nonce=\"{nonce}\"";
            return $"<link rel=\"stylesheet\" href=\"{href}\"{nonceAttr} />";
        }

        #endregion

        #region URL helpers

        private static string AssetUrl(string relativeAsset)
        {
            var distPath = DistPath().Trim('/');
            var assetPath = NormalizePath(relativeAsset).TrimStart('/');
            var appBase = AppBasePath();

            return string.IsNullOrEmpty(appBase)
                ? $"/{distPath}/{assetPath}"
                : $"{appBase}/{distPath}/{assetPath}";
        }

        private static string AppBasePath()
        {
            var appPath = HttpContext.Current?.Request?.ApplicationPath;
            return string.IsNullOrEmpty(appPath) || appPath == "/" ? string.Empty : appPath.TrimEnd('/');
        }

        private static string DistPath()
        {
            var path = ConfigurationManager.AppSettings["ViteDistPath"];
            return string.IsNullOrWhiteSpace(path) ? "wwwroot/dist" : path.Trim().Trim('~', '/');
        }

        private static string ManifestVirtualPath() =>
            "~/" + DistPath().Replace("\\", "/").Trim('/') + "/.vite/manifest.json";

        #endregion

        #region Utility

        private static string GetNonce() =>
            HttpContext.Current?.Items[ContentSecurityPolicyFilter.NonceKey] as string ?? string.Empty;

        private static bool IsCssEntry(string path) =>
            !string.IsNullOrWhiteSpace(path) &&
            CssExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            var normalized = path.Replace("\\", "/").Trim().TrimStart('~', '/');

            return normalized.StartsWith("./", StringComparison.Ordinal)
                ? normalized.Substring(2)
                : normalized;
        }

        #endregion

        #region Manifest model

        private class ManifestEntry
        {
            [JsonProperty("file")] public string File { get; set; }
            [JsonProperty("src")] public string Src { get; set; }
            [JsonProperty("isEntry")] public bool IsEntry { get; set; }
            [JsonProperty("css")] public List<string> Css { get; set; }
            [JsonProperty("imports")] public List<string> Imports { get; set; }
        }

        #endregion
    }
}
