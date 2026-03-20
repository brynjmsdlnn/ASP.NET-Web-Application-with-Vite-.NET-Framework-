# ASP.NET Web Application with Vite (.NET Framework)

ASP.NET MVC/Web API project built on **.NET Framework 4.8.1** with **Vite** and **Tailwind CSS 4.2** for frontend bundling.
This template keeps the classic ASP.NET project structure while using modern utility-first frontend tooling for JS/CSS builds.

## Tech Stack

- .NET Framework 4.8.1
- ASP.NET MVC 5 + ASP.NET Web API 5
- Vite (frontend bundler)
- NuGet (`packages.config`) + Node package manager (NPM)
- CSS: Tailwind CSS 4.2

## Repository Structure

- `ASP.NET Web Application with Vite (.NET Framework).csproj` - Main project file
- `Controllers/` - MVC controllers
- `App_Start/` - Route, Web API, and MVC configuration
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

## Useful Scripts

- `npm run dev` - start Vite dev server on `localhost:5173`
- `npm run build` - build production assets into `wwwroot/dist`
- `npm run preview` - preview Vite output

## Notes

- `node_modules`, `bin`, `obj`, `.vs`, and `wwwroot/dist` are ignored in `.gitignore`
  to keep tracked files source-focused.
- This project uses `packages.config` and classic ASP.NET project style.

## License

This project is provided as-is for learning/template purposes.

