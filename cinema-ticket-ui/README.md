# Cinema Ticket UI

This is the Angular frontend for the Cinema Ticket Purchasing System. It's designed to work with both the Java Spring Boot and C# backends.

## Prerequisites

- Node.js (v18 or higher)
- npm (v9 or higher)

## Development Server

For development with live reload and API proxy to the backend:

```bash
npm start
# or
ng serve
```

Navigate to `http://localhost:4200/`. The application will automatically reload when you change source files. API calls to `/api/*` are proxied to `http://localhost:8080` (Java backend).

## Building for Production

### Build and Deploy to Java Backend

To build the Angular app and automatically copy the minified files to the Java backend:

```bash
npm run build
# or for production optimized build
npm run build:prod
```

This will:
1. Build the Angular application with production optimizations
2. Minify and bundle all JavaScript and CSS files
3. Automatically copy the build artifacts to:
   - `../cinema-ticket-java/src/main/resources/static/` (source folder)
   - `../cinema-ticket-java/target/classes/static/` (compiled folder for immediate testing)
   - `../cinema-ticket-C#/wwwroot/` (C# backend)

### Build Scripts

- `npm start` - Start development server with hot reload (port 4200)
- `npm run build` - Build for production and sync to backends
- `npm run build:prod` - Alias for production build
- `npm run build:java` - Build and deploy specifically for Java backend
- `npm run watch` - Build in watch mode for development
- `npm test` - Run unit tests

## Project Structure

```
cinema-ticket-ui/
├── src/
│   ├── app/           # Angular components, services, and modules
│   ├── assets/        # Static assets (images, fonts, etc.)
│   └── index.html     # Main HTML file
├── scripts/
│   └── sync-static.mjs # Script to copy build files to backends
├── proxy.conf.json    # Proxy configuration for development
└── angular.json       # Angular CLI configuration
```

## Integration with Java Backend

The Java Spring Boot backend serves the Angular static files from `src/main/resources/static/`. The backend is configured to:

- Serve all static files (HTML, CSS, JS) from the `/static` folder
- Handle SPA routing by redirecting all non-API requests to `index.html`
- Allow Angular router to handle client-side routing

### Backend Configuration

The Java backend includes:
- `WebConfig.java` - Configures resource handlers and SPA routing
- `SecurityConfig.java` - Allows public access to static resources
- Static files are served from `classpath:/static/`

## Development Workflow

1. **Start the backend** (Java on port 8080):
   ```bash
   cd ../cinema-ticket-java
   ./mvnw spring-boot:run
   ```

2. **Start the frontend** (Angular on port 4200):
   ```bash
   cd cinema-ticket-ui
   npm start
   ```

3. **Make changes** to Angular code - changes will be hot-reloaded

4. **Build for production** when ready to deploy:
   ```bash
   npm run build
   ```

5. **Test the production build** by accessing the Java backend directly:
   ```
   http://localhost:8080
   ```

## Code Scaffolding

Generate Angular components, services, and other artifacts:

```bash
ng generate component component-name
ng generate service service-name
ng generate module module-name
```

## Further Help

- [Angular Documentation](https://angular.dev)
- [Angular CLI Documentation](https://angular.dev/tools/cli)
- [Spring Boot Static Resources](https://docs.spring.io/spring-boot/docs/current/reference/html/web.html#web.servlet.spring-mvc.static-content)
