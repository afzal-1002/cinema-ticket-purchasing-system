# Frontend Build & Deployment Guide

## Overview

The Cinema Ticket UI (Angular frontend) is configured to automatically build and deploy to the Java Spring Boot backend. When you run `npm run build`, the minified production files are automatically copied to the Java backend's static resources folder.

## Quick Start

### 1. Build Frontend and Deploy to Java Backend

```bash
cd cinema-ticket-ui
npm run build
```

This single command will:
- ✓ Build the Angular app with production optimizations
- ✓ Minify and bundle all JavaScript and CSS files
- ✓ Copy files to `cinema-ticket-java/src/main/resources/static/`
- ✓ Copy files to `cinema-ticket-java/target/classes/static/` (for immediate testing)
- ✓ Also copy to `cinema-ticket-C#/wwwroot/` (C# backend compatibility)

### 2. Run the Java Backend

```bash
cd cinema-ticket-java
mvn spring-boot:run
```

### 3. Access the Application

Open your browser and navigate to:
```
http://localhost:8080
```

The Java backend will serve the Angular frontend from its static resources folder.

## Build Commands

| Command | Description |
|---------|-------------|
| `npm run build` | Production build + deploy to backends |
| `npm run build:prod` | Same as above (alias) |
| `npm run build:java` | Specifically for Java backend |
| `npm start` | Development server (port 4200 with proxy) |

## How It Works

### Build Process

1. **Angular Build**: `ng build` compiles TypeScript, optimizes bundles, and generates:
   - `index.html` - Main HTML file
   - `main-*.js` - Application bundle (minified)
   - `polyfills-*.js` - Browser polyfills (minified)
   - `styles-*.css` - CSS bundle (minified)
   - `favicon.ico` - App icon

2. **Sync Script**: `scripts/sync-static.mjs` automatically copies files to:
   - **Source folder**: `cinema-ticket-java/src/main/resources/static/`
     - Used by Maven/Spring Boot to include in the final JAR
   - **Compiled folder**: `cinema-ticket-java/target/classes/static/`
     - Allows immediate testing without rebuilding Java app
   - **C# folder**: `cinema-ticket-C#/wwwroot/`
     - For C# backend compatibility

### Java Backend Configuration

The Java backend includes these configurations:

#### WebConfig.java
```java
@Configuration
public class WebConfig implements WebMvcConfigurer {
    // Serves static files from classpath:/static/
    // Handles SPA routing by returning index.html for non-API routes
}
```

#### SecurityConfig.java
```java
// Allows public access to static resources:
.requestMatchers(
    "/",
    "/index.html",
    "/**/*.js",
    "/**/*.css"
).permitAll()
```

## Development vs Production

### Development Mode (Recommended for Development)

```bash
# Terminal 1: Start Java backend
cd cinema-ticket-java
mvn spring-boot:run

# Terminal 2: Start Angular dev server
cd cinema-ticket-ui
npm start
```

Access at: `http://localhost:4200` (with hot reload)

**Benefits:**
- Hot reload for instant feedback
- Source maps for debugging
- API calls proxied to backend via `proxy.conf.json`

### Production Mode (For Testing/Deployment)

```bash
# Build frontend
cd cinema-ticket-ui
npm run build

# Run backend (serves frontend)
cd cinema-ticket-java
mvn spring-boot:run
```

Access at: `http://localhost:8080` (production build)

**Benefits:**
- Optimized bundles (smaller file sizes)
- Minified code (faster load times)
- Single server deployment
- Tests the actual production configuration

## File Structure After Build

```
cinema-ticket-java/
├── src/
│   └── main/
│       └── resources/
│           └── static/              ← Source files (committed to git)
│               ├── index.html
│               ├── main-*.js
│               ├── polyfills-*.js
│               ├── styles-*.css
│               └── favicon.ico
└── target/
    └── classes/
        └── static/                   ← Compiled files (temporary, not committed)
            ├── index.html
            ├── main-*.js
            ├── polyfills-*.js
            ├── styles-*.css
            └── favicon.ico
```

## Troubleshooting

### Build files not updated?

```bash
cd cinema-ticket-ui
npm run build
```

### Backend not serving frontend?

1. Check if static files exist:
   ```bash
   ls -la cinema-ticket-java/src/main/resources/static/
   ```

2. Restart the Java backend:
   ```bash
   cd cinema-ticket-java
   mvn spring-boot:run
   ```

### 404 on page refresh?

This is expected if SPA routing isn't configured. The `WebConfig.java` handles this by returning `index.html` for all non-API routes.

### CORS errors during development?

Make sure you're using the Angular dev server (`npm start`) which proxies API calls to the backend.

## Production Deployment

For production deployment, build the frontend and package the Java backend:

```bash
# 1. Build frontend
cd cinema-ticket-ui
npm run build

# 2. Package Java backend (includes static files)
cd cinema-ticket-java
mvn clean package

# 3. Deploy the generated JAR
java -jar target/cinema-ticket-system-*.jar
```

The JAR file will include all static frontend files and can be deployed as a single artifact.

## Additional Notes

- Static files in `src/main/resources/static/` are automatically included in the Spring Boot JAR
- The sync script handles multiple backend targets (Java and C#) simultaneously
- Build artifacts use content hashing (e.g., `main-PHTSG4NO.js`) for cache busting
- The `target/` folder is temporary and should not be committed to version control
