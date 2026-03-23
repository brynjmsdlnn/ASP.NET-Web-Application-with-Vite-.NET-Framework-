# ASP.NET Web Application with Vite (.NET Framework)

ASP.NET MVC/Web API project built on **.NET Framework 4.8.1** with **Vite** and **Tailwind CSS 4.2** for frontend bundling.
This template keeps the classic ASP.NET project structure while using modern utility-first frontend tooling for JS/CSS builds.

## Tech Stack

- .NET Framework 4.8.1
- ASP.NET MVC 5 + ASP.NET Web API 2
- Vite (frontend bundler)
- NuGet (`packages.config`) + Node package manager (NPM)
- CSS: Tailwind CSS 4.2
- Icons: Lucide

## Repository Structure

- `ASP.NET Web Application with Vite (.NET Framework).csproj` - Main project file
- `Controllers/` - MVC controllers
- `App_Start/` - Route, Web API, and MVC configuration
- `Filters/` - ASP.NET filters (CSP headers)
- `Helpers/` - Helper classes
- `Middleware/` - Vite helper middleware
- `Scripts/app.js` - JavaScript entry source
- `Styles/app.css` - Tailwind + custom CSS entry source
- `Views/` - Razor views
- `Web.config` - ASP.NET application configuration
- `vite.config.mjs` - Vite build/dev configuration
- `package.json` - Frontend dependencies and scripts
- `packages.config` - NuGet dependencies

## Frontend Build

Vite is configured to build assets into:

- `wwwroot/dist`

The Vite config points to:

- `Scripts/app.js` (script entry)
- `Styles/app.css` (style entry)

## Requirements

- Visual Studio 2019/2022 with ASP.NET workload
- .NET Framework 4.8.1 SDK installed
- Node.js 20.19+ (or 22.12+; Vite 8 ESM-only runtime requirement)
- npm (or your preferred package manager) running on a supported Node runtime

## Getting Started

### 1) Restore NuGet packages

```powershell
nuget restore "ASP.NET Web Application with Vite (.NET Framework).csproj"
```

or restore inside Visual Studio (right-click project > **Restore NuGet Packages**).

### 2) Install frontend dependencies

```powershell
npm install
```

### 3) Build frontend assets

```powershell
npm run build
```

### 4) Run project

Open the project in Visual Studio and run with IIS Express.

You can run Vite dev server separately when needed:

```powershell
npm run dev
```

### 5) Enable CSP security headers (already included)

This project uses a global action filter to emit a nonce-based `Content-Security-Policy` header.

- `Filters/ContentSecurityPolicyFilter.cs`
  - Generates a random nonce in `OnActionExecuting`
  - Stores it in `HttpContext.Items[NonceKey]`
  - Uses the nonce in `OnResultExecuting` to emit `script-src` and `style-src`
  - Adds Vite exception sources when dev mode is enabled
  - Reads Vite dev-mode settings from config and writes to:
    - `CSP_EXTRA_SCRIPT_SRC`
    - `CSP_EXTRA_STYLE_SRC`
    - `CSP_EXTRA_CONNECT_SRC`
- Registered globally in `App_Start/FilterConfig.cs`
- Applied via `Global.asax.cs` through `FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)`
- `Helpers/NonceHelper.cs` exposes `@Html.CspNonce()` for Razor views

`Views/Shared/_Layout.cshtml` applies the nonce to the inline theme script.
`Helpers/ViteHelper.cs` focuses on rendering `<script>` and `<link>` tags.

## Configuration

CSP + Vite dev-server behavior is controlled through `Web.config` app settings:

- `UseViteDevServer` (`true`/`false`)
- `ViteDevServerOrigin` (for example: `http://localhost:5173`)
- `ViteDistPath` (for production build output, defaulting to `wwwroot/dist`)

## Useful Scripts

- `npm run dev` - start Vite dev server on `localhost:5173`
- `npm run build` - build production assets into `wwwroot/dist`
- `npm run preview` - preview Vite output

## Notes

- `node_modules`, `bin`, `obj`, `.vs`, and `wwwroot/dist` are ignored in `.gitignore`
  to keep tracked files source-focused.
- This project uses `packages.config` and classic ASP.NET project style.
- Theme and mobile menu icons are rendered through `lucide` with Tailwind class swaps (`dark:block`, `hidden`).

## Template Dependency Notes

- This template keeps `$safeprojectname$` as the tokenized namespace/assembly value for template instantiation.
- Frontend legacy browser bundles were removed:
  - `bootstrap`
  - `jQuery`
  - `jQuery.Validation`
  - `Microsoft.jQuery.Unobtrusive.Validation`
  - `Modernizr`
- Previously removed during cleanup:
  - `Microsoft.AspNet.Web.Optimization`
  - `WebGrease`
  - `Antlr` / `Antlr3.Runtime`
- `Microsoft.Web.Infrastructure` is intentionally kept because MVC/WebPages pre-start initialization requires it.

## Validation tradeoff (template decision)

- This template favors a Vite-first front-end flow and keeps the jQuery unobtrusive validation stack out by default.
- Practical effect:
  - Data annotations still enforce validation on the server (`ModelState` on postback).
  - Client-side annotation-based validation is not enabled automatically from `Html.*For(...)` helpers.
  - Forms with a legacy MVC UX expectation (`data-val-*` auto-wired by `jquery.validate`) need a replacement path.
- If you want the traditional MVC unobtrusive behavior:
  - Re-add `jQuery`, `jQuery.Validation`, and `Microsoft.jQuery.Unobtrusive.Validation`.
  - Keep/restore `ClientValidationEnabled` and `UnobtrusiveJavaScriptEnabled` in config.
  - Include the required JS scripts in your layout (`jquery`, `jquery.validate`, `jquery.validate.unobtrusive`).
- Modern alternative for this template:
  - Keep server-side validation as the source of truth and add targeted client validation via your preferred Vite-managed JS.
  - Use existing model constraints to generate server validation messages while validating critical UX paths in custom JS modules.

## License

This project is provided as-is for learning/template purposes.
