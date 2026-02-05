# Cinema Ticket Purchasing System

## Complete Full-Stack Application with Spring Boot + Angular

This project implements a comprehensive cinema ticket purchasing system for Lab Tasks 3-4.

## 🎯 Project Overview

A full-stack web application that allows users to:
- Register and manage their profiles
- Browse available movie screenings
- View cinema seat layouts in real-time
- Reserve and cancel seat reservations
- Administrators can manage screenings

## 🏗️ Architecture

### Backend: Spring Boot (Java)
- RESTful API
- JWT Authentication
- Spring Security
- JPA/Hibernate (Entity Framework)
- H2 Database (development)
- MySQL/SQL Server support (production)

### Frontend: Angular
- Responsive UI with Bootstrap
- Component-based architecture
- HTTP client for API integration
- Route guards for authentication
- Real-time seat selection

## ✅ Requirements Implementation

### Task 3: Core Features
- [x] User registration and authentication
- [x] User profile editing with concurrency control (Optimistic Locking)
- [x] Administrator user management (edit/delete users)
- [x] Screening creation and deletion by administrators
- [x] Cascade deletion of reservations when screening is deleted
- [x] Parallelism handling for concurrent operations

### Task 4: Advanced Features
- [x] Seat reservation with row/seat position (x,y)
- [x] Seat cancellation functionality
- [x] Display room occupancy with free/reserved seats
- [x] Conflict handling for concurrent seat reservations
- [x] Unique constraint + optimistic locking
- [x] Production-ready configuration

## 📋 Technologies Used

### Backend
- **Java 17** - Programming language
- **Spring Boot 3.2.1** - Framework
- **Spring Security** - Authentication & Authorization
- **Spring Data JPA** - Database access
- **JWT (JSON Web Tokens)** - Secure authentication
- **Lombok** - Reduce boilerplate
- **H2 Database** - In-memory database for development
- **MySQL/SQL Server** - Production database options
- **Maven** - Build tool

### Frontend (To be implemented)
- **Angular 17** - Frontend framework
- **TypeScript** - Programming language
- **Bootstrap 5** - CSS framework
- **RxJS** - Reactive programming

## 📁 Project Structure

```
cinema-ticket-purchasing-system/
├── Backend/                          # Spring Boot Backend
│   ├── src/
│   │   ├── main/
│   │   │   ├── java/com/cinema/ticketsystem/
│   │   │   │   ├── config/          # Security & Data Init
│   │   │   │   ├── controller/      # REST API Controllers
│   │   │   │   ├── dto/             # Data Transfer Objects
│   │   │   │   ├── model/           # JPA Entities
│   │   │   │   ├── repository/      # Data Access
│   │   │   │   ├── security/        # JWT & Auth
│   │   │   │   ├── service/         # Business Logic
│   │   │   │   └── CinemaTicketSystemApplication.java
│   │   │   └── resources/
│   │   │       ├── application.properties
│   │   │       └── application-prod.properties
│   │   └── test/
│   ├── pom.xml                      # Maven configuration
│   └── README.md
├── Frontend/                        # Angular Frontend (To be added)
└── README.md                        # This file
```

## 🗄️ Database Schema

### User Table
- `id` (PK, Long)
- `email` (String, Unique)
- `password` (String, encrypted)
- `first_name` (String)
- `last_name` (String)
- `phone_number` (String)
- `is_admin` (Boolean)
- `created_at` (Timestamp)
- `version` (Long) - For optimistic locking

### Cinema Table
- `id` (PK, Long)
- `name` (String)
- `rows` (Integer)
- `seats_per_row` (Integer)

### Screening Table
- `id` (PK, Long)
- `cinema_id` (FK -> Cinema)
- `movie_title` (String)
- `start_date_time` (Timestamp)
- `created_at` (Timestamp)

### Reservation Table
- `id` (PK, Long)
- `user_id` (FK -> User)
- `screening_id` (FK -> Screening)
- `row_number` (Integer)
- `seat_number` (Integer)
- `created_at` (Timestamp)
- `version` (Long) - For optimistic locking
- **Unique Constraint**: (screening_id, row_number, seat_number)

## 🚀 Getting Started

### Prerequisites
- Java 17 or higher
- Maven 3.6+
- Node.js 18+ and npm (for Angular frontend)
- Git

### Backend Setup

1. **Navigate to Backend directory:**
```bash
cd Backend
```

2. **Build the project:**
```bash
mvn clean install
```

3. **Run the application:**
```bash
mvn spring-boot:run
```

4. **Access the application:**
- API: http://localhost:8080
- H2 Console: http://localhost:8080/h2-console
  - JDBC URL: `jdbc:h2:mem:cinemadb`
  - Username: `sa`
  - Password: (empty)

5. **Default Admin Login:**
- Email: `admin@cinema.com`
- Password: `Admin123!`

### Frontend Setup (When Angular is added)

1. **Navigate to Frontend directory:**
```bash
cd Frontend
```

2. **Install dependencies:**
```bash
npm install
```

3. **Run development server:**
```bash
ng serve
```

4. **Access the application:**
- Open browser: http://localhost:4200

## 🔧 Configuration

### Development Database (H2)
Default configuration in `application.properties`:
```properties
spring.datasource.url=jdbc:h2:mem:cinemadb
spring.h2.console.enabled=true
```

### Production Database

#### MySQL Configuration
Update `application-prod.properties`:
```properties
spring.datasource.url=jdbc:mysql://localhost:3306/cinemadb
spring.datasource.username=your_username
spring.datasource.password=your_password
spring.jpa.database-platform=org.hibernate.dialect.MySQLDialect
```

