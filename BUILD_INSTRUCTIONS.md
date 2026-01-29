# Build, Run, and Test Guide

This repository hosts three deliverables:

- **cinema-ticket-ui** – Angular SPA (build-time dependency only)
- **cinema-ticket-java** – Spring Boot backend that serves `http://localhost:8080`
- **cinema-ticket-C#** – ASP.NET Core backend that serves 
      `http://localhost:5125` (or `https://localhost:7137` when the HTTPS profile is used)

## ⚠️ **Important: Build Order**

**Always build in this order:**
1. **Frontend (Angular)** - Build first to generate static files
2. **Backend (Java)** - Build second to package with frontend files
3. **Backend (C#)** - Build last to include frontend files

---

## 1. Frontend (Angular) - **BUILD FIRST**

> Angular **must be built first** - the generated static files are automatically copied into both backends.

### Prerequisites

cd cinema-ticket-ui
npm install

### Build Production Bundle

npm run build

**What this does:**
- Builds Angular with production optimizations (minification, bundling)
- Output: `cinema-ticket-ui/dist/cinema-ticket-ui/browser/`
- Automatically copies files to:
  - `cinema-ticket-java/src/main/resources/static/` ✓
  - `cinema-ticket-java/target/classes/static/` ✓
  - `cinema-ticket-C#/wwwroot/` ✓

**Generated files:**
- `index.html` - Main HTML file
- `main-*.js` - Minified app bundle (~450 KB)
- `polyfills-*.js` - Browser polyfills (~35 KB)
- `styles-*.css` - Minified styles (~2.5 KB)
- `favicon.ico` - App icon


### Optional: Run Tests
npm run test

---

## 2. Java Backend (Spring Boot) - **BUILD SECOND**

> ⚠️ **Prerequisite:** Frontend must be built first (section 1)


### Build the Backend
cd cinema-ticket-java
mvn clean package -DskipTests

**What this does:**
- Compiles Java source code
- Packages application with frontend static files
- Creates executable JAR: `target/cinema-ticket-java-0.0.1-SNAPSHOT.jar`
- JAR includes all frontend files from `src/main/resources/static/`

### Run the Application


java -jar target/cinema-ticket-java-0.0.1-SNAPSHOT.jar

**Option 2: Run with Maven (Development)**

**Option 2: Run with Maven (Development)**
mvn spring-boot:run

**Access the application:**
- Frontend + APIs: `http://localhost:8080`

- SPA routing handled by `WebConfig.java` (all non-API routes → `index.html`)

### Run Tests
mvn test

---

## 3. C# Backend (ASP.NET Core) - **BUILD THIRD**


cd cinema-ticket-C#
dotnet ef database update

### (Optional) Seed Default Accounts

dotnet run -- --seed-only

### Build & Run the API
ault Accounts
dotnet run -- --seed-only

### Build & Run the API
dotnet build
dotnet run


dotnet test- Frontend + APIs: `http://localhost:5125`
- HTTPS (after `dotnet dev-certs https --trust`): `https://localhost:7137`
- Swagger UI: `http://localhost:5125/swagger`

### Run Tests
dotnet test


---

## 4. Manual End-to-End Testing Checklist

Use after both backends are running with fresh Angular assets.

### Testing Workflow

1. **Build and Start Application:**
   - Build frontend: `cd cinema-ticket-ui && npm run build`
   - Start Java backend: `cd cinema-ticket-java && java -jar target/*.jar`
   - OR start C# backend: `cd cinema-ticket-C# && dotnet run`

2. **Authentication Testing:**
   - Register new user via `/api/auth/register` or UI
   - Login with credentials (customer & admin)
   - Verify JWT token handling

3. **Profile Management:**
   - Fetch user profile: `/api/users/me`
   - Update user information
   - Confirm concurrency handling via row versions

4. **Cinema/Movie CRUD (Admin Only):**
   - Create/update/delete cinemas
   - Create/update/delete movies
   - Verify UI reflects changes immediately

5. **Screenings:**
   - Create new screenings
   - List upcoming screenings
   - Delete screenings
   - Verify upcoming list in Angular UI

6. **Seat Selection & Reservations:**
   - View seat map for screening
   - Reserve seats
   - View reservations
   - Cancel reservations
   - Validate conflict/concurrency responses

7. **Analytics/Payments (Java-only):**
   - Exercise reporting endpoints
   - Test payment flows if enabled

8. **Static Hosting & SPA Routing:**
   - Refresh deep links (e.g., `/dashboard`, `/movies`, `/profile`)
   - Confirm SPA fallback works (no 404 errors)
   - Verify Angular router handles navigation

---

## 5. Complete Build Process (All Components)

### Full Build Script (Execute in Order)

# Step 1: Build Frontend (MUST BE FIRST)
cd cinema-ticket-ui
npm install
npm run build

# Step 2: Build Java Backend (SECOND)
cd ../cinema-ticket-java
mvn clean package -DskipTests

# Step 3: Build C# Backend (THIRD)
cd ../cinema-ticket-C#
dotnet build

### Run Both Backends Simultaneously

**Terminal 1 - Java Backend:**

cd cinema-ticket-java
java -jar target/cinema-ticket-java-0.0.1-SNAPSHOT.jar

Access at: `http://localhost:8080`

**Terminal 2 - C# Backend:**

cd cinema-ticket-C#
dotnet run

Access at: `http://localhost:5125` (HTTP) or `https://localhost:7137` (HTTPS)

> **Note:** Use `dotnet run --launch-profile https` for HTTPS. First run `dotnet dev-certs https --trust` to trust the development certificate.

---

## 6. Quick Reference Commands

### Frontend Development (with hot reload)

cd cinema-ticket-ui
npm start
# Access at http://localhost:4200 (proxies API calls to backend)

### Java Development (without building JAR)

cd cinema-ticket-java
mvn spring-boot:run
# Access at http://localhost:8080

### Check JAR File

cd cinema-ticket-java
ls -lh target/*.jar

### Verify Static Files

# Check if frontend files exist in Java backend
ls -la cinema-ticket-java/src/main/resources/static/

# Check if frontend files exist in C# backend
ls -la cinema-ticket-C#/wwwroot/

---

## 7. Troubleshooting

### Frontend not showing in backend?
1. Make sure you built the frontend first: `cd cinema-ticket-ui && npm run build`
2. Verify files exist: `ls cinema-ticket-java/src/main/resources/static/`
3. Rebuild backend to include updated files

### JAR file not found?

cd cinema-ticket-ui && npm run build
cd ../cinema-ticket-java && mvn clean package -DskipTestsuild frontend and backend in order:
```bash
cd cinema-ticket-ui && npm run build
cd ../cinema-ticket-java && mvn clean package -DskipTests
```

---

Keep this guide in sync whenever build steps change (new scripts, ports, or environments).