#### SQL Server Configuration
Update `application-prod.properties`:
```properties
spring.datasource.url=jdbc:sqlserver://localhost:1433;databaseName=cinemadb
spring.datasource.username=your_username
spring.datasource.password=your_password
spring.jpa.database-platform=org.hibernate.dialect.SQLServerDialect
```

### Run in Production Mode
```bash
mvn clean package
java -jar -Dspring.profiles.active=prod target/ticket-system-1.0.0.jar
```

## 🔐 API Documentation

### Authentication Endpoints
```
POST /api/auth/register     - Register new user
POST /api/auth/login        - Login (returns JWT token)
```

### User Endpoints
```
GET    /api/users           - Get all users (Admin only)
GET    /api/users/me        - Get current user
PUT    /api/users/{id}      - Update user (optimistic locking)
DELETE /api/users/{id}      - Delete user (Admin only)
```

### Cinema Endpoints
```
GET    /api/cinemas         - Get all cinemas (public)
```

### Screening Endpoints
```
GET    /api/screenings      - Get all screenings (public)
GET    /api/screenings/{id} - Get screening with seat map
POST   /api/screenings      - Create screening (Admin only)
DELETE /api/screenings/{id} - Delete screening (Admin only)
```

### Reservation Endpoints
```
GET    /api/reservations/my                                          - Get user's reservations
POST   /api/reservations                                             - Reserve a seat
DELETE /api/reservations/screening/{id}/row/{row}/seat/{seat}       - Cancel reservation
```

## 🔒 Security Features

1. **JWT Authentication**
   - Stateless authentication
   - Token-based authorization
   - Automatic token validation

2. **Password Encryption**
   - BCrypt hashing
   - Salted passwords
   - Secure storage

3. **Role-Based Access Control**
   - USER role for regular users
   - ADMIN role for administrators
   - Method-level security with `@PreAuthorize`

4. **CORS Configuration**
   - Configured for Angular frontend
   - Allows credentials
   - Restricts origins in production

## 🔄 Concurrency Control

### User Updates (Optimistic Locking)
- `@Version` annotation on User entity
- Prevents lost updates when multiple admins edit same user
- Returns 409 Conflict on version mismatch

### Seat Reservations (Conflict Prevention)
- Unique database constraint on (screening_id, row, seat)
- `@Version` annotation on Reservation entity
- First reservation wins, second gets error
- Clear error message: "This seat is already reserved"

## 🧪 Testing the API

### Example: Register a User
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "password123",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890"
  }'
```

### Example: Create a Screening (Admin)
```bash
curl -X POST http://localhost:8080/api/screenings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "cinemaId": 1,
    "movieTitle": "Inception",
    "startDateTime": "2026-02-15T20:00:00"
  }'
```

### Example: Reserve a Seat
```bash
curl -X POST http://localhost:8080/api/reservations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "screeningId": 1,
    "row": 5,
    "seat": 10
  }'
```

## 📊 Pre-seeded Data

The application automatically creates:

**5 Cinemas:**
1. Cinema Grand Plaza - 10 rows × 15 seats = 150 seats
2. Cinema Royal - 8 rows × 12 seats = 96 seats
3. Cinema Star - 12 rows × 20 seats = 240 seats
4. Cinema Downtown - 6 rows × 10 seats = 60 seats
5. Cinema Luxury - 5 rows × 8 seats = 40 seats

**1 Admin User:**
- Email: admin@cinema.com
- Password: Admin123!

## 🎨 Frontend Features (To be implemented)

1. **User Pages:**
   - Registration/Login page
   - Profile management
   - Browse screenings
   - Interactive seat selection (grid view)
   - My reservations page

2. **Admin Pages:**
   - User management dashboard
   - Screening creation/deletion
   - View all reservations

3. **Responsive Design:**
   - Mobile-friendly
   - Bootstrap components
   - Modern UI/UX

## 📝 Development Notes

### Key Design Decisions

1. **Optimistic Locking vs Pessimistic Locking**
   - Used optimistic locking (`@Version`) for better performance
   - Suitable for web applications with low conflict probability
   - Allows concurrent reads without locks

2. **Cascade Delete**
   - Screening deletion automatically removes reservations
   - Prevents orphaned data
   - Configured in JPA relationship

3. **JWT Token Storage**
   - Tokens contain userId and isAdmin claims
   - 7-day expiration
   - Client stores in localStorage/sessionStorage

4. **Error Handling**
   - Consistent error response format
   - Clear messages for concurrent conflicts
   - HTTP status codes follow REST conventions

## 🐛 Common Issues & Solutions

### Issue: Port 8080 already in use
```bash
# Find process using port 8080
netstat -ano | findstr :8080
# Kill the process (replace PID)
taskkill /PID <PID> /F
```

### Issue: H2 Console not accessible
- Ensure `spring.h2.console.enabled=true` in application.properties
- Check the correct JDBC URL: `jdbc:h2:mem:cinemadb`

### Issue: JWT token expired
- Login again to get a new token
- Token validity: 7 days (configurable in application.properties)

## 📚 Learning Resources

- [Spring Boot Documentation](https://spring.io/projects/spring-boot)
- [Spring Security Guide](https://spring.io/guides/topicals/spring-security-architecture)
- [JPA & Hibernate](https://spring.io/guides/gs/accessing-data-jpa/)
- [Angular Documentation](https://angular.io/docs)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)

## 🤝 Contributing

This is a lab task project. For improvements or bug fixes:
1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit for review

## 📄 License

This project is created for educational purposes as part of a university lab assignment.

## 👤 Author

Created for Lab Tasks 3-4: Cinema Ticket Purchasing System

---

**Happy Coding! 🎬🎟️**
